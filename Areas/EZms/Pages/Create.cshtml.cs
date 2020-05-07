using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EZms.Core.Extensions;
using EZms.Core.Helpers;
using EZms.Core.Models;
using EZms.Core.Repositories;
using EZms.Core.Routing;
using EZms.UI.Areas.EZms.Models;
using EZms.UI.Infrastructure.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace EZms.UI.Areas.EZms.Pages
{
    [Authorize(Roles = "EZmsAdmin,EZmsEditor,Administrators")]
    public class CreateModel : ContentPageModel
    {
        private readonly IContentRepository _contentRepository;
        private readonly IContentVersionRepository _contentVersionRepository;
        private readonly IMapper _mapper;
        private readonly EditorReflectionService _editorReflectionService;
        private readonly ICachedContentTypeControllerMappings _typeMappings;

        public CreateModel(
            IContentRepository contentRepository,
            IContentVersionRepository contentVersionRepository,
            IMapper mapper,
            EditorReflectionService editorReflectionService)
        {
            _contentRepository = contentRepository;
            _contentVersionRepository = contentVersionRepository;
            _mapper = mapper;
            _editorReflectionService = editorReflectionService;
            _typeMappings = ServiceLocator.Current.GetInstance<ICachedContentTypeControllerMappings>();

            var httpContext = ServiceLocator.Current.GetInstance<IHttpContextAccessor>().HttpContext;
            httpContext.Items.Add("Area", "EZMS");
        }

        public void OnGet(int parentId, string guid)
        {
            var content = new Content { ContentTypeGuid = guid };

            var modelType = _typeMappings.GetContentType(guid);
            var pageType = modelType.BaseType;
            var model = EditorReflectionService.GetDefault(pageType);
            ((Content)model).ContentTypeGuid = guid;
            content.ModelAsJson = JsonConvert.SerializeObject(model, Formatting.None, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
            });

            ParentId = parentId;
            Properties = _editorReflectionService.GetModelProperties(content, out var _);
            //ContentModel = JsonConvert.DeserializeObject(content.ModelAsJson, content.ModelType) as IContent;
            ContentTypeGuid = guid;
        }

        public async Task<IActionResult> OnPostAsync(int id, ContentPostModel model, IFormCollection formCollection)
        {try
            {
                var content = new Content { ContentTypeGuid = model.ContentTypeGuid };

                var modelType = _typeMappings.GetContentType(model.ContentTypeGuid);
                var pageType = modelType.BaseType;
                var cModel = EditorReflectionService.GetDefault(pageType);
                ((Content)cModel).ContentTypeGuid = model.ContentTypeGuid;
                content.ModelAsJson = JsonConvert.SerializeObject(cModel, Formatting.None, new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                });
                content.ContentTypeGuid = model.ContentTypeGuid;

                if (string.IsNullOrWhiteSpace(model.UrlSlug) && model.ParentId.HasValue)
                {
                    model.UrlSlug = model.Name.ReplaceDiacritics();
                }

                if (!string.IsNullOrWhiteSpace(model.UrlSlug))
                    model.UrlSlug = model.UrlSlug.ReplaceDiacritics();

                if (model.Id == model.ParentId)
                {
                    ParentId = content.ParentId;
                    Name = content.Name;
                    UrlSlug = content.UrlSlug;
                    Properties = _editorReflectionService.GetModelProperties(content, out var _, formCollection);
                    ContentTypeGuid = model.ContentTypeGuid;

                    ModelState.AddModelError(nameof(IContent.ParentId), $"{nameof(IContent.ParentId)} can't be set to the current content id");

                    TryValidateModel(model);
                    return Page();
                }

                content.Name = model.Name;
                content.UrlSlug = model.UrlSlug;
                content.ParentId = model.ParentId;
                _editorReflectionService.AddPropertiesToModel(content, formCollection);

                try
                {
                    await _contentRepository.Create(content);

                    var cv = _mapper.Map<ContentVersion>(content);
                    cv.ContentId = content.Id;
                    var version = await _contentVersionRepository.Create(cv);

                    content.SavedVersion = version.Id;
                    await _contentRepository.Update(content);
                }
                catch (Exception ex)
                {
                    ParentId = content.ParentId;
                    Name = content.Name;
                    UrlSlug = content.UrlSlug;
                    Properties = _editorReflectionService.GetModelProperties(content, out var _, formCollection);
                    ContentTypeGuid = model.ContentTypeGuid;

                    ModelState.AddModelError("Error", ex.Message);
                    TryValidateModel(model);
                    return Page();
                }

                return RedirectToPage("Edit", new { id = content.Id });
            }
            catch
            {
                throw;
            }
        }
    }
}