using System.Web;
using System.Web.Mvc;

namespace ASPNETMvc4GoogleOAuth
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}