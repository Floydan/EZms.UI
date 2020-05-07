using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace EZms.UI.Areas.EZms.Models
{
    public class ModelProperty
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public object Value { get; set; }
        public string ValidationMessage { get; set; }
        public int Order { get; set; }
        public int ReadOrder { get; set; }
        public Type Type { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public ModelExplorer ModelExplorer { get; set; }
        public Expression Expression { get; set; }
        public ModelMetadata Metadata { get; set; }
        public bool IsEnumerable { get; set; }
        public string UiHint { get; set; }
        public IEnumerable<ModelProperty> Properties { get; set; }
        public bool IsComplexType => Properties != null && Properties.Any();
    }
}
