using APBD_3.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBD_3.MiddleWare
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;

        public LogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IStudentServiceDb service)
        {
            if(context.Request != null)
            {
                string path = context.Request.Path; // /api/enrollments
                string method = context.Request.Method; // GET, POST , PUT , DELETE
                string queryString = context.Request.QueryString.ToString(); //?name=Ahmad
                string bodyStr = "";

                using(StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                }

                //save log: I could implenet the saving to log file over here but since in the future
                // I might want to save this log to the database i will implement it in IStudentService and inject it
                String request = "Path :" + path + "\nMethod: " + method + "\nQuery String: " + queryString + "\nBody: " + bodyStr;
                service.logRequest(request);
            }

            if (_next != null) await _next(context);//run next middleware
        }
    }
}
