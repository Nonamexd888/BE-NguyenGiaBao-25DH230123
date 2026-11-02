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

        // GET: Admin/Orders  hoặc  Admin/Orders?customerId=3
        public ActionResult Index(int? customerId, int? page)
        {
            IQueryable<Order> orders = db.Orders.Include(o => o.Customer);
            Customer customer = null;

            if (customerId.HasValue)
            {
                customer = db.Customers.Find(customerId.Value);
                if (customer == null)
                {
                    return HttpNotFound();
                }
                orders = orders.Where(o => o.CustomerID == customerId.Value);
            }

            ViewBag.CustomerId = customerId;
            ViewBag.CustomerName = customer != null ? customer.CustomerName : null;

            orders = orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID);

            int pageNumber = page ?? 1;
            int pageSize = 10;
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