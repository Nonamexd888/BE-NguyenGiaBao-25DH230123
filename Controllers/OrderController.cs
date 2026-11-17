using BE_NguyenGiaBao_25DH230123.Models;
using BE_NguyenGiaBao_25DH230123.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace BE_NguyenGiaBao_25DH230123.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order/Checkout
        [Authorize]
        public ActionResult Checkout()
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Home");

            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null) return RedirectToAction("Login", "Account");

            var model = new CheckoutVM
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.TotalValue(),
                OrderDate = DateTime.Now,
                ShippingAddress = customer.CustomerAddress,
                CustomerID = customer.CustomerID,
                Username = customer.Username
            };
            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Home");

            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                string paymentStatus = "Chưa thanh toán";
                switch (model.PaymentMethod)
                {
                    case "Tiền mặt": paymentStatus = "Thanh toán tiền mặt"; break;
                    case "Mua trước trả sau": paymentStatus = "Trả góp"; break;
                }

                string orderStatus = model.PaymentMethod == "Tiền mặt" ? "Đang xử lý" : "Chờ thanh toán";

                var order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.TotalValue(),
                    PaymentStatus = paymentStatus,
                    PaymentMethod = model.PaymentMethod,
                    ShippingMethod = model.ShippingMethod,
                    ShippingAddress = model.ShippingAddress,

                    OrderDetails = cart.Items.Select(item => new OrderDetail
                    {
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                };

                db.Orders.Add(order);
                db.SaveChanges();

                Session["Cart"] = null; // Xóa giỏ hàng
                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            model.CartItems = cart.Items.ToList();
            model.TotalAmount = cart.TotalValue();
            model.CustomerID = customer.CustomerID;
            model.Username = customer.Username;
            return View(model);
        }

        // GET: Order/OrderSuccess/5
        [Authorize]
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product")
                .SingleOrDefault(o => o.OrderID == id && o.Customer.Username == User.Identity.Name);
            if (order == null) return HttpNotFound();
            return View(order);
        }

        // GET: Order/MyOrder
        [Authorize]
        public ActionResult MyOrder(string tab = "all", string searchTerm = "", bool showAll = false)
        {
            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null) { return RedirectToAction("Login", "Account"); }

            var orders = db.Orders
                .Include("OrderDetails.Product")
                .Where(o => o.CustomerID == customer.CustomerID);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                int orderId;
                bool isNumber = int.TryParse(searchTerm, out orderId);
                orders = orders.Where(o => (isNumber && o.OrderID == orderId) ||
                    o.OrderDetails.Any(d => d.Product.ProductName.Contains(searchTerm)));
            }

            if (tab != "all")
            {
                if (tab == "pending") orders = orders.Where(o => o.PaymentStatus == "Chưa thanh toán");
                else if (tab == "processing") orders = orders.Where(o => o.PaymentStatus == "Thanh toán tiền mặt"); // Tạm map
                else if (tab == "shipping") orders = orders.Where(o => o.ShippingMethod != null); // Tạm map
                else if (tab == "delivered") orders = orders.Where(o => o.PaymentStatus == "Đã thanh toán"); // Tạm map
                else if (tab == "cancelled") orders = orders.Where(o => o.PaymentStatus == "Đã hủy"); // Tạm map
            }

            orders = orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID);

            var resultList = orders.ToList();
            if (showAll)
            {
                resultList = resultList.Take(50).ToList();
            }
            else if (tab == "all")
            {
                var tempList = new List<Order>();
                foreach (var group in resultList.GroupBy(o => o.PaymentStatus))
                {
                    tempList.AddRange(group.Take(2));
                }
                resultList = tempList.OrderByDescending(o => o.OrderDate).ToList();
            }
            else
            {
                resultList = resultList.Take(2).ToList();
            }

            ViewBag.CurrentTab = tab;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.ShowAll = showAll;

            return View(resultList);
        }

        // Hàm xử lý nút "Mua lại"
        [Authorize]
        public ActionResult Reorder(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product").SingleOrDefault(o => o.OrderID == id);
            if (order == null) return HttpNotFound();

            var cart = Session["Cart"] as Cart ?? new Cart();
            foreach (var item in order.OrderDetails)
            {
                cart.AddItem(item.ProductID, item.Product.ProductImage, item.Product.ProductName,
                             item.UnitPrice, 1, item.Product.Category.CategoryName);
            }
            Session["Cart"] = cart;

            return RedirectToAction("Index2", "Cart");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}