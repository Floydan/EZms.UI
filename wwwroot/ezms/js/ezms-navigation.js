(function ($) {
    window.EzmsNavigation = function (antiForgeryToken, setLangUrl, updateNavUrl, updateHierarchyUrl, createUrl, publishUrl, unpublishUrl, deleteUrl) {

        const setLanguageUrl = setLangUrl;
        const updateNavigationUrl = updateNavUrl;
        const updateSiteHierarchyUrl = updateHierarchyUrl;
        const createContentUrl = createUrl;
        const publishContentUrl = publishUrl;
        const unpublishContentUrl = unpublishUrl;
        const deleteContentUrl = deleteUrl;

        function getAntiForgeryToken() {
            var token = antiForgeryToken;
            token = $(token).val();
            return token;
        }

        function setHash(iframe) {
            const src = iframe.src.toLowerCase();
            window.location.hash = convertToRelative(src);
            const form = $('#selectLanguage');
            if (form) {
                let action = setLanguageUrl;

                let start = action.indexOf('&');
                if (start === -1) start = action.length;

                action = action.splice(start, 0, encodeURIComponent(`#${src}`));
                form.attr('action', `${action.replace('&amp;', '&')}`);
            }
        }

        function setIframeSrc(src) {
            if (event) event.preventDefault();
            $('.loading').show();
            src = src.toLowerCase();
            const frameSrc = document.getElementById('page-frame').src.toLowerCase();
            if (frameSrc !== src)
                document.getElementById('page-frame').src = src;

            return false;
        }

        function updateNavigation(iframe) {
            $('.loading').hide();
            iframe.style.height = '';
            iframe.style.height = (window.document.body.scrollHeight - 90) + 'px';
            var navigationContainer = $('#cms-site-navigation');

            $.ajax(updateNavigationUrl).done(function (result) {
                if (!result) return;
                const navigation = navigationContainer.find('> ul');
                if (navigation.length !== 0) {
                    navigation.remove();
                }

                navigationContainer.append($(result));
                bindSortable();

                $('.js-content-link', navigationContainer).off('click').on('click', function (e) {
                    if (e.ctrlKey) return true;
                    e.preventDefault();

                    const url = $(this).data('editurl');
                    setIframeSrc(url);
                });

                bindHoverMenuClick();

                const parts = iframe.src.split('/');
                const id = (parts[parts.length - 1].match(/[0-9]+/) || ['about:blank'])[0];
                
                if (id !== 'about:blank')
                    $(`a[data-contentid = ${id}]`, navigationContainer).addClass('font-weight-bold');
            });
        }

        function bindSortable() {
            $('ul.sortable').sortable({
                placeholder: 'sortable-placeholder',
                connectWith: 'ul.sortable',
                revert: true,
                dropOnEmpty: true,
                start: function () {
                    $('#page-frame').off('load');
                    $('.draggable-placeholder').show();
                },
                stop: function () {
                    $('#page-frame').on('load', function () { updateNavigation(this); });
                    $('.draggable-placeholder').hide();
                },
                update: function (evt, ui) {
                    //find new parent and update sortorder on siblings and self
                    const contentId = ui.item.data('id');
                    const ul = ui.item.closest('ul');
                    const children = ul.find('li').get().map(function (el) {
                        return $(el).data('id');
                    });

                    $.ajax(updateSiteHierarchyUrl,
                        {
                            dataType: 'json',
                            method: 'POST',
                            contentType: 'application/json; charset=utf-8',
                            data: JSON.stringify({
                                contentId: contentId,
                                parentId: ul.data('parentid'),
                                children: children
                            }),
                            headers: {
                                RequestVerificationToken: getAntiForgeryToken()
                            }
                        }).done(function () {
                            document.getElementById('page-frame').contentWindow.location.reload();
                        });

                }
            });
            $('li.draggable').draggable({
                connectToSortable: 'ul.sortable',
                revert: 'invalid',
                gready: true,
                handle: 'a',
                snapTolerance: 10
            });
            $('ul, li').disableSelection();
        }

        function bindHoverMenuClick() {
            $('.hover-menu').off('click').on('click',
                function (e) {
                    e.stopPropagation();
                    const contextMenu = $('#context-menu');

                    const contentId = $(this).data('id');
                    const isPublished = $(this).data('published') === true;
                    const top = e.pageY - 54;
                    const left = e.pageX - contextMenu.width();

                    handlePublishStateLinks(contextMenu, contentId, isPublished);

                    $('#delete-link', contextMenu).attr('href', `${deleteContentUrl.replace('0', contentId)}`).off('click').on('click', function(evt) {
                        evt.preventDefault();
                        setIframeSrc(convertToRelative(this.href));
                        hideContextMenu();
                    });

                    const form = $('#create-sub-page', contextMenu);

                    $('button', form).off('click').on('click',
                        function (evt) {
                            evt.stopPropagation();
                            evt.preventDefault();
                            $('#page-frame').attr('src', `${createContentUrl}?parentId=${contentId}&guid=${$('select', form).val()}`);

                            hideContextMenu();

                            $('select', form).val('');

                            return false;
                        });

                    contextMenu.css({
                        display: 'block',
                        top: top,
                        left: left
                    }).addClass('show');
                });


            //$('#context-menu a').off('click').on('click',
            //    function () {
            //        hideContextMenu();
            //    });
        }

        function convertToRelative(url) {
            return url.replace(/^(?:\/\/|[^\/]+)*/, '');
        }

        function handlePublishStateLinks(contextMenu, contentId, isPublished) {
            const publishLink = $('#publish-link', contextMenu).hide();
            const unpublishLink = $('#unpublish-link', contextMenu).hide();

            if (isPublished) unpublishLink.show();
            else publishLink.show();

            publishLink.attr('href', `${publishContentUrl.replace('0', contentId)}`);
            unpublishLink.attr('href', `${unpublishContentUrl.replace('0', contentId)}`);

            $('.js-publish-state-link').off('click').on('click', function (e) {
                e.stopPropagation();
                e.preventDefault();
                hideContextMenu();

                var url = this.href;

                $.ajax(url,
                    {
                        method: 'GET',
                        headers: {
                            RequestVerificationToken: getAntiForgeryToken()
                        }
                    }).done(function () {
                        $('.loading').show();
                        const iframe = $('#page-frame').get(0);
                        if (iframe.src.toLowerCase() === url.replace(/\?handler=.*$/i, '').toLowerCase())
                            iframe.contentWindow.location.reload();
                        else
                            updateNavigation(iframe);
                    });

                return false;
            });
        }

        function hideContextMenu() {
            const contextMenu = $('#context-menu');
            contextMenu.removeClass('show').hide();
            $('select', contextMenu).val('');
        }

        function init() {

            $('#page-frame').on('load', function () { setHash(this); updateNavigation(this); });

            updateNavigation($('#page-frame').get(0));
            bindSortable();

            var hash = window.location.hash;

            if (hash && hash.length !== 0) {
                hash = hash.substr(1);

                setIframeSrc(hash);
            }

            bindHoverMenuClick();

            $('#cms-site-navigation').on('click', hideContextMenu);
            $('#context-menu').on('click', function(e) { e.stopPropagation(); });

            $('.sidebar-toggle').on('click', function () {
                $('.sidebar-1').toggleClass('active');
            });

            $('.sidebar-toggle-2').on('click', function () {
                $('#cms-file-browser').toggleClass('active-2');
            });
        }

        return {
            init: init,
            setIframeSrc: setIframeSrc
        };
    };
})(jQuery);