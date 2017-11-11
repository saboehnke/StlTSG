using System;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace StlTSG
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
#if !DEBUG
            KeepAlive();
#endif
        }

        private static void KeepAlive()
        {
            Action remove = HttpContext.Current.Cache["Refresh"] as Action;
            if (remove is Action)
            {
                HttpContext.Current.Cache.Remove("Refresh");
                remove.EndInvoke(null);
            }
            
            Action work = () =>
            {
                while (true)
                {
                    Thread.Sleep(60000);
                    WebClient refresh = new WebClient();
                    try
                    {
                        //refresh.UploadString("http://www.cdar.co/", string.Empty);
                    }
                    catch (Exception ex)
                    {
                        
                    }
                    finally
                    {
                        refresh.Dispose();
                    }
                }
            };
            work.BeginInvoke(null, null);
            
            HttpContext.Current.Cache.Add(
                "Refresh",
                work,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.Normal,
                (s, o, r) => { KeepAlive(); }
            );
        }
    }
}
