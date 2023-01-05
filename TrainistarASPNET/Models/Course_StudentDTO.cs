namespace TrainistarASPNET.Models
{
    public class Course_StudentDTO
    {
        public string idCourse { get; set; }
        public string idStudent { get; set; }
        public float mark { get; set; }

        public Course_StudentDTO(string idCourse_, string idStudent_, int mark_)
        {
            this.idCourse = idCourse_;
            this.idStudent = idStudent_;
            this.mark = mark_;
        }
    }
}
