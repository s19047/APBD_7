using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_3.DTOs.Responses
{
	public class LoginResponse
	{
		public string loginToken { get; set; }
		public Guid refreshToken { get; set; }
	}
}
