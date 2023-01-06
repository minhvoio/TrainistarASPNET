using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using TrainistarASPNET.Models;

namespace TrainistarASPNET.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        BaseResponse baseResponse = new BaseResponse();
        //Dictionary<String, Auth> tokenUserMap = new Dictionary<string, Auth>();

        private readonly IConfiguration _config;

        private List<Auth> appUsers = new List<Auth>
        {
            new Auth {  FullName = "Minh Vo",  UserName = "admin", Password = "admin", UserRole = "Admin" },
            new Auth {  FullName = "Gus Fring",  UserName = "manager", Password = "manager", UserRole = "Manager" },
            new Auth {  FullName = "Heisenberg",  UserName = "trainer", Password = "trainer", UserRole = "Trainer" },
            new Auth {  FullName = "Wednesday",  UserName = "student", Password = "student", UserRole = "Student" }
        };

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        [AllowAnonymous]
        public BaseResponse Login([FromBody] Auth login)
        {
            IActionResult response = Unauthorized();
            Auth user = AuthenticateUser(login);

            if (user != null)
            {
                var tokenString = GenerateJWTToken(user);
                response = Ok(new
                {
                    token = tokenString,
                    userDetails = user,
                });

                baseResponse.code = "1";
                baseResponse.message = "Login succeeded";
                baseResponse.tokenResult = response;
                return baseResponse;
            }

            baseResponse.code = "-1";
            baseResponse.message = "User login failed, please check your account";
            return baseResponse;
        }

        Auth AuthenticateUser(Auth loginCredentials)
        {
            Auth user = appUsers.SingleOrDefault(x => x.UserName == loginCredentials.UserName && x.Password == loginCredentials.Password);
            return user;
        }

        string GenerateJWTToken(Auth userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserName),
                new Claim("fullName", userInfo.FullName.ToString()),
                new Claim("role",userInfo.UserRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(43200),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Route("me")]
        [AllowAnonymous]
        [HttpGet]
        public BaseResponse validateUserByToken ()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            String jwtBearer = HttpContext.Request.Headers["Authorization"];
            String token = jwtBearer.Substring(7);
            
            try { 
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidAudience = _config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"])),
                        ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                if (jwtToken != null)
                {
                    baseResponse.code = "1";
                    baseResponse.message = "Valid token";
                    baseResponse.tokenResult = new JsonResult(jwtToken.Payload);
                }

            } catch (Exception ex)
            {
                baseResponse.code = "-1";
                baseResponse.message = "Invalid token";
            }
            return baseResponse;
        }
    }
}
