var PriceTier = new Object();

PriceTier.init = function () {
    PriceTier.configureProductPopup();
    $('a#aProductSelectionPopupOpener').click(function () {
        PriceTier.loadProductData(true, 1);
    });
    $('a#aProductsAddToTier').click(PriceTier.addProductsToTier);
    $('a#aProductsSearch').click(function () {
        PriceTier.loadProductData(false, 1);
    });
    $('a#aProductsSearchReset').click(PriceTier.resetControls);

    $(document).on('click', 'input.itemscheckbox:checkbox', CheckboxSelection.trackSelectedItems);
}

PriceTier.addProductsToTier = function () {
    var dvPopup = $('div#dvProductSelectionPopup');
    var inventoryIds = CheckboxSelection.getSelectedValues();

    if (inventoryIds.length) {
        $.ajax({
            url: dvPopup.data().addproductstotierurl,
            type: "POST",
            data: JSON.stringify(inventoryIds),
            dataType: "json",
            contentType: 'application/json',
            success: function (data) {
                $('div#dvPriceTierItems').html(data.result);
                $('div#dvProductSelectionPopup').dialog('close');
                PriceTier.resetControls();
            },
            error: function (result) {
                console.error("error: " + result);
            }
        });
    }
}

PriceTier.removeProduct = function (id) {
    var dvPriceTier = $('div#dvPriceTierItems');
    var dataToPost = {};
    dataToPost.id = id;

    if (dvPriceTier.length && id) {
        $.ajax({
            url: dvPriceTier.data().removeproducturl,
            type: "POST",
            data: JSON.stringify(dataToPost),
            dataType: "json",
            contentType: 'application/json',
            success: function (data) {
                $('div#dvPriceTierItems').html(data.result);
            },
            error: function (result) {
                //console.error("error: " + result);
                console.log(result);
            }
        });
    }
}

PriceTier.loadProductData = function (isOpenPopup, pageNumber) {
    var page = 1;
    if (pageNumber) {
        page = pageNumber;
    }

    var aOpener = $('a#aProductSelectionPopupOpener');
    if (aOpener.length) {
        var requestData = {
            ProductNumber: $('input#ProductNumber').val(),
            ProductName: $('input#ProductName').val(),
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

PriceTier.updatePrice = function (event) {
    //var ctrl = $(this);
    PriceTier.manageProcessing(true);
    var dvPriceTier = $('div#dvPriceTierItems');

    if (event && dvPriceTier) {
        var requestData = {};
        requestData.ProductId = event.getAttribute("productId");
        requestData.Price = event.value;

        $.ajax({
            url: dvPriceTier.data().updatepriceurl,
            type: "POST",
            data: JSON.stringify(requestData),
            dataType: "json",
            contentType: 'application/json',
            success: function (result) {
                PriceTier.manageProcessing(false);
            },
            error: function (result) {
                PriceTier.manageProcessing(false);
                $('div#searchResults').html('Could not update the price.');
            }
        });
    }
}

PriceTier.manageProcessing = function (show) {
    if (show) {
        $('div.processing').show();
        $('input#btnSaveTier').hide();
    }
    else {
        $('div.processing').hide();
        $('input#btnSaveTier').show();
    }
}
PriceTier.configureProductPopup = function () {
    $('div#dvProductSelectionPopup').dialog(
        {
            autoOpen: false,
            hide: { duration: 500 },
            modal: true,
            show: { duration: 500 },
            width: 800,
            close: function (event, ui) {
                PriceTier.resetControls();
            }
        }
    );
}

PriceTier.resetControls = function () {
    $('input#ProductNumber').val('');
    $('input#ProductName').val('');    
    PriceTier.loadProductData(false, 1);
    CheckboxSelection.resetSelection();
}

$(document).ready(PriceTier.init);