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
using Api.Payloads;
using System.Runtime.CompilerServices;
using Api.Services;
using System.IO;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IUserAuthService _userAuthService;
        private readonly IUserService _userService;
        private readonly AuthenticationOptions _authOptions;

        public AuthController(IUserAuthService userAuthService,
                              IUserService userService,
                              IHttpClientFactory clientFactory,
                              IOptions<AuthenticationOptions> authOptions)
        {
            _userAuthService = userAuthService;
            _userService = userService;
            _clientFactory = clientFactory;
            _authOptions = authOptions.Value;
        }

        [Route("register/google")]
        [HttpPost]
        /// <summary>
        ///Registers new account if provided with valid Google token
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult<JWTAuthPayload>> GoogleRegister()
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
                        _authOptions.Google.ClientID,
                        _authOptions.Google.IosID
                    },
                });

                //Create new account
                //Check if the user already exists in DB
                var sub = googleJWTPayload.Subject;
                User user = await _userAuthService.GetUserByGoogleId(sub);
                if (user != null)
                    return CreateJwtAuthPayload(user.Id);

                //if user does not exists
                //create an account
                user = new User()
                {
                    GoogleId = sub,
                    FacebookId = null,
                    Email = googleJWTPayload.Email,
                    GivenName = googleJWTPayload.GivenName,
                    LastName = googleJWTPayload.FamilyName,
                };

                await _userService.CreateUser(user);
                // Returns the 'access_token' and the type in lower case
                return CreateJwtAuthPayload(user.Id);
            }
            catch (InvalidJwtException e)
            {
                return Unauthorized("Invalid token: " + e.Message);
            }
        }


        [Route("register/facebook")]
        [HttpPost]
        //TODO: replace object with concrete type
        public async Task<ActionResult<JWTAuthPayload>> FacebookRegister()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return BadRequest("Missing Authorization header");

            string authToken = Request.Headers["Authorization"];
            if (authToken.StartsWith("Bearer ")) //strip bearer
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


            using var authResponseStream = await response.Content.ReadAsStreamAsync();
            FacebookAuthPayload.Data authPayload = (await JsonSerializer.DeserializeAsync<FacebookAuthPayload>(authResponseStream)).PayloadData;



            if (authPayload.IsValid == false)
                return Unauthorized("Auth token not valid");

            if (authPayload.AppId != _authOptions.Facebook.AppID)
                return Unauthorized("Bad AppID in auth token");

            //TODO: Verify if both methods use the same timezone and date specifications
            if (authPayload.ExpiresAt <= DateTimeOffset.Now.ToUnixTimeSeconds() ||
            authPayload.DataAccessExpiresAt <= DateTimeOffset.Now.ToUnixTimeSeconds())
                return Unauthorized("Token expired");


            //Finally check if the user exists in the database
            //Create new account
            string sub = authPayload.UserId;
            User user = await _userAuthService.GetUserByFacebookId(sub);
            if (user != null)
                return CreateJwtAuthPayload(user.Id);


            response = await httpClient.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://graph.facebook.com/{sub}?fields=email,first_name,last_name&access_token={_authOptions.Facebook.AppID}|{_authOptions.Facebook.AppSecret}")
            });

            using var userInfoResponseStream = await response.Content.ReadAsStreamAsync();
            FacebookUserDataPayload userPayload = await JsonSerializer.DeserializeAsync<FacebookUserDataPayload>(userInfoResponseStream);

            //if user does not exists
            //create an account
            user = new User()
            {
                GoogleId = null,
                FacebookId = sub,
                GivenName = userPayload.FirstName,
                LastName = userPayload.LastName,
                Email = userPayload.Email
            };

            await _userService.CreateUser(user);
            // Returns the 'access_token' and the type in lower case
            return CreateJwtAuthPayload(user.Id);
        }

        [Route("refresh")]
        [HttpGet]
        [Authorize(Policy = "TokenHasRefreshClaim")]
        public async Task<ActionResult<JWTAuthPayload>> GetRefereshToken()
        {
            //TODO: Handle bad id format 
            var id = int.Parse(User.Claims.First(x => x.Type == "sub").Value);
            return CreateJwtAuthPayload(id);
        }

        private JWTAuthPayload CreateJwtAuthPayload(int userId)
        {
            return new JWTAuthPayload()
            {
                access_token = CreateAccessToken(userId),
                refresh_token = CreateRefreshToken(userId),
                user_id = userId
            };
        }

        private string CreateAccessToken(int userId)
        {
            return CreateJWTString(new Claim[] { new Claim("scope", "Access"), new Claim("sub", userId.ToString()) });
        }

        private string CreateRefreshToken(int userId)
        {
            return CreateJWTString(new Claim[] { new Claim("scope", "Refresh"), new Claim("sub", userId.ToString()) });
        }

        private string CreateJWTString(Claim[] claims)
        {
            //TODO: evaluate this copied code 
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.JWT.Key));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_authOptions.JWT.ExpiryMinutes),
                Issuer = _authOptions.JWT.Issuer,
                Audience = _authOptions.JWT.Audience, //TODO SECURITY: verify audience? 
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var access_token = tokenHandler.WriteToken(token);

            return access_token;
        }
    }
}
