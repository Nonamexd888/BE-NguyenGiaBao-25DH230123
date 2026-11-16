using BE_NguyenGiaBao_25DH230123.Models;
using BE_NguyenGiaBao_25DH230123.Models.ViewModel;
using PagedList;
using System.Linq;
using System.Web.Mvc;
using System.Net;

namespace BE_NguyenGiaBao_25DH230123.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                                p.ProductDescription.Contains(searchTerm) ||
                                                p.Category.CategoryName.Contains(searchTerm));
            }

            int pageNumber = page ?? 1;
            int pageSize = 6;

            // top 10 sản phẩm bán chạy nhất
            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(10).ToList();

            // 20 sản phẩm bán ế nhất, phân trang
            model.NewProducts = products.OrderBy(p => p.OrderDetails.Count()).Take(20).ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Home/ProductDetails/5
        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product pro = db.Products.Find(id);
            if (pro == null)
            {
                return HttpNotFound();
            }

            // lấy tất cả các sản phẩm cùng danh mục
            var products = db.Products.Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID).AsQueryable();

            ProductDetailsVM model = new ProductDetailsVM();

            // Đoạn code liên quan tới phân trang
            // Lấy số trang hiện tại (mặc định là trang 1 nếu không có giá trị)
            int pageNumber = page ?? 1;
            int pageSize = model.PageSize;  // Số sản phẩm mỗi trang
            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(8).ToPagedList(pageNumber, pageSize);

            if (quantity.HasValue)
            {
                model.quantity = quantity.Value;
            }
            model.estimatedValue = model.quantity * pro.ProductPrice;

            return View(model);
        }

        // GET: Home/ProductList?categoryId=1
        // (SLIDE CHỈ MÔ TẢ, KHÔNG CÓ CODE: mình tự viết theo mô tả)
        public ActionResult ProductList(int? categoryId, int? page)
        {
            if (categoryId == null)
            {
                return RedirectToAction("Index");
            }
            var category = db.Categories.Find(categoryId);
            if (category == null)
            {
                return HttpNotFound();
            }

            var products = db.Products.Where(p => p.CategoryID == categoryId.Value);

            ViewBag.CategoryName = category.CategoryName;
            ViewBag.CategoryId = categoryId.Value;

            int pageNumber = page ?? 1;
            int pageSize = 4;  // Số sản phẩm mỗi trang
            return View(products.OrderBy(p => p.ProductName).ToPagedList(pageNumber, pageSize));
        }

        // Menu danh mục hiển thị trên header của _CustomerLayout
        [ChildActionOnly]
        public ActionResult CategoriesPV()
        {
            return PartialView("_CategoriesPV", db.Categories.OrderBy(c => c.CategoryName).ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}