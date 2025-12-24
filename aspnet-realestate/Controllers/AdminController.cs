using aspnet_realestate.Models;
using aspnet_realestate.Repositories;
using aspnet_realestate.ViewModels;
using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Identity kütüphanesi eklendi
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace aspnet_realestate.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // UserRepository yerine UserManager kullanıyoruz
        private readonly UserManager<AppUser> _userManager;
        private readonly AmenitiesGroupRepository _amenitiesGroupRepository;
        private readonly AmenitiesRepository _amenitiesRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly CategoryFieldsRepository _categoryFieldsRepository;
        private readonly SettingRepository _settingRepository;
        private readonly PropertyRepository _propertyRepository;
        private readonly MessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly INotyfService _notyf;

        public AdminController(
            UserManager<AppUser> userManager, // UserRepository yerine UserManager enjekte edildi
            AmenitiesGroupRepository amenitiesGroupRepository,
            AmenitiesRepository amenitiesRepository,
            IMapper mapper,
            INotyfService notyf,
            CategoryRepository categoryRepository,
            SettingRepository settingRepository,
            CategoryFieldsRepository categoryFieldsRepository,
            PropertyRepository propertyRepository,
            MessageRepository messageRepository)
        {
            _userManager = userManager;
            _amenitiesGroupRepository = amenitiesGroupRepository;
            _amenitiesRepository = amenitiesRepository;
            _categoryRepository = categoryRepository;
            _categoryFieldsRepository = categoryFieldsRepository;
            _settingRepository = settingRepository;
            _propertyRepository = propertyRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
            _notyf = notyf;
        }

        // Helper: Identity'den giriş yapan kullanıcının string ID'sini alır
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [Route("admin/dashboard")]
        public async Task<IActionResult> Index()
        {
            var totalProperties = await _propertyRepository.CountAsync();
            var activeProperties = await _propertyRepository.CountAsync(p => p.Status == "published");

            // Kullanıcı sayısı Identity üzerinden çekiliyor
            var totalUsers = await _userManager.Users.CountAsync();

            var model = new DashboardViewModel
            {
                TotalProperties = totalProperties,
                ActiveProperties = activeProperties,
                TotalUsers = totalUsers
            };
            return View(model);
        }

        // --------------------------------------------------------------
        // İLAN YÖNETİMİ
        // --------------------------------------------------------------

        [HttpGet("admin/properties")]
        public async Task<IActionResult> Properties(string? search)
        {
            // Veritabanından tüm property kayıtlarını çekiyoruz
            var properties = await _propertyRepository.GetAllWithIncludesAsync();

            // Eğer arama yapıldıysa filtrele
            if (!string.IsNullOrEmpty(search))
            {
                properties = properties
                    .Where(p =>
                        p.Title.Contains(search) ||
                        (p.Category != null && p.Category.Name.Contains(search))
                    )
                    .ToList();
            }

            // Listeyi ViewModel’e dönüştür
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

            // Arama değerini ViewBag üzerinden View’a gönder
            ViewBag.Search = search;

            return View(model);
        }

        [HttpGet("admin/property/add")]
        public async Task<IActionResult> AddProperty()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var amenitiesGroups = await _amenitiesGroupRepository.GetAllWithAmenitiesAsync();

            ViewBag.Categories = categories ?? new List<Categories>();

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

        [HttpPost("admin/property/add")]
        public async Task<IActionResult> AddProperty(PropertyViewModel model)
        {

            if (!ModelState.IsValid)
            {
                model.AmenitiesGroups = await _amenitiesGroupRepository.GetAllWithAmenitiesAsync();
                model.CustomFields = await _categoryFieldsRepository.GetFieldsByCategoryIdAsync(model.CategoryId);
                return View(model);
            }


            string baseSlug = Slugify(); // Metod sende mevcut
            string slug = baseSlug;
            int counter = 1;
            while (await _propertyRepository.AnyAsync(p => p.Slug == slug))
            {
                slug = $"{baseSlug}-{counter++}";
            }


            string customData = "";
            if (model.CustomFields != null && model.CustomFields.Any())
            {
                customData = string.Join(";;",
                    model.CustomFields
                          .Where(cf => !string.IsNullOrWhiteSpace(cf.Value))
                          .Select(cf => $"{cf.FieldName}:{cf.Value}")
                );
            }


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


            PropertyDestination? destination = await _propertyRepository.GetDestinationAsync(
                model.Country ?? "", model.City ?? "", model.District ?? ""
            );

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


            List<string> otherParts = new();

            if (!string.IsNullOrEmpty(customData))
                otherParts.Add(customData);

            if (!string.IsNullOrEmpty(amenitiesData))
                otherParts.Add(amenitiesData);

            if (model.Type == "rent")
            {
                List<string> rentParts = new();
                if (model.DailyPrice.HasValue) rentParts.Add($"daily:{model.DailyPrice}");
                if (model.WeeklyPrice.HasValue) rentParts.Add($"weekly:{model.WeeklyPrice}");
                if (model.MonthlyPrice.HasValue) rentParts.Add($"monthly:{model.MonthlyPrice}");
                if (rentParts.Any())
                    otherParts.Add(string.Join(";;", rentParts));
            }

            string otherData = string.Join(";;", otherParts);


            var property = new Property
            {
                UserId = GetUserId(), // String ID alınıyor
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
                Other = otherData,
                Slug = slug,
                Status = "approval",
                ApprovalUserId = null,
                SeoTitle = null,
                SeoDescription = null,
                SeoKeywords = null,
                PublishedAt = null,
                IsActive = false,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                CoverImage = model.CoverImageUrl
            };

            await _propertyRepository.AddAsync(property);


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

            _notyf.Success("İlan başarıyla eklendi!");
            return RedirectToAction("Properties");
        }

        [HttpGet("admin/property/edit/{id}")]
        public async Task<IActionResult> EditProperty(int id)
        {

            var property = await _propertyRepository.GetByIdWithIncludesAsync(id);
            if (property == null)
                return NotFound();


            var categories = await _categoryRepository.GetAllAsync();
            var amenitiesGroups = await _amenitiesGroupRepository.GetAllWithAmenitiesAsync();

            var statusData = new List<(string value, string text)>
            {
                ("approval", "Onay Bekliyor"),
                ("suspend", "Askıya Al"),
                ("accepted", "Kabul Et"),
                ("rejected", "Reddet")
            };

            ViewBag.Statuses = statusData.Select(s => new SelectListItem
            {
                Value = s.value,
                Text = s.text

            }).ToList();


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
                Status = property.Status,
                SeoTitle = property.SeoTitle,
                SeoDescription = property.SeoDescription,
                SeoKeywords = property.SeoKeywords
            };


            if (!string.IsNullOrEmpty(property.Other))
            {
                // örnek Other: "metrekare:120;;oda:3;;GroupName(Air Conditioning,Elevator);;daily:50;;monthly:900"
                var parts = property.Other.Split(";;");

                foreach (var p in parts)
                {
                    if (string.IsNullOrWhiteSpace(p)) continue;

                    // Rent parts (daily, weekly, monthly)
                    if (p.StartsWith("daily:"))
                    {
                        if (decimal.TryParse(p.Split(':')[1], out var dp)) viewModel.DailyPrice = dp;
                        continue;
                    }
                    if (p.StartsWith("weekly:"))
                    {
                        if (decimal.TryParse(p.Split(':')[1], out var wp)) viewModel.WeeklyPrice = wp;
                        continue;
                    }
                    if (p.StartsWith("monthly:"))
                    {
                        if (decimal.TryParse(p.Split(':')[1], out var mp)) viewModel.MonthlyPrice = mp;
                        continue;
                    }

                    // Olanak grubu formatı: GroupName(item1,item2)
                    if (p.Contains("(") && p.Contains(")"))
                    {
                        var groupName = p.Substring(0, p.IndexOf("("));
                        var inside = p.Substring(p.IndexOf("(") + 1, p.LastIndexOf(")") - p.IndexOf("(") - 1);
                        var amenityNames = inside.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                        // grup adıyla eşleştirip ID'leri al
                        var grp = amenitiesGroups.FirstOrDefault(g => string.Equals(g.Name, groupName, StringComparison.OrdinalIgnoreCase));
                        if (grp != null)
                        {
                            foreach (var aName in amenityNames)
                            {
                                var amen = grp.Amenities.FirstOrDefault(a => string.Equals(a.Name, aName, StringComparison.OrdinalIgnoreCase));
                                if (amen != null)
                                {
                                    viewModel.SelectedAmenities.Add(amen.Id);
                                }
                            }
                        }
                        continue;
                    }

                    // custom field formatı: fieldName:value
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

        [HttpPost("admin/property/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(int id, PropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {

                model.AmenitiesGroups = await _amenitiesGroupRepository.GetAllWithAmenitiesAsync();
                model.CustomFields = model.CustomFields ?? await _categoryFieldsRepository.GetFieldsByCategoryIdAsync(model.CategoryId);
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                ViewBag.PropertyId = id;
                return View("EditProperty", model);
            }

            var property = await _propertyRepository.GetByIdWithIncludesAsync(id);
            if (property == null) return NotFound();

            string baseSlug = Slugify(); // Metod sende mevcut
            string slug = baseSlug;
            int counter = 1;
            while (await _propertyRepository.AnyAsync(p => p.Slug == slug && p.Id != id))
            {
                slug = $"{baseSlug}-{counter++}";
            }


            string customData = "";
            if (model.CustomFields != null && model.CustomFields.Any())
            {
                customData = string.Join(";;",
                    model.CustomFields
                          .Where(cf => !string.IsNullOrWhiteSpace(cf.Value))
                          .Select(cf => $"{cf.FieldName}:{cf.Value}")
                );
            }


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


            var destination = await _propertyRepository.GetDestinationAsync(
                model.Country ?? "", model.City ?? "", model.District ?? "");
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
            property.Other = otherData;
            property.Slug = slug;
            property.CoverImage = string.IsNullOrEmpty(model.CoverImageUrl) ? property.CoverImage : model.CoverImageUrl;

            property.Status = model.Status;

            if (property.Status == "accepted")
            {
                property.IsActive = true;
                property.ApprovalUserId = GetUserId(); // String ID
                property.PublishedAt = DateTime.Now;
            }
            else
            {
                property.IsActive = false;
            }

            property.SeoTitle = model.SeoTitle;
            property.SeoDescription = model.SeoDescription;
            property.SeoKeywords = model.SeoKeywords;
            property.Updated = DateTime.Now;

            await _propertyRepository.UpdateAsync(property);


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

            _notyf.Success("İlan başarıyla güncellendi!");
            return RedirectToAction("Properties");
        }

        [HttpGet("admin/property/delete/{id}")]
        public async Task<IActionResult> DeleteProperty(int id)
        {

            var property = await _propertyRepository.GetByIdWithIncludesAsync(id);

            if (property == null)
                return NotFound();

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


        [HttpPost("admin/property/delete/{id}")]
        public async Task<IActionResult> DeleteProperty(int id, PropertyViewModel model)
        {

            var property = await _propertyRepository.GetByIdWithIncludesAsync(id);
            if (property == null)
                return NotFound();


            if (property.Images != null && property.Images.Any())
            {
                await _propertyRepository.ClearImagesAsync(property.Id);
            }


            var destination = property.PropertyDestination;


            await _propertyRepository.DeleteAsync(property.Id);


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



        [HttpGet("admin/property/get-category-fields/{categoryId}")]
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
        // KATEGORİ YÖNETİMİ
        // --------------------------------------------------------------

        [HttpGet("admin/categories")]
        public async Task<IActionResult> Categories(string? search)
        {
            var allCategories = await _categoryRepository.GetAllAsync();
            var filteredCategories = allCategories;

            search = search?.ToLower();
            ViewData["Search"] = search ?? "";

            if (!string.IsNullOrEmpty(search))
            {
                filteredCategories = allCategories
                    .Where(g =>
                        (g.Name ?? "").ToLower().Contains(search) ||
                        (g.Description ?? "").ToLower().Contains(search))
                    .ToList();
            }

            var model = filteredCategories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = allCategories
                    .FirstOrDefault(parent => parent.Id == c.ParentCategoryId)?.Name ?? "-",
                IsActive = c.IsActive,
                Created = c.Created
            }).ToList();

            ViewBag.Search = search;
            return View(model);
        }

        [HttpGet("admin/category/add")]
        public async Task<IActionResult> AddCategory()
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            await SetParentCategoryListAsync(); // Mevcut metodun
            return View();
        }

        [HttpPost("admin/category/add")]
        public async Task<IActionResult> AddCategory(CategoryViewModel model)
        {
            // Validasyon hatası varsa listeyi tekrar doldur
            if (!ModelState.IsValid)
            {
                await SetParentCategoryListAsync();
                return View(model);
            }

            var categories = await _categoryRepository.GetAllAsync();

            // Aynı parent altında aynı isimde kategori var mı?
            var existingByName = categories.FirstOrDefault(c =>
                (c.Name ?? "").ToLower() == model.Name!.ToLower() &&
                c.ParentCategoryId == model.ParentCategoryId);

            if (existingByName != null)
            {
                ModelState.AddModelError(nameof(model.Name), "Bu isimde bir kategori zaten mevcut.");
                await SetParentCategoryListAsync();
                return View(model);
            }

            // Aynı parent altında aynı slug var mı?
            var existingBySlug = categories.FirstOrDefault(c =>
                c.Slug != null &&
                model.Slug != null &&
                c.Slug.ToLower() == model.Slug.ToLower() &&
                c.ParentCategoryId == model.ParentCategoryId);

            if (existingBySlug != null)
            {
                ModelState.AddModelError(nameof(model.Slug), "Bu slug zaten aynı alt kategoride kullanılmış.");
                await SetParentCategoryListAsync();
                return View(model);
            }

            var category = _mapper.Map<Categories>(model);
            category.Created = DateTime.Now;
            category.Updated = DateTime.Now;

            await _categoryRepository.AddAsync(category);
            _notyf.Success("Harika! " + category.Name + " başarıyla eklendi.");
            return RedirectToAction("Categories");
        }

        [HttpGet("admin/category/edit/{id}")]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            // Burası gereksiz olabilir ama orijinal kodda varsa kalabilir
            if (!ModelState.IsValid)
            {
                return View();
            }

            await SetParentCategoryListAsync(id);

            var model = _mapper.Map<CategoryViewModel>(category);
            return View(model);
        }

        [HttpPost("admin/category/edit/{id}")]
        public async Task<IActionResult> EditCategory(int id, CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await SetParentCategoryListAsync(id);
                return View(model);
            }

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _notyf.Error("Güncellenmek istenen kategori bulunamadı.");
                return RedirectToAction("Categories");
            }

            var categories = await _categoryRepository.GetAllAsync();

            // Aynı parent altında aynı isim var mı? (Kendisi hariç)
            var existingByName = categories.FirstOrDefault(c =>
                c.Id != id &&
                c.ParentCategoryId == model.ParentCategoryId &&
                (c.Name ?? "").ToLower() == model.Name!.ToLower());

            if (existingByName != null)
            {
                ModelState.AddModelError(nameof(model.Name), "Bu isimde bir kategori zaten mevcut.");
                await SetParentCategoryListAsync(id); // View dönerken listeyi doldurmalıyız
                return View(model);
            }

            // Aynı parent altında aynı slug var mı? (Kendisi hariç)
            var existingBySlug = categories.FirstOrDefault(c =>
                c.Id != id &&
                c.ParentCategoryId == model.ParentCategoryId &&
                c.Slug != null &&
                model.Slug != null &&
                c.Slug.ToLower() == model.Slug.ToLower());

            if (existingBySlug != null)
            {
                ModelState.AddModelError(nameof(model.Slug), "Bu slug zaten aynı alt kategoride kullanılmış.");
                await SetParentCategoryListAsync(id); // View dönerken listeyi doldurmalıyız
                return View(model);
            }

            if (model.ParentCategoryId == category.Id)
            {
                ModelState.AddModelError(nameof(model.ParentCategoryId), "Bir kategori kendi altına taşınamaz.");
                await SetParentCategoryListAsync(id); // View dönerken listeyi doldurmalıyız
                return View(model);
            }

            category.Name = model.Name;
            category.Slug = model.Slug;
            category.Description = model.Description;
            category.ImageUrl = model.ImageUrl;
            category.ParentCategoryId = model.ParentCategoryId == 0 ? 0 : model.ParentCategoryId;
            category.IsActive = model.IsActive;
            category.Updated = DateTime.Now;

            await _categoryRepository.UpdateAsync(category);

            _notyf.Success("Kategori " + category.Name + " başarıyla güncellendi.");
            return RedirectToAction("Categories");
        }

        [HttpGet("admin/category/delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<CategoryViewModel>(category);
            return View(model);
        }

        [HttpPost("admin/category/delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id, CategoryViewModel model)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            await _categoryRepository.DeleteAsync(id); // id parametresi kullanılmalı
            _notyf.Success("Harika! " + category.Name + " başarıyla silindi.");
            return RedirectToAction("Categories");
        }

        // --------------------------------------------------------------
        // KATEGORİ ALANLARI YÖNETİMİ
        // --------------------------------------------------------------

        [HttpGet("admin/category/fields")]
        public async Task<IActionResult> CategoryFields(string? search)
        {
            // Kategorileri ve kategori alanlarını çek
            var allCategories = await _categoryRepository.GetAllAsync();
            var allFields = await _categoryFieldsRepository.GetAllAsync();

            search = search?.Trim();
            ViewData["Search"] = search ?? "";

            var filteredFields = allFields;
            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                filteredFields = allFields
                    .Where(f =>
                        (!string.IsNullOrEmpty(f.FieldName) && f.FieldName!.ToLower().Contains(s)) ||
                        (allCategories.FirstOrDefault(cat => cat.Id == f.CategoryId)?.Name?.ToLower().Contains(s) == true)
                    )
                    .ToList();
            }

            var model = filteredFields.Select(f => new CategoryFieldsViewModel
            {
                Id = f.Id,
                FieldName = f.FieldName,
                CategoryId = f.CategoryId,
                GroupName = allCategories.FirstOrDefault(cat => cat.Id == f.CategoryId)?.Name ?? "-",
                IsActive = f.IsActive,
                Created = f.Created
            }).ToList();

            ViewBag.Search = search;
            return View(model);
        }

        [HttpGet("admin/category/field/add")]
        public async Task<IActionResult> AddCategoryField()
        {
            await SetParentCategoryListAsync(); // Mevcut metodun
            return View();
        }

        [HttpPost("admin/category/field/add")]
        public async Task<IActionResult> AddCategoryField(CategoryFieldsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await SetParentCategoryListAsync();
                return View(model);
            }

            // Aynı kategoride aynı alan adı var mı?
            var existingField = await _categoryFieldsRepository.FirstOrDefaultAsync(f =>
                f.CategoryId == model.CategoryId &&
                f.FieldName!.ToLower() == model.FieldName!.ToLower());

            if (existingField != null)
            {
                ModelState.AddModelError(nameof(model.FieldName), "Bu alan adı zaten bu kategoride mevcut.");
                await SetParentCategoryListAsync();
                return View(model);
            }

            var categoryField = _mapper.Map<CategoryFields>(model);
            categoryField.Created = DateTime.Now;
            categoryField.Updated = DateTime.Now;

            await _categoryFieldsRepository.AddAsync(categoryField);
            _notyf.Success("Harika! " + categoryField.FieldName + " başarıyla eklendi.");
            return RedirectToAction("CategoryFields");
        }

        [HttpGet("admin/category/field/edit/{id}")]
        public async Task<IActionResult> EditCategoryField(int id)
        {
            var categoryField = await _categoryFieldsRepository.GetByIdAsync(id);
            if (categoryField == null)
            {
                return NotFound();
            }

            await SetActiveCategoryListAsync(id); // Mevcut metodun
            var model = _mapper.Map<CategoryFieldsViewModel>(categoryField);
            return View(model);
        }

        [HttpPost("admin/category/field/edit/{id}")]
        public async Task<IActionResult> EditCategoryField(int id, CategoryFieldsViewModel model)
        {
            var categoryField = await _categoryFieldsRepository.GetByIdAsync(id);
            if (categoryField == null)
            {
                return RedirectToAction("CategoryFields");
            }

            if (!ModelState.IsValid)
            {
                await SetActiveCategoryListAsync(id);
                return View(model);
            }

            // Aynı kategoride aynı alan adı var mı? (Kendisi hariç kontrol - id != f.Id)
            var existingField = await _categoryFieldsRepository.FirstOrDefaultAsync(f =>
                f.Id != id &&
                f.CategoryId == model.CategoryId &&
                f.FieldName!.ToLower() == model.FieldName!.ToLower());

            if (existingField != null)
            {
                ModelState.AddModelError(nameof(model.FieldName), "Bu isimde bir alan zaten bu kategoride mevcut.");
                await SetActiveCategoryListAsync(id);
                return View(model);
            }

            categoryField.FieldName = model.FieldName;
            categoryField.CategoryId = model.CategoryId;
            categoryField.IsActive = model.IsActive;
            categoryField.Updated = DateTime.Now;

            await _categoryFieldsRepository.UpdateAsync(categoryField);
            _notyf.Success("Harika! " + categoryField.FieldName + " başarıyla güncellendi.");
            return RedirectToAction("CategoryFields");
        }

        [HttpGet("admin/category/field/delete/{id}")]
        public async Task<IActionResult> DeleteCategoryField(int id)
        {
            var categoryField = await _categoryFieldsRepository.GetByIdAsync(id);
            if (categoryField == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<CategoryFieldsViewModel>(categoryField);
            return View(model);
        }

        [HttpPost("admin/category/field/delete/{id}")]
        public async Task<IActionResult> DeleteCategoryField(int id, CategoryFieldsViewModel model)
        {
            var categoryField = await _categoryFieldsRepository.GetByIdAsync(id);
            if (categoryField == null)
            {
                return NotFound();
            }

            await _categoryFieldsRepository.DeleteAsync(id); // id parametresini kullanmak daha güvenlidir
            _notyf.Success("Harika! " + categoryField.FieldName + " başarıyla silindi.");
            return RedirectToAction("CategoryFields");
        }

        // --------------------------------------------------------------
        // OLANAK GRUPLARI YÖNETİMİ
        // --------------------------------------------------------------

        [HttpGet("admin/amenities/groups")]
        public async Task<IActionResult> AmenitiesGroups(string? search)
        {
            var groups = await _amenitiesGroupRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                groups = groups
                    .Where(g =>
                        (g.Name ?? "").ToLower().Contains(search) ||
                        (g.Description ?? "").ToLower().Contains(search))
                    .ToList();

                ViewData["Search"] = search;
            }

            // Automapper kullanarak ViewModel'e dönüştür
            var model = _mapper.Map<List<AmenitiesGroupViewModel>>(groups);
            ViewBag.Search = search;
            return View(model);
        }

        [HttpGet("admin/amenities/group/add")]
        public IActionResult AddAmenitiesGroup()
        {
            return View();
        }

        [HttpPost("admin/amenities/group/add")]
        public async Task<IActionResult> AddAmenitiesGroup(AmenitiesGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Aynı isimde grup kontrolü
            var existingGroup = await _amenitiesGroupRepository.FirstOrDefaultAsync(g => g.Name == model.Name);

            if (existingGroup != null)
            {
                ModelState.AddModelError("Name", "Bu grup adı zaten kullanılıyor.");
                return View(model);
            }

            var group = _mapper.Map<AmenitiesGroup>(model);
            group.Created = DateTime.Now;
            group.Updated = DateTime.Now;

            await _amenitiesGroupRepository.AddAsync(group);
            _notyf.Success($"Harika! {group.Name} başarıyla eklendi.");

            return RedirectToAction("AmenitiesGroups");
        }

        [HttpGet("admin/amenities/group/edit/{id}")]
        public async Task<IActionResult> EditAmenitiesGroup(int id)
        {
            var group = await _amenitiesGroupRepository.GetByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<AmenitiesGroupViewModel>(group);
            return View(model);
        }

        [HttpPost("admin/amenities/group/edit/{id}")]
        public async Task<IActionResult> EditAmenitiesGroup(int id, AmenitiesGroupViewModel model)
        {
            var group = await _amenitiesGroupRepository.GetByIdAsync(id);
            if (group == null)
            {
                return RedirectToAction("AmenitiesGroups"); // Yazım hatası düzeltildi: AmenitiesGroup -> AmenitiesGroups
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kendisi hariç aynı isimde başka grup var mı?
            var existingGroup = await _amenitiesGroupRepository.FirstOrDefaultAsync(g =>
                g.Name == model.Name && g.Id != id);

            if (existingGroup != null)
            {
                ModelState.AddModelError("Name", "Bu grup adı başka bir grupta zaten kullanılıyor.");
                return View(model);
            }

            group.Name = model.Name;
            group.Description = model.Description;
            group.ImageUrl = model.ImageUrl;
            group.IsActive = model.IsActive;
            group.Updated = DateTime.Now;

            await _amenitiesGroupRepository.UpdateAsync(group);
            _notyf.Success($"Harika! {group.Name} başarıyla güncellendi.");

            return RedirectToAction("AmenitiesGroups");
        }

        [HttpGet("admin/amenities/group/delete/{id}")]
        public async Task<IActionResult> DeleteAmenitiesGroup(int id)
        {
            var group = await _amenitiesGroupRepository.GetByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<AmenitiesGroupViewModel>(group);
            return View(model);
        }

        [HttpPost("admin/amenities/group/delete/{id}")]
        public async Task<IActionResult> DeleteAmenitiesGroup(int id, AmenitiesGroupViewModel model)
        {
            var group = await _amenitiesGroupRepository.GetByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            // İlişkilendirilmiş alt olanaklar (amenities) kontrolü
            var hasAmenities = await _amenitiesRepository.FirstOrDefaultAsync(a => a.AmenitiesGroupId == id);
            if (hasAmenities != null)
            {
                _notyf.Error("Bu grup, içinde kayıtlı olanaklar bulunduğu için silinemez. Önce içindeki olanakları silmelisiniz.");
                return RedirectToAction("AmenitiesGroups");
            }

            await _amenitiesGroupRepository.DeleteAsync(id);
            _notyf.Success($"Grup ({group.Name}) başarıyla silindi.");

            return RedirectToAction("AmenitiesGroups");
        }

        // --------------------------------------------------------------
        // OLANAK YÖNETİMİ (AMENITIES)
        // --------------------------------------------------------------

        [HttpGet("admin/amenities")]
        public async Task<IActionResult> Amenities(string? search)
        {
            var amenities = await _amenitiesRepository.GetAllAsync();
            var groups = await _amenitiesGroupRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                amenities = amenities
                    .Where(a =>
                        (a.Name ?? "").ToLower().Contains(search) ||
                        (a.Description ?? "").ToLower().Contains(search) ||
                        (groups.FirstOrDefault(g => g.Id == a.AmenitiesGroupId)?.Name ?? "").ToLower().Contains(search)
                    )
                    .ToList();
            }

            var model = amenities.Select(a => new AmenitiesViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                AmenitiesGroupId = a.AmenitiesGroupId,
                GroupName = groups.FirstOrDefault(g => g.Id == a.AmenitiesGroupId)?.Name ?? "-",
                IsActive = a.IsActive,
                Created = a.Created
            }).ToList();

            ViewBag.Search = search;
            return View(model);
        }

        [HttpGet("admin/amenities/add")]
        public async Task<IActionResult> AddAmenities()
        {
            await PopulateGroupsViewBagAsync();
            return View();
        }

        [HttpPost("admin/amenities/add")]
        public async Task<IActionResult> AddAmenities(AmenitiesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateGroupsViewBagAsync();
                return View(model);
            }

            // Aynı grupta aynı isimli olanak kontrolü
            var existingAmenities = await _amenitiesRepository.FirstOrDefaultAsync(a =>
                a.Name.ToLower() == model.Name.ToLower() &&
                a.AmenitiesGroupId == model.AmenitiesGroupId);

            if (existingAmenities != null)
            {
                ModelState.AddModelError("Name", "Bu olanak adı seçilen grupta zaten mevcut.");
                await PopulateGroupsViewBagAsync();
                return View(model);
            }

            var amenities = _mapper.Map<Amenities>(model);
            amenities.Created = DateTime.Now;
            amenities.Updated = DateTime.Now;

            await _amenitiesRepository.AddAsync(amenities);
            _notyf.Success($"{amenities.Name} başarıyla eklendi.");

            return RedirectToAction("Amenities");
        }

        [HttpGet("admin/amenities/edit/{id}")]
        public async Task<IActionResult> EditAmenities(int id)
        {
            var amenities = await _amenitiesRepository.GetByIdAsync(id);
            if (amenities == null)
            {
                return NotFound();
            }

            await PopulateGroupsViewBagAsync();
            var model = _mapper.Map<AmenitiesViewModel>(amenities);
            return View(model);
        }

        [HttpPost("admin/amenities/edit/{id}")]
        public async Task<IActionResult> EditAmenities(int id, AmenitiesViewModel model)
        {
            var amenities = await _amenitiesRepository.GetByIdAsync(id);
            if (amenities == null)
            {
                return RedirectToAction("Amenities");
            }

            if (!ModelState.IsValid)
            {
                await PopulateGroupsViewBagAsync();
                return View(model);
            }

            // Kendisi hariç aynı grupta aynı isimde olanak var mı?
            var existingAmenities = await _amenitiesRepository.FirstOrDefaultAsync(a =>
                a.Name.ToLower() == model.Name.ToLower() &&
                a.AmenitiesGroupId == model.AmenitiesGroupId &&
                a.Id != id);

            if (existingAmenities != null)
            {
                ModelState.AddModelError("Name", "Bu olanak adı bu grupta zaten kullanılıyor.");
                await PopulateGroupsViewBagAsync();
                return View(model);
            }

            amenities.Name = model.Name;
            amenities.Description = model.Description;
            amenities.ImageUrl = model.ImageUrl;
            amenities.AmenitiesGroupId = model.AmenitiesGroupId;
            amenities.IsActive = model.IsActive;
            amenities.Updated = DateTime.Now;

            await _amenitiesRepository.UpdateAsync(amenities);
            _notyf.Success($"{amenities.Name} başarıyla güncellendi.");

            return RedirectToAction("Amenities");
        }

        [HttpGet("admin/amenities/delete/{id}")]
        public async Task<IActionResult> DeleteAmenities(int id)
        {
            var amenities = await _amenitiesRepository.GetByIdAsync(id);
            if (amenities == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<AmenitiesViewModel>(amenities);
            return View(model);
        }

        [HttpPost("admin/amenities/delete/{id}")]
        public async Task<IActionResult> DeleteAmenities(int id, AmenitiesViewModel model)
        {
            var amenities = await _amenitiesRepository.GetByIdAsync(id);
            if (amenities == null)
            {
                return NotFound();
            }

            // NOT: İlerleyen aşamalarda bu olanağın herhangi bir ilanda (Property) 
            // kullanılıp kullanılmadığını kontrol etmek iyi bir pratik olabilir.

            await _amenitiesRepository.DeleteAsync(id);
            _notyf.Success($"{amenities.Name} başarıyla silindi.");

            return RedirectToAction("Amenities");
        }
        // --------------------------------------------------------------
        // MESAJ YÖNETİMİ
        // --------------------------------------------------------------
        [HttpGet("admin/property/messages")]
        public IActionResult Messages()
        {
            return View();
        }

        [HttpGet("admin/property/messages/list")]
        public IActionResult SearchMessage(string? search)
        {
            // 1. ADIM: Mesajı olan ilanları sorgula
            var query = _propertyRepository.Where(x => x.Messages.Any(m => m.Deleted == null));

            // 2. ADIM: Arama Filtresi
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Slug.Contains(search) ||
                    x.Title.Contains(search)
                );
            }

            // 3. ADIM: Veriyi Şekillendirme (AppUser.FullName'e dikkat)
            var results = query
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Slug,
                    UserFullName = x.User != null ? x.User.FullName : "Sistem", // İlan sahibinin adı

                    // Son silinmemiş mesajı getirir
                    LastMessage = x.Messages
                        .Where(m => m.Deleted == null)
                        .OrderByDescending(m => m.Created)
                        .Select(m => new { m.Content, m.Created })
                        .FirstOrDefault(),

                    TotalMessages = x.Messages.Count(m => m.Deleted == null)
                })
                .ToList();

            // 4. ADIM: En son mesaj gelene göre sıralama
            results = results
                .OrderByDescending(x => x.LastMessage?.Created)
                .ToList();

            if (results.Count == 0)
            {
                return Json(new { success = false, message = "Mesajı olan herhangi bir ilan bulunamadı." });
            }

            return Json(new { success = true, data = results });
        }

        [HttpPost("admin/property/message/add")]
        public async Task<IActionResult> AddMessage(CreateMessageViewModel model)
        {
            if (string.IsNullOrEmpty(model.Slug) || string.IsNullOrEmpty(model.Content))
                return Json(new { status = false, message = "İlan kodu ve mesaj boş olamaz." });

            var property = await _propertyRepository.GetPropertyBySlugAsync(model.Slug);
            if (property == null) return Json(new { status = false, message = "İlan bulunamadı." });

            // Mesaj oluştur (UserId string olarak GetUserId()'den gelir)
            var message = new Messages
            {
                PropertyId = property.Id,
                Content = model.Content,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = GetUserId(), // Identity'den gelen string ID
                IsRead = false
            };

            await _messageRepository.AddAsync(message);
            return Json(new { status = true, message = "Başarıyla yorum eklendi." });
        }

        [HttpGet("admin/property/get-messages/{propertyId}")]
        public async Task<IActionResult> GetPropertyMessages(int propertyId)
        {
            try
            {
                // Mesajları çek ve AppUser tablosuyla (User) birleştir
                var rawMessages = await _messageRepository.Where(x => x.PropertyId == propertyId)
                    .Include(x => x.User) // AppUser bilgilerini getirir
                    .Include(x => x.RepliedMessage)
                    .OrderByDescending(x => x.Created)
                    .ToListAsync();

                var result = rawMessages.Select(x => new
                {
                    x.Id,
                    UserFullName = x.User != null ? x.User.FullName : "Misafir", // AppUser.FullName kullanılıyor
                    x.Content,
                    Created = x.Created.ToString("dd.MM.yyyy HH:mm"),

                    // Hiyerarşi
                    RepliedMessageId = x.RepliedMessageId,
                    IsReply = x.RepliedMessageId != null,
                    RepliedContent = x.RepliedMessage != null ? x.RepliedMessage.Content : null,

                    // Soft Delete Durumu
                    Deleted = x.Deleted
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sunucu Hatası: " + ex.Message });
            }
        }

        [HttpPost("admin/property/message/reply")]
        public async Task<IActionResult> ReplyMessage(int propertyId, int repliedMessageId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Cevap içeriği boş olamaz." });
            }

            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null) return Json(new { success = false, message = "İlan bulunamadı." });

            var message = new Messages
            {
                PropertyId = propertyId,
                RepliedMessageId = repliedMessageId,
                Content = content,
                Created = DateTime.Now,
                UserId = GetUserId(), // Identity string ID
                IsRead = true
            };

            await _messageRepository.AddAsync(message);
            return Json(new { success = true, message = "Cevabınız gönderildi." });
        }

        [HttpPost("admin/property/message/update")]
        public async Task<IActionResult> UpdateMessage(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Mesaj içeriği boş olamaz." });
            }

            var message = await _messageRepository.GetByIdAsync(id);
            if (message == null) return Json(new { success = false, message = "Mesaj bulunamadı." });

            message.Content = content;
            message.Updated = DateTime.Now;

            await _messageRepository.UpdateAsync(message);
            return Json(new { success = true, message = "Mesaj güncellendi." });
        }

        [HttpPost("admin/property/message/delete/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _messageRepository.GetByIdAsync(id);
            if (message == null) return Json(new { success = false, message = "Mesaj bulunamadı." });

            // SOFT DELETE: Veritabanından silmiyor, tarih atıyoruz
            message.Deleted = DateTime.Now;
            message.Updated = DateTime.Now;

            await _messageRepository.UpdateAsync(message);
            return Json(new { success = true, message = "Mesaj başarıyla silindi." });
        }

        // --------------------------------------------------------------
        // KULLANICI YÖNETİMİ (IDENTITY)
        // --------------------------------------------------------------

        [HttpGet("admin/users")]
        public async Task<IActionResult> Users(string? search)
        {
            // Identity'den tüm kullanıcıları IQueryable olarak alıyoruz
            var query = _userManager.Users;

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    u.UserName.ToLower().Contains(search) ||
                    u.FullName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.PhoneNumber.Contains(search));

                ViewData["Search"] = search;
            }

            var users = await query.ToListAsync();
            var model = _mapper.Map<List<UserViewModel>>(users);

            ViewBag.Search = search;
            return View(model);
        }

        [HttpGet("admin/user/add")]
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost("admin/user/add")]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Identity'de kullanıcı adı ve email kontrolü CreateAsync içinde otomatik yapılır 
            // ama özel hata mesajı döndürmek istersen manuel kontrolü koruyabiliriz:
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                ModelState.AddModelError("Email", "Bu email adresi zaten bir kullanıcı tarafından kullanılıyor.");
                return View(model);
            }

            var user = new AppUser
            {
                FullName = model.FullName,
                UserName = model.UserName, // Identity için UserName benzersiz olmalı
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsActive = true,
                Created = DateTime.Now,
                Updated = DateTime.Now
            };

            // Identity ile kullanıcı oluşturma (Şifre burada otomatik hash'lenir)
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // İsteğe bağlı: Kullanıcıya varsayılan bir rol atama
                // await _userManager.AddToRoleAsync(user, "User");

                _notyf.Success($"Harika! {user.UserName} kullanıcısı başarıyla eklendi.");
                return RedirectToAction("Users");
            }

            // Eğer Identity tarafında bir hata oluşursa (şifre politikası vb.)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet("admin/user/edit/{id}")]
        public async Task<IActionResult> EditUser(string id) // int -> string yapıldı
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<UserViewModel>(user);
            return View(model);
        }

        [HttpPost("admin/user/edit/{id}")]
        public async Task<IActionResult> EditUser(string id, UserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return RedirectToAction("Users");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Email veya UserName değişikliği varsa çakışma kontrolü
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null && existingUser.Id != id)
            {
                ModelState.AddModelError("Email", "Bu email adresi başka bir kullanıcıya ait.");
                return View(model);
            }

            // Alanları güncelle
            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.Bio = model.Bio;
            user.ProfileImageUrl = model.ProfileImageUrl;
            user.IsActive = model.IsActive;
            user.Updated = DateTime.Now;

            // Identity üzerinden güncelle
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _notyf.Success($"Kullanıcı ({user.UserName}) başarıyla güncellendi.");
                return RedirectToAction("Users");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet("admin/user/delete/{id}")]
        public async Task<IActionResult> DeleteUser(string id) // int -> string yapıldı
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<UserViewModel>(user);
            return View(model);
        }

        [HttpPost("admin/user/delete/{id}")]
        public async Task<IActionResult> DeleteUser(string id, UserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Identity üzerinden sil
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                _notyf.Success($"{user.UserName} kullanıcısı başarıyla silindi.");
                return RedirectToAction("Users");
            }

            _notyf.Error("Kullanıcı silinirken bir hata oluştu.");
            return RedirectToAction("Users");
        }

        // --------------------------------------------------------------
        // AYARLAR YÖNETİMİ
        // --------------------------------------------------------------

        [HttpGet("admin/settings")]
        public async Task<IActionResult> Settings()
        {
            var setting = await _settingRepository.GetByIdAsync(1);

            if (setting == null)
            {
                _notyf.Warning("Sistem ayarları veritabanında bulunamadı.");
                return NotFound();
            }

            var model = _mapper.Map<SettingViewModel>(setting);
            return View(model);
        }

        [HttpPost("admin/settings")]
        [ValidateAntiForgeryToken] // Güvenlik için eklendi
        public async Task<IActionResult> Settings(SettingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var setting = await _settingRepository.GetByIdAsync(1);
            if (setting == null)
            {
                return NotFound();
            }

            setting.SiteTitle = model.SiteTitle;
            setting.SiteDescription = model.SiteDescription;
            setting.SiteKeywords = model.SiteKeywords;
            setting.LogoUrl = model.LogoUrl;
            setting.FaviconUrl = model.FaviconUrl;
            setting.SitePhoneNumber = model.SitePhoneNumber;
            setting.SiteEmail = model.SiteEmail;
            setting.SiteAddress = model.SiteAddress;
            setting.MapEmbedCode = model.MapEmbedCode;
            setting.InstagramUrl = model.InstagramUrl;
            setting.FacebookUrl = model.FacebookUrl;
            setting.TwitterUrl = model.TwitterUrl;
            setting.YoutubeUrl = model.YoutubeUrl;
            setting.Updated = DateTime.Now;

            await _settingRepository.UpdateAsync(setting);

            _notyf.Success("Site ayarları başarıyla güncellendi!");

            return View(model);
        }

        // --------------------------------------------------------------
        // SERVİSLER
        // --------------------------------------------------------------
        private async Task SetParentCategoryListAsync(int? excludeCategoryId = null)
        {
            var categories = await _categoryRepository.GetAllAsync();

            // Sadece aktif kategorileri al
            var activeCategories = categories
                .Where(c => c.IsActive)
                .Where(c => excludeCategoryId == null || c.Id != excludeCategoryId) // Kendini hariç tut
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
                .ToList();

            ViewBag.Categories = activeCategories;
        }

        private async Task SetActiveCategoryListAsync(int? selectedCategoryId = null)
        {
            var categories = await _categoryRepository.GetAllAsync();

            // Sadece aktif olan kategorileri al
            var activeCategories = categories
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToList();

            ViewBag.Categories = new SelectList(
                activeCategories,
                "Id",
                "Name",
                selectedCategoryId // seçili kategori
            );
        }

        private async Task PopulateGroupsViewBagAsync()
        {
            var groups = await _amenitiesGroupRepository.GetAllAsync();
            ViewBag.Groups = groups.Where(g => g.IsActive).Select(g => new SelectListItem
            {
                Text = g.Name,
                Value = g.Id.ToString()
            }).ToList();
        }

        private static string Slugify()
        {
            // Rastgele string oluşturma
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var randomString = new char[12];
            for (int i = 0; i < randomString.Length; i++)
            {
                randomString[i] = chars[random.Next(chars.Length)];
            }

            // Bugünün tarihi
            string datePart = DateTime.Now.ToString("yyyyMMdd");

            // Birleştir
            return new string(randomString) + "_" + datePart;
        }
    }
}
