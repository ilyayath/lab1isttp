using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerchDomain.Model;
using MerchInfrastructure;

namespace MerchInfrastructure.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly MerchShopeContext _context;
        private readonly UserManager<User> _userManager;
        public ReviewsController(MerchShopeContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int merchId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Оцінка має бути від 1 до 5.";
                return RedirectToAction("Details", "Merchandises", new { id = merchId });
            }

            var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.Username == User.Identity.Name);
            if (buyer == null)
            {
                TempData["Error"] = "Покупець не знайдений.";
                return RedirectToAction("Details", "Merchandises", new { id = merchId });
            }

            // Перевірка, чи користувач купував цей товар (опціонально)
            var hasPurchased = await _context.OrderItems
                .AnyAsync(oi => oi.MerchId == merchId && oi.Order.BuyerId == buyer.Id);
            if (!hasPurchased)
            {
                TempData["Error"] = "Ви можете залишити відгук лише на товари, які придбали.";
                return RedirectToAction("Details", "Merchandises", new { id = merchId });
            }

            var review = new Review
            {
                MerchandiseId = merchId,
                BuyerId = buyer.Id,
                Rating = rating,
                Comment = comment,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Відгук успішно додано!";
            return RedirectToAction("Details", "Merchandises", new { id = merchId });
        }
    }
}