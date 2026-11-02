using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BE_NguyenGiaBao_25DH230123.Filters;

namespace BE_NguyenGiaBao_25DH230123.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeController : Controller
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}