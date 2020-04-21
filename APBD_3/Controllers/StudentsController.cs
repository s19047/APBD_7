using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using APBD_3.DTOs.Requests;
using APBD_3.DTOs.Responses;
using APBD_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace APBD_3.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public StudentsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        List<Student> students = new List<Student>();

        String CONNECTION_STRING = @"Data Source=LAPTOP-11FAC326\SQLEXPRESS;Initial Catalog=s19047;Integrated Security=True";

        [HttpGet]
        [Authorize(Roles = "employee")]
        public IActionResult GetStudents(String orderby = "FirstName")
        {
            

            using (var client = new SqlConnection(CONNECTION_STRING))
            using(var command = new SqlCommand())
            {
                command.Connection = client;
                command.CommandText = "select s.FirstName, s.LastName, s.BirthDate, st.Name as Studies, e.Semester "+
                                       "from Student s "+
                                       "join Enrollment e on e.IdEnrollment = s.IdEnrollment " +
                                       "join Studies st on st.IdStudy = e.IdStudy; ";
                client.Open();
                var response = command.ExecuteReader();
                while (response.Read()) {
                    var st = new Student();
                    st.FirstName = response["FirstName"].ToString();
                    st.DoB = DateTime.Parse(response["BirthDate"].ToString());
                    st.LastName = response["LastName"].ToString();
                    st.Studies = response["Studies"].ToString();
                    st.Semester = int.Parse(response["Semester"].ToString());

                    students.Add(st);
                }

            }
            return Ok(students);
        }

        //getting the semester number by Index number

        [HttpGet("{indexNumber}")]
        public IActionResult GetSemester(string indexNumber)
        {
            int semester = 0;
            using (var client = new SqlConnection(CONNECTION_STRING))
            using (var command = new SqlCommand())
            {
                command.Connection = client;
                command.CommandText = "SELECT e.Semester " +
                                       "FROM Student s , Enrollment e " +
                                       "WHERE s.IdEnrollment = e.IdEnrollment and s.IndexNumber = '@index';";
                command.Parameters.AddWithValue("index",indexNumber);
                client.Open();
                var response = command.ExecuteReader();
                if (response.Read())
                {
                    semester = int.Parse(response["Semester"].ToString());
                }

            }

               
                return Ok(semester);
        }



        [HttpPost]
        public IActionResult Login(LoginRequest loginRequest)
        {
            PasswordHashHandler hash = new PasswordHashHandler();
            //check pass in db
            var index = loginRequest.indexNumber;
            int IdLogin;
            using (var client = new SqlConnection(CONNECTION_STRING))
            using (var command = new SqlCommand())
            {
                command.Connection = client;
                command.CommandText = "SELECT Password From UserLogin WHERE IndexNumber = @index;";
                command.Parameters.AddWithValue("index", index);
               // command.Parameters.AddWithValue("pass", pass);

                client.Open();
                var response = command.ExecuteReader();
                if (response.Read())
                {
                    IdLogin = int.Parse(response["IdLogin"].ToString());
                    var pass = response["Password"];
                    var salt = response["PasswordSalt"].ToString();
                    if (hash.CreateHash(loginRequest.password, salt).Equals(pass))
                        return BadRequest("incorrect password");

                }
                else
                   return BadRequest("User was not found");
                

                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, IdLogin.ToString()),
                new Claim(ClaimTypes.Name, index),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "student")
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken
                (
                    issuer: "Gakko",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: credentials
                );
                // Create login response 
                LoginResponse loginResponse = new LoginResponse
                {
                    loginToken = new JwtSecurityTokenHandler().WriteToken(token),
                    refreshToken = Guid.NewGuid()
                };


                // Add refresh token into the database 
                command.CommandText = "UPDATE UserLogin SET RefreshToken = @refresh WHERE IdLogin = @id";
                command.Parameters.AddWithValue("id", IdLogin);
                command.Parameters.AddWithValue("refresh", loginResponse.refreshToken);
                var addRefresh = command.ExecuteNonQuery();

                return Ok(loginResponse);
            }

        }


        // Note I can simply accept the old refresh token and create a new one after checking with the database 
        // but i thought this way is more secure
        [HttpPost("refresh-token")]

        public IActionResult RefreshToken(RefreshRequest request)
        {
            //Validate our token 
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(request.accessToken,
                new Microsoft.IdentityModel.Tokens.TokenValidationParameters 
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = "Gakko",
                    ValidAudience = "Students",
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]))
                }, out validatedToken);

            var jwtToken = validatedToken as JwtSecurityToken;

            //if null or algorithm doesn't match the one we used before -> return SecurityTokenEception???

            if (jwtToken == null || jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                return BadRequest(new SecurityTokenException("Invalid token"));

            var index = principal.Identity.Name;

            //check in db if refreshToken exists
            using (var client = new SqlConnection(CONNECTION_STRING))
            using (var command = new SqlCommand())
            {
                command.Connection = client;
                command.CommandText = "Select RefreshToken from UserLogin where IndexNumber = @index";
                command.Parameters.AddWithValue("index", index);
                client.Open();
                var response = command.ExecuteReader();
                if (response.Read())
                {
                    var oldToken = response["RefreshToken"];
                    if (!oldToken.Equals(request.refreshToken))
                        return BadRequest("invalid refresh token");
                }

                // Create login response with new refreshToken 
                LoginResponse loginResponse = new LoginResponse
                {
                    loginToken = request.accessToken,
                    refreshToken = Guid.NewGuid()
                };


                // Add new refresh token into the database 
                command.CommandText = "UPDATE UserLogin SET RefreshToken = @refresh WHERE IndexNumber = @index";
                command.Parameters.AddWithValue("index", index);
                command.Parameters.AddWithValue("refresh", loginResponse.refreshToken);
                var addRefresh = command.ExecuteNonQuery();

                return Ok(loginResponse);
            }

            
        }

        /*
                [HttpPost]
                public IActionResult CreateStudent(Student student)
                {
                    student.IndexNumber = $"s{new Random().Next(1,20000)}";
                    return Ok(student);
                }
                [HttpDelete]
                public IActionResult DeleteStudent(int id)
                {

                    _DbService.deleteStudent(id);
                    return Ok("Delete completed");
                }

                //I realize the task said that id should be inputted 
                //however i feel like having a student inputed makes more sense
                //since i want to be able to update things like first or last name

                [HttpPut]
                public IActionResult UpdateStudent(Student student)
                {
                    _DbService.UpdateStudent(student);
                    return Ok("Update completed");
                }

            */
    }
}