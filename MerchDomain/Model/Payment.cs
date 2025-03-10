using System;
using System.Collections.Generic;

namespace MerchDomain.Model;

public partial class Payment : Entity
{


    public string TypePayment { get; set; } = null!;

    public virtual ICollection<MerchOrder> MerchOrders { get; set; } = new List<MerchOrder>();
}