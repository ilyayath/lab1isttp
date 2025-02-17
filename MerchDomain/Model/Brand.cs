﻿using System;
using System.Collections.Generic;

namespace MerchDomain.Model;

public partial class Brand: Entity
{

    public string BrandName { get; set; } = null!;

    public virtual ICollection<Merchandise> Merchandises { get; set; } = new List<Merchandise>();
}
