using FastFileSend.Database;
using FastFileSend.Main;
using FastFileSend.Main.Enum;
using FastFileSend.Main.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace FastFileSend.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        [Authorize]
        [Route("Get")]
        public IActionResult Get(long ticks)
        {
            int myId = Convert.ToInt32(User.Identity.Name);
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                var containsMyId = db.transactions.Include("files_idx").Where(x => x.sender_id == myId || x.receiver_id == myId).ToList();
                DateTime minimum = new DateTime(ticks);
                var upToDate = containsMyId.Where(x => x.date > DateTime.UtcNow.AddDays(-7)).Where(x => x.date > minimum).ToList();

                List<HistoryModel> historyList = new List<HistoryModel>();
                foreach (var item in upToDate)
                {
                    HistoryModel historyItem = new HistoryModel();
                    historyItem.Receiver = item.receiver_id;
                    historyItem.Sender = item.sender_id;
                    historyItem.Id = item.download_idx;
                    files file = item.files_idx;
                    FileItem fileItem = new FileItem(file.file_idx, file.file_name, file.file_size, file.file_crc32, file.file_creationdate, new Uri(file.file_url));
                    historyItem.File = fileItem;
                    historyItem.Status = (HistoryModelStatus)item.status;
                    historyItem.Date = item.date;

                    historyItem.File.Name = historyItem.File.Name.Trim();

                    historyList.Add(historyItem);
                }

                // online update
                db.users.Find(myId).user_lastonline = DateTime.UtcNow;
                db.SaveChangesAsync();

                return Ok(historyList);
            }
        }
    }
}
