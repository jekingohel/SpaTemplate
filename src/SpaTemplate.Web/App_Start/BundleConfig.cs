using System.Web.Optimization;

namespace SpaTemplate
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles
                .Add(new ScriptBundle("~/css")
                    .Include(
                        "~/content/styles*"
                    ));

            bundles
                .Add(new ScriptBundle("~/app")
                    .Include(
                        "~/content/main*",
                        "~/content/polyfills*",
                        "~/content/runtime*",
                        "~/content/vendor*"
                    ));
            BundleTable.EnableOptimizations = false;
        }
    }
}