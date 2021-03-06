using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Auth;
using static Google.Apis.Auth.GoogleJsonWebSignature;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Api.Configuration.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApiContext _dbContext;
        private readonly AuthenticationOptions _authOptions;

        public AuthController(ILogger<AuthController> logger,
                              IConfiguration configuration,
                              ApiContext dbContext,
                              IHttpClientFactory clientFactory,
                              IOptions<AuthenticationOptions> authOptions)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
            _clientFactory = clientFactory;
            _authOptions = authOptions.Value;
        }

        [Route("register/google")]
        [HttpPost]
        //TODO: replace object with concrete type
        //Registers new account if provided with valid Google token 
        public async Task<IActionResult> GoogleRegister()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return BadRequest("Missing Authorization header");

            string authToken = Request.Headers["Authorization"];
            if (authToken.StartsWith("Bearer "))
                authToken = new string(authToken.Skip(7).ToArray());
            

            try
            {
                //TODO SECURITY: Verify Issuer
                var googleJWTPayload = await GoogleJsonWebSignature.ValidateAsync(authToken, new ValidationSettings()
                {
                    Audience = new List<string> {
                        _configuration["IosID"],
                        _configuration["ClientID"]
                    },
                });

                //Create new account
                //Check if the user already exists in DB
                var sub = googleJWTPayload.Subject;
                if (await _dbContext.Users.FirstOrDefaultAsync(x => x.GoogleId == sub) != null)
                    return Ok(new
                    {
                        access_token = CreateJWTString(),
                        refresh_token = CreateJWTString(new Claim[] { new Claim("scope", "Refresh") }),

                    });

                //if user does not exists
                //create an account
                User u = new User()
                {
                    GoogleId = sub,
                    FacebookId = null,
                };

                await _dbContext.Users.AddAsync(u);
                await _dbContext.SaveChangesAsync();
            }
            catch (InvalidJwtException e)
            {
                return Unauthorized("Invalid token: " + e.Message);
            }

            // Returns the 'access_token' and the type in lower case
            return Ok(new
            {
                access_token = CreateJWTString(),
                refresh_token = CreateJWTString(new Claim[] { new Claim("scope", "Refresh") }),

            });
        }


        [Route("register/facebook")]
        [HttpPost]
        //TODO: replace object with concrete type
        public async Task<IActionResult> FacebookRegister()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return BadRequest("Missing Authorization header");

            string authToken = Request.Headers["Authorization"];
            if (authToken.StartsWith("Bearer "))
                authToken = new string(authToken.Skip(7).ToArray());

            var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://graph.facebook.com/debug_token?input_token={authToken}&access_token={_authOptions.Facebook.AppID}|{_authOptions.Facebook.AppSecret}")
            });

            //TODO: possibly refactor into handler chain?
            if (!response.IsSuccessStatusCode)
                return Unauthorized("FB graph API rejected the token");

            var responseBodyStream = await response.Content.ReadAsStreamAsync();
            FacebookAuthPayload.Data payload = (await JsonSerializer.DeserializeAsync<FacebookAuthPayload>(responseBodyStream)).PayloadData;

            if (payload.IsValid == false)
                return Unauthorized("Auth token not valid");

            if (payload.AppId != _authOptions.Facebook.AppID)
                return Unauthorized("Bad AppID in auth token");

            //TODO: Verify if both methods use the same timezone and date specifications
            if (payload.ExpiresAt <= DateTimeOffset.Now.ToUnixTimeSeconds() ||
            payload.DataAccessExpiresAt <= DateTimeOffset.Now.ToUnixTimeSeconds())
                return Unauthorized("Token expired");


            //Finally check if the user exists in the database
            //Create new account
            var sub = payload.UserId;
            if (await _dbContext.Users.FirstOrDefaultAsync(x => x.FacebookId == sub) != null)
                return Ok(new
                {
                    access_token = CreateJWTString(),
                    refresh_token = CreateJWTString(new Claim[] { new Claim("scope", "Refresh") }),
                });

            //if user does not exists
            //create an account
            User u = new User()
            {
                GoogleId = null,
                FacebookId = sub,
            };

            await _dbContext.Users.AddAsync(u);
            await _dbContext.SaveChangesAsync();


            // Returns the 'access_token' and the type in lower case
            return Ok(new
            {
                access_token = CreateJWTString(),
                refresh_token = CreateJWTString(new Claim[] { new Claim("scope", "Refresh") }),
            });
        }

        [Route("refresh")]
        [HttpGet]
        [Authorize(Policy = "TokenHasRefreshClaim")]
        public async Task<IActionResult> GetRefereshToken()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return BadRequest("Missing Authorization header");

            string authToken = Request.Headers["Authorization"];
            if (authToken.StartsWith("Bearer "))
                authToken = new string(authToken.Skip(7).ToArray());
                
            return Ok(new
            {
                access_token = CreateJWTString(),
                refresh_token = CreateJWTString(new Claim[] { new Claim("scope", "Refresh") }),
            });
        }

        // Creates the signed JWT
        // With set of standard claims
        private string CreateJWTString()
        {
            return CreateJWTString(new Claim[] { new Claim("scope", "Access") });
        }

        private string CreateJWTString(Claim[] claims)
        {
            //TODO: evaluate this copied code 
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:JWT:Key"]));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Authentication:JWT:ExpiryMinutes"])),
                Issuer = _configuration["Authentication:JWT:Issuer"],
                Audience = _configuration["Authentication:JWT:Audience"],
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var access_token = tokenHandler.WriteToken(token);

            return access_token;
        }
    }
}
