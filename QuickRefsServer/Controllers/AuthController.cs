using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickRefsServer.Models;

using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace QuickRefsServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly QuickRefsDbContext _context;
        private readonly IDistributedCache _cache;

        public AuthController(QuickRefsDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }



        [HttpPost("login")]
        public ActionResult<bool> Login(Credential credential)
        {
            var user = _context.Users.SingleOrDefault(u => u.Name == credential.Name);
            if(user == null)
            {
                return BadRequest("ユーザが見つかりません");
            }

            var salt = user.Salt;
            var hash = CalculatePasswordHash(credential.Password, salt);
            bool isLoginOK = user.PasswordHash == hash;
            if (isLoginOK)
            {
                // ログインセッションを保存する
                Guid id = Guid.NewGuid();
                var options = new DistributedCacheEntryOptions();
                options.AbsoluteExpiration = DateTimeOffset.MaxValue;
                _cache.SetString(String.Format("quickrefs:sessionId:{0}", id), user.Id.ToString(), options);
                return Ok(id);
            }
            else
            {
                return BadRequest("ログインに失敗しました");
            }
        }

        [HttpPost("signup")]
        public async Task<ActionResult<bool>> Signup(UserInfo userInfo)
        {
            Console.WriteLine("name: {0}, password: {1}", userInfo.Name, userInfo.Password);
            bool isSignupOK = !_context.Users.Any(u => u.Name ==　userInfo.Name);

            if (isSignupOK)
            {
                User user = new User();

                user.Id = Guid.NewGuid();
                user.Name = userInfo.Name;
                user.ScreenName = "Anonymous";
                var salt = GenerateSalt();
                user.PasswordHash = CalculatePasswordHash(userInfo.Password, salt);
                user.Salt = salt;
                user.CreatedAt = DateTime.Now.ToUniversalTime();
                user.UpdatedAt = DateTime.Now.ToUniversalTime();

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(true);
            }
            else
            {
                return BadRequest("すでに使われているユーザー名です");
            }
        }

        [HttpPost("auth")]
        public ActionResult<bool> Auth(string name, string password)
        {
            bool isAuthOK = _context.Users.Any(user => user.Name == name && user.PasswordHash == password);
            if (isAuthOK)
            {
                return Ok(isAuthOK);
            }
            else
            {
                return BadRequest();
            }
        }

        byte[] GenerateSalt()
        {
            byte[] salt = new byte[128 / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(salt);
            }
            return salt;
        }

        string CalculatePasswordHash(string password, byte[] salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

    }
}
