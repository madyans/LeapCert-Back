using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Minio.DataModel;

namespace leapcert_back.Dtos.MinIo
{
    public class BucketItemDto
    {
        public string ObjectName { get; set; }
        public string eTag { get; set; }
        public string contentType { get; set; }
    }
}