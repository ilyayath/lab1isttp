using System;
using System.Collections.Generic;

namespace MerchDomain.Model;

public partial class City : Entity
{

    public string? CityName { get; set; }

    public virtual ICollection<Buyer> Buyers { get; set; } = new List<Buyer>();
}
