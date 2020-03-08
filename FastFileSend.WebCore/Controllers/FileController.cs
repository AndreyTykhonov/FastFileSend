using FastFileSend.Main.Enum;
using FastFileSend.Main.Models;
using FastFileSend.WebCore.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        public IActionResult SetStatus(int download, int status)
        {
            using (MyDbContext db = new MyDbContext())
            {
                db.History.First(x => x.Id == download).Status = (HistoryModelStatus)status;
                db.SaveChanges();
            }

            return Ok();
        }

        [Authorize]
        [Route("GetStatus")]
        public IActionResult GetStatus(int download)
        {
            using (MyDbContext db = new MyDbContext())
            {
                return Ok(db.History.First(x => x.Id == download).Status);
            }
        }

        [Authorize]
        [Route("Send")]
        public IActionResult Send(int target, int file)
        {
            int myId = Convert.ToInt32(User.Identity.Name);

            using (MyDbContext db = new MyDbContext())
            {

                HistoryModel newSend = new HistoryModel()
                {
                    Sender = myId,
                    Receiver = target,
                    Status = 0,
                    Id = FindEmptpyTransactionId(),
                    Date = DateTime.UtcNow
                };

                newSend.File = db.Files.Find(file);

                db.History.Add(newSend);
                db.SaveChanges();

                return Ok(newSend.Id);
            }
        }

        private int FindEmptpyTransactionId()
        {
            using (MyDbContext db = new MyDbContext())
            {
                do
                {
                    int newId = new Random().Next(999999);
                    if (db.History.Find(newId) is null)
                    {
                        return newId;
                    }
                }
                while (true);
            }
        }

        [Authorize]
        [Route("Upload")]
        public IActionResult Upload(string file)
        {
            using (MyDbContext db = new MyDbContext())
            {
                int newId = FindEmptpyFileId();

                FileItem newFile = JsonConvert.DeserializeObject<FileItem>(file);
                newFile.CreationDate = DateTime.Now;
                newFile.Id = newId;

                db.Files.Add(newFile);
                db.SaveChanges();

                return Ok(newId);
            }
        }

        private int FindEmptpyFileId()
        {
            using (MyDbContext db = new MyDbContext())
            {
                do
                {
                    int newId = new Random().Next(999999);
                    if (db.Files.Find(newId) is null)
                    {
                        return newId;
                    }
                }
                while (true);
            }
        }
    }
}
