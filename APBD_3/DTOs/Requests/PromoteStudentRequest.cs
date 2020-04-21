using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_3.DTOs.Requests
{
    public class PromoteStudentRequest
    {
        [Required]
        public String Studies { get; set; }

        [Required]
        public int Semester { get; set; }


    }
}
