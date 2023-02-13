using Business.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var productList = _unitOfWork.ProductRepository.GetAll(includeProperties:"Category");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            var shopCart = new ShopingCart()
            {
                Product = _unitOfWork.ProductRepository.GetFirstOrDifault(p => p.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };

            return View(shopCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShopingCart shopingCart)
        {
            //var claimIdentity = User.Identity as ClaimsIdentity;
            //var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var claim = GetClaimIdentityValue();

            shopingCart.UserId = claim.Value;

            var shopCartDb = _unitOfWork.ShopingCartRepository.GetFirstOrDifault(
                u => u.UserId == claim.Value && u.ProductId == shopingCart.ProductId);


            if (shopCartDb == null)
            {
                _unitOfWork.ShopingCartRepository.Add(shopingCart);
            }
            else
            {
                _unitOfWork.ShopingCartRepository.IncrementShopCartCount(shopCartDb, shopingCart.Count);
            }

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        private Claim GetClaimIdentityValue()
        {
            var claimIdentity = User.Identity as ClaimsIdentity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            return claim;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
