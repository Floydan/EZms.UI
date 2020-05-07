using System.Collections.Generic;
using System.Linq;
using EZms.Core.Models;

namespace EZms.UI.Areas.EZms.Models
{
    public class ContentPostModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string UrlSlug { get; set; }
        public string ContentTypeGuid { get; set; }

        public List<ModelProperty> Properties { get; set; }
        public int SavedVersion { get; set; }
        public int PublishedVersion { get; set; }
        public bool Published { get; set; }
        public IOrderedEnumerable<ContentVersion> ContentVersions { get; set; }
        public IEnumerable<string> AllowedGroups { get; set; }
    }
}