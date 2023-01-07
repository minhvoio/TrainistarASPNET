using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using TrainistarASPNET.Models;

namespace TrainistarASPNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        BaseResponse response = new BaseResponse();

        private readonly IConfiguration _configuration;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
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

        public String getNextUserId()
        {
            string query = @"select max(convert(idUser,signed)) from user_";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            string userId = "";
            using (MySqlConnection con = new MySqlConnection(data))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    userId = table.Rows[0][0].ToString();
                    reader.Close();
                    con.Close();
                }
            }
            int temp = Int32.Parse(userId) + 1;
            userId = temp.ToString();
            return userId;
        }

        [Route("{username}")]
        [HttpDelete]
        [Authorize(Policy = Policies.Admin)]
        public JsonResult DeleteUser(string username)
        {
            string query = @"delete from User_ where userName=@userName";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            response.code = "1";
            response.message = "Delete succeeded";
            try
            {
                using (MySqlConnection con = new MySqlConnection(data))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userName", username);
                        reader = cmd.ExecuteReader();
                        table.Load(reader);
                        reader.Close();
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                response.code = "-1";
                response.message = "Delete user failed";
            }

            return new JsonResult(response);
        }

        [Route("{username}")]
        [HttpGet]
        [Authorize(Policy = Policies.Manager)]
        public JsonResult getUser(string username)
        {
            string query = @"select * from User_ where userName=@userName";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            using (MySqlConnection con = new MySqlConnection(data))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@userName", username);
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    con.Close();
                }
            }
            return new JsonResult(table);
        }

        [Route("all")]
        [HttpGet]
        [Authorize(Policy = Policies.Admin)]
        public JsonResult getAllUser()
        {
            string query = @"select * from User_";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            using (MySqlConnection con = new MySqlConnection(data))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    con.Close();
                }
            }
            return new JsonResult(table);
        }
        [Route("auth")]
        [HttpPost]
        public JsonResult Login(UserAuthentication user)
        {
            string query = @"select * from User_ 
            where userName=@username and password=@password
            ";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            RoleAuthentication response = new RoleAuthentication();
            try
            {
                using (MySqlConnection con = new MySqlConnection(data))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", user.username);
                        cmd.Parameters.AddWithValue("@password", toSHA256(user.password));
                        reader = cmd.ExecuteReader();
                        table.Load(reader);
                        response.role = table.Rows[0][8].ToString();
                        response.name = table.Rows[0][3].ToString() + " " + table.Rows[0][4].ToString();
                        reader.Close();
                        con.Close();
                    }
                }

                if (table.Rows[0][0].ToString() != null)
                {
                    response.code = "1";
                    response.message = "Login succeeded";

                }
            }
            catch (Exception ex)
            {
                response.code = "-1";
                response.message = "User login failed, please check your account";
            }
            return new JsonResult(response);
        }
        [Route("create")]
        [HttpPost]
        public JsonResult SignUp(UserDTO user)
        {
            string query = @"insert into user_ values (@iduser,@username,@password,@firstname,@lastname,@email,@phonenumber,@gender,@typeuser)";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            user.idUser = getNextUserId();
            try
            {
                using (MySqlConnection con = new MySqlConnection(data))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@iduser", user.idUser);
                        cmd.Parameters.AddWithValue("@username", user.username);
                        cmd.Parameters.AddWithValue("@password", toSHA256(user.password));
                        cmd.Parameters.AddWithValue("@firstname", user.firstName);
                        cmd.Parameters.AddWithValue("@lastname", user.lastName);
                        cmd.Parameters.AddWithValue("@email", user.email);
                        cmd.Parameters.AddWithValue("@phonenumber", user.phoneNumber);
                        cmd.Parameters.AddWithValue("@gender", user.gender);
                        cmd.Parameters.AddWithValue("@typeUser", user.typeUser);
                        reader = cmd.ExecuteReader();
                        table.Load(reader);
                        reader.Close();
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                response.code = "-1";
                response.message = "Create user failed";
            }
            response.code = "1";
            response.message = "Create succeeded";
            return new JsonResult(response);
        }

        [Route("{username}")]
        [HttpPatch]
        [Authorize(Policy = Policies.Admin)]
        public JsonResult UpdateUser(string username, [FromBody] UserDTO user)
        {
            string query = @"update user_ set
            password=@password,
            firstName=@firstname,
            lastName=@lastname,
            email=@email,
            phoneNumber=@phonenumber,
            gender=@gender,
            typeUser=@typeuser
            where userName=@username";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            response.code = "1";
            response.message = "Update succeeded";

            using (MySqlConnection con = new MySqlConnection(data))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@password", toSHA256(user.password));
                    cmd.Parameters.AddWithValue("@firstname", user.firstName);
                    cmd.Parameters.AddWithValue("@lastname", user.lastName);
                    cmd.Parameters.AddWithValue("@email", user.email);
                    cmd.Parameters.AddWithValue("@phonenumber", user.phoneNumber);
                    cmd.Parameters.AddWithValue("@gender", user.gender);
                    cmd.Parameters.AddWithValue("@typeuser", user.typeUser);
                    cmd.Parameters.AddWithValue("@username", username);
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    con.Close();
                }
            }
            return new JsonResult(response);
        }

    }
}
