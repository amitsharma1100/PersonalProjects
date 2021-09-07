var MudProduct = new Object();

MudProduct.init = function () {
    //MudProduct.configureSearchableOptionList();
    MudProduct.configurePopUp();
    $('a#aProductSelectionPopupOpener').click(function () {
        MudProduct.loadProductData(true, 1);
    });

    $('a#aProductsSearch').click(function () {
        MudProduct.loadProductData(false, 1);
    });
    $('a#aProductsSearchReset').click(MudProduct.resetControls);

    $('a#aProductsAddToMud').click(MudProduct.addComponentsToProduct);
    $(document).on('click', 'a.removeComponent', MudProduct.removeComponent);
    $(document).on('blur', 'input.quantity', MudProduct.updateQuantity);

    $('input#btnSaveMudProduct').on('click', MudProduct.validateForm);

    $(document).on('click', 'input.itemscheckbox:checkbox', CheckboxSelection.trackSelectedItems);
}

MudProduct.validateForm = function () {
    var isValidData = MudProduct.isInitialInventorySet() && MudProduct.isAtleastOneComponentAdded();
    if (isValidData == false && $(this).data().iseditmode === "False") {
        alert('Please make sure you have added atleast 1 componenet for the Mud product and Initial inventory is greater than 0.');
        return false;
    }
}

MudProduct.configurePopUp = function () {
    $('div#dvProductSelectionPopup').dialog(
        {
            autoOpen: false,
            hide: { duration: 500 },
            modal: true,
            show: { duration: 500 },
            width: 700,
            close: function (event, ui) {
                MudProduct.resetControls();
            }
        }
    );
}

MudProduct.configureSearchableOptionList = function () {
    $('select#SelectedProductIds').searchableOptionList({
        showSelectAll: true,
        texts: {
            //noItemsAvailable: 'Go on, nothing to see here',
            selectAll: '',
            selectNone: 'Unselect all',
            quickDelete: 'X',
            searchplaceholder: 'Click here to search and add Products...'
        }
    });
}


MudProduct.loadProductData = function (isOpenPopup, pageNumber) {
    var page = 1;
    if (pageNumber) {
        page = pageNumber;
    }

    var aOpener = $('a#aProductSelectionPopupOpener');
    if (aOpener.length) {
        var requestData = {
            ProductNumber: $('input#searchProductNumber').val(),
            ProductName: $('input#searchProductName').val(),
            Page: page,
        };

        $.ajax({
            url: aOpener.data().getproducturl,
            type: "GET",
            data: requestData,
            success: function (result) {
                $('div#dvProductSearchResponse').html(result);
               
                if (isOpenPopup) {
                    $('div#dvProductSelectionPopup').dialog('open');                   
                }

                CheckboxSelection.setCheckboxState();
            },
            error: function (result) {
                $('div#searchResults').html('Could not fetch the product list ');
            }
        });
    }
}

MudProduct.addComponentsToProduct = function () {
    var dvPopup = $('div#dvProductSelectionPopup');
  
    var productIds = CheckboxSelection.getSelectedValues();

    if (productIds.length) {
        $.ajax({
            url: dvPopup.data().addcomponentsurl,
            type: "POST",
            data: JSON.stringify(productIds),
            dataType: "json",
            contentType: 'application/json',
            success: function (data) {
                $('div#dvComponents').html(data.result);
                $('div#dvProductSelectionPopup').dialog('close');
                MudProduct.resetControls();
            },
            error: function (result) {
                console.error("error: " + result);
            }
        });
    }
    else {
        alert("Select atleast 1 componenent to add.");
    }
}

MudProduct.removeComponent = function () {
    var source = $(this);
    var dvComponents = $('div#dvComponents');
    if (dvComponents.length) {
        var tr = $(this).closest('tr');
        $.ajax({
            url: dvComponents.data().removeurl,
            type: "POST",
            data: JSON.stringify({
                productId: tr.data().productid,
                //quantity: tr.find('input.orderItemQuantity').val()
            }),
            dataType: "json",
            contentType: 'application/json',
            success: function (data) {
                dvComponents.html(data.result);
            },
            error: function (result) {
                console.error("error: " + result);
            }
        });
    }
}

MudProduct.isInitialInventorySet = function () {
    return $('input[data-changetype="created"]').val() > 0;
}

MudProduct.isAtleastOneComponentAdded = function () {
    return $('div#dvComponents').html() !== '';
}

MudProduct.updateQuantity = function () {
    var dvComponents = $('div#dvComponents');

    if (dvComponents.length) {
        var quantityBox = $(this);
        var quantityAvailable = parseInt(quantityBox.data().quantityavailable);
        var newQuantity = parseInt(quantityBox.val());
        var currentQuantity = quantityBox.data().originalquantity;

        if (currentQuantity === newQuantity)
            return false;

        if (newQuantity > quantityAvailable || newQuantity < 1) {
            quantityBox.val(currentQuantity);
            quantityBox.select();
            quantityBox.bounce();
            return false;
        }

        var tr = quantityBox.closest('tr');
        tr.highlightMedium();
        $.ajax({
            url: dvComponents.data().updatequantityurl,
            type: "POST",
            data: JSON.stringify({
                productId: tr.data().productid,
                quantity: newQuantity
            }),
            dataType: "json",
            contentType: 'application/json',
            success: function (data) {
                dvComponents.html(data.result);
                quantityBox.data().originalquantity = newQuantity;
            },
            error: function (result) {
                console.error("error: " + result);
            }
        });
    }
}

MudProduct.resetControls = function () {
    $('input#searchProductNumber').val('');
    $('input#searchProductName').val('');    
    MudProduct.loadProductData(false, 1);
    CheckboxSelection.resetSelection();
}

$(document).ready(MudProduct.init);