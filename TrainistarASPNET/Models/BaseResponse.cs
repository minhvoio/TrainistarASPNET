using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TrainistarASPNET.Models
{
    public class BaseResponse 
    {
        public string code { get; set; }

        public string message { get; set; }

        public IActionResult tokenResult { get; set; }

    }
}
