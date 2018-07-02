

using System.ComponentModel.DataAnnotations;

namespace Ass2WithOAuth.Models
{
    public class StockRequest
    {
        public int StockRequestID { get; set; }

        public int StoreID { get; set; }
        public Store Store { get; set; }

        public int ProductID { get; set; }
        public Product Product { get; set; }

        [Range(0, 100000)]
        public int Quantity { get; set; }
    }
}
