﻿using FastFileSend.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FastFileSend.Web.Controllers
{
    public static class Security
    {
        static fastfilesendEntities db = new fastfilesendEntities();

        public static bool PasswordValid(int id, string password)
        {
            users myId = db.users.First(x => x.user_idx == id);
            return myId.user_password == password;
        }
    }
}