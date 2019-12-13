using FastFileSend.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FastFileSend.Web.Controllers
{
    public static class Connection
    {
        public static fastfilesendEntities db = new fastfilesendEntities();
    }
}