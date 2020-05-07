using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZms.Core.Models;
using EZms.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EZms.UI.Areas.EZms.Pages
{
    [Authorize(Roles = "EZmsAdmin,EZmsEditor,Administrators")]
    public class DeleteModel : PageModel
    {
        private readonly IContentRepository _contentRepository;
        public Content PageContent { get; private set; }

        public DeleteModel(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }


        public async Task<IActionResult> OnGetAsync(int id)
        {
            var content = await _contentRepository.GetContent(id);

            PageContent = content;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                await _contentRepository.Delete(id);

                return Content("");
            }
            catch
            {
                return Page();
            }
        }
    }
}