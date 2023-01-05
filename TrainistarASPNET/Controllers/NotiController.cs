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
    public class NotiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public NotiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public String getNextId()
        {
            string query = @"select max(convert(id,signed)) from noti";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            string notiId = "";
            using (MySqlConnection con = new MySqlConnection(data))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    notiId = table.Rows[0][0].ToString();
                    reader.Close();
                    con.Close();
                }
            }
            if (notiId == "") { return "1"; }
            else
            {
                int temp = Int32.Parse(notiId) + 1;
                notiId = temp.ToString();
                return notiId;
            }
        }
        [Route("create")]
        [HttpPost]
        [Authorize(Policy = Policies.Trainer)]
        public JsonResult createNoti(NotiDTO noti)
        {
            string query = @"insert into noti values (@id,@data)";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            noti.id = getNextId();
            BaseResponse response = new BaseResponse();
            using (MySqlConnection con = new MySqlConnection(data))
            {
                //Mở connection
                con.Open();
                //Execute script mysql
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", noti.id);
                    cmd.Parameters.AddWithValue("@data", noti.data);
                    reader = cmd.ExecuteReader();
                    //Load data về table
                    table.Load(reader);
                    //Dóng connection
                    reader.Close();
                    con.Close();
                    response.code = "1";
                    response.message = "Đã add noti";
                }
            }
            //Paste data từ table về dưới dạng json
            return new JsonResult(response);
        }
        [Route("all")]
        [HttpGet]
        [Authorize(Policy = Policies.Student)]
        public JsonResult getAllNoti()
        {
            string query = @"select * from noti order by id desc";
            DataTable table = new DataTable();
            string data = _configuration.GetConnectionString("DBConnect");
            MySqlDataReader reader;
            BaseResponse response = new BaseResponse();
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
