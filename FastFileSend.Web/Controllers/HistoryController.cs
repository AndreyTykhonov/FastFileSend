using FastFileSend.Database;
using FastFileSend.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace FastFileSend.Web.Controllers
{

    public class HistoryController : ApiController
    {
        public JsonResult<List<HistoryItem>> Get(int id, string password)
        {
            if (!Security.PasswordValid(id, password))
            {
                throw new Exception("Wrong password!");
            }

            using (fastfilesendEntities db = new fastfilesendEntities())
            {

                var containsMyId = db.transactions.Where(x => x.sender_id == id || x.receiver_id == id).ToList();

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

                return Json(historyList);
            }
        }

        private FileItem FilesToFileItem(int file_id)
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                files file = db.files.First(x => x.file_idx == file_id);

                FileItem fileItem = new FileItem();
                fileItem.CRC32 = file.file_crc32;
                fileItem.CreationDate = file.file_creationdate;
                fileItem.Id = file.file_idx;
                fileItem.Name = file.file_name;
                fileItem.Size = file.file_size;
                fileItem.Url = file.file_url;
                return fileItem;
            }
        }
    }
}
