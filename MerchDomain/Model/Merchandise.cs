using System;
using System.Collections.Generic;

namespace MerchDomain.Model;

public partial class Merchandise : Entity
{

    public int? TeamId { get; set; }

    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public int? SizeId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual Brand? Brand { get; set; }
    public string ImageUrl { get; set; }
    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Size? Size { get; set; }

    public virtual Team? Team { get; set; }

    public List<UserCart> UserCarts { get; set; }

}