using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;


namespace APBD_3.Handlers
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthHandler(
                 IOptionsMonitor<AuthenticationSchemeOptions> options,
                 ILoggerFactory logger,
                 UrlEncoder encoder,
                 ISystemClock clock
             //IStudentsDbService service
             ) : base(options, logger, encoder, clock)
        {

        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                 return AuthenticateResult.Fail("No authorization header");

            //Authorization: Basic dh7482hf

            // gives me the base64 part: dh7482hf
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            //convert base64 to bytes
            var credentialByte = Convert.FromBase64String(authHeader.Parameter);
            // convert the array of bytes into string
            var credentials = Encoding.UTF8.GetString(credentialByte).Split(":");  // Ahmad123:pas1233 so we split it


            if (credentials.Length != 2)
                return AuthenticateResult.Fail("Incorrect authorization header value");

            //TODO check credentials in the db

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "bob123"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "student")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name); //Basic
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);

        }
    }
}
