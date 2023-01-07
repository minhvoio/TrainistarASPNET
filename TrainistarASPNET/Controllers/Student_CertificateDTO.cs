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
    public class Student_CertificateController : ControllerBase
    {
        BaseResponse response = new BaseResponse();
        private readonly IConfiguration _configuration;
        public Student_CertificateController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("create")]
        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public JsonResult CreateStudent_Certificate(Student_CertificateDTO student_certificate)
        {
            //Tạo câu query
            string query = @"insert into student_certificate values ( 
            @idCertificate,
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
                        cmd.Parameters.AddWithValue("@idCertificate", student_certificate.idCertificate);
                        cmd.Parameters.AddWithValue("@idStudent", student_certificate.idStudent);
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
                response.message = "Create this student_certificate failed";
            }
            response.code = "1";
            response.message = "Create succeeded";
            return new JsonResult(response);
        }
        [Route("get/{idStudent}")]
        [HttpGet]
        public JsonResult getCertWithStudentId(String id)
        {
            string query = @"select * from student_certificate left join certificate on student_certificate.idCertificate=certificate.idCertificate where idStudent=@id";
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
                        cmd.Parameters.AddWithValue("@id", id);
                        reader = cmd.ExecuteReader();
                        table.Load(reader);
                        reader.Close();
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                response.code = "-1";
                response.message = "Get failed";
                return new JsonResult(response);
            }
            return new JsonResult(table);
        }
    }
}

