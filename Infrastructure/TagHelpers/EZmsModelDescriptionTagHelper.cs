using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace EZms.UI.Infrastructure.TagHelpers
{
    [HtmlTargetElement("property-description")]
    public class EZmsModelDescriptionTagHelper : TagHelper
    {
        private readonly IStringLocalizer _localizer;

        public EZmsModelDescriptionTagHelper(IStringLocalizer localizer = null)
        {
            _localizer = localizer;
        }

        [HtmlAttributeName("for")]
        public ModelExpression Property { get; set; }
        
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var description = Property?.Metadata?.Description;

            var propertyName = $"{Property?.Metadata?.ContainerType?.Name}.{Property?.Metadata?.Name}.Description";

            var localizedDescription = _localizer?[propertyName];
            if (!string.IsNullOrWhiteSpace(localizedDescription) && localizedDescription != propertyName)
                description = localizedDescription;

            output.TagName = null;
            output.Content.SetHtmlContent(!string.IsNullOrWhiteSpace(description)
                ? $"<small class='text-muted font-italic'>{description}</small>"
                : "");
        }
    }
}
