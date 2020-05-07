using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EZms.UI.Areas.EZms.Pages.Admin
{
    public static class ManageNavPages
    {
        private static string Index => "Index";

        private static string Users => "Users";

        private static string Roles => "Roles";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string UsersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Users);

        public static string RolesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Roles);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}