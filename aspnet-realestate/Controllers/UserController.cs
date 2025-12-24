using aspnet_realestate.Models;
using aspnet_realestate.Repositories;
using aspnet_realestate.ViewModels;
using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace aspnet_realestate.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly INotyfService _notyf;
        private readonly IMapper _mapper;
        private readonly PropertyRepository _propertyRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly AmenitiesGroupRepository _amenitiesGroupRepository;
        private readonly CategoryFieldsRepository _categoryFieldsRepository;
        private readonly MessageRepository _messageRepository;

        public UserController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            INotyfService notyf,
            IMapper mapper,
            PropertyRepository propertyRepository,
            CategoryRepository categoryRepository,
            AmenitiesGroupRepository amenitiesGroupRepository,
            CategoryFieldsRepository categoryFieldsRepository,
            MessageRepository messageRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
            _mapper = mapper;
            _propertyRepository = propertyRepository;
            _categoryRepository = categoryRepository;
            _amenitiesGroupRepository = amenitiesGroupRepository;
            _categoryFieldsRepository = categoryFieldsRepository;
            _messageRepository = messageRepository;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private string Slugify(string text) => text.ToLower()
            .Replace(" ", "-").Replace("ı", "i").Replace("ş", "s")
            .Replace("ç", "c").Replace("ğ", "g").Replace("ü", "u")
            .Replace("ö", "o").Replace(".", "").Replace(",", "");

        // --------------------------------------------------------------
        // DASHBOARD
        // --------------------------------------------------------------
        [Route("user/dashboard")]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var userProperties = await _propertyRepository.Where(x => x.UserId == userId).ToListAsync();
            var propertyIds = userProperties.Select(x => x.Id).ToList();

            var totalMessages = await _messageRepository
                .Where(x => propertyIds.Contains(x.PropertyId))
                .CountAsync();

            var viewModel = new UserDashboardViewModel
            {
                MyTotalProperties = userProperties.Count,
                MyActiveProperties = userProperties.Count(x => x.IsActive),
                MyNewMessages = totalMessages
            };

            return View(viewModel);
        }

        // --------------------------------------------------------------
        // İLAN LİSTELEME
        // --------------------------------------------------------------
        [Route("user/properties")]
        public async Task<IActionResult> Properties(string? search)
        {
            var userId = GetUserId();
            var properties = await _propertyRepository.Where(p => p.UserId == userId)
                .Include(p => p.Category)
                .Include(p => p.Images) // Resim önizlemesi için gerekli
                .OrderByDescending(p => p.Created)
                .ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                properties = properties.Where(p => p.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var model = properties.Select(p => new PropertyViewModel
            {
                Id = p.Id,
                CoverImageUrl = p.CoverImage,
                Title = p.Title,
                CategoryName = p.Category?.Name,
                Type = p.Type,
                Status = p.Status,
                IsActive = p.IsActive,
            }).ToList();

            ViewBag.Search = search;
            return View(model);
        }

        // --------------------------------------------------------------
        // İLAN EKLEME
        // --------------------------------------------------------------
        [HttpGet("user/property/add")]
        public async Task<IActionResult> AddProperty()
        {
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            // AdminController'daki gibi view model hazırlanıyor
            var amenitiesGroups = await _amenitiesGroupRepository.GetAllWithAmenitiesAsync();
            var viewModel = new PropertyViewModel
            {
                CustomFields = new List<PropertyCustomFieldViewModel>(),
                AmenitiesGroups = amenitiesGroups.Select(g => new PropertyAmenitiesGroupViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    Amenities = g.Amenities.Select(a => new PropertyAmenitiesItemViewModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        ImageUrl = a.ImageUrl
                    }).ToList()
                }).ToList(),
                Images = new List<PropertyImageViewModel>()
            };

            return View(viewModel);
        }

        [HttpPost("user/property/add")]
        public async Task<IActionResult> AddProperty(PropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                model.AmenitiesGroups = await _amenitiesGroupRepository.GetAllWithAmenitiesAsync();
                return View(model);
            }

            // --- 1. Slug Oluşturma ---
            string baseSlug = Slugify(model.Title);
            string slug = baseSlug;
            int counter = 1;
            while (await _propertyRepository.AnyAsync(p => p.Slug == slug))
            {
                slug = $"{baseSlug}-{counter++}";
            }

            // --- 2. Custom Fields (String Birleştirme) ---
            string customData = "";
            if (model.CustomFields != null && model.CustomFields.Any())
            {
                customData = string.Join(";;",
                    model.CustomFields
                        .Where(cf => !string.IsNullOrWhiteSpace(cf.Value))
                        .Select(cf => $"{cf.FieldName}:{cf.Value}")
                );
            }

            // --- 3. Amenities (String Birleştirme) ---
            var allGroups = await _amenitiesGroupRepository.GetAllWithAmenitiesAsync();
            string amenitiesData = "";

            if (model.SelectedAmenities != null && model.SelectedAmenities.Any())
            {
                var selectedAmenities = allGroups
                    .Select(g =>
                    {
                        var selected = g.Amenities
                            .Where(a => model.SelectedAmenities.Contains(a.Id))
                            .ToList();

                        if (!selected.Any()) return null;
                        return $"{g.Name}({string.Join(",", selected.Select(a => a.Name))})";
                    })
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                amenitiesData = string.Join(";;", selectedAmenities);
            }

            // --- 4. Destinasyon (Get or Create) ---
            // AdminController'daki gibi manuel kontrol
            var destination = await _propertyRepository.GetDestinationAsync(model.Country ?? "", model.City ?? "", model.District ?? "");
            if (destination == null)
            {
                destination = new PropertyDestination
                {
                    Country = model.Country ?? "",
                    City = model.City ?? "",
                    District = model.District ?? ""
                };
                await _propertyRepository.AddDestinationAsync(destination);
            }

            // --- 5. Other Verisini Birleştirme ---
            List<string> otherParts = new();

            if (!string.IsNullOrEmpty(customData)) otherParts.Add(customData);
            if (!string.IsNullOrEmpty(amenitiesData)) otherParts.Add(amenitiesData);

            if (model.Type == "rent")
            {
                List<string> rentParts = new();
                if (model.DailyPrice.HasValue) rentParts.Add($"daily:{model.DailyPrice}");
                if (model.WeeklyPrice.HasValue) rentParts.Add($"weekly:{model.WeeklyPrice}");
                if (model.MonthlyPrice.HasValue) rentParts.Add($"monthly:{model.MonthlyPrice}");
                if (rentParts.Any()) otherParts.Add(string.Join(";;", rentParts));
            }

            string otherData = string.Join(";;", otherParts);

            // --- 6. Kayıt ---
            var property = new Property
            {
                UserId = GetUserId(), // Current User
                Title = model.Title,
                Description = model.Description,
                Type = model.Type,
                CategoryId = model.CategoryId,
                PropertyDestinationId = destination.Id,
                Address = model.Address ?? "",
                MapLat = model.MapLat,
                MapLng = model.MapLng,
                Price = model.Type == "sale" ? model.Price : 0,
                Currency = model.Currency,
                Other = otherData, // Dolu veri buraya gidiyor
                Slug = slug,
                Status = "approval", // Kullanıcı eklediği için onay bekler
                ApprovalUserId = null,
                IsActive = false,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                CoverImage = model.CoverImageUrl
            };

            await _propertyRepository.AddAsync(property);

            // --- 7. Görseller ---
            if (model.Images != null && model.Images.Any())
            {
                int order = 1;
                foreach (var img in model.Images.Where(i => !string.IsNullOrEmpty(i.ImageUrl)))
                {
                    await _propertyRepository.AddImageAsync(new PropertyImage
                    {
                        PropertyId = property.Id,
                        ImageUrl = img.ImageUrl!,
                        Order = order++
                    });
                }
            }

            _notyf.Success("İlanınız eklendi ve onay sürecine gönderildi.");
            return RedirectToAction("Properties");
        }

        // --------------------------------------------------------------
        // İLAN DÜZENLEME
        // --------------------------------------------------------------
        [HttpGet("user/property/edit/{id}")]
        public async Task<IActionResult> EditProperty(int id)
        {
            var userId = GetUserId();
            var property = await _propertyRepository.GetByIdWithIncludesAsync(id);

            // Güvenlik Kontrolü
            if (property == null || property.UserId != userId) return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            var amenitiesGroups = await _amenitiesGroupRepository.GetAllWithAmenitiesAsync();

            var viewModel = new PropertyViewModel
            {
                Title = property.Title,
                Description = property.Description ?? "",
                Type = property.Type ?? "sale",
                CategoryId = property.CategoryId,
                Country = property.PropertyDestination?.Country,
                City = property.PropertyDestination?.City,
                District = property.PropertyDestination?.District,
                Address = property.Address,
                MapLat = property.MapLat,
                MapLng = property.MapLng,
                Price = property.Price ?? 0m,
                Currency = property.Currency ?? "TRY",
                DailyPrice = null,
                WeeklyPrice = null,
                MonthlyPrice = null,
                CustomFields = new List<PropertyCustomFieldViewModel>(),
                SelectedAmenities = new List<int>(),
                AmenitiesGroups = amenitiesGroups.Select(g => new PropertyAmenitiesGroupViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    Amenities = g.Amenities.Select(a => new PropertyAmenitiesItemViewModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        ImageUrl = a.ImageUrl
                    }).ToList()
                }).ToList(),
                Images = property.Images?.OrderBy(i => i.Order).Select(i => new PropertyImageViewModel
                {
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText
                }).ToList() ?? new List<PropertyImageViewModel>(),
                CoverImage = property.CoverImage,
                Status = property.Status, // Kullanıcıya gösterim için
                SeoTitle = property.SeoTitle,
                SeoDescription = property.SeoDescription,
                SeoKeywords = property.SeoKeywords
            };

            // --- Other Verisini Parçalama (Admin ile Birebir Aynı) ---
            if (!string.IsNullOrEmpty(property.Other))
            {
                var parts = property.Other.Split(";;");

                foreach (var p in parts)
                {
                    if (string.IsNullOrWhiteSpace(p)) continue;

                    // Rent parts
                    if (p.StartsWith("daily:")) { if (decimal.TryParse(p.Split(':')[1], out var dp)) viewModel.DailyPrice = dp; continue; }
                    if (p.StartsWith("weekly:")) { if (decimal.TryParse(p.Split(':')[1], out var wp)) viewModel.WeeklyPrice = wp; continue; }
                    if (p.StartsWith("monthly:")) { if (decimal.TryParse(p.Split(':')[1], out var mp)) viewModel.MonthlyPrice = mp; continue; }

                    // Olanaklar
                    if (p.Contains("(") && p.Contains(")"))
                    {
                        var groupName = p.Substring(0, p.IndexOf("("));
                        var inside = p.Substring(p.IndexOf("(") + 1, p.LastIndexOf(")") - p.IndexOf("(") - 1);
                        var amenityNames = inside.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                        var grp = amenitiesGroups.FirstOrDefault(g => string.Equals(g.Name, groupName, StringComparison.OrdinalIgnoreCase));
                        if (grp != null)
                        {
                            foreach (var aName in amenityNames)
                            {
                                var amen = grp.Amenities.FirstOrDefault(a => string.Equals(a.Name, aName, StringComparison.OrdinalIgnoreCase));
                                if (amen != null) viewModel.SelectedAmenities.Add(amen.Id);
                            }
                        }
                        continue;
                    }

                    // Custom Fields
                    if (p.Contains(":"))
                    {
                        var sp = p.Split(':', 2);
                        viewModel.CustomFields.Add(new PropertyCustomFieldViewModel
                        {
                            FieldName = sp[0],
                            Value = sp.Length > 1 ? sp[1] : ""
                        });
                    }
                }
            }

            ViewBag.Categories = categories;
            ViewBag.PropertyId = id;

            return View(viewModel);
        }

        [HttpPost("user/property/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(int id, PropertyViewModel model)
        {
            var userId = GetUserId();
            var property = await _propertyRepository.GetByIdWithIncludesAsync(id);

            // Güvenlik: Başkasının ilanını değiştiremez
            if (property == null || property.UserId != userId) return NotFound();

            if (!ModelState.IsValid)
            {
                model.AmenitiesGroups = await _amenitiesGroupRepository.GetAllWithAmenitiesAsync();
                // custom fields null gelirse tekrar doldur
                if (model.CustomFields == null || !model.CustomFields.Any())
                {
                    model.CustomFields = await _categoryFieldsRepository.GetFieldsByCategoryIdAsync(model.CategoryId);
                }
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                ViewBag.PropertyId = id;
                return View(model);
            }

            // Slug: Sadece title değiştiyse ve slug çakışması varsa güncelle (opsiyonel, burada admin gibi bırakıyoruz)
            // Admin kodunda update kısmında slug güncelleme mantığı vardı, onu buraya da alıyoruz:
            string baseSlug = Slugify(model.Title);
            string slug = baseSlug;
            int counter = 1;
            while (await _propertyRepository.AnyAsync(p => p.Slug == slug && p.Id != id))
            {
                slug = $"{baseSlug}-{counter++}";
            }

            // --- Update için Other Birleştirme ---
            string customData = "";
            if (model.CustomFields != null && model.CustomFields.Any())
            {
                customData = string.Join(";;",
                    model.CustomFields.Where(cf => !string.IsNullOrWhiteSpace(cf.Value)).Select(cf => $"{cf.FieldName}:{cf.Value}")
                );
            }

            var allGroups = await _amenitiesGroupRepository.GetAllWithAmenitiesAsync();
            string amenitiesData = "";
            if (model.SelectedAmenities != null && model.SelectedAmenities.Any())
            {
                var selectedAmenities = allGroups.Select(g =>
                {
                    var selected = g.Amenities.Where(a => model.SelectedAmenities.Contains(a.Id)).ToList();
                    if (!selected.Any()) return null;
                    return $"{g.Name}({string.Join(",", selected.Select(a => a.Name))})";
                }).Where(s => !string.IsNullOrEmpty(s)).ToList();

                amenitiesData = string.Join(";;", selectedAmenities);
            }

            List<string> otherParts = new();
            if (!string.IsNullOrEmpty(customData)) otherParts.Add(customData);
            if (!string.IsNullOrEmpty(amenitiesData)) otherParts.Add(amenitiesData);

            if (model.Type == "rent")
            {
                List<string> rentParts = new();
                if (model.DailyPrice.HasValue) rentParts.Add($"daily:{model.DailyPrice.Value}");
                if (model.WeeklyPrice.HasValue) rentParts.Add($"weekly:{model.WeeklyPrice.Value}");
                if (model.MonthlyPrice.HasValue) rentParts.Add($"monthly:{model.MonthlyPrice.Value}");
                if (rentParts.Any()) otherParts.Add(string.Join(";;", rentParts));
            }

            string otherData = string.Join(";;", otherParts);

            // Destinasyon kontrolü
            var destination = await _propertyRepository.GetDestinationAsync(model.Country ?? "", model.City ?? "", model.District ?? "");
            if (destination == null)
            {
                destination = new PropertyDestination
                {
                    Country = model.Country ?? "",
                    City = model.City ?? "",
                    District = model.District ?? ""
                };
                await _propertyRepository.AddDestinationAsync(destination);
            }

            // Değerleri Eşle
            property.Title = model.Title;
            property.Description = model.Description;
            property.Type = model.Type;
            property.CategoryId = model.CategoryId;
            property.PropertyDestinationId = destination.Id;
            property.Address = model.Address ?? "";
            property.MapLat = model.MapLat;
            property.MapLng = model.MapLng;
            property.Price = model.Type == "sale" ? model.Price : (decimal?)null;
            property.Currency = model.Currency;
            property.Other = otherData; // <-- Other verisi güncelleniyor
            property.Slug = slug;
            property.CoverImage = string.IsNullOrEmpty(model.CoverImageUrl) ? property.CoverImage : model.CoverImageUrl;

            // Kullanıcı düzenleyince statü onaya düşer
            property.Status = "approval";
            property.IsActive = false;
            property.ApprovalUserId = null; // Onaylayan temizlenir

            property.SeoTitle = model.SeoTitle;
            property.SeoDescription = model.SeoDescription;
            property.SeoKeywords = model.SeoKeywords;
            property.Updated = DateTime.Now;

            await _propertyRepository.UpdateAsync(property);

            // Resimler
            await _propertyRepository.ClearImagesAsync(id);
            if (model.Images != null && model.Images.Any())
            {
                int order = 1;
                foreach (var img in model.Images.Where(i => !string.IsNullOrEmpty(i.ImageUrl)))
                {
                    await _propertyRepository.AddImageAsync(new PropertyImage
                    {
                        PropertyId = property.Id,
                        ImageUrl = img.ImageUrl!,
                        Order = order++
                    });
                }
            }

            _notyf.Information("İlan güncellendi ve tekrar onay sürecine alındı.");
            return RedirectToAction("Properties");
        }

        // --------------------------------------------------------------
        // İLAN SİLME
        // --------------------------------------------------------------
        [HttpGet("user/property/delete/{id}")]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var userId = GetUserId();
            var property = await _propertyRepository.GetByIdWithIncludesAsync(id);

            if (property == null || property.UserId != userId) return NotFound();

            var model = new PropertyViewModel
            {
                Id = property.Id,
                Title = property.Title,
                Country = property.PropertyDestination?.Country,
                City = property.PropertyDestination?.City,
                District = property.PropertyDestination?.District
            };

            return View(model);
        }

        [HttpPost("user/property/delete/{id}")]
        public async Task<IActionResult> DeleteProperty(int id, PropertyViewModel model)
        {
            var userId = GetUserId();
            var property = await _propertyRepository.GetByIdWithIncludesAsync(id);

            if (property == null || property.UserId != userId) return NotFound();

            // Resimleri temizle
            if (property.Images != null && property.Images.Any())
            {
                await _propertyRepository.ClearImagesAsync(property.Id);
            }

            var destination = property.PropertyDestination;

            await _propertyRepository.DeleteAsync(property.Id);

            // Destinasyon boşta kaldıysa sil (Admin mantığı)
            if (destination != null)
            {
                bool usedByOthers = await _propertyRepository.IsDestinationUsedByOthers(destination.Id);
                if (!usedByOthers)
                {
                    await _propertyRepository.DeleteDestinationAsync(destination);
                }
            }

            _notyf.Success("İlan başarıyla silindi.");
            return RedirectToAction("Properties");
        }

        // --------------------------------------------------------------
        // AJAX: KATEGORİ ALANLARI
        // --------------------------------------------------------------
        [HttpGet("user/property/get-category-fields/{categoryId}")]
        public async Task<IActionResult> GetCategoryFieldsJson(int categoryId)
        {
            var fields = await _categoryFieldsRepository.GetFieldsByCategoryIdAsync(categoryId);

            if (fields == null || !fields.Any())
                return Json(new { success = false, fields = new List<object>() });

            var fieldList = fields.Select(f => new
            {
                f.FieldName
            });

            return Json(new { success = true, fields = fieldList });
        }

        // --------------------------------------------------------------
        // AYARLAR (SETTINGS)
        // --------------------------------------------------------------
        [HttpGet("user/settings")]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Public");

            var model = _mapper.Map<UserProfileViewModel>(user);
            return View(model);
        }

        [HttpPost("user/settings")]
        public async Task<IActionResult> Settings(UserProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Public");

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Bio = model.Bio;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                _notyf.Success("Profil bilgileriniz başarıyla güncellendi.");
                return View(model);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}