﻿using FastFileSend.Main;
using FastFileSend.Main.Enum;
using FastFileSend.Main.Models;
using FastFileSend.WebCore.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace FastFileSend.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        [Authorize]
        [Route("Get")]
        public IActionResult Get(long ticks)
        {
            int myId = Convert.ToInt32(User.Identity.Name);
            using (MyDbContext db = new MyDbContext())
            {
                var containsMyId = db.History.Include(x => x.File).Where(x => x.Sender == myId || x.Receiver == myId).ToList();
                DateTime minimum = new DateTime(ticks);
                var upToDate = containsMyId.Where(x => x.Date > DateTime.UtcNow.AddDays(-7)).Where(x => x.Date > minimum).ToList();

                // online update
                db.Users.Find(myId).LastOnline = DateTime.UtcNow;
                db.SaveChanges();

                return Ok(upToDate);
            }
        }
    }
}
