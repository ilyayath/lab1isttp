using System;
using System.Collections.Generic;

namespace MerchDomain.Model;

public partial class OrderItem : Entity
{
    public int OrderId { get; set; }

    public int MerchId { get; set; }

    public int Quantity { get; set; }

    public virtual Merchandise Merch { get; set; } = null!;

    public virtual MerchOrder Order { get; set; } = null!;
}
