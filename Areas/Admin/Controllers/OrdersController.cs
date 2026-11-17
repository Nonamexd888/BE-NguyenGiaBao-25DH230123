using BE_NguyenGiaBao_25DH230123.Models;
using BE_NguyenGiaBao_25DH230123.Filters;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BE_NguyenGiaBao_25DH230123.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Orders  or  Admin/Orders?customerId=3
        public ActionResult Index(int? customerId, int? page, string searchName, string searchStatus, string searchDate)
        {
            IQueryable<Order> orders = db.Orders.Include(o => o.Customer);
            Customer customer = null;

            // 1. Lọc theo khách hàng
            if (customerId.HasValue)
            {
                customer = db.Customers.Find(customerId.Value);
                if (customer == null)
                {
                    return HttpNotFound();
                }
                orders = orders.Where(o => o.CustomerID == customerId.Value);
            }

            // 2. Lọc theo Tên khách hàng
            if (!string.IsNullOrEmpty(searchName))
            {
                orders = orders.Where(o => o.Customer.CustomerName.Contains(searchName));
            }

            // 3. Lọc theo Trạng thái thanh toán
            if (!string.IsNullOrEmpty(searchStatus))
            {
                orders = orders.Where(o => o.PaymentStatus == searchStatus);
            }

            // 4. Lọc theo Ngày đặt hàng
            if (!string.IsNullOrEmpty(searchDate))
            {
                DateTime date;
                if (DateTime.TryParse(searchDate, out date))
                {
                    // Chỉ lấy phần Ngày
                    orders = orders.Where(o => DbFunctions.TruncateTime(o.OrderDate) == date);
                }
            }

            // Lưu lại các giá trị tìm kiếm để hiển thị lại trên Form
            ViewBag.CustomerId = customerId;
            ViewBag.CustomerName = customer != null ? customer.CustomerName : null;
            ViewBag.SearchName = searchName;
            ViewBag.SearchStatus = searchStatus;
            ViewBag.SearchDate = searchDate;

            // Sắp xếp mới nhất lên đầu
            orders = orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID);

            // Phân trang: Yêu cầu file PPT là 15 đơn hàng/trang
            int pageNumber = page ?? 1;
            int pageSize = 15;

            return View(orders.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .SingleOrDefault(o => o.OrderID == id.Value);

            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Admin/Orders/UpdateStatus/5
        public ActionResult UpdateStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            // Trả về view cập nhật trạng thái
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string paymentStatus, string shippingMethod)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                order.PaymentStatus = paymentStatus;
                order.ShippingMethod = shippingMethod;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = order.OrderID });
            }
            return View(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}