function clearModalInputs(target) {
    event.preventDefault();
    $('input,select,textarea', target).val('').attr('value', '').trigger('change');
    return false;
}

function addModalToForm() {
    const modal = $(event.target).parents('.modal');

    const body = modal.find('.modal-body').clone();

    const form = $(`#${modal.data('form-id')} > .groups`);
    const groups = $('> .model-group', form);

    const inputs = $('input,select,textarea', body);
    for (let i = 0; i < inputs.length; i++) {
        const input = $(inputs[i]);
        if (input.attr('name')) {
            input.attr('name', input.attr('name').replace('[0]', `[${groups.length}]`));
        }
    }

    const container = $(`<div class="model-group group-${groups.length} border border-success p-2 mt-2" style="position:relative;">
                            <a href="#" class="temporary-entry-sort-btn text-secondary p-2 sortable-handle"><span class="fas fa-sort"></span></a>
                            <a href="#" class="temporary-entry-remove-btn btn btn-secondary btn-sm"><span class="fas fa-trash"></span></a>
                        </div>`);
    container.append(body);
    form.append(container);

    $('.temporary-entry-remove-btn').off('click').on('click', deleteTemporaryEntry);
    $(document).trigger('reinitialize-change-listeners').trigger('autosave');
}

function deleteTemporaryEntry(e) {
    if(e) e.preventDefault();
    $(e.target).parents('.model-group').remove();
    $(document).trigger('reinitialize-change-listeners').trigger('autosave');
    return false;
}