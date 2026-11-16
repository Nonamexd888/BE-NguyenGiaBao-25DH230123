using BE_NguyenGiaBao_25DH230123.Models;
using BE_NguyenGiaBao_25DH230123.Models.ViewModel;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace BE_NguyenGiaBao_25DH230123.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                //kiểm tra xem tên đăng nhập đã tồn tại chưa
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }
                //nếu chưa tồn tại thì tạo bản ghi thông tin tài khoản trong bảng User
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password, // Lưu ý: Nên mã hóa mật khẩu trước khi lưu
                    UserRole = "C"
                };
                db.Users.Add(user);
                // và tạo bản ghi thông tin khách hàng trong bảng Customer
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };
                db.Customers.Add(customer);
                //lưu thông tin tài khoản và thông tin khách hàng vào CSDL
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                        && u.Password == model.Password
                        && u.UserRole == "C");
                if (user != null)
                {
                    // Lưu trạng thái đăng nhập vào session
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    //lưu thông tin xác thực người dùng vào cookie
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }

        // GET: Account/Logout  (SLIDE KHÔNG CÓ CODE, mình tự viết theo mô tả)
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Remove("Username");
            Session.Remove("UserRole");
            return RedirectToAction("Index", "Home");
        }

        // Lấy thông tin hồ sơ của người đang đăng nhập (null nếu không có hồ sơ khách hàng)
        private ProfileVM GetProfile()
        {
            string username = User.Identity.Name;
            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null) return null;
            return new ProfileVM
            {
                Username = customer.Username,
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone,
                CustomerEmail = customer.CustomerEmail,
                CustomerAddress = customer.CustomerAddress
            };
        }

        // GET: Account/ProfileInfo
        [Authorize]
        public ActionResult ProfileInfo()
        {
            var current = GetProfile();
            if (current == null) return RedirectToAction("Index", "Home");
            ViewBag.Current = current;
            return View(current);
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileVM model)
        {
            string username = User.Identity.Name;
            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null) return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                customer.CustomerPhone = model.CustomerPhone;
                customer.CustomerEmail = model.CustomerEmail;
                customer.CustomerAddress = model.CustomerAddress;
                db.SaveChanges();
                TempData["Success"] = "Đã cập nhật thông tin tài khoản.";
                return RedirectToAction("ProfileInfo");
            }

            ViewBag.Current = GetProfile();
            model.Username = customer.Username;
            model.CustomerName = customer.CustomerName;
            return View("ProfileInfo", model);
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            string username = User.Identity.Name;
            var user = db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null) return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                if (user.Password != model.OldPassword)
                {
                    ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không đúng.");
                }
                else
                {
                    user.Password = model.NewPassword;
                    db.SaveChanges();
                    TempData["Success"] = "Đã đổi mật khẩu.";
                    return RedirectToAction("ProfileInfo");
                }
            }

            var current = GetProfile();
            if (current == null) return RedirectToAction("Index", "Home");
            ViewBag.Current = current;
            return View("ProfileInfo", current);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}