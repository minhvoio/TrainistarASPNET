using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using TrainistarASPNET.Models;

namespace TrainistarASPNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        BaseResponse response = new BaseResponse();
        private readonly IConfiguration _configuration;
        public RatingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("create")]
        [HttpPost]
        [Authorize(Policy = Policies.Trainer)]
        public JsonResult CreateRating(RatingDTO rating)
        {
            //Tạo câu query
            string query = @"insert into rating values ( 
            @idRating,
            @rating,
            @idCourse,
            @idStudent
            )";
            //Hứng data query về table
            DataTable table = new DataTable();
            //Lấy chuỗi string connect vào db (setup ở appsettings.json)
            string data = _configuration.GetConnectionString("DBConnect");
            //Tạo con reader data mysql
            MySqlDataReader reader;
            //Gọi connect tới mysql
            try
            {
                using (MySqlConnection con = new MySqlConnection(data))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@idRating", rating.idRating);
                        cmd.Parameters.AddWithValue("@rating", rating.rating);
                        cmd.Parameters.AddWithValue("@idCourse", rating.idCourse);
                        cmd.Parameters.AddWithValue("@idStudent", rating.idStudent);
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
                response.message = "Create this rating failed";
            }
            response.code = "1";
            response.message = "Create succeeded";
            return new JsonResult(response);
        }
        
        [Route("all/{idCourse}")]
        [HttpGet]
        public JsonResult getAllRatingOfCourse(string idCourse)
        {
            string query = @"select R.idRating, R.rating, R.idCourse, R.idStudent, U.firstName, U.lastName, U.gender 
            from Rating R 
            join User_ U on R.idStudent=U.idUser 
            where R.idCourse = @idCourse";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            using (MySqlConnection con = new MySqlConnection(data))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@idCourse", idCourse);
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    con.Close();
                }
            }
            return new JsonResult(table);
        }
    }

    
}