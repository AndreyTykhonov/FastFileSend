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
        public void Get(int id)
        {
            Connection.db.users.First(x => x.user_idx == id).user_lastonline = DateTime.Now;
            Connection.db.SaveChanges();
        }
    }
}
