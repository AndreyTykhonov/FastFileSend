﻿using FastFileSend.Database;
using FastFileSend.Main;
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
    public class HistoryController : ControllerBase
    {
        [Authorize]
        [Route("Get")]
        public IActionResult Get()
        {
            int myId = Convert.ToInt32(User.Identity.Name);
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                var containsMyId = db.transactions.Where(x => x.sender_id == myId || x.receiver_id == myId).ToList();

                List<HistoryItem> historyList = new List<HistoryItem>();

                foreach (var item in containsMyId.Where(x => x.date > DateTime.UtcNow.AddDays(-7)))
                {
                    HistoryItem historyItem = new HistoryItem();
                    historyItem.Receiver = item.receiver_id;
                    historyItem.Sender = item.sender_id;
                    historyItem.Id = item.download_idx;
                    FileItem fileItem = FilesToFileItem(item.file_id);
                    historyItem.File = fileItem;
                    historyItem.Status = item.status;
                    historyItem.Date = item.date;

                    historyList.Add(historyItem);
                }

                return Ok(historyList);
            }
        }

        private FileItem FilesToFileItem(int file_id)
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                files file = db.files.First(x => x.file_idx == file_id);

                FileItem fileItem = new FileItem(file.file_idx, file.file_name, file.file_size, file.file_crc32, file.file_creationdate, file.file_url);
                return fileItem;
            }
        }
    }
}