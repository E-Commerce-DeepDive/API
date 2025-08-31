using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.DTO.ImageUploading
{
    public class UploadImageResponse
    {
        public string Url { get; set; } = null!;
        public string PublicId { get; set; } = null!;
    }
}
