﻿using System;
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

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApiContext _context;
        private readonly AuthenticationOptions _authOptions;

        public AuthController(ApiContext context,
                              IHttpClientFactory clientFactory,
                              IOptions<AuthenticationOptions> authOptions)
        {
            _context = context;
            _clientFactory = clientFactory;
            _authOptions = authOptions.Value;
        }

        [Route("register/google")]
        [HttpPost]
        //TODO: replace object with concrete type
        //Registers new account if provided with valid Google token 
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
                User user = await _context.Users.FirstOrDefaultAsync(x => x.GoogleId == sub);
                if (user != null)
                    return CreateJwtAuthPayload(user.Id);

                //if user does not exists
                //create an account
                user = new User()
                {
                    GoogleId = sub,
                    FacebookId = null,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
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
            string sub = payload.UserId;
            User user = await _context.Users.FirstOrDefaultAsync(x => x.FacebookId == sub);
            if (user != null)
                return CreateJwtAuthPayload(user.Id);

            //if user does not exists
            //create an account
            user = new User()
            {
                GoogleId = null,
                FacebookId = sub,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
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
