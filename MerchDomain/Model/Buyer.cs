using System;
using System.Collections.Generic;

namespace MerchDomain.Model;

public partial class Buyer: Entity
{

    public string Username { get; set; } = null!;

    public int? CityId { get; set; }

    public string? Address { get; set; }

    public virtual City? City { get; set; }

    public virtual ICollection<MerchOrder> MerchOrders { get; set; } = new List<MerchOrder>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
