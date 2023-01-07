using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
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

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        private String toSHA256(String text)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(text));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [Route("admin")]
        [HttpPost]
        [AllowAnonymous]
        public JsonResult SigninAdmin([FromBody] Auth signin)
        {
            IActionResult response = Unauthorized();
            string query = @"select * from Admin 
            where userName=@username and password=@password
            ";

            DataTable table = new DataTable();
            string data = _config.GetConnectionString("DBConnect");
            MySqlDataReader reader;

            try
            {
                using (MySqlConnection con = new MySqlConnection(data))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", signin.UserName);
                        cmd.Parameters.AddWithValue("@password", signin.Password);
                        reader = cmd.ExecuteReader();
                        table.Load(reader);

                        signin.UserRole = "Manager";

                        signin.UserID = table.Rows[0][0].ToString();
                        signin.FullName = table.Rows[0][3].ToString();

                        reader.Close();
                        con.Close();
                    }
                }

                if (table.Rows[0][0].ToString() != null)
                {
                    var tokenString = GenerateJWTToken(signin);
                    response = Ok(new
                    {
                        token = tokenString,
                        userDetails = signin,
                    });

                    baseResponse.code = "1";
                    baseResponse.message = "Login succeeded";
                    baseResponse.tokenResult = response;
                    return new JsonResult(baseResponse);

                }
            }
            catch (Exception ex)
            {
                baseResponse.code = "-1";
                baseResponse.message = "User login failed, please check your account";
            }
            return new JsonResult(baseResponse);
        }

        [Route("manager")]
        [HttpPost]
        [AllowAnonymous]
        public JsonResult SigninManager([FromBody] Auth signin)
        {
            IActionResult response = Unauthorized();
            string query = @"select * from Manager 
            where userName=@username and password=@password
            ";

            DataTable table = new DataTable();
            string data = _config.GetConnectionString("DBConnect");
            MySqlDataReader reader;

            try
            {
                using (MySqlConnection con = new MySqlConnection(data))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", signin.UserName);
                        cmd.Parameters.AddWithValue("@password", signin.Password);
                        reader = cmd.ExecuteReader();
                        table.Load(reader);

                        signin.UserRole = "Manager";

                        signin.UserID = table.Rows[0][0].ToString();
                        signin.FullName = table.Rows[0][3].ToString();

                        reader.Close();
                        con.Close();
                    }
                }

                if (table.Rows[0][0].ToString() != null)
                {
                    var tokenString = GenerateJWTToken(signin);
                    response = Ok(new
                    {
                        token = tokenString,
                        userDetails = signin,
                    });

                    baseResponse.code = "1";
                    baseResponse.message = "Login succeeded";
                    baseResponse.tokenResult = response;
                    return new JsonResult(baseResponse);

                }
            }
            catch (Exception ex)
            {
                baseResponse.code = "-1";
                baseResponse.message = "User login failed, please check your account";
            }
            return new JsonResult(baseResponse);
        }

        [Route("trainer-student")]
        [HttpPost]
        [AllowAnonymous]
        public JsonResult SigninTrainerStudent([FromBody] Auth signin)
        {
            IActionResult response = Unauthorized();
            string query = @"select * from User_ 
            where userName=@username and password=@password
            ";

            DataTable table = new DataTable();
            string data = _config.GetConnectionString("DBConnect");
            MySqlDataReader reader;

            try
            {
                using (MySqlConnection con = new MySqlConnection(data))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", signin.UserName);
                        cmd.Parameters.AddWithValue("@password", toSHA256(signin.Password));
                        reader = cmd.ExecuteReader();
                        table.Load(reader);

                        if (table.Rows[0][8].ToString() == "1")
                        {
                            signin.UserRole = "Student";
                        }
                        else signin.UserRole = "Trainer";

                        signin.UserID = table.Rows[0][0].ToString();
                        signin.FullName = table.Rows[0][3].ToString() + " " + table.Rows[0][4].ToString();

                        reader.Close();
                        con.Close();
                    }
                }

                if (table.Rows[0][0].ToString() != null)
                {
                    var tokenString = GenerateJWTToken(signin);
                    response = Ok(new
                    {
                        token = tokenString,
                        userDetails = signin,
                    });

                    baseResponse.code = "1";
                    baseResponse.message = "Login succeeded";
                    baseResponse.tokenResult = response;
                    return new JsonResult(baseResponse);

                }
            }
            catch (Exception ex)
            {
                baseResponse.code = "-1";
                baseResponse.message = "User login failed, please check your account";
            }
            return new JsonResult(baseResponse);
        }
                
        string GenerateJWTToken(Auth userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", userInfo.UserID),
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
        public BaseResponse validateUserByToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            String jwtBearer = HttpContext.Request.Headers["Authorization"];
            String token = jwtBearer.Substring(7);

            try
            {
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

            }
            catch (Exception ex)
            {
                baseResponse.code = "-1";
                baseResponse.message = "Invalid token";
            }
            return baseResponse;
        }
    }
}
