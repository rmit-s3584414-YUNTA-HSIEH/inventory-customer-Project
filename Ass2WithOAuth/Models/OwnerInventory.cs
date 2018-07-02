using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Ass2WithOAuth.Models
{
    public class OwnerInventory
    {
        [Key, ForeignKey("Product"), Display(Name = "Product ID")]
        public int ProductID { get; set; }
        public Product Product { get; set; }

        [Range(0, 100000)]
        [Display(Name = "Stock Level")]
        public int StockLevel { get; set; }
    }
}
