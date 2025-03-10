using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace MerchDomain.Model;

public partial class Brand : Entity
{
    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [Display(Name = "Бренд")]
    public string BrandName { get; set; } = null!;

    public virtual ICollection<Merchandise> Merchandises { get; set; } = new List<Merchandise>();
}