// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function ($, marked) {

    marked.setOptions({
    });

    const markdownToHtmlConvertor = function (markdown) {
        return marked(markdown);
    };

    const editors = $('.markdown-editor');

    for (let i = 0; i < editors.length; i++) {
        const editor = editors.get(i);

        //do shit
        const textarea = $('textarea', editor);
        const btnPreview = $('.btn-preview', editor);
        const btnWrite = $('.btn-write', editor);
        const preview = $('.preview', editor);
        const toolbarFunctions = $('.toolbar .functions', editor);
        const functionButtons = $('span.fas', toolbarFunctions);

        btnPreview.bind('click', (e) => {
            e.preventDefault();

            preview.html(marked(textarea.val()));
            toolbarFunctions.hide();
            textarea.hide();
            preview.show();

            btnPreview.addClass('active');
            btnWrite.removeClass('active');

            return false;
        });

        btnWrite.bind('click', (e) => {
            e.preventDefault();

            textarea.show();
            toolbarFunctions.show();
            preview.hide();

            btnPreview.removeClass('active');
            btnWrite.addClass('active');

            return false;
        });

        textarea.bind('input propertychange textarea-adjust', w => { textareaAdjust(w.target); });
        textarea.bind('keydown', e => {
            if (e.ctrlKey && /^(?:b|i|k){1}$/i.test(e.key)) {
                e.preventDefault();
                const target = $(e.target);
                switch (e.key.toLowerCase()) {
                    case 'k':
                        formatText(target, 'link');
                        break;
                    case 'b':
                        formatText(target, 'bold');
                        break;
                    case 'i':
                        formatText(target, 'italic');
                        break;
                }
            }

            if (e.keyCode === 13) {
                const val = e.target.value.substr(0, e.target.selectionStart);

                const lines = val.split('\n');
                const numberMatch = lines[lines.length - 1].match(/^(\d+)\./) || [];
                if (numberMatch.length > 1) {
                    e.preventDefault();

                    const num = parseInt(numberMatch[1]) + 1;
                    const numAsTxt = `\n${num}. `;

                    document.execCommand('insertText', false, numAsTxt);

                    //e.target.value = val + numAsTxt + e.target.value.substr(e.target.selectionStart);
                    //e.target.setSelectionRange(e.target.selectionStart + numAsTxt.length, e.target.selectionStart + numAsTxt.length);
                    scrollToLine($(e.target), lines.length - 1);

                    e.target.selectionStart = e.target.selectionStart;
                }
            }
        });

        textarea.bind('focusout', (e) => {
            textarea.data('position-start', e.target.selectionStart);
            textarea.data('position-end', e.target.selectionEnd);
        });

        textareaAdjust(textarea);

        functionButtons.bind('click', e => formatText(textarea, $(e.target).data('actiontype')));
    }

    $('.markdown-content').each((i, el) => {
        const e = $(el);
        e.html(marked(e.text().trim())).show();
    });

    function scrollToLine($textarea, lineNumber) {
        const lineHeight = parseInt($textarea.css('line-height'));
        $textarea.scrollTop(lineNumber * lineHeight);
    }

    function textareaAdjust(textarea) {
        if (!textarea) return;
        textarea = $(textarea);
        textarea.css('max-height', '268px');
        textarea.css('height', textarea.prop('scrollHeight') + 'px');

    }

    function formatText(textarea, type) {
        type = type.toLowerCase();

        const selectionStart = textarea[0].selectionStart || parseInt(textarea.data('position-start'));
        const selectionEnd = textarea[0].selectionEnd || parseInt(textarea.data('position-end'));

        textarea[0].selectionStart = selectionStart;
        textarea[0].selectionEnd = selectionEnd;

        textarea[0].setSelectionRange(selectionStart, selectionEnd);

        let val = $(textarea).val();
        let selection = val.substr(selectionStart, selectionEnd - selectionStart).trim('\r\n');

        let startKey = '', endKey = '';

        switch (type) {
            case 'header':
                startKey = '## ';
                break;
            case 'bold':
                startKey = '**';
                endKey = '**';
                break;
            case 'italic':
                startKey = '_';
                endKey = '_';
                break;
            case 'quote':
                startKey = '\n> ';
                break;
            case 'code':
                startKey = '```\n';
                endKey = '\n```';
                break;
            case 'link':
                startKey = '[';
                endKey = '](url)';
                break;
            case 'image':
                startKey = '![';
                endKey = '](url)';
                break;
            case 'ulist':
                startKey = '- ';
                break;
            case 'olist':
                startKey = '. ';
                break;
            case 'clist':
                startKey = '- [ ] ';
                break;
        }

        textarea.focus();

        if (selectionStart === selectionEnd && type !== 'olist') {

            const result = document.execCommand('insertText', false, startKey + endKey);
            if (!result) {
                val = [val.slice(0, selectionStart), startKey, endKey, val.slice(selectionStart)].join('');
                textarea.val(val);
            }

            if (type === 'link')
                textarea[0].setSelectionRange(selectionStart + 1, selectionStart + 1);
            else {
                textarea[0].setSelectionRange(selectionStart + startKey.length, selectionStart + startKey.length);
            }
        } else {

            let lines = selection.split('\n');
            const lineCount = lines.length;
            for (let i = 0, l = lineCount; i < l; i++) {
                lines[i] = [type === 'olist' ? i + 1 : '', startKey, lines[i], endKey].join('');
            }

            const newSelection = lineCount !== 1 ? lines.join('\n') : lines[0];

            const result = document.execCommand('insertText', false, newSelection);
            if (!result) {
                val = [
                    val.slice(0, selectionStart),
                    newSelection,
                    val.slice(selectionEnd)
                ].join('');


                textarea.val(val);
            }

            if (type === 'link') {
                textarea[0].setSelectionRange(
                    selectionStart + selection.length + 3,
                    selectionEnd + 6);
            } else {
                if (!result) {
                    const selectionPoint = selectionEnd + ((startKey.length + endKey.length) * lineCount);
                    textarea[0].setSelectionRange(selectionPoint, selectionPoint);
                }
            }
        }
    }

    imagePasteHandler('.markdown-editor > textarea');
    imagePasteHandler('.image-form-input');

    $(function () {
        $('[data-toggle="tooltip"]').tooltip();

        if (window.top !== window) {
            $('header').hide();
            $('footer').hide();
        }

        if (window.location.hash) {
            var form = $('#selectLanguage');
            if (form) form.attr('action', `${form.attr('action')}/#${window.location.hash}`);
        }

        $('.js-administration input[type=checkbox]').on('change',
            function () {
                var box = $(this);
                var isChecked = box.is(':checked');

                if (isChecked) box.val(box.data('value'));
                else box.val('false');
            });
    });

})(jQuery, marked);

if (!String.prototype.splice) {
    /**
     * {JSDoc}
     *
     * The splice() method changes the content of a string by removing a range of
     * characters and/or adding new characters.
     *
     * @this {String}
     * @param {number} start Index at which to start changing the string.
     * @param {number} delCount An integer indicating the number of old chars to remove.
     * @param {string} newSubStr The String that is spliced in.
     * @return {string} A new string with the spliced substring.
     */
    String.prototype.splice = function (start, delCount, newSubStr) {
        return this.slice(0, start) + newSubStr + this.slice(start + Math.abs(delCount));
    };
}