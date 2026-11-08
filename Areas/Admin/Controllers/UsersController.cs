using BE_NguyenGiaBao_25DH230123.Filters;
using BE_NguyenGiaBao_25DH230123.Models;
using BE_NguyenGiaBao_25DH230123.Models.ViewModel;
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
    public class UsersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Users
        public ActionResult Index(string searchTerm, int? page)
        {
            var users = db.Users.Where(u => u.UserRole == "A");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                users = users.Where(u => u.Username.Contains(searchTerm));
            }

            var model = new AdminUserSearchVM
            {
                SearchTerm = searchTerm,
                Users = users.OrderBy(u => u.Username).ToPagedList(page ?? 1, 10)
            };
            return View(model);
        }

        // GET: Admin/Users/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View(new AdminUserVM());
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminUserVM model)
        {
            if (model.Username != null)
            {
                model.Username = model.Username.Trim();
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu");
            }

            if (!string.IsNullOrEmpty(model.Username) && db.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
            }

            if (ModelState.IsValid)
            {
                db.Users.Add(new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "A"
                });
                db.SaveChanges();
                TempData["Success"] = "Đã thêm tài khoản " + model.Username;
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Admin/Users/Edit/admin
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.Find(id);
            if (user == null || user.UserRole != "A")
            {
                return HttpNotFound();
            }
            return View(new AdminUserVM { Username = user.Username });
        }

        // POST: Admin/Users/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminUserVM model)
        {
            var user = db.Users.Find(model.Username);
            if (user == null || user.UserRole != "A")
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                    db.SaveChanges();
                    TempData["Success"] = "Đã đổi mật khẩu tài khoản " + user.Username;
                }
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Admin/Users/Delete/admin
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.Find(id);
            if (user == null || user.UserRole != "A")
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Users/Delete/admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var user = db.Users.Find(id);
            if (user == null || user.UserRole != "A")
            {
                return HttpNotFound();
            }

            if (string.Equals(user.Username, User.Identity.Name, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction("Index");
            }

            if (db.Customers.Any(c => c.Username == user.Username))
            {
                TempData["Error"] = "Tài khoản này đang gắn với khách hàng nên không thể xóa.";
                return RedirectToAction("Index");
            }

            db.Users.Remove(user);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa tài khoản " + user.Username;
            return RedirectToAction("Index");
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