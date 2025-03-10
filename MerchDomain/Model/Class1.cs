// MerchDomain/Model/UserCart.cs
using Microsoft.AspNetCore.Identity;

namespace MerchDomain.Model
{
    public class UserCart
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Зовнішній ключ до IdentityUser
        public int MerchandiseId { get; set; }
        public int Quantity { get; set; }

        // Навігаційні властивості
        public IdentityUser User { get; set; }
        public Merchandise Merchandise { get; set; }
    }
}