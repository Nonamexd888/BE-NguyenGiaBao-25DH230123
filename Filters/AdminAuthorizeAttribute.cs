using BE_NguyenGiaBao_25DH230123.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BE_NguyenGiaBao_25DH230123.Filters
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            string username = httpContext.User.Identity.Name;
            using (var db = new MyStoreEntities())
            {
                return db.Users.Any(u => u.Username == username && u.UserRole == "A");
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            {
                area = "Admin",
                controller = "Account",
                action = "Login",
                returnUrl = filterContext.HttpContext.Request.RawUrl
            }));
        }
    }
}