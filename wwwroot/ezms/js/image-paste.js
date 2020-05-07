// ==UserScript==
// @name         StackEdit Image Extension
// @namespace    http://chri.sh/
// @version      0.4.1
// @description  Add image-pasting capability to StackEdit editor.
// @author       chrahunt
// @match        https://stackedit.io/editor
// @run-at       document-end
// @grant        none
// @downloadURL  https://gist.github.com/chrahunt/c27686b095a390c26ff8/raw/se-image-paste.user.js
// ==/UserScript==

function imagePasteHandler(selector) {

    function onPaste(event) {

        const clipboardData = event.clipboardData || event.originalEvent.clipboardData || window.ClipboardData;
        const items = clipboardData.items || [];

        if (items.length === 1 && /text\/plain/.test(items[0].type)) {
            //check if youtube an rewrite pasted data with embed

            items[0].getAsString(w => {
                const regex1 = new RegExp('^https://www\\\.youtube\\\.com/watch\\\?v=(\\w+)(?:$|&t=(\\d+))?$', 'i');
                const regex2 = new RegExp('^https://youtu\\\.be/([^\\\?]*)(?:$|\\\?t=(\\d+))', '');
                let matches = regex1.exec(w) || [];

                if (matches.length === 0)
                    matches = regex2.exec(w) || [];

                if (matches.length > 1) {
                    const id = matches[1];
                    let time = '';

                    if (matches.length > 2 && matches[2]) {
                        time = `?start=${matches[2]}`;
                    }

                    const target = $(event.target);
                    const embedCode =
                        `\n<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/${id}${time}" frameborder="0" allow="accelerometer; encrypted-media; gyroscope;" allowfullscreen></iframe>\n`;

                    let val = target.val();
                    val = val.replace(w, embedCode);
                    target.val(val);

                    setTimeout(() => target.trigger('textarea-adjust'), 100);
                }
            });
        }

        uploadFiles(items, event);
    }

    function uploadFiles(items, event) {
        for (let i = 0; i < items.length; i++) {
            const item = items[i];

            if (!/text\/(html|plain)/.test(item.type)) {
                const file = getFile(item);
                if (!file) return;

                const fileId = Date.now();
                let fileName = file.name;
                const folderName = $($(event.target).attr('data-folder-reference')).val();
                if (folderName && folderName.length !== 0) {
                    fileName = pathJoin([folderName, fileName]);
                }

                insertUploadToken(fileId, fileName, event);

                const formData = new FormData();
                formData.append('file', file, fileName);

                $.ajax({
                    url: `${window.baseUrl}ezms/imageupload/upload`,
                    type: 'post',
                    method: 'post',
                    data: formData,
                    cache: false,
                    contentType: false,
                    processData: false,
                    success: result => {
                        replaceUploadToken(fileId, (result || [])[0], event);
                    },
                    error: error => {
                        console.error(error);
                    },
                    complete: data => {
                        $(event.target).trigger('upload-complete', { success: data.status === 200, result: data.status === 200 ? data.responseJSON[0] : '' });
                    }
                });
            }
        }
    }

    function isFileObject(item) {
        return (item.name !== 'undefined' && typeof item.size !== 'undefined');
    }

    function getFile(item) {
        if (isFileObject(item)) return item;
        if (typeof item.getAsFile !== 'undefined') return item.getAsFile();
        return null;
    }

    function getExtension(fileName) {
        if (!fileName) return '';
        const parts = fileName.split('.');
        return parts[parts.length - 1].toLowerCase();
    }

    function pathJoin(parts, sep) {
        const separator = sep || '/';
        parts = parts.map((part, index) => {
            if (index) {
                part = part.replace(new RegExp('^' + separator), '');
            }
            if (index !== parts.length - 1) {
                part = part.replace(new RegExp(separator + '$'), '');
            }
            return part;
        });
        return parts.join(separator);
    }

    function onDrop(event) {
        event.stopPropagation();
        event.preventDefault();

        const dataTransfer = event.dataTransfer || event.originalEvent.dataTransfer;
        if (!dataTransfer) return false;

        const items = dataTransfer.items;

        uploadFiles(items, event);

        return false;
    }

    function onChange(event) {
        event.stopPropagation();
        event.preventDefault();

        const files = event.target.files;
        if (!files) return false;

        uploadFiles(files, event);

        return false;
    }

    function insertUploadToken(id, filename, event) {
        if (!event || !event.target) return;

        const target = $(event.target);
        const val = target.val();

        let position = -1;
        try {
            if (typeof event.target.selectionStart !== 'undefined') {
                position = event.target.selectionStart || -1;
            }
        } catch (ex) {
            //This check fails miserably on iphone so we swallow this exception
        }

        const token = `![Uploading file](${id})`;

        if (target.data('no-md')) {
            const sibling = target.parents('.input-group').find('input[type=text]');
            if (sibling && sibling.length !== 0)
                sibling.val('Uploading file...');
            else
                target.val('Uploading file...');
            return;
        }
        else if (!target.data('no-md') && (target.attr('type') === 'file' || target.hasClass('drop-area'))) {
            $('div[data-js-element="upload-status-container"]').removeClass('d-none');
            $('.upload-status')
                .show()
                .append(`<li data-id="${id}">Uploading file '${filename}'</div>`);
            return;
        }

        if (position < 0)
            target.val(val + token);
        else {
            const output = [val.slice(0, position), token, val.slice(position)];
            target.val(output.join(''));
        }
    }

    function replaceUploadToken(id, filePath, event) {
        if (!event || !event.target) return;

        const target = $(event.target);
        let val = target.val();

        const addImageChar = isImage(filePath);

        var fullPath = pathJoin([window.baseUrl, 'files', filePath]);

        const uploadToken = `!\[Uploading file]\(${id})`;
        const replacementToken = `${addImageChar ? '!' : ''}[image-${id}](${fullPath})`;

        if (target.data('no-md') === true) {
            val = fullPath;
        }
        else if (!target.data('no-md') && (target.attr('type') === 'file' || target.hasClass('drop-area'))) {
            $('div[data-js-element="upload-status-container"]').removeClass('d-none');
            $('.upload-status')
                .find(`li[data-id=${id}]`)
                .html(`Upload "${id}" complete, filepath: "${fullPath}"`);
            return;
        } else {
            val = val.replace(uploadToken, replacementToken);
        }

        const sibling = target.parents('.input-group').find('input[type=text]');
        if (sibling && sibling.length !== 0) {
            sibling.val(val);
            sibling.trigger('upload-complete').trigger('change');
        } else {
            target.val(val);
            target.trigger('change');
        }
    }

    function isImage(path) {
        const extension = getExtension(path);
        return /jpg|jpeg|bmp|gif|png|tiff/i.test(extension);
    }

    function onDragOver(evt) {
        evt.stopPropagation();
        evt.preventDefault();

        const dataTransfer = evt.dataTransfer || evt.originalEvent.dataTransfer;
        if (!dataTransfer) return;

        dataTransfer.dropEffect = 'copy';
    }

    function runOnEditor(fn, retry) {
        retry = retry || 0;

        if (retry > 2) return;

        const editor = $(selector);
        if (!editor || editor.length === 0) {
            setTimeout(function () {
                runOnEditor(fn, ++retry);
            }, 100);
        } else {
            fn(editor);
        }
    }

    if (typeof (selector) === 'string') {
        runOnEditor(function (editors) {

            for (let i = 0, l = editors.length; i < l; i++) {
                const elem = $(editors[i]);
                elem.data('no-md', editors[i].localName === 'input' && editors[i].type === 'text');

                elem.off('paste').on('paste', onPaste);
                elem.off('dragover').on('dragover', onDragOver);
                elem.off('drop').on('drop', onDrop);
                //elem.off('change').on('change', onChange);
                const fileSibling = elem.parents('.input-group').find('input[type=file]');
                if (fileSibling && fileSibling.length !== 0) {
                    fileSibling
                        .off('change')
                        .on('change', onChange)
                        .off('click')
                        .on('click', function (e) { $(e.target).val(''); });
                    fileSibling.data('no-md', elem.data('no-md'));
                }
            }
        });

        return $(selector);
    } else {
        for (let i = 0, l = selector.length; i < l; i++) {
            const elem = $(selector[i]);
            elem.data('no-md', selector[i].localName === 'input' && selector[i].type === 'text');

            elem.off('paste').on('paste', onPaste);
            elem.off('dragover').on('dragover', onDragOver);
            elem.off('drop').on('drop', onDrop);

            const fileSibling = elem.parents('.input-group').find('input[type=file]');
            if (fileSibling && fileSibling.length !== 0) {
                fileSibling
                    .off('change')
                    .on('change', onChange)
                    .off('click')
                    .on('click', function (e) { $(e.target).val(''); });
                fileSibling.data('no-md', elem.data('no-md'));
            }
        }

        return selector;
    }
}
