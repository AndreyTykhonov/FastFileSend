using FastFileSend.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace FastFileSend.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        [Authorize]
        [Route("SetStatus")]
        public void SetStatus(int download_idx, int status)
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                db.transactions.First(x => x.download_idx == download_idx).status = status;
                db.SaveChanges();
            }
        }

        [Authorize]
        [Route("Send")]
        public IActionResult Send(int target, int file)
        {
            int myId = Convert.ToInt32(User.Identity.Name);

            using (fastfilesendEntities db = new fastfilesendEntities())
            {

                transactions newSend = new transactions()
                {
                    file_id = file,
                    sender_id = myId,
                    receiver_id = target,
                    status = 0,
                    download_idx = FindEmptpyTransactionId(),
                    date = DateTime.UtcNow
                };

                db.transactions.Add(newSend);
                db.SaveChanges();

                return Ok(newSend.download_idx);
            }
        }

        private int FindEmptpyTransactionId()
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                do
                {
                    int newId = new Random().Next(999999);
                    if (!db.transactions.Any(x => x.download_idx == newId))
                    {
                        return newId;
                    }
                }
                while (true);
            }
        }

        [Authorize]
        [Route("Upload")]
        public IActionResult Upload(string name, long size, int crc32, string url)
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {

                int newId = FindEmptpyFileId();

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

                return Ok(newId);
            }
        }

        private int FindEmptpyFileId()
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
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
}
