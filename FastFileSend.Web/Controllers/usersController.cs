using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using FastFileSend.Web.Models;

namespace FastFileSend.Web.Controllers
{
    public class usersController : ApiController
    {
        private fastfilesendEntities db = new fastfilesendEntities();

        // GET: api/users
        public IQueryable<users> Getusers()
        {
            return db.users;
        }

        // GET: api/users/5
        [ResponseType(typeof(users))]
        public IHttpActionResult Getusers(int id)
        {
            users users = db.users.Find(id);
            if (users == null)
            {
                return NotFound();
            }

            return Ok(users);
        }

        // PUT: api/users/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putusers(int id, users users)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != users.user_idx)
            {
                return BadRequest();
            }

            db.Entry(users).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!usersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/users
        [ResponseType(typeof(users))]
        public IHttpActionResult Postusers(users users)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.users.Add(users);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (usersExists(users.user_idx))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = users.user_idx }, users);
        }

        // DELETE: api/users/5
        [ResponseType(typeof(users))]
        public IHttpActionResult Deleteusers(int id)
        {
            users users = db.users.Find(id);
            if (users == null)
            {
                return NotFound();
            }

            db.users.Remove(users);
            db.SaveChanges();

            return Ok(users);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool usersExists(int id)
        {
            return db.users.Count(e => e.user_idx == id) > 0;
        }
    }
}