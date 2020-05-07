using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EZms.UI.Areas.EZms.Models
{
    public class UserPostModel
    {
        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public IEnumerable<string> SelectedRoles { get; set; }
    }
}
