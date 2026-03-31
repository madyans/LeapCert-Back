using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Dtos.MinIo
{
    public class UploadFileDto
    {
        [FromForm(Name = "File")]
        public IFormFile File { get; set; }
        [FromForm(Name = "path")]
        public string path { get; set; }
    }
}