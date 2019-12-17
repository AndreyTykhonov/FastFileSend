using FastFileSend.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace FastFileSend.Web.Controllers
{
    public class SendController : ApiController
    {
        public JsonResult<int> Get(int id, string password, int target, int file)
        {
            if (!Security.PasswordValid(id, password))
            {
                throw new Exception("Wrong password!");
            }

            using (fastfilesendEntities db = new fastfilesendEntities())
            {

                transactions newSend = new transactions()
                {
                    file_id = file,
                    sender_id = id,
                    receiver_id = target,
                    status = 0,
                    download_idx = FindEmptpyId()
                };

                db.transactions.Add(newSend);
                db.SaveChanges();

                return Json(newSend.download_idx);
            }
        }

        private int FindEmptpyId()
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
    }
}
