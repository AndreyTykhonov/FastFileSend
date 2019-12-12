﻿using FastFileSend.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace FastFileSend.Web.Controllers
{
    public class DownloadedController : ApiController
    {
        private fastfilesendEntities db = new fastfilesendEntities();

        public void Get(int file)
        {
            db.transactions.First(x => x.file_id == file).status = 1;
            db.SaveChanges();
        }
    }
}