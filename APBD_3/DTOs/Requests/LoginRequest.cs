using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_3.DTOs.Requests
{
	public class LoginRequest
	{
		// IndexNumber must be between s1 and s20000
		[RegularExpression("^s((1[0-9]{0,4})|([1-9][0-9]{0,3})|20000)$")]
		[Required]
		[MaxLength(100)]
		public string indexNumber { get; set; }

		[Required]
		public string password { get; set; }
	}
}
