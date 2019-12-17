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
    public class LastOnlineController : ApiController
    {
        public JsonResult<DateTime> Get(int id)
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                return Json(db.users.First(x => x.user_idx == id).user_lastonline);
            }
        }
    }
}
