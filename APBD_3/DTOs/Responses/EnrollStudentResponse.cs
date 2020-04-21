using APBD_3.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_3.DTOs.Responses
{
    public class EnrollStudentResponse
    {
        public string IndexNumber {get; set;}

        public int IdEnrollment { get; set; }

        public int Semester { get; set; }
    }
}
