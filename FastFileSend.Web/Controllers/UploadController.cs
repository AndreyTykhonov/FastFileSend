using FastFileSend.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace FastFileSend.Web.Controllers
{
    public class UploadController : ApiController
    {
        private fastfilesendEntities db = new fastfilesendEntities();

        public JsonResult<int> Get(string name, long size, int crc32, string url)
        {
            int newId = FindEmptpyId();

            files newFile = new files()
            {
                file_idx = newId,
                file_name = name,
                file_size = size,
                file_crc32 = crc32,
                file_creationdate = DateTime.Now,
                file_url = url
            };

            db.files.Add(newFile);
            db.SaveChanges();

            return Json(newId);
        }

        private int FindEmptpyId()
        {
            do
            {
                int newId = new Random().Next(999999);
                if (!db.files.Any(x => x.file_idx == newId))
                {
                    return newId;
                }
            }
            while (true);
        }
    }
}
