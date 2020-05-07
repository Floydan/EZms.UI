using System;
using System.Collections.Generic;
using System.Text;
using EZms.Core.Extensions;
using EZms.Core.Loaders;
using EZms.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace EZms.UI.Infrastructure.TagHelpers
{

    /// <inheritdoc />
    /// <summary>
    /// <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.ITagHelper" /> implementation targeting &lt;a&gt; elements.
    /// </summary>
    [HtmlTargetElement("a", Attributes = "ezms-content")]
    [HtmlTargetElement("a", Attributes = "ezms-contentid")]
    public class EZmsAnchorTagHelper : TagHelper
    {
        private readonly IContentLoader _contentLoader;
        private const string ContentAttributeName = "ezms-content";
        private const string ContentIdAttributeName = "ezms-contentid";
        private const string Href = "href";
        private IDictionary<string, string> _routeValues;

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper" />.
        /// </summary>
        /// <param name="generator">The <see cref="T:Microsoft.AspNetCore.Mvc.ViewFeatures.IHtmlGenerator" />.</param>
        /// <param name="contentLoader"></param>
        public EZmsAnchorTagHelper(IHtmlGenerator generator, IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
            Generator = generator;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        protected IHtmlGenerator Generator { get; }

        [HtmlAttributeName("ezms-content")]
        public IContent Content { get; set; }

        [HtmlAttributeName("ezms-contentid")]
        public int? ContentId { get; set; }


        /// <summary>
        /// Gets or sets the <see cref="T:Microsoft.AspNetCore.Mvc.Rendering.ViewContext" /> for the current request.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if user provides an <c>href</c> attribute.</remarks>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (output.Attributes.ContainsName(Href))
            {
                if (Content != null || ContentId != null)
                    throw new InvalidOperationException("You can't supply a href and content/contentid");
            }
            else
            {
                if (Content != null && ContentId != null)
                    throw new InvalidOperationException("You can't supply both content and contentId");

                if (Content != null)
                {
                    output.Attributes.Add(Href, Content.GetContentFullUrlSlug());
                }
                else
                {
                    output.Attributes.Add(Href, ContentId.GetContentFullUrlSlug());
                }

                var routeValueDictionary = (RouteValueDictionary)null;
                if (_routeValues != null && _routeValues.Count > 0)
                    routeValueDictionary = new RouteValueDictionary(_routeValues);

                var tagBuilder = Generator.GenerateActionLink(ViewContext, string.Empty, null, null, null, null, null, routeValueDictionary, null);
                output.MergeAttributes(tagBuilder);
            }
        }
    }
}
