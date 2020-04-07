using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Application.ViewModels.Systems;
using TeduCoreApp.Data.Enums;
using TeduCoreApp.Extensions;
using TeduCoreApp.Models;
using TeduCoreApp.Utilities.Constants;

namespace TeduCoreApp.Controllers
{
    public class CartController : Controller
    {
        private IProductService _productService;
        private IBillService _billService;
        public CartController(IProductService productService, IBillService billService)
        {
            _productService = productService;
            _billService = billService;
        }

        [Route("cart.html", Name = "Cart")]
        public IActionResult Index()
        {
            ViewData["BodyClass"] = "shopping_cart_page";
            return View();
        }

        [Route("checkout.html", Name = "Checkout")]
        [HttpGet]
        public IActionResult Checkout()
        {
            ViewData["BodyClass"] = "checkout_page";
            var model =  new CheckoutViewModel();
            var session = HttpContext.Session.Get<List<ShoppingCartViewModel>>(CommonConstants.CartSession);
            if (session.Any(x => x.Color == null && x.Size ==null))
            {
                return Redirect("/cart.html");
            }
            model.Carts = session;
            return View(model);
        }

        [Route("checkout.html", Name = "Checkout")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            var session = HttpContext.Session.Get<List<ShoppingCartViewModel>>(CommonConstants.CartSession);
            if (ModelState.IsValid)
            {
                if (session != null)
                {
                    var details = new List<BillDetailViewModel>();
                    foreach (var item in session)
                    {
                        details.Add(new BillDetailViewModel()
                        {
                            Product = item.Product,
                            Price = item.Price,
                            ColorId = item.Color.Id,
                            SizeId = item.Size.Id,
                            Quantity = item.Quantity,
                            ProductId = item.Product.Id
                        });
                    }

                    var billViewModel = new BillViewModel()
                    {
                        CustomerMobile = model.CustomerMobile,
                        BillDetails = details,
                        BillStatus = BillStatus.New,
                        CustomerName = model.CustomerName,
                        CustomerAddress = model.CustomerMessage,
                        CustomerMessage = model.CustomerMessage
                    };

                    if (User.Identity.IsAuthenticated == true)
                    {
                        billViewModel.CustomerId = Guid.Parse(User.GetSpecificClaim("UserId"));
                    }
                    _billService.Create(billViewModel);

                    try
                    {
                        _billService.Save();
                        ViewData["Success"] = true;
                    }
                    catch (Exception e)
                    {
                        ViewData["Success"] = false;
                        ModelState.AddModelError("", e.Message);
                    }
                }
            }
            model.Carts = session;
            return View(model);
        }

        #region AJAX Request
        /// <summary>
        /// Get list item
        /// </summary>
        /// <returns></returns>
        public IActionResult GetCart()
        {
            var session = HttpContext.Session.Get<List<ShoppingCartViewModel>>(CommonConstants.CartSession);
            if (session == null)
            {
                session = new List<ShoppingCartViewModel>();
            }

            return new OkObjectResult(session);
        }

        /// <summary>
        /// remove all product in cart
        /// </summary>
        /// <returns></returns>
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CommonConstants.CartSession);
            return new OkObjectResult("Ok");
        }

        public IActionResult AddToCart(int productId, int quantity, int color, int size)
        {
            //get product detail
            var product = _productService.GetById(productId);

            //get session with item list object
            var session = HttpContext.Session.Get<List<ShoppingCartViewModel>>(CommonConstants.CartSession);
            if (session != null)
            {
                //convert string to list object
                bool hasChange = false;

                //check exist with item productid
                if (session.Any(x => x.Product.Id == productId))
                {
                    foreach (var item in session)
                    {
                        if (item.Product.Id == productId)
                        {
                            item.Quantity += quantity;
                            item.Price = product.PromotionPrice ?? product.Price;
                            hasChange = true;
                        }
                        
                    }
                }
                else
                {
                    session.Add(new ShoppingCartViewModel()
                    {
                        Product = product,
                        Quantity = quantity,
                        Color = _billService.GetColor(color),
                        Size = _billService.GetSize(size),
                        Price = product.PromotionPrice ?? product.Price
                    });
                    hasChange = true;
                }
                //update back to cart
                if (hasChange)
                {
                    HttpContext.Session.Set(CommonConstants.CartSession, session);
                }
            }
            else
            {
                //add new cart
                var cart = new List<ShoppingCartViewModel>();
                cart.Add(new ShoppingCartViewModel()
                {
                    Product = product,
                    Quantity = quantity,
                    Color = _billService.GetColor(color),
                    Size = _billService.GetSize(size),
                    Price = product.PromotionPrice ?? product.Price
                });
                HttpContext.Session.Set(CommonConstants.CartSession, cart);
            }
            return new OkObjectResult(productId);
        }
        /// <summary>
        /// remove from cart
        /// </summary>
        /// <returns></returns>
        public IActionResult RemoveFromCart(int productId)
        {
            var session = HttpContext.Session.Get<List<ShoppingCartViewModel>>(CommonConstants.CartSession);
            if (session != null)
            {
                bool hasChange = false;
                foreach (var item in session)
                {
                    if (item.Product.Id == productId)
                    {
                        session.Remove(item);
                        hasChange = true;
                        break;
                    }
                }

                if (hasChange)
                {
                    HttpContext.Session.Set(CommonConstants.CartSession, session);
                }
                return new OkObjectResult(productId);
            }

            return new EmptyResult();
        }

        public IActionResult UpdateCart(int productId, int quantity, int color, int size)
        {
            var session = HttpContext.Session.Get<List<ShoppingCartViewModel>>(CommonConstants.CartSession);
            if (session != null)
            {
                bool hasChanged = false;
                foreach (var item in session)
                {
                    if (item.Product.Id == productId)
                    {
                        var product = _productService.GetById(productId);
                        item.Product = product;
                        item.Size = _billService.GetSize(size);
                        item.Color = _billService.GetColor(color);
                        item.Quantity = quantity;
                        item.Price = product.PromotionPrice ?? product.Price;
                        hasChanged = true;
                    }
                }
                if (hasChanged)
                {
                    HttpContext.Session.Set(CommonConstants.CartSession, session);
                }
                return new OkObjectResult(productId);
            }
            return new EmptyResult();
        }

        public IActionResult GetColors()
        {
            var color = _billService.GetColors();
            return new OkObjectResult(color);
        }

        public IActionResult GetSizes()
        {
            var size = _billService.GetSizes();
            return new OkObjectResult(size);
        }

        #endregion
    }
}