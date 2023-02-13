using Data.Models;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class ShopingCartViewModel
    {
        public IEnumerable<ShopingCart> ListOfShopingCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
