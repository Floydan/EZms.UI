using System;
using System.Collections.Generic;
using EZms.UI.Areas.EZms.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EZms.UI.Infrastructure.TagHelpers
{
    /// <summary>
    /// <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.ITagHelper" /> implementation targeting &lt;input&gt; elements with an <c>asp-for</c> attribute.
    /// </summary>
    [HtmlTargetElement("input", Attributes = "property", TagStructure = TagStructure.WithoutEndTag)]
    public class EZmsInputTagHelper : TagHelper
    {
        private static readonly Dictionary<string, string> DefaultInputTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
      {
        "HiddenInput",
        InputType.Hidden.ToString().ToLowerInvariant()
      },
      {
        "Password",
        InputType.Password.ToString().ToLowerInvariant()
      },
      {
        "Text",
        InputType.Text.ToString().ToLowerInvariant()
      },
      {
        "PhoneNumber",
        "tel"
      },
      {
        "Url",
        "url"
      },
      {
        "EmailAddress",
        "email"
      },
      {
        "Date",
        "date"
      },
      {
        "DateTime",
        "datetime-local"
      },
      {
        "DateTime-local",
        "datetime-local"
      },
      {
        "DateTimeOffset",
        "text"
      },
      {
        "Time",
        "time"
      },
      {
        "Week",
        "week"
      },
      {
        "Month",
        "month"
      },
      {
        "Byte",
        "number"
      },
      {
        "SByte",
        "number"
      },
      {
        "Int16",
        "number"
      },
      {
        "UInt16",
        "number"
      },
      {
        "Int32",
        "number"
      },
      {
        "UInt32",
        "number"
      },
      {
        "Int64",
        "number"
      },
      {
        "UInt64",
        "number"
      },
      {
        "Single",
        InputType.Text.ToString().ToLowerInvariant()
      },
      {
        "Double",
        InputType.Text.ToString().ToLowerInvariant()
      },
      {
        "Boolean",
        InputType.CheckBox.ToString().ToLowerInvariant()
      },
      {
        "Decimal",
        InputType.Text.ToString().ToLowerInvariant()
      },
      {
        "String",
        InputType.Text.ToString().ToLowerInvariant()
      },
      {
        "IFormFile",
        "file"
      },
      {
        "IEnumerable`IFormFile",
        "file"
      }
    };
        private static readonly Dictionary<string, string> Rfc3339Formats = new Dictionary<string, string>(StringComparer.Ordinal)
        {
      {
        "date",
        "{0:yyyy-MM-dd}"
      },
      {
        "datetime",
        "{0:yyyy-MM-ddTHH\\:mm\\:ss.fffK}"
      },
      {
        "datetime-local",
        "{0:yyyy-MM-ddTHH\\:mm\\:ss.fff}"
      },
      {
        "time",
        "{0:HH\\:mm\\:ss.fff}"
      }
    };
        private const string ForAttributeName = "asp-for";
        private const string FormatAttributeName = "asp-format";

        /// <summary>
        /// Creates a new <see cref="T:Rentals.Infrastructure.TagHelpers.InputTagHelper" />.
        /// </summary>
        /// <param name="generator">The <see cref="T:Microsoft.AspNetCore.Mvc.ViewFeatures.IHtmlGenerator" />.</param>
        public EZmsInputTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        protected IHtmlGenerator Generator { get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        ///// <summary>
        ///// An expression to be evaluated against the current model.
        ///// </summary>
        //[HtmlAttributeName("asp-for")]
        //public ModelExpression For { get; set; }

        /// <summary>
        /// The format string (see https://msdn.microsoft.com/en-us/library/txafckwd.aspx) used to format the
        /// <see cref="P:Rentals.Infrastructure.TagHelpers.For" /> result. Sets the generated "value" attribute to that formatted string.
        /// </summary>
        /// <remarks>
        /// Not used if the provided (see <see cref="P:Rentals.Infrastructure.TagHelpers.InputTypeName" />) or calculated "type" attribute value is
        /// <c>checkbox</c>, <c>password</c>, or <c>radio</c>. That is, <see cref="P:Rentals.Infrastructure.TagHelpers.Format" /> is used when calling
        /// <see cref="M:Microsoft.AspNetCore.Mvc.ViewFeatures.IHtmlGenerator.GenerateTextBox(Microsoft.AspNetCore.Mvc.Rendering.ViewContext,Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExplorer,System.String,System.Object,System.String,System.Object)" />.
        /// </remarks>
        [HtmlAttributeName("asp-format")]
        public string Format { get; set; }

        /// <summary>The type of the &lt;input&gt; element.</summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases. Also used to determine the <see cref="T:Microsoft.AspNetCore.Mvc.ViewFeatures.IHtmlGenerator" />
        /// helper to call and the default <see cref="P:Rentals.Infrastructure.TagHelpers.Format" /> value. A default <see cref="P:Rentals.Infrastructure.TagHelpers.Format" /> is not calculated
        /// if the provided (see <see cref="P:Rentals.Infrastructure.TagHelpers.InputTypeName" />) or calculated "type" attribute value is <c>checkbox</c>,
        /// <c>hidden</c>, <c>password</c>, or <c>radio</c>.
        /// </remarks>
        [HtmlAttributeName("type")]
        public string InputTypeName { get; set; }

        [HtmlAttributeName("property")]
        public ModelProperty Property { get; set; }

        public ModelExplorer ModelExplorer { get; set; }

        /// <summary>The name of the &lt;input&gt; element.</summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases. Also used to determine whether <see cref="P:Rentals.Infrastructure.TagHelpers.For" /> is
        /// valid with an empty <see cref="P:Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExpression.Name" />.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>The value of the &lt;input&gt; element.</summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases. Also used to determine the generated "checked" attribute
        /// if <see cref="P:Rentals.Infrastructure.TagHelpers.InputTypeName" /> is "radio". Must not be <c>null</c> in that case.
        /// </remarks>
        public string Value { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="P:Rentals.Infrastructure.TagHelpers.For" /> is <c>null</c>.</remarks>
        /// <exception cref="T:System.InvalidOperationException">
        /// Thrown if <see cref="P:Rentals.Infrastructure.TagHelpers.Format" /> is non-<c>null</c> but <see cref="P:Rentals.Infrastructure.TagHelpers.Property" /> is <c>null</c>.
        /// </exception>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Property == null)
                throw new ArgumentNullException(nameof(Property));

            ModelExplorer = Property.ModelExplorer;

            Value = Property.Value?.ToString() ?? "";
            Name = Property.Name;

            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (InputTypeName != null)
                output.CopyHtmlAttribute("type", context);
            if (Name != null)
                output.Attributes.Add("name", Name);
            if (Value != null)
                output.Attributes.Add("value", Value);

            var metadata = ModelExplorer.Metadata;
            var modelExplorer = ModelExplorer;

            if (metadata == null)
                throw new InvalidOperationException(string.Format("{0} - {1} - {2} - {3}", "<input>", "asp-for", "IModelMetadataProvider", Name));
            string inputTypeHint;
            string inputType;
            if (string.IsNullOrEmpty(InputTypeName))
            {
                inputType = GetInputType(modelExplorer, out inputTypeHint);
            }
            else
            {
                inputType = InputTypeName.ToLowerInvariant();
                inputTypeHint = null;
            }
            if (!output.Attributes.ContainsName("type"))
                output.Attributes.SetAttribute("type", inputType);
            IDictionary<string, object> htmlAttributes = null;
            if (string.IsNullOrEmpty(Name) &&
                string.IsNullOrEmpty(ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix) && !string.IsNullOrEmpty(Name))
            {
                htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
                    {
                        "name",
                        Name
                    }
                };
            }

            var tagBuilder = inputType == "hidden" ? GenerateHidden(modelExplorer, htmlAttributes) : (inputType == "checkbox" ? GenerateCheckBox(modelExplorer, output, htmlAttributes) : (inputType == "password" ? Generator.GeneratePassword(ViewContext, modelExplorer, Name, null, htmlAttributes) : (inputType == "radio" ? GenerateRadio(modelExplorer, htmlAttributes) : GenerateTextBox(modelExplorer, inputTypeHint, inputType, htmlAttributes))));
            if (tagBuilder == null)
                return;
            output.MergeAttributes(tagBuilder);
            if (!tagBuilder.HasInnerHtml)
                return;
            output.Content.AppendHtml(tagBuilder.InnerHtml);
        }

        /// <summary>
        /// Gets an &lt;input&gt; element's "type" attribute value based on the given <paramref name="modelExplorer" />
        /// or <see cref="T:Microsoft.AspNetCore.Mvc.ViewFeatures.InputType" />.
        /// </summary>
        /// <param name="modelExplorer">The <see cref="T:Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExplorer" /> to use.</param>
        /// <param name="inputTypeHint">When this method returns, contains the string, often the name of a
        /// <see cref="P:Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata.ModelType" /> base class, used to determine this method's return value.</param>
        /// <returns>An &lt;input&gt; element's "type" attribute value.</returns>
        protected string GetInputType(ModelExplorer modelExplorer, out string inputTypeHint)
        {
            foreach (var inputTypeHint1 in GetInputTypeHints(modelExplorer))
            {
                if (DefaultInputTypes.TryGetValue(inputTypeHint1, out var str))
                {
                    inputTypeHint = inputTypeHint1;
                    return str;
                }
            }
            inputTypeHint = InputType.Text.ToString().ToLowerInvariant();
            return inputTypeHint;
        }

        private TagBuilder GenerateCheckBox(ModelExplorer modelExplorer, TagHelperOutput output, IDictionary<string, object> htmlAttributes)
        {
            if (modelExplorer.Metadata.ModelType == typeof(string))
            {
                if (modelExplorer.Model != null && !bool.TryParse(modelExplorer.Model.ToString(), out _))
                    throw new InvalidOperationException(string.Format("{0} - {1} - {2}", "asp-for", modelExplorer.Model, typeof(bool).FullName));
            }
            else if (modelExplorer.Metadata.ModelType != typeof(bool))
                throw new InvalidOperationException(string.Format("{0} - {1} - {2} - {3} - {4} - {5} - {6}", "<input>", "asp-for", modelExplorer.ModelType.FullName, typeof(bool).FullName, typeof(string).FullName, "type", "checkbox"));
            //var hiddenForCheckbox = Generator.GenerateHiddenForCheckbox(ViewContext, modelExplorer, Name);
            //if (hiddenForCheckbox != null)
            //{
            //    var tagRenderMode = output.TagMode == TagMode.SelfClosing ? TagRenderMode.SelfClosing : TagRenderMode.StartTag;
            //    hiddenForCheckbox.TagRenderMode = tagRenderMode;
            //    if (!hiddenForCheckbox.Attributes.ContainsKey("name") && !string.IsNullOrEmpty(Name))
            //        hiddenForCheckbox.MergeAttribute("name", Name);
            //    if (ViewContext.FormContext.CanRenderAtEndOfForm)
            //        ViewContext.FormContext.EndOfFormContent.Add(hiddenForCheckbox);
            //    else
            //        output.PostElement.AppendHtml(hiddenForCheckbox);
            //}
            if (htmlAttributes == null) htmlAttributes = new Dictionary<string, object>();
            if (htmlAttributes.ContainsKey("style"))
                htmlAttributes["style"] = htmlAttributes["style"] + ";width:auto;";
            else
            {
                htmlAttributes.Add("style", "width:auto;");
            }
            return Generator.GenerateCheckBox(ViewContext, modelExplorer, Name, new bool?(), htmlAttributes);
        }

        private TagBuilder GenerateRadio(ModelExplorer modelExplorer, IDictionary<string, object> htmlAttributes)
        {
            if (Value == null)
                throw new InvalidOperationException(string.Format("{0} - {1} - {2} - {3}", "<input>", "Value".ToLowerInvariant(), "type", "radio"));
            return Generator.GenerateRadioButton(ViewContext, modelExplorer, Name, Value, new bool?(), htmlAttributes);
        }

        private TagBuilder GenerateTextBox(ModelExplorer modelExplorer, string inputTypeHint, string inputType, IDictionary<string, object> htmlAttributes)
        {
            var format = Format;
            if (string.IsNullOrEmpty(format))
            {
                if (!modelExplorer.Metadata.HasNonDefaultEditFormat && string.Equals("week", inputType, StringComparison.OrdinalIgnoreCase) && (modelExplorer.Model is DateTime || modelExplorer.Model is DateTimeOffset))
                    modelExplorer = modelExplorer.GetExplorerForModel(FormatWeekHelper.GetFormattedWeek(modelExplorer));
                else
                    format = GetFormat(modelExplorer, inputTypeHint, inputType);
            }
            if (htmlAttributes == null)
                htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            htmlAttributes["type"] = inputType;
            if (string.Equals(inputType, "file") && string.Equals(inputTypeHint, "IEnumerable`IFormFile", StringComparison.OrdinalIgnoreCase))
                htmlAttributes["multiple"] = "multiple";
            return Generator.GenerateTextBox(ViewContext, modelExplorer, Name, modelExplorer.Model, format, htmlAttributes);
        }

        private TagBuilder GenerateHidden(ModelExplorer modelExplorer, IDictionary<string, object> htmlAttributes)
        {
            var obj = ModelExplorer.Model;
            byte[] inArray;
            if ((inArray = obj as byte[]) != null)
                obj = Convert.ToBase64String(inArray);
            if (htmlAttributes == null)
                htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            htmlAttributes["type"] = "hidden";
            return Generator.GenerateTextBox(ViewContext, modelExplorer, Name, obj, Format, htmlAttributes);
        }

        private string GetFormat(ModelExplorer modelExplorer, string inputTypeHint, string inputType)
        {
            return !string.Equals("month", inputType, StringComparison.OrdinalIgnoreCase) ? (!string.Equals("decimal", inputTypeHint, StringComparison.OrdinalIgnoreCase) || !string.Equals("text", inputType, StringComparison.Ordinal) || !string.IsNullOrEmpty(modelExplorer.Metadata.EditFormatString) ? (ViewContext.Html5DateRenderingMode != Html5DateRenderingMode.Rfc3339 || modelExplorer.Metadata.HasNonDefaultEditFormat || !(typeof(DateTime) == modelExplorer.Metadata.UnderlyingOrModelType) && !(typeof(DateTimeOffset) == modelExplorer.Metadata.UnderlyingOrModelType) ? modelExplorer.Metadata.EditFormatString : (!string.Equals("text", inputType) || !string.Equals("DateTimeOffset", inputTypeHint, StringComparison.OrdinalIgnoreCase) ? (!Rfc3339Formats.TryGetValue(inputType, out var str) ? modelExplorer.Metadata.EditFormatString : str) : Rfc3339Formats["datetime"])) : "{0:0.00}") : "{0:yyyy-MM}";
        }

        private static IEnumerable<string> GetInputTypeHints(ModelExplorer modelExplorer)
        {
            if (!string.IsNullOrEmpty(modelExplorer.Metadata.TemplateHint))
                yield return modelExplorer.Metadata.TemplateHint;
            if (!string.IsNullOrEmpty(modelExplorer.Metadata.DataTypeName))
                yield return modelExplorer.Metadata.DataTypeName;
            var fieldType = modelExplorer.ModelType;
            if (typeof(bool?) != fieldType)
                fieldType = modelExplorer.Metadata.UnderlyingOrModelType;
            foreach (var typeName in TemplateRenderer.GetTypeNames(modelExplorer.Metadata, fieldType))
                yield return typeName;
        }
    }
}
