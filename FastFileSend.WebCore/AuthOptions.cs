using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.WebCore
{
    public class AuthOptions
    {
        public const string ISSUER = "FastFileSend.Server"; // издатель токена
        public const string AUDIENCE = "FastFileSend.Client"; // потребитель токена
        const string KEY = "TransparentElephant";   // ключ для шифрации
        public const int LIFETIME = 24 * 60; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
