using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AIInterviewPlatform.Application.DTOs.Resume
{
    public class ResumeUploadRequest
    {
        public IFormFile File { get; set; }
    }
}
