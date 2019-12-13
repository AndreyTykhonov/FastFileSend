using FastFileSend.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FastFileSend.Web.Controllers
{
    public class OnlineController : ApiController
    {
        private fastfilesendEntities db = new fastfilesendEntities();

        public void Get(int id)
        {
            db.users.First(x => x.user_idx == id).user_lastonline = DateTime.Now;
            db.SaveChanges();
        }
    }
}
