using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EZms.Core.Helpers;
using EZms.Core.Models;
using EZms.Core.Repositories;
using EZms.Core.Services;
using EZms.UI.Areas.EZms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace EZms.UI.Areas.EZms.Pages
{
    [Authorize(Roles = "EZmsAdmin,EZmsEditor,Administrators")]
    public class IndexModel : PageModel
    {
        public IEnumerable<NavigationNode> Navigation { get; private set; }

        private readonly INavigationService _navigationService;
        private readonly IContentRepository _contentRepository;

        public IndexModel(INavigationService navigationService, IContentRepository contentRepository)
        {
            _navigationService = navigationService;
            _contentRepository = contentRepository;
            var httpContext = ServiceLocator.Current.GetInstance<IHttpContextAccessor>().HttpContext;
            httpContext.Items.Add("Area", "EZMS");
        }

        public async Task OnGetAsync()
        {
            Navigation = await _navigationService.CreateContentNavigation();
        }

        public async Task<IActionResult> OnGetNavigationAsync()
        {
            var navigation = await _navigationService.CreateContentNavigation();
            return PartialView("Shared/_Navigation", navigation);
        }

        public async Task<IActionResult> OnPostUpdateSiteHierarchyAsync([FromBody]HierarchyUpdateModel model)
        {
            await _contentRepository.UpdateParentId(model.ContentId, model.ParentId, false);

            await _contentRepository.UpdateSortOrder(model.ParentId, model.Children, true);

            return new JsonResult(new { success = true });
        }


        public IActionResult OnPostSetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                var url = new Uri(new Regex("^/ezms[#]*", RegexOptions.IgnoreCase | RegexOptions.Singleline).Replace(returnUrl, string.Empty), UriKind.RelativeOrAbsolute);
                if (url.IsAbsoluteUri)
                    returnUrl = $"/ezms#{url.PathAndQuery}";
            }

            return LocalRedirect(returnUrl ?? Url.Page("Index"));
        }

        [NonAction]
        protected virtual PartialViewResult PartialView(string viewName, object model)
        {
            var viewData = new ViewDataDictionary(
                ServiceLocator.Current.GetInstance<IModelMetadataProvider>(),
                new ModelStateDictionary()
            ) {
                Model = model
            };

            return new PartialViewResult {
                ViewName = viewName,
                ViewData = viewData,
                TempData = TempData
            };
        }
    }
}