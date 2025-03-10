using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchDomain.Model
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string StatusName { get; set; }
        public string PaymentType { get; set; }
        public string ShipmentType { get; set; }
        public decimal TotalAmount { get; set; }
    }
}