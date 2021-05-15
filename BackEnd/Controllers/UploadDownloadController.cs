using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BackEnd.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;

namespace BackEnd.Controllers
{
    [Route("api")]
    [ApiController]
    public class UploadDownloadController: ControllerBase
    {
        private IHostingEnvironment _hostingEnvironment;
        private IConfiguration _config;
        public UploadDownloadController(IHostingEnvironment environment, IConfiguration config) {
            _hostingEnvironment = environment;
            this._config = config;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var permittedContentType = _config["FileLimits:ContentType"];
            var permittedSize = Int64.Parse(_config["FileLimits:FileSize"]);
            var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            if(!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            if (file.Length > 0) {
                //  ContentType control
                if (file.ContentType != permittedContentType)
                {
                   return new UnsupportedMediaTypeResult();
                }
                // size control
                if (file.Length / 1000 > permittedSize)
                {
                    return BadRequest("File size exceeded.");
                }
            
                var filePath = Path.Combine(uploads, file.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                    await file.CopyToAsync(fileStream);
                }

            }
            return Ok();
        }

        [HttpGet]
        [Route("download")]
        public async Task<IActionResult> Download([FromQuery] string file) {
            var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            var filePath = Path.Combine(uploads, file);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(filePath), file); 
        }

        [HttpGet]
        [Route("files")]
        public IActionResult Files() {
            //var result =  new List<string>();
            var result =  new List<FileModel>();

            var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            if(Directory.Exists(uploads))
            {   
                var provider = _hostingEnvironment.ContentRootFileProvider;
                foreach (string fileName in Directory.GetFiles(uploads))
                {
                    var model = new FileModel();
                    var fileInfo = provider.GetFileInfo(fileName);
                    long length = new System.IO.FileInfo(fileInfo.Name).Length / 1000;
                    model.Name =  new System.IO.FileInfo(fileInfo.Name).Name;
                    model.Size = length.ToString();
                    model.Date = new System.IO.FileInfo(fileInfo.Name).LastWriteTime.ToString();
                    result.Add(model);
                }
            }
            return Ok(result);
        }  


        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if(!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}