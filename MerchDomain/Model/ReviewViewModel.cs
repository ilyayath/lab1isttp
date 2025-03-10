namespace MerchDomain.Model
{
    public class ReviewViewModel
    {
        public string BuyerName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }
    }
}