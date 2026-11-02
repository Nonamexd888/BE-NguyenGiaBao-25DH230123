using BE_NguyenGiaBao_25DH230123.Models;
using BE_NguyenGiaBao_25DH230123.Models.ViewModel;
using BE_NguyenGiaBao_25DH230123.Filters;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BE_NguyenGiaBao_25DH230123.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers
        public ActionResult Index(string searchTerm, string sortOrder, int? page)
        {
            var model = new CustomerSearchVM();
            var customers = db.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                customers = customers.Where(c =>
                    c.CustomerName.Contains(searchTerm) ||
                    c.CustomerPhone.Contains(searchTerm) ||
                    c.CustomerEmail.Contains(searchTerm) ||
                    c.CustomerAddress.Contains(searchTerm) ||
                    c.Username.Contains(searchTerm));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    customers = customers.OrderByDescending(c => c.CustomerName);
                    break;
                case "id_asc":
                    customers = customers.OrderBy(c => c.CustomerID);
                    break;
                case "id_desc":
                    customers = customers.OrderByDescending(c => c.CustomerID);
                    break;
                default:
                    customers = customers.OrderBy(c => c.CustomerName);
                    break;
            }

            model.SearchTerm = searchTerm;
            model.SortOrder = sortOrder;

            int pageNumber = page ?? 1;
            int pageSize = 10;
            model.Customers = customers.ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Admin/Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
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