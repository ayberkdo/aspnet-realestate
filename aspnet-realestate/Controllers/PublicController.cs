using aspnet_realestate.Models;
using aspnet_realestate.ViewModels;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_realestate.Controllers
{
    public class PublicController : Controller
    {
        // Identity servislerini dahil ediyoruz
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly INotyfService _notyf;

        public PublicController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            INotyfService notyf)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
        }

        public IActionResult Index() => View();

        [Route("about")]
        public IActionResult About() => View();

        [Route("properties")]
        public IActionResult Properties() => View();

        [Route("property/{id}")]
        public IActionResult PropertyDetail(int id)
        {
            ViewBag.PropertyId = id;
            return View();
        }

        [Route("contact")]
        public IActionResult Contact() => View();

        [Route("faq")]
        public IActionResult FAQ() => View();

        // --- LOGIN İŞLEMLERİ ---

        [HttpGet("login")]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Identity'nin kendi giriş mekanizmasını kullanıyoruz
            // PasswordSignInAsync: Şifre kontrolü, lockout ve cookie oluşturmayı tek seferde yapar.
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                _notyf.Success("Harika! Başarılı bir şekilde oturum açtınız.");

                // Rol kontrolüne göre yönlendirme
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "User");
            }

            if (result.IsLockedOut)
            {
                _notyf.Error("Hesabınız çok fazla hatalı giriş denemesi nedeniyle kilitlendi.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya parola.");
            }

            return View(model);
        }

        // --- REGISTER İŞLEMLERİ ---

        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Yeni AppUser nesnesi oluşturma
            var newUser = new AppUser
            {
                FullName = model.FullName,
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Created = DateTime.Now,
                IsActive = true
            };

            // Kullanıcıyı oluştur (Şifreleme burada otomatik yapılır)
            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                // Kullanıcıya varsayılan "User" rolünü ata
                await _userManager.AddToRoleAsync(newUser, "User");

                _notyf.Success("Harika! Hesabınız oluşturuldu. Şimdi giriş yapabilirsiniz.");
                return RedirectToAction("Login");
            }

            // Hataları Modelstate'e ekle (Örn: Şifre çok kısa, Email formatı hatalı vb.)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // --- LOGOUT ---

        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [Route("access-denied")]
        public IActionResult AccessDenied() => View();
    }
}