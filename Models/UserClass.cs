using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leapcert_back.Models
{
    public class UserClass
    {
        [Key]
        public int codigo { get; set; }
        public int codigo_usuario { get; set; }
        public int codigo_curso { get; set; }
        public DateTime data_matricula { get; set; }

        public Class ClassJoin { get; set; }
        public User UserJoin { get; set; }
    }
}