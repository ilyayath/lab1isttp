﻿using System;
using System.Collections.Generic;

namespace MerchDomain.Model;

public partial class Size : Entity
{


    public string SizeName { get; set; } = null!;

    public virtual ICollection<Merchandise> Merchandises { get; set; } = new List<Merchandise>();
}