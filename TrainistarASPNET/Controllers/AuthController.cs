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


        private List<Auth> appUsers = new List<Auth>
        {
            new Auth {  UserID = "1", FullName = "Minh Vo",  UserName = "admin", Password = "XzJm*ui4", UserRole = "Admin" },
            new Auth {  UserID = "1", FullName = "Doris Hrycek",  UserName = "manager1", Password = "HFlhSRhX", UserRole = "Manager" },
            new Auth {  UserID = "2", FullName = "Boyd Windas",  UserName = "manager2", Password = "0mzifbxRdrZf", UserRole = "Manager" },
            new Auth {  UserID = "3", FullName = "Dayle Lenihan",  UserName = "manager3", Password = "Sw5I3AkVDd", UserRole = "Manager" },
            new Auth {  UserID = "4", FullName = "Steffi Renn",  UserName = "manager4", Password = "k5rFAPXz", UserRole = "Manager" },
            new Auth {  UserID = "5", FullName = "Robb Blois",  UserName = "manager5", Password = "9tkVpEw", UserRole = "Manager" },
            new Auth {  UserID = "6", FullName = "Aurelea Battey",  UserName = "manager6", Password = "WXCgbRS6u51j", UserRole = "Manager" },
            new Auth {  UserID = "7", FullName = "Thomasin Coppard",  UserName = "manager7", Password = "9x0nFXMryao", UserRole = "Manager" },
            new Auth {  UserID = "8", FullName = "Benita Adenet",  UserName = "manager8", Password = "YjXXzkfNK", UserRole = "Manager" },
            new Auth {  UserID = "9", FullName = "Robinet Reeves",  UserName = "manager9", Password = "JW5yGMzW1Fen", UserRole = "Manager" },
            new Auth {  UserID = "10", FullName = "Webster Hayden",  UserName = "manager10", Password = "EffXaQ", UserRole = "Manager" },
            new Auth {  UserID = "11", FullName = "Bogart Barrick",  UserName = "manager11", Password = "SPIsZo6zRA", UserRole = "Manager" },
            new Auth {  UserID = "12", FullName = "Jillene Morrison",  UserName = "manager12", Password = "2MBKSs1", UserRole = "Manager" },
            new Auth {  UserID = "13", FullName = "Layton Garbutt",  UserName = "manager13", Password = "kEDOUMEBvR", UserRole = "Manager" },
            new Auth {  UserID = "14", FullName = "Scarface MacTeague",  UserName = "manager14", Password = "Gh8lOsvxsI5", UserRole = "Manager" },
            new Auth {  UserID = "15", FullName = "Nathanil Drinkall",  UserName = "manager15", Password = "DIulZ81gPYe", UserRole = "Manager" },
            new Auth {  UserID = "16", FullName = "Bone Idell",  UserName = "manager16", Password = "a9Q1J27c", UserRole = "Manager" },
            new Auth {  UserID = "17", FullName = "Timothee Bottinelli",  UserName = "manager17", Password = "tKAq4cWW", UserRole = "Manager" },
            new Auth {  UserID = "18", FullName = "Jeramie Reside",  UserName = "manager18", Password = "ZntN0nJGmZN", UserRole = "Manager" },
            new Auth {  UserID = "19", FullName = "Luelle Burry",  UserName = "manager19", Password = "gWRjZtdJ9", UserRole = "Manager" },
            new Auth {  UserID = "20", FullName = "Darn Kneebone",  UserName = "manager20", Password = "SgZJPFPDk", UserRole = "Manager" },
        };

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

        [Route("admin-manager")]
        [HttpPost]
        [AllowAnonymous]
        public BaseResponse SigninAdmin([FromBody] Auth login)
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
