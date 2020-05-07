using EZms.Core.Enums;
using EZms.Core.Helpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EZms.UI.Infrastructure.TagHelpers
{
    [HtmlTargetElement("ezms-edit-button", TagStructure = TagStructure.WithoutEndTag)]
    public class EZmsEditButton : TagHelper
    {
        /// <summary>
        /// The view context of this tag helper, for accessing HttpContext on render.
        /// </summary>
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Processes this tag, rendering some lovely HTML.
        /// </summary>
        /// <param name="context">The context to render in.</param>
        /// <param name="output">The output to render to.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            if (!(ViewContext.HttpContext.Items["contentid"] is int contentId) || 
                (!ViewContext.HttpContext.User.IsInRole(EZmsRoles.EZmsEditor.ToString()) && 
                !ViewContext.HttpContext.User.IsInRole(EZmsRoles.EZmsAdmin.ToString())))
            {
                output.Content.SetHtmlContent(HtmlString.Empty);
                return;
            }

            var urlHelper = ServiceLocator.Current.GetInstance<IUrlHelper>();
            var ezmsBaseUrl = urlHelper.Page("/Index", new {area = "EZms"});
            var ezmsEditUrl = urlHelper.Page("/Edit", new {area = "EZms", id = contentId});
            
            var html = $@"<a class='btn btn-warning rounded-left rounded-0 position-fixed mb-3' style='position: absolute; bottom: 0; right: 0; z-index: 10000' href='{ezmsBaseUrl}/#{ezmsEditUrl}'>EZms <ion-icon name='rocket' size='small' style='position:relative;top:4px;'></ion-icon></a>";

            output.Content.SetHtmlContent(html);
        }
    }
}
