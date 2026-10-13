using System.Web;
using System.Web.Mvc;

namespace BE_NguyenGiaBao_25DH230123
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
