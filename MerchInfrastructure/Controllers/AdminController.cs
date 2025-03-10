using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerchDomain.Model;
using MerchInfrastructure;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace MerchInfrastructure.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly MerchShopeContext _context;

        public AdminController(MerchShopeContext context)
        {
            _context = context;
        }

        // Список товарів
        public async Task<IActionResult> Merchandises()
        {
            var merchandises = await _context.Merchandises
                .Include(m => m.Category)
                .Include(m => m.Brand)
                .Include(m => m.Team)
                .Include(m => m.Size)
                .ToListAsync();
            return View(merchandises);
        }

        // Додавання товару (GET)
        [HttpGet]
        public IActionResult CreateMerchandise()
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = categories;
            Debug.WriteLine($"Categories count: {categories?.Count ?? 0}");

            var brands = _context.Brands.ToList();
            ViewBag.Brands = brands;
            Debug.WriteLine($"Brands count: {brands?.Count ?? 0}");

            var teams = _context.Teams.ToList();
            ViewBag.Teams = teams;
            Debug.WriteLine($"Teams count: {teams?.Count ?? 0}");

            var sizes = _context.Sizes.ToList();
            ViewBag.Sizes = sizes;
            Debug.WriteLine($"Sizes count: {sizes?.Count ?? 0}");

            return View();
        }

        // Додавання товару (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMerchandise(Merchandise merchandise, string imageUrl)
        {
            // Виведення всіх властивостей для дебагу
            Debug.WriteLine("Received Merchandise Properties:");
            var props = merchandise.GetType().GetProperties();
            foreach (var prop in props)
            {
                var value = prop.GetValue(merchandise) ?? "null";
                Debug.WriteLine($"{prop.Name}: {value}");
            }
            Debug.WriteLine($"Name: {merchandise.Name}, Price: {merchandise.Price}, ImageUrl: {imageUrl}");

            // Обробка зображення з URL
            if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetAsync(imageUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            var imageBytes = await response.Content.ReadAsByteArrayAsync();
                            var uri = new Uri(imageUrl);
                            var fileName = Path.GetFileName(uri.AbsolutePath);
                            var fileExtension = Path.GetExtension(fileName);
                            if (string.IsNullOrEmpty(fileExtension))
                            {
                                fileExtension = ".jpg"; // За замовчуванням
                            }
                            var newFileName = Guid.NewGuid().ToString() + fileExtension;
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", newFileName);

                            // Перевірка та створення папки
                            var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                                Debug.WriteLine($"Created directory: {directory}");
                            }

                            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                            merchandise.ImageUrl = "/images/" + newFileName;
                            Debug.WriteLine($"Image downloaded and saved to: {filePath}");
                        }
                        else
                        {
                            ModelState.AddModelError("imageUrl", "Не вдалося завантажити зображення з URL.");
                            Debug.WriteLine($"Failed to download image from URL: {imageUrl}, Status: {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("imageUrl", $"Помилка завантаження зображення: {ex.Message}");
                    Debug.WriteLine($"Error downloading image: {ex.Message}");
                }
            }

            // Видаляємо "UserCarts" із ModelState, якщо воно є
            if (ModelState.ContainsKey("UserCarts"))
            {
                ModelState.Remove("UserCarts");
                Debug.WriteLine("Removed UserCarts from ModelState.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Merchandises.Add(merchandise);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("Merchandise added successfully.");
                    return RedirectToAction("Merchandises");
                }
                catch (DbUpdateException ex)
                {
                    Debug.WriteLine($"DbUpdateException: {ex.Message}");
                    Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message ?? "No inner exception"}");
                    ModelState.AddModelError("", "Не вдалося зберегти товар у базі даних. Перевірте введені дані.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unexpected error: {ex.Message}");
                    ModelState.AddModelError("", "Сталася неочікувана помилка при збереженні товару.");
                }
            }

            Debug.WriteLine("ModelState is invalid:");
            foreach (var error in ModelState)
            {
                Debug.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Teams = _context.Teams.ToList();
            ViewBag.Sizes = _context.Sizes.ToList();
            return View(merchandise);
        }

        // Редагування товару (GET)
        [HttpGet]
        public async Task<IActionResult> EditMerchandise(int id)
        {
            var merchandise = await _context.Merchandises.FindAsync(id);
            if (merchandise == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Teams = _context.Teams.ToList();
            ViewBag.Sizes = _context.Sizes.ToList();
            return View(merchandise);
        }

        // Редагування товару (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMerchandise(Merchandise merchandise, string imageUrl)
        {
            Debug.WriteLine("Received Merchandise Properties:");
            var props = merchandise.GetType().GetProperties();
            foreach (var prop in props)
            {
                var value = prop.GetValue(merchandise) ?? "null";
                Debug.WriteLine($"{prop.Name}: {value}");
            }
            Debug.WriteLine($"Name: {merchandise.Name}, Price: {merchandise.Price}, ImageUrl: {imageUrl}");

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            var response = await httpClient.GetAsync(imageUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                                var uri = new Uri(imageUrl);
                                var fileName = Path.GetFileName(uri.AbsolutePath);
                                var fileExtension = Path.GetExtension(fileName);
                                if (string.IsNullOrEmpty(fileExtension))
                                {
                                    fileExtension = ".jpg";
                                }
                                var newFileName = Guid.NewGuid().ToString() + fileExtension;
                                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", newFileName);

                                // Видаляємо старе зображення
                                if (!string.IsNullOrEmpty(merchandise.ImageUrl))
                                {
                                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", merchandise.ImageUrl.TrimStart('/'));
                                    if (System.IO.File.Exists(oldImagePath))
                                    {
                                        System.IO.File.Delete(oldImagePath);
                                    }
                                }

                                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                                merchandise.ImageUrl = "/images/" + newFileName;
                                Debug.WriteLine($"Image downloaded and saved to: {filePath}");
                            }
                            else
                            {
                                ModelState.AddModelError("imageUrl", "Не вдалося завантажити зображення з URL.");
                                Debug.WriteLine($"Failed to download image from URL: {imageUrl}, Status: {response.StatusCode}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("imageUrl", $"Помилка завантаження зображення: {ex.Message}");
                        Debug.WriteLine($"Error downloading image: {ex.Message}");
                    }
                }

                // Видаляємо "UserCarts" із ModelState
                if (ModelState.ContainsKey("UserCarts"))
                {
                    ModelState.Remove("UserCarts");
                    Debug.WriteLine("Removed UserCarts from ModelState.");
                }

                if (ModelState.IsValid)
                {
                    _context.Update(merchandise);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("Merchandise updated successfully.");
                    return RedirectToAction("Merchandises");
                }
            }

            Debug.WriteLine("ModelState is invalid:");
            foreach (var error in ModelState)
            {
                Debug.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Teams = _context.Teams.ToList();
            ViewBag.Sizes = _context.Sizes.ToList();
            return View(merchandise);
        }

        // Решта методів без змін
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMerchandise(int id)
        {
            var merchandise = await _context.Merchandises.FindAsync(id);
            if (merchandise != null)
            {
                _context.Merchandises.Remove(merchandise);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Merchandises");
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.MerchOrders
                .Include(o => o.Buyer)
                .Include(o => o.Status)
                .Include(o => o.Payment)
                .Include(o => o.Shipment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Merch)
                .ToListAsync();
            ViewBag.Statuses = await _context.OrderStatuses.ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, int statusId)
        {
            var order = await _context.MerchOrders.FindAsync(orderId);
            if (order != null)
            {
                order.StatusId = statusId;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Orders");
        }
    }
}