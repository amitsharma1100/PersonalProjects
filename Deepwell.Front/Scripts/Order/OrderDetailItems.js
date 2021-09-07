var OrderDetailItems = new Object();

OrderDetailItems.init = function () {
    OrderDetailItems.configureProductPopup();
    $('a#aProductsAddToOrder').click(OrderDetailItems.addProductsToOrder);
    $(document).on('blur', 'input.orderItemQuantity', OrderDetailItems.updateQuantity);
    $(document).on('click', 'a[data-itemaction]', OrderDetailItems.updateOrderItemStatus);
    $(document).on('click', 'a.returnQuantityPopupOpener', function () {
        var source = $(this);
        var currentStatus = source.parent().parent().data('currentstatus');
        var ctrl = $('div#dvProcessReturnMultipleItems');
        if (ctrl) {
            ctrl.data('itemstatus', currentStatus);
        }

        $('div#dvOrderItemsReturnPopUp' + source.data().index).last().dialog('open');
    });

    $(document).on('click', 'a.shipQuantityPopupOpener', function () {
        var source = $(this);        
        $('div#dvOrderItemsShipPopUp' + source.data().index).last().dialog('open');
    });

    $(document).on('click', 'a[data-item="ship"]', OrderDetailItems.shipOrderItem);
    $(document).on('click', 'a[data-item="return"]', OrderDetailItems.returnOrderItem);
}

OrderDetailItems.configureProductPopup = function () {
    $('div.orderItemsReturnPopUp,div.orderItemsShipPopUp').dialog(
        {
            autoOpen: false,
            height: 215,
            hide: { duration: 500 },
            maxHeight: 400,
            maxWidth: 200,
            modal: true,
            show: { duration: 500 },
            width: 300
        }
    );
}

OrderDetailItems.addProductsToOrder = function () {
    var dvPopup = $('div#dvProductSelectionPopup');
    var inventoryIds = CheckboxSelection.getSelectedValues();   

    if (inventoryIds.length) {
        $.ajax({
            url: dvPopup.data().addproductstoorderurl,
            type: "POST",
            data: JSON.stringify(inventoryIds),
            dataType: "json",
            contentType: 'application/json',
            success: function (data) {
                $('div#dvOrderDetailItems').html(data.result);
                $('div#dvOrderSummary').html(data.orderSummary);
                $('div#dvProductSelectionPopup').dialog('close');
                OrderDetail.resetControls();
            },
            error: function (result) {
                console.error("error: " + result);
            }
        });
    }
}

OrderDetailItems.updateQuantity = function () {
    var dvOrderItems = $('div#dvOrderDetailItems');

    if (dvOrderItems.length) {
        var quantityBox = $(this);
        var quantityAvailable = parseInt(quantityBox.closest('tr').find('span.spnQuantityAvailable').text());
        var newQuantity = parseInt(quantityBox.val());
        var currentQuantity = quantityBox.data().originalquantity;

        if (currentQuantity === newQuantity)
            return false;

        //if (newQuantity > quantityAvailable) {
        //    quantityBox.val(currentQuantity);
        //    quantityBox.select();
        //    quantityBox.bounce();
        //    return false;
        //}

        var tr = quantityBox.closest('tr');

        OrderDetailItems.manageProcessing(tr, true);
        $.ajax({
            url: dvOrderItems.data().updatequantityurl,
            type: "POST",
            data: JSON.stringify({
                orderId: dvOrderItems.data().orderid,
                orderDetailId: tr.data().orderdetailid,
                locationId: tr.data().locationid,
                productId: tr.data().productid,
                quantity: newQuantity
            }),
            dataType: "json",
            contentType: 'application/json',
            success: function (data) {
                $('div#dvOrderDetailItems').empty();
                $('div#dvOrderDetailItems').html(data.result);
                $('div#dvOrderSummary').html(data.orderSummaryHtml);
                quantityBox.data().originalquantity = newQuantity;
                OrderDetailItems.configureProductPopup();   
                OrderDetailItems.manageProcessing(tr, false);
            },
            error: function (result) {
                OrderDetailItems.manageProcessing(tr, false);
                console.error("error: " + result);
            }
        });
    }
}

OrderDetailItems.manageProcessing = function (tr, show) {
    if (show) {
        tr.highlightMedium();
        tr.find('a').hide();
        tr.find('div.processing').show();
    }
    else {
        tr.find('a').show();
        tr.find('div.processing').hide();
    }
}

OrderDetailItems.updateOrderItemStatus = function () {
    var source = $(this);
    var itemAction = source.data().itemaction;

    if (OrderDetailItems.shouldUpdateItemStatus(itemAction) == false)
        return;

    var dvOrderItems = $('div#dvOrderDetailItems');
    if (dvOrderItems.length) {
        var tr = $(this).closest('tr');
        $.ajax({
            url: dvOrderItems.data().orderitemupdateurl,
            type: "POST",
            data: JSON.stringify({
                locationId: tr.data().locationid,
                productId: tr.data().productid,
                itemAction: itemAction,
                orderDetailId: tr.data().orderdetailid,
                orderId: dvOrderItems.data().orderid,
                quantity: tr.find('input.orderItemQuantity').val()
            }),
            dataType: "json",
            contentType: 'application/json',
            success: function (data) {
                if (data.success === true) {
                    $('div.orderItemsShipPopUp').dialog('close');
                    $('div#dvOrderDetailItems').html(data.result);
                    $('div#dvOrderSummary').html(data.orderSummaryHtml);
                    OrderDetailItems.manageVisibilityOfSaveButton(data.isOrderHeaderEditable);
                    OrderDetailItems.configureProductPopup();
                }
                else {
                    tr.find('input.orderItemQuantity').bounce();
                    tr.find('input.orderItemQuantity').select();
                    tr.next('tr').find('div').html(data.message);
                    tr.next('tr').find('div').show();
                    tr.next('tr').fadeIn('slow').delay(4000).fadeOut('slow');
                    console.log(data.message);
                }
            },
            error: function (result) {
                console.error("error: " + result);
            }
        });
    }
}

OrderDetailItems.manageVisibilityOfSaveButton = function (isOrderHeaderEditable) {
    $('input#IsOrderHeaderEditable').val(isOrderHeaderEditable);
    OrderDetail.manageAddressControls();
}

OrderDetailItems.validateQuantity = function (quantityBox) {
    var dvOrderItems = $('div#dvOrderDetailItems');

    if (dvOrderItems.length) {
        if (quantityBox === undefined)
            quantityBox = $(this);
        else
            quantityBox = $(quantityBox);

        var quantityAvailable = -1;
        if (quantityBox.data().quantityavailable !== undefined) {
            quantityAvailable = quantityBox.data().quantityavailable;
        }

        var quantityToUpdate = parseInt(quantityBox.val());
        var maxQuantity = quantityBox.data().maxquantity;
        var quantityAvailable = quantityBox.data().quantityavailable;

        if (quantityToUpdate < 1) {
            OrderDetailItems.displayErrorMessage(quantityBox, 'Quantity should be greater than 0');
            return false;
        }

        if (quantityAvailable == 0) {
            OrderDetailItems.displayErrorMessage(quantityBox, 'This item is currently out of stock.');
            return false;
        }

        if (quantityToUpdate > maxQuantity) {
            OrderDetailItems.displayErrorMessage(quantityBox, 'You can ship/return ' + maxQuantity + ' quantity for this item.');
            return false;
        }

        if (quantityAvailable !== -1 && quantityToUpdate > quantityAvailable) {
            OrderDetailItems.displayErrorMessage(quantityBox, 'Only ' + quantityAvailable + ' quantity is available for this item in inventory.');
            return false;
        }

        return true;
    }
}

OrderDetailItems.displayErrorMessage = function (quantityBox, message) {
    quantityBox.val(quantityBox.data().maxquantity);
    quantityBox.select();
    quantityBox.bounce();

    var dvMessage = quantityBox.siblings('div.message');
    dvMessage.html(message).show();
    dvMessage.fadeIn('slow').delay(4000).fadeOut('slow');
}

OrderDetailItems.shipOrderItem = function () {
    var source = $(this);
    var itemAction = source.data().item;
    var tr = $('div#dvOrderDetailItems').find('tr#' + source.data().trid);
    var quantityBox = source.siblings('input.shipQuantity');

    if (OrderDetailItems.validateQuantity(quantityBox) == false)
        return;

    source.hide();
    var dvOrderItems = $('div#dvOrderDetailItems');
    if (dvOrderItems.length) {
        $.ajax({
            url: dvOrderItems.data().orderitemupdateurl,
            type: "POST",
            data: JSON.stringify({
                locationId: tr.data().locationid,
                productId: tr.data().productid,
                itemAction: itemAction,
                orderDetailId: tr.data().orderdetailid,
                orderId: dvOrderItems.data().orderid,
                quantityToUpdate: quantityBox.val(),
                quantity: quantityBox.data().maxquantity
            }),
            dataType: "json",
            contentType: 'application/json',
            success: function (data) {
                if (data.success === true) {
                    $('div.orderItemsShipPopUp').dialog('close');
                    $('div#dvOrderDetailItems').html(data.result);
                    $('div#dvOrderSummary').html(data.orderSummaryHtml);
                    $('div#dvOrderActionButtons').find('#aOrderInvoice').show();
                    OrderDetailItems.manageVisibilityOfSaveButton(data.isOrderHeaderEditable);
                    OrderDetailItems.configureProductPopup();
                }
                else {
                    OrderDetailItems.displayErrorMessage(quantityBox, data.message);
                    console.log(data.message);
                }

                source.show();
            },
            error: function (result) {
                console.error("error: " + result);
                source.show();
            }
        });
    }
}


OrderDetailItems.returnOrderItem = function () {
    var source = $(this);
    var itemAction = source.data().item;
    var tr = $('div#dvOrderDetailItems').find('tr#' + source.data().trid);
    var quantityBox = source.siblings('input.returnQuantity');

    if (OrderDetailItems.validateQuantity(quantityBox) == false)
        return;

    source.hide();
    var dvOrderItems = $('div#dvOrderDetailItems');
    if (dvOrderItems.length) {
        $.ajax({
            url: dvOrderItems.data().addreturnitemtosession,
            type: "POST",
            data: JSON.stringify({
                currentStatus: tr.data().currentstatus,
                locationId: tr.data().locationid,
                productId: tr.data().productid,
                itemAction: itemAction,
                orderDetailId: tr.data().orderdetailid,
                orderId: dvOrderItems.data().orderid,
                quantityToUpdate: quantityBox.val(),
                quantity: quantityBox.data().maxquantity,
                price: tr.data().price,
                taxAmount: source.siblings('input.limitQuantity').val(),
                orderprocessid: tr.data().orderprocessid,
            }),
            dataType: "json",
            contentType: 'application/json',
            success: function (data) {
                if (data.success === true) {
                    $('div.orderItemsReturnPopUp').dialog('close');
                    $('a#aCreditMemoMultiplePopupOpener').show();
                }
                else {
                    OrderDetailItems.displayErrorMessage(quantityBox, data.message);
                }

                source.show();
            },
            error: function (result) {
                console.error("error: " + result);
                source.show();
            }
        });
    }
}

OrderDetailItems.shouldUpdateItemStatus = function (itemAction) {
    if (itemAction === "remove" || itemAction === "cancel" || itemAction === "return") {
        return confirm("Please confirm to " + itemAction + " the item?");
    }
    else {
        return true;
    }
}

$(document).ready(OrderDetailItems.init);