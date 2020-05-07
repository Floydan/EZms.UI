using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EZms.UI.Areas.EZms.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EZms.UI.Infrastructure.TagHelpers
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.ITagHelper" /> implementation targeting any HTML element with an <c>asp-validation-for</c>
    /// attribute.
    /// </summary>
    [HtmlTargetElement("span", Attributes = "asp-validation-property")]
    public class EZmsValidationMessageTagHelper : TagHelper
    {
        private const string DataValidationForAttributeName = "data-valmsg-for";
        private const string ValidationForAttributeName = "asp-validation-for";

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.AspNetCore.Mvc.TagHelpers.ValidationMessageTagHelper" />.
        /// </summary>
        /// <param name="generator">The <see cref="T:Microsoft.AspNetCore.Mvc.ViewFeatures.IHtmlGenerator" />.</param>
        public EZmsValidationMessageTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        protected IHtmlGenerator Generator { get; }

        [HtmlAttributeName("asp-validation-property")]
        public ModelProperty Property { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="P:Microsoft.AspNetCore.Mvc.TagHelpers.ValidationMessageTagHelper.For" /> is <c>null</c>.</remarks>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var messageTagHelper = this;
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (messageTagHelper.Property == null)
                return;
            var htmlAttributes = (IDictionary<string, object>)null;
            if (string.IsNullOrEmpty(messageTagHelper.Property.Name) && string.IsNullOrEmpty(messageTagHelper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix) && output.Attributes.ContainsName("data-valmsg-for"))
                htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "data-valmsg-for",
                        "-non-empty-value-"
                    }
                };
            string message = null;
            if (!output.IsContentModified)
            {
                var childContentAsync = await output.GetChildContentAsync();
                if (!childContentAsync.IsEmptyOrWhiteSpace)
                    message = childContentAsync.GetContent();
            }
            var validationMessage = messageTagHelper.Generator.GenerateValidationMessage(messageTagHelper.ViewContext, messageTagHelper.Property.ModelExplorer, messageTagHelper.Property.Name, message, null, htmlAttributes);
            if (validationMessage != null)
            {
                output.MergeAttributes(validationMessage);
                if (!output.IsContentModified && validationMessage.HasInnerHtml)
                    output.Content.SetHtmlContent(validationMessage.InnerHtml);
            }
            htmlAttributes = null;
            message = null;
        }
    }
}
