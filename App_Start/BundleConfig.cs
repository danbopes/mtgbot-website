using System.Web.Optimization;

namespace MTGBotWebsite.App_Start
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            /*bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/base.css",
                "~/Content/grid.css",
                "~/Content/ie.css",
                "~/Content/main.css",
                "~/Content/mobile.css",
                "~/Content/text.css",
                "~/Content/colours.css"
            ));*/

            bundles.Add(new StyleBundle("~/Content/plugin-css").Include(
                "~/Content/plugins/colorpicker/colorpicker.css",
                "~/Content/plugins/datatables/datatables.css",
                "~/Content/plugins/elfinder/elfinder.css",
                "~/Content/plugins/fancybox/fancybox.css",
                //"~/Content/plugins/fullcalendar/fullcalendar.css",
                "~/Content/plugins/isotope/isotope.css",
                "~/Content/plugins/multiselect/multiselect.css",
                "~/Content/plugins/select2/select2.css",
                "~/Content/plugins/selectbox/selectbox.css",
                "~/Content/plugins/slidernav/slidernav.css",
                //"~/Content/plugins/slidernav/smallipop.css",
                "~/Content/plugins/syntaxhighlighter/syntaxhighlighter.css",
                "~/Content/plugins/syntaxhighlighter/shThemeDefault.css",
                "~/Content/plugins/tagit/tagit.css",
                "~/Content/plugins/themeroller/themeroller.css",
                "~/Content/plugins/tinyeditor/tinyeditor.css",
                "~/Content/plugins/tiptip/tiptip.css",
                "~/Content/plugins/uistars/uistars.css",
                "~/Content/plugins/uitotop/uitotop.css",
                "~/Content/plugins/uniform/uniform.css"
            ));


            bundles.Add(new ScriptBundle("~/bundles/adminica").IncludeDirectory("~/Scripts/adminica/", "*.js"));

            bundles.Add(new ScriptBundle("~/bundles/plugins").Include(
                "~/Scripts/jquery/jquery.js",
                "~/Scripts/jquery/jqueryui.js",
                "~/Scripts/jquery/jquery-migrate.js",
                "~/Scripts/modernizr/modernizr.js",
                "~/Scripts/prefixfree/prefixfree.js",
                //"~/Scripts/pjax/pjax.js",
                "~/Scripts/isotope/isotope.js",
                "~/Scripts/autogrow/autogrow.js",
                "~/Scripts/colorpicker/colorpicker.js",
                "~/Scripts/cookie/cookie.js",
                "~/Scripts/datatables/datatables.js",
                "~/Scripts/elfinder/elfinder.js",
                "~/Scripts/dragscroll/dragScroll.js",
                "~/Scripts/tinyeditor/tinyeditor.js",
                "~/Scripts/fancybox/fancybox.js",
                "~/Scripts/flot/flot_excanvas.js",
                "~/Scripts/flot/flot.js",
                "~/Scripts/flot/flot_resize.js",
                "~/Scripts/flot/flot_pie.js",
                "~/Scripts/flot/flot_pie_update.js",
                //"~/Scripts/fullcalendar/fullcalendar.js",
                //"~/Scripts/fullcalendar/fullcalendar_gcal.js",
                "~/Scripts/hoverintent/hoverIntent.js",
                "~/Scripts/iscroll/iscroll.js",
                "~/Scripts/knob/knob.js",
                "~/Scripts/multiselect/multiselect.js",
                "~/Scripts/select2/select2.js",
                "~/Scripts/selectbox/selectbox.js",
                "~/Scripts/slidernav/slidernav.js",
                "~/Scripts/smallipop/smallipop.js",
                "~/Scripts/sparkline/sparkline.js",
                "~/Scripts/syntaxhighlighter/shCore.js",
                "~/Scripts/syntaxhighlighter/shBrushJScript.js",
                "~/Scripts/syntaxhighlighter/shBrushXml.js",
                "~/Scripts/tagit/tagit.js",
                "~/Scripts/timepicker/timepicker.js",
                "~/Scripts/tinyeditor/tinyeditor.js",
                "~/Scripts/tiptip/tiptip.js",
                "~/Scripts/touchpunch/touchpunch.js",
                "~/Scripts/uistars/uistars.js",
                "~/Scripts/uitotop/uitotop.js",
                "~/Scripts/uniform/uniform.js",
                "~/Scripts/validation/validation.js",
                "~/Scripts/tmpl/tmpl.js",
                "~/Scripts/sound/sound.js",
                "~/Scripts/mtgbot/global.js"
            ));
            /* bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));*/
        }
    }
}