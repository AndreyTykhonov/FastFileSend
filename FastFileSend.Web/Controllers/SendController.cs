using FastFileSend.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FastFileSend.Web.Controllers
{
    public class SendController : ApiController
    {
        private fastfilesendEntities db = new fastfilesendEntities();

        public void Get(int id, string password, int target, int file)
        {
            if (!Security.PasswordValid(id, password))
            {
                throw new Exception("Wrong password!");
            }

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
        }

        private int FindEmptpyId()
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
