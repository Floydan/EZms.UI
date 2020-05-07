using System.Collections.Generic;
using System.Linq;
using EZms.Core.Extensions;
using EZms.Core.Helpers;
using EZms.Core.Models;
using EZms.Core.Routing;
using EZms.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace EZms.UI.Infrastructure.Helpers
{
    public static class SelectListHelpers
    {
        public static IEnumerable<SelectListItem> ContentTypes()
        {
            var typeMappings = ServiceLocator.Current.GetInstance<ICachedContentTypeControllerMappings>();
            var localizer = ServiceLocator.Current.GetInstance<IStringLocalizer>();
            var pageTypes = typeMappings.GetAllContentTypes();

            var selectListItems = pageTypes.Select(w =>
            {
                var pageData = w.GetPageDataValues();
                return new SelectListItem(pageData.Name, pageData.Guid);
            }).ToList();

            selectListItems.Insert(0, new SelectListItem(localizer["Page types"], ""));

            return selectListItems;
        }

        public static IEnumerable<SelectListItem> GetTagsAsSelectListItems(List<string> tags)
        {
            if (tags.IsNullOrEmpty()) return Enumerable.Empty<SelectListItem>();

            return tags.Select(w => new SelectListItem(w, w));
        }

        public static IEnumerable<SelectListItem> GetSiteNavigationContentItems(int? selectedId)
        {
            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

            var navigation = navigationService.CreateContentNavigation().GetAwaiter().GetResult();

            var listItems = new List<SelectListItem> { new SelectListItem("", "") };
            
            foreach (var node in navigation)
            {
                var nodeType = !node.Type.GenericTypeArguments.IsNullOrEmpty() ? node.Type.GenericTypeArguments.First() : node.Type;
                var pageData = nodeType.GetPageDataValues();

                listItems.Add(new SelectListItem($"{node.Name} ({pageData.Name})", node.Id.ToString(), node.Id == selectedId));
                if (!node.Children.IsNullOrEmpty())
                    listItems.AddRange(ChildrenAsSelectListItems(node.Children, 2, selectedId));
            }

            return listItems;
        }

        public static IEnumerable<SelectListItem> GetUserGroups(IEnumerable<string> selectedValues)
        {
            var localizer = ServiceLocator.Current.GetInstance<IStringLocalizer>();
            var roleManager = ServiceLocator.Current.GetInstance<RoleManager<IdentityRole>>();
            var roles = roleManager.Roles;

            var listItems = new List<SelectListItem> { new SelectListItem(localizer["Inherit from parent"], "") };

            listItems.AddRange(roles.Select(w => new SelectListItem(w.Name, w.Id)));

            return listItems;
        }

        private static IEnumerable<SelectListItem> ChildrenAsSelectListItems(IEnumerable<NavigationNode> nodes, int level, int? selectedId)
        {
            var listItems = new List<SelectListItem>();

            foreach (var node in nodes)
            {
                var nodeType = !node.Type.GenericTypeArguments.IsNullOrEmpty() ? node.Type.GenericTypeArguments.First() : node.Type;
                var pageData = nodeType.GetPageDataValues();

                var padding = new string('-', level);
                listItems.Add(new SelectListItem($"{padding} {node.Name} ({pageData.Name})", node.Id.ToString(), node.Id == selectedId));
                if (!node.Children.IsNullOrEmpty())
                    listItems.AddRange(ChildrenAsSelectListItems(node.Children, level + 2, selectedId));
            }

            return listItems;
        }
    }
}
