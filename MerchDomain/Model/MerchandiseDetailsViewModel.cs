namespace MerchDomain.Model
{
    public class MerchandiseDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public string TeamName { get; set; }

        public string ImageUrl { get; set; }
        public string SizeName { get; set; }
        public List<ReviewViewModel> Reviews { get; set; }
    }
}