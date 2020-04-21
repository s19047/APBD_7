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
using APBD_3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace APBD_3.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        String CONNECTION_STRING = @"Data Source=LAPTOP-11FAC326\SQLEXPRESS;Initial Catalog=s19047;Integrated Security=True";

        private IStudentServiceDb _service;
        public EnrollmentController(IStudentServiceDb service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            var response = _service.EnrollStudent(request);

            if (response is null)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(EnrollStudent), response);
        }



        [HttpPost("promotions")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(PromoteStudentRequest request)
        {
            var response = _service.PromoteStudents(request);

            if (response is null)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(PromoteStudents), response);
        }

       
    }
}

