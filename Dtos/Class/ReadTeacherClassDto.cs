using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leapcert_back.Dtos.Class
{
    public class ReadTeacherClassDto
    {
        public int codigo_professor { get; set; }
        public string nome_professor { get; set; }
        public int codigo_curso { get; set; }
        public string nome_curso { get; set; }
    }
}