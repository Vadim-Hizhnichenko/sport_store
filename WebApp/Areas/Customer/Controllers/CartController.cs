using System.Security.Claims;
using Business.Contracts;
using Business.Repository;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.ViewModels;
using static System.Net.WebRequestMethods;

namespace WebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShopingCartViewModel ShopingCartViewModel { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {

            //var claimIdentity = User.Identity as ClaimsIdentity;
            //var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var claim = GetClaimIdentityValue();


            ShopingCartViewModel = new ShopingCartViewModel()
            {
                ListOfShopingCart = _unitOfWork.ShopingCartRepository.GetAll(u => u.UserId == claim.Value, "Product"),
                OrderHeader = new OrderHeader()

            };
            foreach (var cart in ShopingCartViewModel.ListOfShopingCart)
            {
                ShopingCartViewModel.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }


            return View(ShopingCartViewModel);

        }

        private Claim GetClaimIdentityValue()
        {
            var claimIdentity = User.Identity as ClaimsIdentity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            return claim;
        }

        public IActionResult Summary()
        {
            //var claimIdentity = User.Identity as ClaimsIdentity;
            //var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var claim = GetClaimIdentityValue();

            ShopingCartViewModel = new ShopingCartViewModel()
            {
                ListOfShopingCart = _unitOfWork.ShopingCartRepository.GetAll(u => u.UserId == claim.Value, "Product"),
                OrderHeader = new OrderHeader()

            };

            ShopingCartViewModel.OrderHeader.User = _unitOfWork.UserRepository.GetFirstOrDifault(
                u => u.Id == claim.Value);


            // autocomplete form
            ShopingCartViewModel.OrderHeader.Name = ShopingCartViewModel.OrderHeader.User.Name;
            ShopingCartViewModel.OrderHeader.PhoneNumber = ShopingCartViewModel.OrderHeader.User.PhoneNumber;
            ShopingCartViewModel.OrderHeader.Country = ShopingCartViewModel.OrderHeader.User.Country;
            ShopingCartViewModel.OrderHeader.Street = ShopingCartViewModel.OrderHeader.User.Street;
            ShopingCartViewModel.OrderHeader.City = ShopingCartViewModel.OrderHeader.User.City;
            ShopingCartViewModel.OrderHeader.PostalCode = ShopingCartViewModel.OrderHeader.User.PostalCode;


            foreach (var cart in ShopingCartViewModel.ListOfShopingCart)
            {
                ShopingCartViewModel.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }

            return View(ShopingCartViewModel);

        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost()
        {
            //var claimIdentity = User.Identity as ClaimsIdentity;
            //var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var claim = GetClaimIdentityValue();

            ShopingCartViewModel.ListOfShopingCart = _unitOfWork.ShopingCartRepository.GetAll(u => u.UserId == claim.Value, "Product");

            ShopingCartViewModel.OrderHeader.OrderStatus = Constants.StatusPending;
            ShopingCartViewModel.OrderHeader.PaymentStatus = Constants.PaymentStatusPending;
            ShopingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
            ShopingCartViewModel.OrderHeader.UserId = claim.Value;


            ShopingCartViewModel.OrderHeader.User = _unitOfWork.UserRepository.GetFirstOrDifault(
                u => u.Id == claim.Value);

            foreach (var cart in ShopingCartViewModel.ListOfShopingCart)
            {
                ShopingCartViewModel.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }

            _unitOfWork.OrderHeaderRepository.Add(ShopingCartViewModel.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShopingCartViewModel.ListOfShopingCart)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShopingCartViewModel.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                _unitOfWork.OrderDetailRepository.Add(orderDetail);
                _unitOfWork.Save();
            }

            //_unitOfWork.ShopingCartRepository.DeleteRange(ShopingCartViewModel.ListOfShopingCart);
            //_unitOfWork.Save();
            //StripeSettings();

            var domainUrl = "https://localhost:44307/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domainUrl + $"Customer/Cart/OrderConfirm?id={ShopingCartViewModel.OrderHeader.Id}",
                CancelUrl = domainUrl + $"Customer/Cart/Index",
            };

            foreach (var item in ShopingCartViewModel.ListOfShopingCart)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name

                        }
                    },
                    Quantity = item.Count,
                };

                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeaderRepository.UpdateStripeSessionIds(ShopingCartViewModel.OrderHeader.Id,session.Id, session.PaymentIntentId);

            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

            //return RedirectToAction("OrderConfirm", "Cart", new { id = ShopingCartViewModel.OrderHeader.Id });

            //return RedirectToAction("Index", "Home");

        }

        public IActionResult OrderConfirm(int id)
        {
            OrderHeader orderHeaderDb = _unitOfWork.OrderHeaderRepository.GetFirstOrDifault(o => o.Id == id);

            var service = new SessionService();
            Session session = service.Get(orderHeaderDb.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(id, Constants.StatusApproved ,Constants.PaymentStatusApproved);
                _unitOfWork.Save();
            }

            List<ShopingCart> shopingCarts = _unitOfWork.ShopingCartRepository.GetAll(s => s.UserId == orderHeaderDb.UserId).ToList();
            _unitOfWork.ShopingCartRepository.DeleteRange(shopingCarts);
            _unitOfWork.Save();

            return View(id);
        }

        public IActionResult IncrementCount(int shopCartId)
        {
            var shopCartDb = _unitOfWork.ShopingCartRepository.GetFirstOrDifault(cart => cart.Id == shopCartId);
            _unitOfWork.ShopingCartRepository.IncrementShopCartCount(shopCartDb, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DecrementCount(int shopCartId)
        {
            var shopCartDb = _unitOfWork.ShopingCartRepository.GetFirstOrDifault(cart => cart.Id == shopCartId);

            if (shopCartDb.Count <= 1)
            {
                _unitOfWork.ShopingCartRepository.Delete(shopCartDb);
            }
            else
            {
                _unitOfWork.ShopingCartRepository.DecrementShopCartCount(shopCartDb, 1);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int shopCartId)
        {
            var shopCartDb = _unitOfWork.ShopingCartRepository.GetFirstOrDifault(cart => cart.Id == shopCartId);
            _unitOfWork.ShopingCartRepository.Delete(shopCartDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public StatusCodeResult StripeSettings()
        {
            var domainUrl = "https://localhost:44307/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domainUrl + $"Customer/Cart/OrderConfirm?id={ShopingCartViewModel.OrderHeader.Id}",
                CancelUrl = domainUrl + $"Customer/Cart/Index",
            };

            foreach (var item in ShopingCartViewModel.ListOfShopingCart)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                            
                        }
                    },
                    Quantity = item.Count,
                };

                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeaderRepository.UpdateStripeSessionIds(ShopingCartViewModel.OrderHeader.Id,
                ShopingCartViewModel.OrderHeader.SessionId,
                ShopingCartViewModel.OrderHeader.PayementIntentId);
 
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
    }
}
