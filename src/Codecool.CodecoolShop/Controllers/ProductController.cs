using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Codecool.CodecoolShop.Daos;
using Codecool.CodecoolShop.Daos.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Codecool.CodecoolShop.Models;
using Codecool.CodecoolShop.Services;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;

namespace Codecool.CodecoolShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        public ProductService ProductService { get; set; }
        public ProductController(ILogger<ProductController> logger, IHostingEnvironment env)
        {
            _env = env;
            _logger = logger;
            ProductService = new ProductService(
                ProductDaoMemory.GetInstance(),
                ProductCategoryDaoMemory.GetInstance(),
                SupplierDaoMemory.GetInstance(),
                ProductCartDaoMemory.GetInstance(),
                UserDaoMemory.GetInstance());
        }
        public IActionResult Cart()
        {
            var cart = ProductService.GetCart();
            return View(cart.ToList());
        }

        public IActionResult Index()
        {
            var model = new IndexModel();
            var products = ProductService.GetAllProducts();
            var categories = ProductService.GetAllCategories();
            var suppliers = ProductService.GetAllSuppliers();
            
            model.Products = products.ToList();
            model.ProductSuppliers = suppliers.ToList();
            model.ProductCategories = categories.ToList();
            return View(model);
            
        }

        public IActionResult IncreaseQuantity(int id)
        {
            var product = ProductService.GetProduct(id);
            ProductService.AddProductToCart(product);

            return RedirectToAction("Cart");
        }

        public IActionResult DecreaseQuantity(int id)
        {
            var product = ProductService.GetProduct(id);
            ProductService.RemoveFromCart(product);
            return RedirectToAction("Cart");
        }

        public IActionResult RemoveItemTotally(int id)
        {
            var product = ProductService.GetProduct(id);
            ProductService.RemoveItemFromCartTotally(product);
            return RedirectToAction("Cart");
        }

        public IActionResult AddToCart(int id)
        {
            var product = ProductService.GetProduct(id);
            ProductService.AddProductToCart(product);
            return RedirectToAction("Index");
        }
        public IActionResult ProductsByCategory(int id)
        {
            var model = new IndexModel();
            var suppliers = ProductService.GetAllSuppliers();
            var categories = ProductService.GetAllCategories();
            foreach(var category in categories)
            {
                if(category.Id == id)
                {
                    var productsForCategory = ProductService.GetProductsForCategory(category);
                    model.Products = productsForCategory.ToList();
                }
            }
            model.ProductSuppliers = suppliers.ToList();
            return View("Index", model);
        }

        public IActionResult ProductsBySupplier(int id)
        {
            var model = new IndexModel();
            var categories = ProductService.GetAllCategories();
            var suppliers = ProductService.GetAllSuppliers();
            foreach (var supplier in suppliers)
            {
                if (supplier.Id == id)
                {
                    var productsForSupplier = ProductService.GetProductsForSupplier(supplier);
                    model.Products = productsForSupplier.ToList();
                }
            }
            model.ProductCategories = categories.ToList();
            return View("Index", model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult MoneyBack()
        {
            return View();
        }

        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(CheckoutModel user)
        {
            if (ModelState.IsValid)
            {
                var cart = ProductService.GetCart().ToList();
                var order = new Order();
                order.OrderDetails = (user, cart);
                ProductService.AddOrder(order);
                return RedirectToAction("Payment");
            }
            
            //string userEmail = user.Email;
            //string userPhoneNumber = user.PhoneNumber;
            //string userAddress = user.Address;
            //string userCity = user.City;
            //string userCountry = user.Country;
            //int userZipCode = user.ZipCode;
            //string userName = user.BuyerName;
            return View();
        }

        public IActionResult Payment()
        {
            
            PaymentModel payment = new PaymentModel();
            payment.Cart = ProductService.GetCart().ToList();

            return View(payment);
        }

        [HttpPost]
        public IActionResult Payment(PaymentModel paymentModel)
        {
            
            if (ModelState.IsValid)
            {
                OrderFinalize();
                return RedirectToAction("Index");
            }
            string name = paymentModel.Name;
            string monthYear = paymentModel.MonthYear;
            int cvv = paymentModel.CVV;
            string cardNumber = paymentModel.CardNumber;
            return View(paymentModel);
        }
        private IHostingEnvironment _env;
        public void OrderFinalize()
        {
            var orders = ProductService.GetAllOrders().ToList();
            var webRoot = _env.WebRootPath;
            var file = System.IO.Path.Combine(webRoot, "json.json");
            string jsonData = JsonConvert.SerializeObject(orders[orders.Count-1], Formatting.Indented);
            System.IO.File.WriteAllText(file, jsonData);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}