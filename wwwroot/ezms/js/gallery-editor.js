(function ($) {

    const editors = $('.gallery-editor');
    for (let i = 0; i < editors.length; i++) {
        const editor = $(editors[i]);
        const modelName = $('input[type=text]', editor)[0].name;
        imagePasteHandler($('input[type=text]', editor)).on('upload-complete blur', function (e, result) {
            createNewInput(e, result, editor, modelName);
        });
    }

    function createNewInput(e, result, editor, modelName) {
        const count = $('input[type=text]', editor).length;
        const isLast = $(e.target).parents('.list-group-item').is(':last-child');
        if ((result && result.success) || $(e.target).val() !== '') {
            if (!isLast) return;
            const template = `<li class="list-group-item">
                                <div class="input-group">
                                    <div class="input-group-prepend">
                                        <small class="input-group-text" style="font-size: 0.75em">#${count + 1}</small>
                                    </div>
                                    <input type="text" placeholder="Paste, drag and drop or browse file to upload..." name="${modelName}" class="form-control" />
                                    <div class="input-group-append">
                                        <div class="custom-file">
                                            <input type="file" class="custom-file-input" style="width: 70px" value="" />
                                            <label class="custom-file-label"></label>
                                        </div>
                                    </div>
                                </div>
                            </li>`;

            editor.append(template);
            imagePasteHandler($('input[type=text]', editor)).off('upload-complete blur').on('upload-complete blur', function (evt, res) { createNewInput(evt, res, editor, modelName); });

            $(document).trigger('reinitialize-change-listeners');
        }
        else {
            if (isLast) return;

            $(e.target).parents('.list-group-item').remove();
        }
        updateLabels(editor);
    }

    function updateLabels(editor) {
        const labels = $('.input-group-text', editor);
        for (let i = 0; i < labels.length; i++) {
            $(labels[i]).text(`#${i + 1}`);
        }
    }

})(jQuery);