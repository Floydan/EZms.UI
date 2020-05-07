using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EZms.UI.Areas.EZms.Pages
{
    [Authorize(Roles = "EZmsAdmin,EZmsEditor,Administrators")]
    public class EditModel : ContentPageModel
    {
        private readonly IContentRepository _contentRepository;
        private readonly IContentVersionRepository _contentVersionRepository;
        private readonly IMapper _mapper;
        private readonly EditorReflectionService _editorReflectionService;
        private readonly ILogger<EditModel> _logger;
        private readonly ICachedContentTypeControllerMappings _typeMappings;

        public EditModel(
            IContentRepository contentRepository,
            IContentVersionRepository contentVersionRepository,
            IMapper mapper,
            EditorReflectionService editorReflectionService,
            ILogger<EditModel> logger)
        {
            _logger = logger;
            _contentRepository = contentRepository;
            _contentVersionRepository = contentVersionRepository;
            _mapper = mapper;
            _editorReflectionService = editorReflectionService;
            _typeMappings = ServiceLocator.Current.GetInstance<ICachedContentTypeControllerMappings>();

            var httpContext = ServiceLocator.Current.GetInstance<IHttpContextAccessor>().HttpContext;
            httpContext.Items.Add("Area", "EZMS");
        }

        public async Task OnGetAsync(int id, int? version)
        {
            var content = await _contentRepository.GetContent(id);

            var contentVersion = await _contentVersionRepository.GetContent(version ?? content.SavedVersion);
            var contentVersions = await _contentVersionRepository.GetAll(content.Id);

            Id = content.Id;
            ParentId = contentVersion?.ParentId ?? content.ParentId;
            Name = contentVersion?.Name ?? content.Name;
            UrlSlug = contentVersion?.UrlSlug ?? content.UrlSlug;

            Properties = _editorReflectionService.GetModelProperties((IContent)contentVersion ?? content, out var modelType);

            ContentTypeGuid = modelType.GetPageDataValues().Guid;
            SavedVersion = contentVersion?.Id ?? 0;
            PublishedVersion = content.PublishedVersion;
            Published = content.Published;
            ContentVersions = contentVersions.OrderByDescending(w => w.Id);
            AllowedGroups = contentVersion != null ? contentVersion.AllowedGroups : content.AllowedGroups;
            
            //ContentModel = JsonConvert.DeserializeObject(content.ModelAsJson, content.ModelType) as IContent;
        }

        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync(int id, ContentPostModel model, IFormCollection formCollection)
        {
            try
            {
                var content = await _contentRepository.GetContent(id);
                var latestUnpublishedVersion = (await _contentVersionRepository.GetAll(id)).Where(w => w != null && w.Id > content.PublishedVersion).OrderByDescending(w => w.Id).FirstOrDefault();
                var contentVersion = await _contentVersionRepository.GetContent(model.SavedVersion);
                var currentVersion = await _contentVersionRepository.GetContent(model.SavedVersion);

                if (string.IsNullOrWhiteSpace(model.UrlSlug) && ParentId.HasValue)
                {
                    model.UrlSlug = Name.ReplaceDiacritics();
                }

                if (!string.IsNullOrWhiteSpace(model.UrlSlug))
                    model.UrlSlug = model.UrlSlug.ReplaceDiacritics();

                if (model.Id == model.ParentId)
                {
                    Id = content.Id;
                    ParentId = contentVersion?.ParentId ?? content.ParentId;
                    Name = contentVersion?.Name ?? content.Name;
                    UrlSlug = contentVersion?.UrlSlug ?? content.UrlSlug;
                    Properties = _editorReflectionService.GetModelProperties((IContent)contentVersion ?? content, out var modelType, formCollection);
                    ContentTypeGuid = modelType.GetPageDataValues().Guid;
                    AllowedGroups = contentVersion != null ? contentVersion.AllowedGroups : content.AllowedGroups;

                    ModelState.AddModelError(nameof(IContent.ParentId), $"{nameof(IContent.ParentId)} can't be set to the current content id");

                    TryValidateModel(model);
                    if (Request.IsAjaxRequest())
                        return new JsonResult(new { Result = false });

                    return Page();
                }

                var createNewVersion = false;
                if (contentVersion == null)
                {
                    createNewVersion = true;
                    contentVersion = _mapper.Map<ContentVersion>(content);
                }
                else
                {
                    if (content.PublishedVersion == contentVersion.Id)
                    {
                        createNewVersion = true;
                    }

                    if (latestUnpublishedVersion != null && latestUnpublishedVersion.Id > contentVersion.Id && latestUnpublishedVersion.Id > content.PublishedVersion)
                    {
                        contentVersion = _mapper.Map<ContentVersion>(content);
                        contentVersion.Id = latestUnpublishedVersion.Id;
                        createNewVersion = false;
                    }
                }

                contentVersion.Name = model.Name;
                contentVersion.UrlSlug = model.UrlSlug;
                contentVersion.ParentId = model.ParentId;
                contentVersion.Order = content.Order;
                contentVersion.AllowedGroups = model.AllowedGroups.IsNullOrEmpty() ? null : model.AllowedGroups.ToList();
                _editorReflectionService.AddPropertiesToModel(contentVersion, formCollection);

                if (string.IsNullOrEmpty(contentVersion.ContentTypeGuid))
                    contentVersion.ContentTypeGuid = _typeMappings.GetContentType(model.ContentTypeGuid).GetPageDataValues().Guid;

                var cv = _mapper.Map<ContentVersion>(contentVersion);
                cv.ContentId = content.Id;  

                if (cv.CompareTo(currentVersion) == 0)
                {
                    try
                    {
                        ContentVersion version;
                        if (createNewVersion)
                            version = await _contentVersionRepository.Create(cv);
                        else
                            version = await _contentVersionRepository.Update(cv);

                        content.SavedVersion = version.Id;
                        await _contentRepository.Update(content);

                        currentVersion = version;
                    }
                    catch (Exception ex)
                    {
                        Id = content.Id;
                        ParentId = contentVersion?.ParentId ?? content.ParentId;
                        Name = contentVersion?.Name ?? content.Name;
                        UrlSlug = contentVersion?.UrlSlug ?? content.UrlSlug;
                        Properties = _editorReflectionService.GetModelProperties((IContent)contentVersion ?? content, out var modelType, formCollection);
                        ContentTypeGuid = modelType.GetPageDataValues().Guid;
                        AllowedGroups = contentVersion != null ? contentVersion.AllowedGroups : content.AllowedGroups;

                        ModelState.AddModelError("Error", ex.Message);
                        TryValidateModel(model);

                        if (Request.IsAjaxRequest())
                            return new JsonResult(new { Result = false });

                        return Page();
                    }
                }

                if (Request.IsAjaxRequest())
                    return new JsonResult(new { Result = true, CurrentVersion = currentVersion });

                return RedirectToPage("Edit", new { id = content.Id });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> OnGetPublishAsync(int id, int? versionId)
        {
            var mapper = ServiceLocator.Current.GetInstance<IMapper>();
            var cvs = await _contentVersionRepository.GetAll(id);
            if (cvs.IsNullOrEmpty())
            {
                var content = await _contentRepository.GetContent(id);
                var cv = mapper.Map<ContentVersion>(content);

                cv.Id = 0;
                cv.ContentId = content.Id;

                var newVersion = await _contentVersionRepository.Create(cv);

                content.SavedVersion = newVersion.Id;
                content.PublishedVersion = newVersion.Id;

                await _contentRepository.Update(content);
            }
            else
            {
                if (versionId.HasValue)
                {
                    var cv = await _contentVersionRepository.GetContent(versionId);
                    var content = mapper.Map<Content>(cv);
                    var dbContent = await _contentRepository.GetContent(id);
                    content.Id = cv.ContentId;
                    content.PublishedVersion = cv.Id;
                    content.SavedVersion = dbContent.SavedVersion;

                    await _contentRepository.Update(content);
                }
                else
                {
                    var cv = cvs.Where(w => w.Published).OrderByDescending(w => w.CreatedAt).FirstOrDefault();
                    if (cv != null)
                    {
                        var content = await _contentRepository.GetContent(id);
                        content.PublishedVersion = cv.Id;

                        await _contentRepository.Update(content);
                    }
                }
            }

            await _contentRepository.Publish(id);

            if (Request.IsAjaxRequest())
                return new JsonResult(new { Result = true });

            return RedirectToPage("Edit", new { id });
        }

        public async Task<IActionResult> OnGetUnPublishAsync(int id)
        {
            await _contentRepository.UnPublish(id);

            if (Request.IsAjaxRequest())
                return new JsonResult(new { Result = true });

            return RedirectToPage("Edit", new { id });
        }
    }
}