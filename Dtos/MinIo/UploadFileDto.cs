using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Dtos.MinIo
{
    public class UploadFileDto
    {
        [FromForm]
        public IFormFile File { get; set; }
        public string path { get; set; }
    }
}