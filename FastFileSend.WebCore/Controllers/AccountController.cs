using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FastFileSend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FastFileSend.WebCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private List<users> users = new List<users>();

        public AccountController()
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                users = db.users.ToList();
            }
        }

        [Route("Token")]
        public IActionResult Token(string username, string password)
        {
            var identity = GetIdentity(username, password);
            if (identity == null)
            {
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

        [Route("Register")]
        public IActionResult Register()
        {
            using (fastfilesendEntities db = new fastfilesendEntities())
            {
                int emptyId = FindEmptpyUserId();
                int randomPassword = new Random().Next(int.MaxValue);
                users newAccount = new users();

                newAccount.user_idx = emptyId;
                newAccount.user_friendlyname = newAccount.user_idx.ToString();
                newAccount.user_registerdate = DateTime.Now;
                newAccount.user_password = randomPassword.ToString();

                db.users.Add(newAccount);
                db.SaveChanges();

                return Ok(newAccount);
            }
        }

        private int FindEmptpyUserId()
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

        private ClaimsIdentity GetIdentity(string username, string password)
        {
            users user = users.FirstOrDefault(x => x.user_idx.ToString() == username && x.user_password == password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.user_idx.ToString()),
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