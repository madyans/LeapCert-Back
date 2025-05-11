using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leapcert_back.Models
{
    public class ClassPath
    {
        [Key]
        public int codigo { get; set; }
        public int codigo_curso { get; set; }
        public string path { get; set; }
    }
}