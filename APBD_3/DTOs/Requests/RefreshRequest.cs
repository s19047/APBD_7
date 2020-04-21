using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_3.DTOs.Requests
{
	public class RefreshRequest
	{
		public string accessToken { get; set; }
		public string refreshToken { get; set; }
	}
}
