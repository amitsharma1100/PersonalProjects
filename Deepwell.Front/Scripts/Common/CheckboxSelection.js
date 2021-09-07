var CheckboxSelection = new Object();

var selectedItemsContainer = 'input#selectedItems';
var ItemsCheckboxes = 'input.itemscheckbox';

CheckboxSelection.init = function (containerSelector, checkBoxSelector) {
    if (containerSelector.length) {
        selectedItemsContainer = containerSelector;
    }

    if (checkBoxSelector.length) {
        ItemsCheckboxes = checkboxSelector;
    }
}

CheckboxSelection.resetSelection = function () {
    console.log('Reseting the selection from input field: ' + $(selectedItemsContainer).val());
    $(selectedItemsContainer).val('');
}

CheckboxSelection.setCheckboxState = function () {
    if ($(selectedItemsContainer).val().length) {
        var selectedCheckboxes = $(selectedItemsContainer).val().split(',');
        jQuery.each(selectedCheckboxes, function (i, val) {
            var controlSelector = 'input:checkbox[data-selectedvalue="' + val + '"]';
            $(controlSelector).prop('checked', true);           
        });

        console.log('Current value: ' + $(selectedItemsContainer).val());
    }
}

CheckboxSelection.trackSelectedItems = function () {
    var currentCheckBox = $(this);
    var checkedValues = CheckboxSelection.getSelectedValues();
    var selectedValue = currentCheckBox.data().selectedvalue.toString();
    if (currentCheckBox.is(':checked')) {
        checkedValues.push(selectedValue);
    }
    else {
        checkedValues = CheckboxSelection.removeItem(selectedValue);
    }

    CheckboxSelection.setValues(checkedValues);

    console.log('Saving checked values: ' + $(selectedItemsContainer).val());
}

CheckboxSelection.setValues = function (currentValues) {
    $(selectedItemsContainer).val(currentValues);
}

CheckboxSelection.getSelectedValues = function () {
    var selectedValues = [];

    if ($(selectedItemsContainer).val().length) {
        selectedValues = $(selectedItemsContainer).val().split(',');
    }

    console.log('Selected Values: ' + selectedValues);

    return selectedValues;
}

CheckboxSelection.removeItem = function (itemToRemove) {
    var currentValues = CheckboxSelection.getSelectedValues();
    currentValues.splice($.inArray(itemToRemove, currentValues), 1);

    return currentValues;
}