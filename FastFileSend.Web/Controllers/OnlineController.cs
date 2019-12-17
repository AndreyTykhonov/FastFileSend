﻿using FastFileSend.Database;
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
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                db.users.First(x => x.user_idx == id).user_lastonline = DateTime.UtcNow;
                db.SaveChanges();
            }
        }
    }
}
