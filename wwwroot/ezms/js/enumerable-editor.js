(function ($) {

    const editors = $('.enumerable-editor');
    for (let i = 0; i < editors.length; i++) {
        const editor = $(editors[i]);
        const modelName = $('input,select,textarea', editor)[0].name;
        const newItemTemplate = $('.new-item', editor).eq(0);
        $('input,select,textarea', editor).on('blur input change propertychange paste', function (e) { createNewInput(e, editor, modelName, newItemTemplate); });
    }

    function createNewInput(e, editor, modelName, template) {
        var isLast = $(e.target).parents('.list-group-item').is(':last-child');
        if ($(e.target).val() !== '') {
            if (!isLast) return;
            
            const clone = createTemplateClone(template);

            editor.append(clone);
            $('input,select,textarea', editor).off('blur input change propertychange paste').on('blur input change propertychange paste', function (evt) { createNewInput(evt, editor, modelName, template); });

            $(document).trigger('reinitialize-change-listeners');

        } else {
            if (isLast) return;

            $(e.target).parents('.list-group-item').remove();
        }
        updateLabels(editor);
    }

    function createTemplateClone(template) {
        var clone = template.clone();
        $('input,select,textarea', clone).val('');

        return clone;
    }

    function updateLabels(editor) {
        const labels = $('.input-group-text', editor);
        for (let i = 0; i < labels.length; i++) {
            $(labels[i]).text(`#${i + 1}`);
        }
    }
})(jQuery);