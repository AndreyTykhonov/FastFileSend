using FastFileSend.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Results;

namespace FastFileSend.Web.Controllers
{
    public class RegisterController : ApiController
    {
        public JsonResult<users> Get()
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                int emptyId = FindEmptpyId();
                int randomPassword = new Random().Next(int.MaxValue);
                users newAccount = new users();

                newAccount.user_idx = emptyId;
                newAccount.user_friendlyname = newAccount.user_idx.ToString();
                newAccount.user_registerdate = DateTime.Now;
                newAccount.user_password = randomPassword.ToString();

                db.users.Add(newAccount);
                db.SaveChanges();

                return Json(newAccount);
            }
        }

        private int FindEmptpyId()
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                do
                {
                    int newId = new Random().Next(999999);
                    if (!db.users.Any(x => x.user_idx == newId))
                    {
                        return newId;
                    }
                }
                while (true);
            }
        }
    }
}
