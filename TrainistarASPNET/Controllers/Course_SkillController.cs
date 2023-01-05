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
    public class Course_SkillController : ControllerBase
    {
        BaseResponse response = new BaseResponse();
        private readonly IConfiguration _configuration;
        public Course_SkillController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("create")]
        [HttpPost]
        [Authorize(Policy = Policies.Manager)]
        public JsonResult CreateCourse_Skill(Course_SkillDTO course_skill)
        {
            //Tạo câu query
            string query = @"insert into course_skill values ( 
            @idCourse,
            @idSkill
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
                        cmd.Parameters.AddWithValue("@idCourse", course_skill.idCourse);
                        cmd.Parameters.AddWithValue("@idSkill", course_skill.idSkill);
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
                response.message = "Create this Course_Skill failed";
            }
            response.code = "1";
            response.message = "Create succeeded";
            return new JsonResult(response);
        }
    }
}

