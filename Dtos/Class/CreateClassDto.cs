using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leapcert_back.Dtos.Class
{
    public class CreateClassDto
    {
        public int codigo_professor { get; set; }
        public string descricao { get; set; }
        public string? avaliacao { get; set; }
        public DateTime created_at { get; set; }
        public string nome { get; set; }
        public int? genero { get; set; }
    }
}