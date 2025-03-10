using Microsoft.AspNetCore.Identity;

namespace MerchDomain.Model
{
    public class User : IdentityUser
    {
        public int? CityId { get; set; }
        public string? Address { get; set; }

        public virtual City? City { get; set; }
        public virtual Buyer? Buyer { get; set; } // Зв’язок із таблицею Buyer (якщо потрібно)
    }
}