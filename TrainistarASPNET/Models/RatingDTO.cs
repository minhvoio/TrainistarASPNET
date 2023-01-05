using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrainistarASPNET.Models
{
    public class RatingDTO
    {
        public string idRating { get; set; }
        public string rating { get; set; }
        public string idCourse { get; set; }
        public string idStudent { get; set; }
    }
}
