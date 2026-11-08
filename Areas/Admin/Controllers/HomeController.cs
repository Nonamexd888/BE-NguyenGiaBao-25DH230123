using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BE_NguyenGiaBao_25DH230123.Filters;
using BE_NguyenGiaBao_25DH230123.Models;
using BE_NguyenGiaBao_25DH230123.Models.ViewModel;

namespace BE_NguyenGiaBao_25DH230123.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Home
        public ActionResult Index()
        {
            var stats = db.Categories
                .Select(c => new CategoryStatVM
                {
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products.Count(),
                    MaxPrice = c.Products.Max(p => (decimal?)p.ProductPrice),
                    MinPrice = c.Products.Min(p => (decimal?)p.ProductPrice),
                    AvgPrice = c.Products.Average(p => (decimal?)p.ProductPrice)
                })
                .OrderBy(s => s.CategoryName)
                .ToList();

            return View(stats);
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