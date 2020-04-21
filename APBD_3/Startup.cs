using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBD_3.Handlers;
using APBD_3.MiddleWare;
using APBD_3.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace APBD_3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = "Gakko",
                        ValidAudience = "Students",
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))
                    };
                });

            //HTTP basic
           // services.AddAuthentication("AuthenticationBasic")
             // .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("AuthenticationBasic", null);
            services.AddTransient<IStudentServiceDb, SqlServerStudentDbService>();
            services.AddControllers().AddXmlSerializerFormatters();

            
            //swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Student API" , Version="v1", Description="Api to enroll and promote students"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStudentServiceDb service)
        {
            //chain of processing units that process our request (middlewares)


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //swagger documentation
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Api V1");
            });

            //logging middleware
            // Since i have it at the top it will log all requests whether it is correct or not
            app.UseMiddleware<LogMiddleware>();

            //MiddleWare to check student authorization if students wants to see his/her grades for example ( require index ) 
            // note you would need to include 'grades' in path and Index as a key in header
            app.UseWhen( context => context.Request.Path.ToString().Contains("grades") ,app => app.Use (async (context, next) =>
            {
                if (!context.Request.Headers.ContainsKey("Index"))
                {
                    //if index is not found => short circuit and return 401
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Index number is required ");
                    return;

                }

               
                //validate index by checking if index exists in the database
                String index = context.Request.Headers["Index"].ToString();

                
                if (!service.CheckIndex(index))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Index number does not exist in the database");
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Student with index number:" + index +" has the following grades: \n" +
                                                                               " bla bla bla get grades from database");
                await next(); // call next middle ware
            }));

            //based on the url decide which endpoint should respond 
            //eg: api/students  StudentsController.getStudents
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            /*we split routing and using the endpoints because we may want to use some
             Authorization before actually calling the endpoint
             calls the endpoints*/
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapControllers();
            });
        }
    }
}
