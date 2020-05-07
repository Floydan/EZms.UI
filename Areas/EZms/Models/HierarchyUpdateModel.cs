using System.Collections.Generic;

namespace EZms.UI.Areas.EZms.Models
{
    public class HierarchyUpdateModel
    {
        public int ContentId { get; set; }
        public int ParentId { get; set; }
        public IEnumerable<int> Children { get; set; }
    }
}
