using FastFileSend.WebCore.DataBase;
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
            using (MyDbContext db = new MyDbContext())
            {
                return Ok(db.Users.Find(id).LastOnline);
            }
        }

        [Authorize]
        [Route("Update")]
        public void Update()
        {
            int myId = Convert.ToInt32(User.Identity.Name);
            using (MyDbContext db = new MyDbContext())
            {
                db.Users.Find(myId).LastOnline = DateTime.UtcNow;
                db.SaveChanges();
            }
        }
    }
}
