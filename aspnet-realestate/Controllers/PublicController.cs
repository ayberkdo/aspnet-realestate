using aspnet_realestate.Models;
using aspnet_realestate.Repositories;
using aspnet_realestate.ViewModels;
using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aspnet_realestate.Controllers
{
    public class PublicController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly INotyfService _notyf;
        private readonly PropertyRepository _propertyRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public PublicController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            INotyfService notyf,
            PropertyRepository propertyRepository,
            CategoryRepository categoryRepository,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
            _propertyRepository = propertyRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        // --- SAYFA YÖNLENDİRMELERİ ---

        // Anasayfa
        public async Task<IActionResult> Index()
        {
            var query = _propertyRepository.Where(x => x.IsActive)
                .Include(x => x.Images)
                .Include(x => x.Category)
                .AsNoTracking()
                .AsQueryable();

            var properties = await query.OrderByDescending(x => x.Created).ToListAsync();
            var model = _mapper.Map<List<PropertyViewModel>>(properties);

            ViewData["Title"] = "Ana Sayfa";
            return View(model);
        }

        // Tüm İlanlar
        [Route("properties")]
        public async Task<IActionResult> Properties(string? search)
        {
            var query = _propertyRepository.Where(x => x.IsActive)
                .Include(x => x.Images)
                .Include(x => x.Category)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
                ViewBag.Search = search;
            }

            var properties = await query.OrderByDescending(x => x.Created).ToListAsync();
            var model = _mapper.Map<List<PropertyViewModel>>(properties);

            ViewData["Title"] = "Tüm İlanlar";
            return View(model);
        }

        // İlan Detay
        [Route("property/{slug}")] // Yeni SEO uyumlu route
        public async Task<IActionResult> PropertyDetail(string slug)
        {
            // Veritabanında slug üzerinden arama yapıyoruz
            var property = await _propertyRepository.Where(x => x.Slug == slug && x.IsActive)
                .Include(x => x.Category)
                .Include(x => x.User)
                .Include(x => x.Images)
                .Include(x => x.PropertyDestination) // Lokasyon bilgileri için şart
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (property == null) return NotFound();

            var model = _mapper.Map<PropertyViewModel>(property);

            // Veritabanındaki 'Other' alanını parçalayarak ViewModel'e doldurma işlemi
            // (Bunu burada manuel yapıyoruz ki detay sayfasında özellikler görünsün)
            if (!string.IsNullOrEmpty(property.Other))
            {
                model.CustomFields = new List<PropertyCustomFieldViewModel>();
                var parts = property.Other.Split(";;");
                foreach (var p in parts)
                {
                    if (p.Contains(":") && !p.Contains("(")) // Custom Field kontrolü
                    {
                        var split = p.Split(':');
                        model.CustomFields.Add(new PropertyCustomFieldViewModel
                        {
                            FieldName = split[0],
                            Value = split[1]
                        });
                    }
                }
            }

            return View(model);
        }

        // Kiralık İlanlar
        [Route("for-rent")]
        public async Task<IActionResult> ForRent()
        {
            // ViewModel'de "sale" / "rent" dediğin için filtreyi buna göre yapıyoruz
            // Eğer veritabanında "Kiralık" yazıyorsa burayı "Kiralık" yapmalısın.
            var properties = await _propertyRepository.Where(x => x.IsActive && (x.Type == "rent" || x.Type == "Kiralık"))
                .Include(x => x.Images)
                .Include(x => x.Category)
                .OrderByDescending(x => x.Created)
                .AsNoTracking()
                .ToListAsync();

            var model = _mapper.Map<List<PropertyViewModel>>(properties);

            ViewData["Title"] = "Kiralık İlanlar";
            return View("Properties", model);
        }

        // Satılık İlanlar
        [Route("for-sale")]
        public async Task<IActionResult> ForSale()
        {
            var properties = await _propertyRepository.Where(x => x.IsActive && (x.Type == "sale" || x.Type == "Satılık"))
                .Include(x => x.Images)
                .Include(x => x.Category)
                .OrderByDescending(x => x.Created)
                .AsNoTracking()
                .ToListAsync();

            var model = _mapper.Map<List<PropertyViewModel>>(properties);

            ViewData["Title"] = "Satılık İlanlar";
            return View("Properties", model);
        }

        // Kategoriye Göre İlanlar
        [Route("category/{slug}")]
        public async Task<IActionResult> CategoryBySlug(string slug)
        {
            var category = await _categoryRepository.FirstOrDefaultAsync(x => x.Slug == slug);
            if (category == null) return NotFound();

            var properties = await _propertyRepository.Where(x => x.CategoryId == category.Id && x.IsActive)
                .Include(x => x.Images)
                .Include(x => x.Category)
                .OrderByDescending(x => x.Created)
                .AsNoTracking()
                .ToListAsync();

            var model = _mapper.Map<List<PropertyViewModel>>(properties);

            ViewData["Title"] = $"{category.Name} İlanları";
            return View("Properties", model);
        }


        // --- LOGIN / REGISTER / LOGOUT ---

        [HttpGet("login")]
        public IActionResult Login()
        {
            
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                _notyf.Success("Harika! Başarılı bir şekilde oturum açtınız.");

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "User");
            }

            if (result.IsLockedOut)
            {
                _notyf.Error("Hesabınız kilitlendi.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya parola.");
            }

            return View(model);
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var newUser = new AppUser
            {
                FullName = model.FullName,
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Created = DateTime.Now,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, "User");
                _notyf.Success("Harika! Hesabınız oluşturuldu. Şimdi giriş yapabilirsiniz.");
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

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