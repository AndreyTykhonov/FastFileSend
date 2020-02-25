using FastFileSend.Database;
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
    public class OnlineController : ControllerBase
    {
        [Authorize]
        [Route("Get")]
        public IActionResult Get(int id)
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                return Ok(db.users.First(x => x.user_idx == id).user_lastonline);
            }
        }

        [Authorize]
        [Route("Update")]
        public void Update()
        {
            int myId = Convert.ToInt32(User.Identity.Name);
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                db.users.First(x => x.user_idx == myId).user_lastonline = DateTime.UtcNow;
                db.SaveChanges();
            }
        }
    }
}
