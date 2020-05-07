using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using EZms.Core.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EZms.UI.Areas.EZms.Models
{
    public class ContentPageModel : PageModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        [Display(Name = "Url", Description = "A unique url to this content")]
        public string UrlSlug { get; set; }
        public string ContentTypeGuid { get; set; }

        public List<ModelProperty> Properties { get; set; }

        public IOrderedEnumerable<IGrouping<string, ModelProperty>> GroupedProperties =>
            Properties.GroupBy(p => p.GroupName).OrderBy(w => w.Key);
        public int SavedVersion { get; set; }
        public int PublishedVersion { get; set; }
        public bool Published { get; set; }
        public IOrderedEnumerable<ContentVersion> ContentVersions { get; set; }

        public IEnumerable<string> AllowedGroups { get; set; }
        public IContent ContentModel { get; set; }
    }
}
