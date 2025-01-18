using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using WebApplication2.Data;
using WebApplication2.Models;
using OtpNet;
using System.Linq;


namespace WebApplication2.Controllers

{
    public class LoginController : Controller
    {
        private readonly IDapperRepository<USER> _userRepository;
        private readonly IDapperRepository<PASSWORD> _passwordRepository;

        public LoginController(IDapperRepository<USER> userRepository, IDapperRepository<PASSWORD> passwordRepository)
        {
            _userRepository = userRepository;
            _passwordRepository = passwordRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var salt = GenerateSalt();
                var passwordHash = HashPassword(model.Password, salt, out int hashRounds);

                var user = new USER
                {
                    UserName = model.UserName,
                    Email = model.Email
                };

                var userId = _userRepository.Insert(user, true);

                var password = new PASSWORD
                {
                    UserId = userId.Value,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    HashRounds = hashRounds,
                    PasswordSetDate = DateTime.UtcNow
                };

                _passwordRepository.Insert(password, false);

                return RedirectToAction("Index", "Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {

            if (ModelState.IsValid)
            {
                var user = _userRepository.GetAll().FirstOrDefault(u => u.Email == model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "The email address does not exist.");
                    return View();
                }

                var passwordDetails = _passwordRepository.GetAll().FirstOrDefault(p => p.UserId == user.Id);

                if (passwordDetails == null || !VerifyPassword(model.Password, passwordDetails))
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View();
                }

                
                
                if (string.IsNullOrEmpty(user.SecretKey))
                {
                    var key = KeyGeneration.GenerateRandomKey(20);
                    var userKey = Base32Encoding.ToString(key);
                    _userRepository.SaveSecretKey(user.Id, userKey);
                    user.SecretKey = userKey;
                }

                TempData["UserId"] = user.Id;
                TempData.Keep("UserId");
                return RedirectToAction("Verify2FA");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Verify2FA()
        {
            if (TempData["UserId"] is int userId)
            {
                var user = _userRepository.GetById(userId);
                ViewBag.SecretKey = user.SecretKey;
                TempData.Keep("UserId");
                return View();
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Verify2FA(string code)
        {
            if (TempData["UserId"] is int userId)
            {
                var user = _userRepository.GetById(userId);
                var roles = _userRepository.GetRoles(userId);
                var secretKey = user.SecretKey;

                var otp = new Totp(Base32Encoding.ToBytes(secretKey));
                if (otp.VerifyTotp(code, out long _))
                {
                    TempData["UserId"] = null;
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email)
                    };
                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    foreach (string role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    if (User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Admin", "Login");
                    }
                    else
                    {
                        return RedirectToAction("Success", "Login");
                    }
                    //var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    //HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    //    new ClaimsPrincipal(claimsIdentity));

                    //return RedirectToAction("Success");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Inavlid 2FA code.");
                }


                //            var userId = user.Id;
                //            var roles = _userRepository.GetRoles(userId);


                //            var claims = new List<Claim>
                //            {
                //                new Claim(ClaimTypes.Name, user.UserName),
                //                new Claim(ClaimTypes.Email, user.Email)     
                //};

                //var identity = new ClaimsIdentity(claims, "Cookies");
                //var principal = new ClaimsPrincipal(identity);

                //foreach (string role in roles)
                //            {
                //                claims.Add(new Claim(ClaimTypes.Role, role));
                //            }

                //            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                //            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                //                new ClaimsPrincipal(claimsIdentity));

                //            if (User.IsInRole("Admin"))
                //            {
                //                return RedirectToAction("Admin", "Login");
                //            }
                //            else
                //            {
                //                return RedirectToAction("Success", "Login");
                //            }
            }

            return View();
        }

        [HttpGet]
        public IActionResult Success()
        {
            var userName = User.Identity.Name;
            ViewBag.UserName = userName;
            return View();
        }

		[Authorize(Roles = "Admin")]
		public IActionResult Admin()
		{
			return View();
		}

		[HttpPost]
		public IActionResult LoginWithGoogle(string provider, string returnUrl)
		{
			return Challenge(new AuthenticationProperties
			{
				RedirectUri = Url.Action("GoogleResponse")
			}, GoogleDefaults.AuthenticationScheme);
		}

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string returnUrl = null, string remoteError = null)
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = new List<Claim>();
            claims.AddRange(
            result.Principal.Identities.FirstOrDefault().Claims);
            claims.Add(new Claim(ClaimTypes.Role, "User"));

            return RedirectToAction("Success");
        }

        private string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt, out int hashRounds)
        {
            hashRounds = 10000;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), hashRounds))
            {
                return Convert.ToBase64String(deriveBytes.GetBytes(32));
            }
        }

        private bool VerifyPassword(string password, PASSWORD passwordModel)
        {
            var computedHash = HashPassword(password, passwordModel.Salt, out _);
            return computedHash == passwordModel.PasswordHash;
        }
    }
}
