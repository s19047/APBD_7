using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace APBD_3.DTOs.Requests
{
    public class EnrollStudentRequest
    {
        // IndexNumber must be between s1 and s20000
        [RegularExpression("^s((1[0-9]{0,4})|([1-9][0-9]{0,3})|20000)$")]
        [Required]
        [MaxLength(100)]
        public string IndexNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public String FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public String LastName { get; set; }

        [Required]
        
        public String BirthDate { get; set; }

        [Required]
        public string Studies { get; set; }


    }
}
