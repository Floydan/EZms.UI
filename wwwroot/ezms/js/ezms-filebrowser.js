(function ($) {
    window.EzmsFileBrowser = function (antiForgeryToken, basePath, folderDataUrl, subFolderDataUrl, fileUsageDataUrl, deleteUrl, editUrl) {

        function getAntiForgeryToken() {
            var token = antiForgeryToken;
            token = $(token).val();
            return token;
        }

        function getFolderData(url) {
            const filesList = $('#file-list');
            filesList.empty();
            $('.mini-loading').show();


            $('.preview-content').addClass('d-none');

            $.ajax(`${folderDataUrl}&virtualPath=${url}`).done(function (result) {
                const folders = result.folders;
                const files = result.files;

                for (var i = 0, l = files.length; i < l; i++) {
                    const file = files[i];

                    const physicalPath = file.physicalPath.split('/');
                    const virtualPath = physicalPath.slice(4, physicalPath.length).join('/');
                    const item = $(`
                        <li class="d-flex align-items-center filebrowser-file" data-virtualpath="${virtualPath}">
                            <div class="text-truncate">${file.name}</div>
                            <div class="ml-auto js-copy" style="cursor: copy;" title="Click to copy path">
                                <i class="far fa-copy"></i>
                            </div>
                        </li>`);

                    item.data('file', file);

                    filesList.append(item);
                }

                bindClickEvents();

                $('.mini-loading').hide();
            });
        }

        function getSubFolders(target, url) {
            $.ajax(`${subFolderDataUrl}&virtualPath=${url}`).done(function (result) {
                const folders = result.folders;

                addSubFolder(target, folders || []);
            });
        }

        function getFileUsageData(url, onComplete) {
            $.ajax(`${fileUsageDataUrl}&virtualPath=${url}`).done(function (result) {
                const contents = result.contents || [];

                if (onComplete)
                    onComplete(contents);
            });
        }

        function deleteFile(url, onComplete) {
            $.ajax(`${deleteUrl}&virtualPath=${url}`).done(function (result) {
                if (onComplete)
                    onComplete(result);
            });
        }

        function addSubFolder(target, folders) {
            const ul = $('<ul class="list-unstyled ml-3"></ul>');
            for (var i = 0, l = folders.length; i < l; i++) {
                const folder = folders[i];

                ul.append($(`
                    <li data-virtualpath="${folder.virtualPath}">
                        <div class="d-flex align-items-center">
                            <div class="mr-2 js-toggle-folder">
                                <i class="fas fa-caret-right"></i>
                            </div>
                            <div class="filebrowser-folder text-truncate">${folder.name}</div>
                            <div class="ml-auto js-copy" title="Click to copy path">
                                <i class="far fa-copy"></i>
                            </div>
                        </div>
                    </li>`));
            }

            $(target).append(ul);

            bindClickEvents();
        }

        function copyFilePath(e) {
            e.preventDefault();
            let virtualPath = $(e.target).data('virtualpath');
            if (!virtualPath)
                virtualPath = $(e.target).parents('[data-virtualpath]').data('virtualpath');

            virtualPath = `/${basePath}/${virtualPath}`;
            addToClipboard(virtualPath);

            return false;
        }

        function addToClipboard(str) {

            var el = document.createElement('textarea');
            // Set value (string to be copied)
            el.value = str;
            // Set non-editable to avoid focus and move outside of view
            el.setAttribute('readonly', '');
            el.style = { position: 'absolute', left: '-9999px' };
            document.body.appendChild(el);
            // Select text inside element
            el.select();
            // Copy text to clipboard
            document.execCommand('copy');
            // Remove temporary element
            document.body.removeChild(el);
        }

        function bindClickEvents() {
            $('.filebrowser-folder').off('click').on('click', function (e) {
                $('.filebrowser-folder').removeClass('active');
                const target = $(e.target).addClass('active');
                let virtualPath = target.data('virtualpath');
                if (!virtualPath)
                    virtualPath = target.parents('[data-virtualpath]').data('virtualpath');
                getFolderData(virtualPath);
            });

            $('.js-toggle-folder').off('click').on('click', function (e) {
                var toggler = $(e.target);
                let virtualPath = toggler.data('virtualpath');
                if (!virtualPath)
                    virtualPath = toggler.parents('[data-virtualpath]').data('virtualpath');

                const parent = toggler.parents('li').eq(0);

                const subFolders = $('> ul', parent);
                const hasSubFolders = !subFolders ? false : subFolders.length !== 0;
                if (!hasSubFolders)
                    getSubFolders(parent, virtualPath);

                if (toggler.hasClass('fa-caret-right')) {
                    toggler.removeClass('fa-caret-right').addClass('fa-caret-down');
                    if (hasSubFolders) subFolders.show();
                } else {
                    toggler.removeClass('fa-caret-down').addClass('fa-caret-right');
                    if (hasSubFolders) subFolders.hide();
                }
            });

            $('.filebrowser-file').off('click').on('click', function (e) {
                const target = $(e.target);
                $('.filebrowser-file').removeClass('active');

                target.addClass('active');

                let file = target.data('file');
                let virtualPath = target.data('virtualpath');
                if (!file) {
                    const parent = target.parents('li').eq(0);
                    file = parent.data('file');
                    virtualPath = parent.data('virtualpath');

                    parent.addClass('active');
                }

                $('.preview-content').removeClass('d-none');

                if (isImage(file.physicalPath)) {
                    $('.preview-image').attr('src', file.physicalPath).removeClass('d-none');
                } else {
                    $('.preview-image').attr('src', '').addClass('d-none');
                }

                $('.preview-name').text(file.name);
                $('.preview-date').text(new Date(file.lastModified).toUTCString());
                $('.preview-size').text(file.size);

                const usedByList = $('.file-used-by');
                const deleteFileButton = $('.delete-file').addClass('d-none');
                usedByList.empty();

                getFileUsageData(virtualPath,
                    (contents) => {
                        if (contents.length === 0) {
                            deleteFileButton.removeClass('d-none');

                            deleteFileButton.off('click').on('click', (evt) => {
                                evt.preventDefault();
                                deleteFile(virtualPath, (result) => {
                                    if (result.result === true) {
                                        $('.filebrowser-file.active').remove();
                                    }
                                });
                            });

                            return;
                        }

                        for (var i = 0, l = contents.length; i < l; i++) {
                            const content = contents[i];

                            const item = $(`<li><a href="${content.url}">${content.name}</a></li>`);
                            item.on('click', (evt) => {
                                evt.preventDefault();
                                window.parent.ezmsNavigation.setIframeSrc(editUrl.replace('0', content.id));
                            });

                            usedByList.append(item);
                        }
                    });
            });

            $('.js-copy').off('click').on('click', copyFilePath);
        }

        function getExtension(fileName) {
            if (!fileName) return '';
            const parts = fileName.split('.');
            return parts[parts.length - 1].toLowerCase();
        }

        function isImage(path) {
            const extension = getExtension(path);
            return /jpg|jpeg|bmp|gif|png|tiff/i.test(extension);
        }

        function init() {
            bindClickEvents();
        }

        return {
            init: init
        };
    };
})(jQuery);