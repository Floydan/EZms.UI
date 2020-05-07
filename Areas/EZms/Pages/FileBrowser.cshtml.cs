using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZms.Core.AzureBlobFileProvider;
using EZms.Core.Extensions;
using EZms.Core.Helpers;
using EZms.Core.Models;
using EZms.Core.Repositories;
using EZms.Core.Services;
using EZms.UI.Areas.EZms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;

namespace EZms.UI.Areas.EZms.Pages
{
    [Authorize(Roles = "EZmsAdmin,EZmsEditor,Administrators")]
    public class FileBrowserModel : PageModel
    {
        public IEnumerable<Folder> Folders { get; set; }
        public string SelectedFile { get; set; }
        public string BasePath { get; }

        private readonly IContentRepository _contentRepository;
        private readonly AzureBlobFileProvider _azureBlobFileProvider;
        private readonly IBlobContainerFactory _blobContainerFactory;

        public FileBrowserModel(IContentRepository contentRepository, AzureBlobFileProvider azureBlobFileProvider, IBlobContainerFactory blobContainerFactory)
        {
            _contentRepository = contentRepository;
            _azureBlobFileProvider = azureBlobFileProvider;
            _blobContainerFactory = blobContainerFactory;
            var httpContext = ServiceLocator.Current.GetInstance<IHttpContextAccessor>().HttpContext;
            httpContext.Items.Add("Area", "EZMS");
            BasePath = azureBlobFileProvider.DocumentContainer;
        }

        public void OnGet()
        {
            var directoryContent = _azureBlobFileProvider.GetDirectoryContents("");
            Folders = directoryContent.Where(f => f.IsDirectory).Select(f => new Folder(f));
        }

        public IActionResult OnGetFolderData(string virtualPath)
        {
            var contents = _azureBlobFileProvider.GetDirectoryContents(virtualPath);
            var folders = contents.Where(w => w.IsDirectory).Cast<AzureBlobFileInfo>().Select(w => new Folder(w));
            var files = contents.Where(w => !w.IsDirectory).Cast<AzureBlobFileInfo>();
            return new JsonResult(new { folders, files });
        }

        public IActionResult OnGetSubFolderData(string virtualPath)
        {
            var contents = _azureBlobFileProvider.GetDirectoryContents(virtualPath);
            var folders = contents.Where(w => w.IsDirectory).Cast<AzureBlobFileInfo>().Select(w => new Folder(w));
            return new JsonResult(new { folders });
        }

        public async Task<IActionResult> OnGetFileUsageDataAsync(string virtualPath)
        {
            var contents = await _contentRepository.FindStringInModel(virtualPath);
            return new JsonResult(new { contents = contents.Select(w => new { w.Id, w.Name, w.Published, Url = w.GetContentFullUrlSlug() }) });
        }

        public async Task<IActionResult> OnGetDeleteAsync(string virtualPath)
        {
            var container = _blobContainerFactory.GetContainer();
            var blob = await container.GetBlobReferenceFromServerAsync(_blobContainerFactory.TransformPath(virtualPath));
            var result = await blob.DeleteIfExistsAsync();
            return new JsonResult(new { result });
        }

        public class Folder
        {
            //private readonly AzureBlobFileProvider _azureBlobFileProvider;

            public Folder()
            {
                //_azureBlobFileProvider = ServiceLocator.Current.GetInstance<AzureBlobFileProvider>();
            }

            public Folder(IFileInfo azureFolder) : this()
            {
                Name = System.IO.Path.GetFileName(azureFolder.Name);
                Path = azureFolder.PhysicalPath;
                VirtualPath = azureFolder.Name;

                //var contents = _azureBlobFileProvider.GetDirectoryContents(Path);
                //Folders = contents.Where(w => w.IsDirectory).Cast<AzureBlobFileInfo>().Select(w => new Folder(w)).ToList();
                //Files = contents.Where(w => !w.IsDirectory).Cast<AzureBlobFileInfo>().ToList();
            }

            public string Name { get; set; }
            public string Path { get; set; }
            public string VirtualPath { get; set; }
            public IEnumerable<Folder> Folders { get; set; }
            public IEnumerable<AzureBlobFileInfo> Files { get; set; }
        }

        [NonAction]
        protected virtual PartialViewResult PartialView(string viewName, object model)
        {
            var viewData = new ViewDataDictionary(
                ServiceLocator.Current.GetInstance<IModelMetadataProvider>(),
                new ModelStateDictionary()
            ) {
                Model = model
            };

            return new PartialViewResult {
                ViewName = viewName,
                ViewData = viewData,
                TempData = TempData
            };
        }
    }
}