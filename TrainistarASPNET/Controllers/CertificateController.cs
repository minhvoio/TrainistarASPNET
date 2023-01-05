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
    public class CertificateController : ControllerBase
    {
        BaseResponse response = new BaseResponse();
        private readonly IConfiguration _configuration;
        public CertificateController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("create")]
        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public JsonResult CreateCertificate(CertificateDTO certificate)
        {
            //Tạo câu query
            string query = @"insert into certificate values ( 
            @idCertificate,
            @nameCertificate
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
                        cmd.Parameters.AddWithValue("@idCertificate", certificate.idCertificate);
                        cmd.Parameters.AddWithValue("@nameCertificate", certificate.nameCertificate);
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
                response.message = "Create this certificate failed";
            }
            response.code = "1";
            response.message = "Create succeeded";
            return new JsonResult(response);
        }

        [Route("all")]
        [HttpGet]
        [Authorize(Policy = Policies.Trainer)]
        public JsonResult getAllCertificate()
        {
            //Tạo câu query
            string query = @"select * from Certificate";
            //Hứng data query về table
            DataTable table = new DataTable();
            //Lấy chuỗi string connect vào db (setup ở appsettings.json)
            string data = _configuration.GetConnectionString("DBConnect");
            //Tạo con reader data mysql
            MySqlDataReader reader;
            //Gọi connect tới mysql
            using (MySqlConnection con = new MySqlConnection(data))
            {
                //Mở connection
                con.Open();
                //Execute script mysql
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    reader = cmd.ExecuteReader();
                    //Load data về table
                    table.Load(reader);
                    //Dóng connection
                    reader.Close();
                    con.Close();
                }
            }
            //Paste data từ table về dưới dạng json
            return new JsonResult(table);
        }
    }
}

