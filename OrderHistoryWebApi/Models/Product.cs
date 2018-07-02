

using System.ComponentModel.DataAnnotations;

namespace OrderHistoryWebApi.Models
{
    public class Product
    {
        public int ProductID { get; set; }

        [Display(Name = "Product Name")]
        public string Name { get; set; }
        public int Price { get; set; }
    }
}
