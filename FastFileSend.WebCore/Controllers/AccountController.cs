﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FastFileSend.WebCore.DataBase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FastFileSend.WebCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        [Route("Token")]
        public IActionResult Token(string username, string password)
        {
            var identity = GetIdentity(username, password);
            if (identity == null)
            {
                if (!UserExists(username))
                {
                    CreateNewUser(username, password);
                    return Token(username, password);
                }

                return BadRequest(new { errorText = "Invalid username or password." });
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };

            return Ok(response);
        }

        private User CreateNewUser(string username, string password)
        {
            using (MyDbContext db = new MyDbContext())
            {
                User newAccount = new User
                {
                    Id = Convert.ToInt32(username),
                    RegisterDate = DateTime.Now,
                    Password = password
                };

                db.Users.Add(newAccount);
                db.SaveChanges();

                return newAccount;
            }
        }

        private bool UserExists(string username)
        {
            using (MyDbContext db = new MyDbContext())
            {
                return db.Users.Any(x => x.Id.ToString() == username);
            }
        }

        [Route("Register")]
        public IActionResult Register()
        {
            using (MyDbContext db = new MyDbContext())
            {
                int emptyId = FindEmptpyUserId();
                int randomPassword = new Random().Next(int.MaxValue);
                User newAccount = CreateNewUser(emptyId.ToString(), randomPassword.ToString());

                return Ok(newAccount);
            }
        }

        private int FindEmptpyUserId()
        {
            using (MyDbContext db = new MyDbContext())
            {
                do
                {
                    int newId = new Random().Next(999999);
                    if (db.Users.Find(newId) is null)
                    {
                        return newId;
                    }
                }
                while (true);
            }
        }

        private ClaimsIdentity GetIdentity(string username, string password)
        {
            using (MyDbContext db = new MyDbContext())
            {
                User user = db.Users.FirstOrDefault(x => x.Id.ToString() == username && x.Password == password);
                if (user != null)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Id.ToString()),
                    //new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role)
                };
                    ClaimsIdentity claimsIdentity =
                    new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                        ClaimsIdentity.DefaultRoleClaimType);
                    return claimsIdentity;
                }

                // если пользователя не найдено
                return null;
            }
        }
    }
}