using System;
using System.Collections.Generic;

namespace MerchDomain.Model;

public partial class OrderStatus : Entity
{


    public string StatusName { get; set; } = null!;

    public virtual ICollection<MerchOrder> MerchOrders { get; set; } = new List<MerchOrder>();
}
