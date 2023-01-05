using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrainistarASPNET.Models
{
    public class SkillDTO
    {
        public string idSkill { get; set; }
        public string nameSkill { get; set; }
        public string level { get; set; }
        public string idManager { get; set; }
    }
}
