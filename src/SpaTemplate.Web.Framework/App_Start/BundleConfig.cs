using System.Web.Optimization;

namespace SpaTemplate.Web.Framework
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles
                .Add(new ScriptBundle("~/css")
                    .Include(
                        "~/ClientApp/dist/styles*"
                    ));

            bundles
                .Add(new ScriptBundle("~/app")
                    .Include(
                        "~/ClientApp/dist/main*",
                        "~/ClientApp/dist/polyfills*",
                        "~/ClientApp/dist/runtime*",
                        "~/ClientApp/dist/vendor*"
                    ));
            BundleTable.EnableOptimizations = false;
        }
    }
}