using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerchDomain.Model;
using MerchInfrastructure;
using Microsoft.AspNetCore.Mvc.Rendering; // Додано для SelectList
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
namespace MerchInfrastructure.Controllers
{
    public class MerchandisesController : Controller
    {
        private readonly MerchShopeContext _context;
        private readonly UserManager<User> _userManager;
        public MerchandisesController(MerchShopeContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        private async Task<int> GetCartCountAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                return await _context.UserCarts
                    .Where(c => c.UserId == userId)
                    .SumAsync(c => c.Quantity);
            }
            else
            {
                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                return cart.Sum(c => c.Quantity);
            }
        }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ViewBag.CartCount = await GetCartCountAsync();
            await base.OnActionExecutionAsync(context, next);
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(int merchandiseId, int quantity = 1)
        {
            var merchandise = await _context.Merchandises.FindAsync(merchandiseId);
            if (merchandise == null) return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var cartItem = await _context.UserCarts
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.MerchandiseId == merchandiseId);

                if (cartItem == null)
                {
                    cartItem = new UserCart
                    {
                        UserId = userId,
                        MerchandiseId = merchandiseId,
                        Quantity = quantity
                    };
                    _context.UserCarts.Add(cartItem);
                }
                else
                {
                    cartItem.Quantity += quantity;
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                var cartItem = cart.FirstOrDefault(c => c.MerchandiseId == merchandiseId);

                if (cartItem == null)
                {
                    cart.Add(new CartItem { MerchandiseId = merchandiseId, Quantity = quantity });
                }
                else
                {
                    cartItem.Quantity += quantity;
                }
                HttpContext.Session.SetObject("Cart", cart);
            }

            return RedirectToAction("Index");
        }

        // Перегляд кошика
        public async Task<IActionResult> Cart()
        {
            List<CartViewModel> cartItems;

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                cartItems = await _context.UserCarts
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Merchandise)
                    .Select(c => new CartViewModel
                    {
                        Merchandise = c.Merchandise,
                        Quantity = c.Quantity
                    })
                    .ToListAsync();
            }
            else
            {
                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                var merchandises = await _context.Merchandises
                    .Where(m => cart.Select(c => c.MerchandiseId).Contains(m.Id))
                    .ToListAsync();

                cartItems = merchandises
                    .Select(m => new CartViewModel
                    {
                        Merchandise = m,
                        Quantity = cart.First(c => c.MerchandiseId == m.Id).Quantity
                    })
                    .ToList();
            }

            // Залишаємо TotalPrice як decimal
            ViewBag.TotalPrice = cartItems.Sum(i => i.Merchandise.Price * i.Quantity);
            return View(cartItems);
        }

        // Видалення товару з кошика
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int merchandiseId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var cartItem = await _context.UserCarts
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.MerchandiseId == merchandiseId);
                if (cartItem != null)
                {
                    _context.UserCarts.Remove(cartItem);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                var cartItem = cart.FirstOrDefault(c => c.MerchandiseId == merchandiseId);
                if (cartItem != null)
                {
                    cart.Remove(cartItem);
                    HttpContext.Session.SetObject("Cart", cart);
                }
            }
            return RedirectToAction("Cart");
        }

        // Оновлення кількості товару в кошику
        [HttpPost]
        public async Task<IActionResult> UpdateCart(int merchandiseId, int quantity)
        {
            if (quantity < 1) return RedirectToAction("Cart");

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var cartItem = await _context.UserCarts
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.MerchandiseId == merchandiseId);
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                var cartItem = cart.FirstOrDefault(c => c.MerchandiseId == merchandiseId);
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                    HttpContext.Session.SetObject("Cart", cart);
                }
            }
            return RedirectToAction("Cart");
        }

        // GET: Merchandises
        public async Task<IActionResult> Index(string searchString, string category, string team, string brand, string size, int page = 1, int pageSize = 10, string sortBy = "name")
        {
            var merchQuery = _context.Merchandises
                .Include(m => m.Category)
                .Include(m => m.Team)
                .Include(m => m.Brand)
                .Include(m => m.Size)
                .AsQueryable();

            // Пошук за назвою
            if (!string.IsNullOrEmpty(searchString))
            {
                merchQuery = merchQuery.Where(m => m.Name.Contains(searchString));
            }

            // Фільтрація
            if (!string.IsNullOrEmpty(category))
                merchQuery = merchQuery.Where(m => m.Category != null && m.Category.CategoryName == category);
            if (!string.IsNullOrEmpty(team))
                merchQuery = merchQuery.Where(m => m.Team != null && m.Team.TeamName == team);
            if (!string.IsNullOrEmpty(brand))
                merchQuery = merchQuery.Where(m => m.Brand != null && m.Brand.BrandName == brand);
            if (!string.IsNullOrEmpty(size))
                merchQuery = merchQuery.Where(m => m.Size != null && m.Size.SizeName == size);

            // Сортування
            merchQuery = sortBy switch
            {
                "name" => merchQuery.OrderBy(m => m.Name),
                "price" => merchQuery.OrderBy(m => m.Price),
                "price_desc" => merchQuery.OrderByDescending(m => m.Price),
                "category" => merchQuery.OrderBy(m => m.Category != null ? m.Category.CategoryName : ""),
                "category_desc" => merchQuery.OrderByDescending(m => m.Category != null ? m.Category.CategoryName : ""),
                _ => merchQuery.OrderBy(m => m.Name)
            };

            // Пагінація
            int totalItems = await merchQuery.CountAsync();
            var pagedItems = await merchQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Передача даних у ViewBag
            ViewBag.Categories = await _context.Categories.Select(c => c.CategoryName).Distinct().ToListAsync();
            ViewBag.Teams = await _context.Teams.Select(t => t.TeamName).Distinct().ToListAsync();
            ViewBag.Brands = await _context.Brands.Select(b => b.BrandName).Distinct().ToListAsync();
            ViewBag.Sizes = await _context.Sizes.Select(s => s.SizeName).Distinct().ToListAsync();
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SortBy = sortBy;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedTeam = team;
            ViewBag.SelectedBrand = brand;
            ViewBag.SelectedSize = size;
            ViewBag.SearchString = searchString;

            return View(pagedItems);
        }

        // GET: Merchandises/Details/5
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var merchandise = await _context.Merchandises
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.Team)
                .Include(m => m.Size)
                .Include(m => m.Reviews)
                    .ThenInclude(r => r.Buyer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchandise == null) return NotFound();

            var model = new MerchandiseDetailsViewModel
            {
                Id = merchandise.Id,
                Name = merchandise.Name,
                Price = merchandise.Price,
                BrandName = merchandise.Brand?.BrandName,
                CategoryName = merchandise.Category?.CategoryName,
                TeamName = merchandise.Team?.TeamName,
                SizeName = merchandise.Size?.SizeName,
                ImageUrl = merchandise.ImageUrl, // Додано ImageUrl
                Reviews = merchandise.Reviews.Select(r => new ReviewViewModel
                {
                    BuyerName = r.Buyer.Username,
                    Rating = r.Rating ?? 0,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate ?? DateTime.Now
                }).ToList()
            };

            return View(model);
        }

        // Додавання відгуку
        [HttpPost]
        public async Task<IActionResult> AddReview(int merchandiseId, int rating, string comment, int buyerId)
        {
            var merchandise = await _context.Merchandises.FindAsync(merchandiseId);
            var buyer = await _context.Buyers.FindAsync(buyerId);

            if (merchandise == null || buyer == null)
            {
                return NotFound();
            }

            var review = new Review
            {
                MerchandiseId = merchandiseId,
                BuyerId = buyerId,
                Rating = rating,
                Comment = comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = merchandiseId });
        }

        // GET: Merchandises/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Merchandises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamId,CategoryId,BrandId,SizeId,Name,Price,Id")] Merchandise merchandise)
        {
            if (ModelState.IsValid)
            {
                _context.Add(merchandise);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(merchandise);
            return View(merchandise);
        }

        // GET: Merchandises/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var merchandise = await _context.Merchandises.FindAsync(id);
            if (merchandise == null) return NotFound();

            PopulateDropdowns(merchandise);
            return View(merchandise);
        }

        // POST: Merchandises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamId,CategoryId,BrandId,SizeId,Name,Price,Id")] Merchandise merchandise)
        {
            if (id != merchandise.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(merchandise);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MerchandiseExists(merchandise.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(merchandise);
            return View(merchandise);
        }

        // GET: Merchandises/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var merchandise = await _context.Merchandises
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.Size)
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (merchandise == null) return NotFound();

            return View(merchandise);
        }

        // POST: Merchandises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var merchandise = await _context.Merchandises.FindAsync(id);
            if (merchandise != null) _context.Merchandises.Remove(merchandise);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Merchandises/Search
        public async Task<IActionResult> Search(string searchString)
        {
            var query = _context.Merchandises.Include(m => m.Brand).Include(m => m.Category).Include(m => m.Size).Include(m => m.Team).AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(m => m.Name.Contains(searchString));
            }
            return View("Index", await query.ToListAsync());
        }

        // GET: Merchandises/FilterByCategory
        public async Task<IActionResult> FilterByCategory(int categoryId)
        {
            var filteredMerch = _context.Merchandises
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.Size)
                .Include(m => m.Team)
                .Where(m => m.CategoryId == categoryId);
            return View("Index", await filteredMerch.ToListAsync());
        }

        // GET: Merchandises/GetAllJson
        [HttpGet]
        public async Task<IActionResult> GetAllJson()
        {
            var merchList = await _context.Merchandises.ToListAsync();
            return Json(merchList);
        }

        private bool MerchandiseExists(int id)
        {
            return _context.Merchandises.Any(e => e.Id == id);
        }

        private void PopulateDropdowns(Merchandise merchandise = null)
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName", merchandise?.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", merchandise?.CategoryId);
            ViewData["SizeId"] = new SelectList(_context.Sizes, "Id", "SizeName", merchandise?.SizeId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "TeamName", merchandise?.TeamId);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout") });
            }

            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.UserCarts
                .Where(c => c.UserId == userId)
                .Include(c => c.Merchandise)
                .Select(c => new CartViewModel
                {
                    Merchandise = c.Merchandise,
                    Quantity = c.Quantity
                })
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Cart");
            }

            ViewBag.TotalPrice = cartItems.Sum(i => i.Merchandise.Price * i.Quantity);
            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string shipmentType, string paymentType)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.UserCarts
                .Where(c => c.UserId == userId)
                .Include(c => c.Merchandise)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Cart");
            }

            if (string.IsNullOrEmpty(shipmentType) || string.IsNullOrEmpty(paymentType))
            {
                ModelState.AddModelError("", "Виберіть тип оплати та доставки.");
                return View(await GetCartItemsAsync(userId));
            }

            var payment = new Payment { TypePayment = paymentType };
            _context.Payments.Add(payment);

            // Використовуємо існуючий тип доставки або додаємо, якщо його немає
            var shipment = await _context.Shipments.FirstAsync(s => s.TypeShipment == shipmentType);
            if (shipment == null)
            {
                shipment = new Shipment { TypeShipment = shipmentType };
                _context.Shipments.Add(shipment);
                await _context.SaveChangesAsync(); // Зберігаємо shipment окремо
            }

            var status = await _context.OrderStatuses.FirstAsync(s => s.StatusName == "Нове");

            var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.Username == User.Identity.Name);
            if (buyer == null)
            {
                buyer = new Buyer { Username = User.Identity.Name };
                _context.Buyers.Add(buyer);
            }

            await _context.SaveChangesAsync(); // Зберігаємо payment, buyer

            var order = new MerchOrder
            {
                BuyerId = buyer.Id,
                PaymentId = payment.Id,
                ShipmentId = shipment.Id,
                StatusId = status.Id,
                OrderDate = DateTime.Now
            };
            _context.MerchOrders.Add(order);

            var orderItems = cartItems.Select(c => new OrderItem
            {
                Order = order,
                MerchId = c.MerchandiseId,
                Quantity = c.Quantity
            }).ToList();
            _context.OrderItems.AddRange(orderItems);

            _context.UserCarts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        // Допоміжний метод для повернення cartItems у разі помилки
        private async Task<List<CartViewModel>> GetCartItemsAsync(string userId)
        {
            return await _context.UserCarts
                .Where(c => c.UserId == userId)
                .Include(c => c.Merchandise)
                .Select(c => new CartViewModel
                {
                    Merchandise = c.Merchandise,
                    Quantity = c.Quantity
                })
                .ToListAsync();
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Orders()
        {
            var userId = _userManager.GetUserId(User);
            var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.Username == User.Identity.Name);
            if (buyer == null)
            {
                return View(new List<MerchOrder>());
            }

            var orders = await _context.MerchOrders
                .Where(o => o.BuyerId == buyer.Id)
                .Include(o => o.Status)
                .Include(o => o.Payment)
                .Include(o => o.Shipment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Merch)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    StatusName = o.Status.StatusName,
                    PaymentType = o.Payment.TypePayment,
                    ShipmentType = o.Shipment.TypeShipment,
                    TotalAmount = o.OrderItems.Sum(oi => oi.Merch.Price * oi.Quantity)
                })
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _context.MerchOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Merch)
                .Include(o => o.Payment)
                .Include(o => o.Shipment)
                .Include(o => o.Status)
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.Buyer.Username != User.Identity.Name)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}