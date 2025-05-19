using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leapcert_back.Dtos.MinIo
{
    public class ListObjectsAsDto

    {
        public string? prefix { get; set; }
        public bool recursive { get; set; }
        public bool versions { get; set; }
    }
}