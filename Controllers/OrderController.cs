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
            //Kiểm tra giỏ hàng trong session,
            //nếu giỏ hàng rỗng hoặc không có sản phẩm thì chuyển hướng về trang chủ
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }
            //Xác thực người dùng đã đăng nhập chưa, nếu chưa thì chuyển hướng tới trang Đăng nhập
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) { return RedirectToAction("Login", "Account"); }
            //Lấy thông tin khách hàng từ CSDL, nếu chưa có thì chuyển hướng tới trang Đăng nhập
            //nếu có rồi thì lấy địa chỉ của khách hàng và gán vào ShippingAddress của CheckoutVM
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null) { return RedirectToAction("Login", "Account"); }

            var model = new CheckoutVM
            { //Tạo dữ liệu hiển thị cho CheckoutVM
                CartItems = cart.Items.ToList(), //Lấy danh sách các sản phẩm trong giỏ hàng
                TotalAmount = cart.TotalValue(), //Tổng giá trị của các mặt hàng trong giỏ
                OrderDate = DateTime.Now, //Mặc định lấy bằng thời điểm đặt hàng
                ShippingAddress = customer.CustomerAddress, // Lấy địa chỉ mặc định từ bảng Customer
                CustomerID = customer.CustomerID,// Lấy mã khách hàng từ bảng Customer
                Username = customer.Username // Lấy tên đăng nhập từ bảng Customer
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
            //Nếu giỏ hàng rỗng sẽ điều hướng tới trang Home
            if (cart == null || !cart.Items.Any()) { return RedirectToAction("Index", "Home"); }
            //Nếu người dùng chưa đăng nhập sẽ điều hướng tới trang Login
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) { return RedirectToAction("Login", "Account"); }
            //nếu khách hàng không khớp với tên đăng nhập, sẽ điều hướng tới trang Login
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null) { return RedirectToAction("Login", "Account"); }

            if (ModelState.IsValid)
            {
                // Thiết lập PaymentStatus dựa trên PaymentMethod
                string paymentStatus = "Chưa thanh toán";
                switch (model.PaymentMethod)
                {
                    case "Tiền mặt": paymentStatus = "Thanh toán tiền mặt"; break;
                    case "Mua trước trả sau": paymentStatus = "Trả góp"; break;
                    default: paymentStatus = "Chưa thanh toán"; break;
                }
                //Tạo đơn hàng và chi tiết đơn hàng liên quan
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
                //Lưu đơn hàng vào CSDL
                db.Orders.Add(order);
                db.SaveChanges();
                // Xóa giỏ hàng sau khi đặt hàng thành công
                Session["Cart"] = null;
                // Điều hướng tới trang Xác nhận đơn hàng
                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            // Form lỗi: nạp lại dữ liệu để hiển thị lại trang
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
            var order = db.Orders.Include("OrderDetails")
                .SingleOrDefault(o => o.OrderID == id && o.Customer.Username == User.Identity.Name);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Order/MyOrder  (SLIDE 47 CHỈ CÓ HÌNH, mình tự viết theo giao diện)
        [Authorize]
        public ActionResult MyOrder(string status, string searchTerm)
        {
            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null) { return RedirectToAction("Login", "Account"); }

            IQueryable<Order> orders = db.Orders.Include("OrderDetails.Product")
                .Where(o => o.CustomerID == customer.CustomerID);

            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.PaymentStatus == status);
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                int orderId;
                int.TryParse(searchTerm, out orderId);
                orders = orders.Where(o => o.OrderID == orderId ||
                    o.OrderDetails.Any(d => d.Product.ProductName.Contains(searchTerm)));
            }

            ViewBag.Status = status;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Statuses = db.Orders.Where(o => o.CustomerID == customer.CustomerID)
                .Select(o => o.PaymentStatus).Distinct().ToList();

            return View(orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID).ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
