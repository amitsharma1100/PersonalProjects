var OrderDetail = new Object();

OrderDetail.init = function () {
    OrderDetail.configureSelect2List();
    OrderDetail.configureProductPopup();
    OrderDetail.disableControlsForEdit();
    OrderDetail.populateShippingAddressSame();
    OrderDetail.bindAddressControls();
    OrderDetail.manageAddressControls();

    $('select#CustomerId').on('change', OrderDetail.getCustomerById);
    $('select#TierId').on('change', OrderDetail.updateTierinSession);
    $('select#InvoiceId').on('change', OrderDetail.viewInvoice);
    $('select#ShipmentId').on('change', OrderDetail.viewShipment);
    $('select#CreditMemoId').on('change', OrderDetail.viewCreditMemoReport);
    $("input[name='rbShippingAddress']").on('change', OrderDetail.populateShippingAddressSame);
    $("#OrderDateTo").datepicker();
    $("#OrderDateFrom").datepicker();

    $('a#aProductSelectionPopupOpener').click(function () {
        OrderDetail.loadProductData(true, 1);
    });

    $('a#aProductsSearch').click(function () {
        OrderDetail.loadProductData(false, 1);
    });
    $('a#aProductsSearchReset').click(OrderDetail.resetControls);
    $('a#aOrderItemInvoice').click(OrderDetail.inoviceShippedItems);
    $('a#aViewShipmentReport').click(OrderDetail.viewShipmentReport);
    $('a#aProcessCreditMemoItems').click(OrderDetail.processReturnItems);

    $(document).on('click', '#aOrderInvoice', function () {
        var source = $(this);
        $('div#dvOrderItemsInvoicePopUp').dialog('open');
        $('input#txtInvoiceTax').val('');
        $('a#aOrderItemInvoice').show();
    });

    $(document).on('keyup', 'input.limitQuantity', function () {
        this.value = this.value.replace(/[^0-9\.]/g, '');
    });

    $(document).on('click', 'input.itemscheckbox:checkbox', CheckboxSelection.trackSelectedItems);

    $(document).on('click', 'input.selectorderitems:checkbox', OrderDetail.manageItemsSelection);

    OrderDetail.configureOrderItemsPopup();

    $('a#aInvoiceMultiplePopupOpener,a#aShipMultiplePopUpOpener,a#aCreditMemoMultiplePopupOpener').click(OrderDetail.popUpOpener);

    $('a#aProcessOrderItems').click(OrderDetail.processOrderItems);

    $('input#txtTaxAmount').on('change', function () {
        OrderDetail.validateTaxAmount($(this), $('div#dvReturnError'), $('a#aProcessCreditMemoItems'));
    });

    $('input#txtTaxPercentage').on('change', function () {
        OrderDetail.validateTaxAmount($(this), $('div#divItemsError'), $('a#aProcessOrderItems'));
    });
}

OrderDetail.validateTaxAmount = function (taxField, errorMsgCtrl, btnProcessCtrl) {
    if (taxField.val() < 0 || taxField.val() > 100) {
        errorMsgCtrl.html('Value must be less than or equal to 100.');
        errorMsgCtrl.show();
        btnProcessCtrl.hide();
    }
    else {
        errorMsgCtrl.html('');
        errorMsgCtrl.hide();
        btnProcessCtrl.show();
    }
}

OrderDetail.processOrderItems = function () {
    var dvOrderDetailItems = $('div#dvOrderDetailItems');
    var currentSource = $(this);
    $('a#aProcessOrderItems').hide();

    var Ids = [];
    var itemData = [];
    dvOrderDetailItems.find(':checked').each(function () {
        var trData = $(this).closest('tr').data();
        itemData.push({
            "InventoryId": trData.inventoryid,
            "LocationId": trData.locationid,
            "OrderDetailId": trData.orderdetailid,
            "OrderProcessId": trData.orderprocessid,
            "ProductId": trData.productid,
            "Quantity": trData.quantity,
            "QuantityReturned": trData.quantityreturned,
        });
    });

    var requestData = {
        OrderId: dvOrderDetailItems.data().orderid,
        Items: itemData,
        Source: currentSource.data().source,
        PoNumber: $('input#txtPoNumber').val(),
        ShippingVia: $('input#txtShippingVia').val(),
        TaxPercent: $('input#txtTaxPercentage').val(),
        TrackingId: $('input#txtTrackingId').val(),
        Comments: $('input#txtComments').val(),
    };

    $.ajax({
        url: dvOrderDetailItems.data().processorderitems,
        type: "POST",
        data: JSON.stringify(requestData),
        dataType: "json",
        contentType: 'application/json'
    }).done(function (data) {
        $('a#aProcessOrderItems').show();
        if (data.success === true) {
            $('div#dvOrderDetailItems').html(data.result);
            $('div#dvOrderSummary').html(data.orderSummary);
            $('div#dvProcessMultipleItems').dialog('close');
            window.location.href = dvOrderDetailItems.data().orderdetailurl + '?ct=' + Date.now() + '#dvItems';
        }
        else {
            var dvItemsError = $('div#divItemsError');
            if (data.errorMessage.length) {
                dvItemsError.html(data.errorMessage);
                dvItemsError.show();
            }
            else {
                dvItemsError.html('');
                dvItemsError.hide();
            }         
        }
    }).fail(function (result) {
        $('a#aProcessOrderItems').show();
    });
}

OrderDetail.popUpOpener = function () {
    var aProcessOrderItems = $('a#aProcessOrderItems');
    aProcessOrderItems.html('');
    var dvItemsError = $('div#divItemsError');
    dvItemsError.html('');
    dvItemsError.hide();
    var source = $(this).data().source;

    var ctrl = $('div#dvProcessReturnMultipleItems');
    if (ctrl) {
        var itemStatus = ctrl.data().itemstatus;
        if (itemStatus == 'Shipped') {
            $('div#dvTaxAmount').hide();
        }
    }

    if ($('div#dvOrderDetailItems').find(':checked').length) {
        var isProcessed = $('div#dvOrderDetailItems').find(':checked').closest('tr').data().orderprocessid > 0;
        if (isProcessed) {
            $('div#dvTaxPercentage').hide();
        }
        else {
            $('div#dvTaxPercentage').show();
        }
    }
    else if (source == 'ship' || source == 'invoice'){
        alert('Select at least one item to ship or invoice.');
        return;
    }

    if (source == 'ship') {
        $('div#dvShippingFields').show();
        $('div#dvComments').hide();
        aProcessOrderItems.html('Ship');
        aProcessOrderItems.attr('title', 'Ship Items');
        aProcessOrderItems.data('source', 'ship');
    }
    else if (source == 'invoice'){
        $('div#dvShippingFields').hide();
        $('div#dvComments').show();
        aProcessOrderItems.html('Invoice');
        aProcessOrderItems.attr('title', 'Invoice Items');
        aProcessOrderItems.data('source', 'invoice');
    }

    if (source == 'ship' || source == 'invoice') {
        $('div#dvProcessMultipleItems').dialog('open');
    }

    if (source == 'return') {
        $('div#dvProcessReturnMultipleItems').dialog('open');
    }
}

OrderDetail.manageItemsSelection = function () {
    var source = $(this)
    var isChecked = source.is(':checked');
    var sourceTable = source.closest('table');
    var trData = source.closest('tr').data();
    var sourceProcessId = parseInt(trData.orderprocessid);
    if (sourceProcessId !== 0) {
        sourceTable.find('input.selectorderitems:checkbox').prop('checked', false);
        sourceTable.find('input.selectorderitems:checkbox').each(function () {
            var currentProcessId = parseInt($(this).closest('tr').data().orderprocessid);
            if (sourceProcessId === currentProcessId) {
                $(this).prop('checked', isChecked);
            }
        });
    }
    else {
        sourceTable.find('input.selectorderitems:checkbox').each(function () {
            var currentProcessId = parseInt($(this).closest('tr').data().orderprocessid);
            if (currentProcessId > 0) {
                $(this).prop('checked', false);
            }
        });
    }

    if (isChecked) {
        if (trData.currentstatus == 'Active') {
            $('a#aShipMultiplePopUpOpener').show();
            $('a#aInvoiceMultiplePopupOpener').show();
        } else if (trData.currentstatus == 'Shipped') {
            $('a#aShipMultiplePopUpOpener').hide();
            $('a#aInvoiceMultiplePopupOpener').show();
        } else if (trData.currentstatus == 'Invoiced') {
            $('a#aShipMultiplePopUpOpener').show();
            $('a#aInvoiceMultiplePopupOpener').hide();
        }
        else {
            $('a#aShipMultiplePopUpOpener').hide();
            $('a#aInvoiceMultiplePopupOpener').hide();
        }
    }
    else {
        if (sourceTable.find('input.selectorderitems:checkbox').is(':checked')) {
            $('a#aShipMultiplePopUpOpener').show();
            $('a#aInvoiceMultiplePopupOpener').show();
        }
        else {
            $('a#aShipMultiplePopUpOpener').hide();
            $('a#aInvoiceMultiplePopupOpener').hide();
        }
    }

    var checkedSelector = $('div#dvOrderDetailItems').find(':checked');
    if (checkedSelector.length) {
        var trData = checkedSelector.closest('tr').data();
        if (trData.currentstatus == 'Shipped') {
            $('a#aShipMultiplePopUpOpener').hide();
        }
    }
}

OrderDetail.manageAddressControls = function () {
    var isOrderHeaderEditable = $('input#IsOrderHeaderEditable').val() === 'True';
    var billingDiv = $('div#dvBillingAddress :input');
    var shippingDiv = $('div#dvShippingAddress :input');
    var changeAddressDiv = $('div#dvAddressChange :input');

    billingDiv.attr("disabled", isOrderHeaderEditable == false);
    shippingDiv.attr("disabled", isOrderHeaderEditable == false);
    changeAddressDiv.attr("disabled", isOrderHeaderEditable == false);
}

OrderDetail.bindAddressControls = function () {
    $('input#BillingAddress_WellName').on('blur', OrderDetail.populateShippingAddressSame);
    $('input#BillingAddress_County').on('blur', OrderDetail.populateShippingAddressSame);
    $('select#BillingAddress_StateId').on('change', OrderDetail.populateShippingAddressSame);
    $('input#BillingAddress_City').on('blur', OrderDetail.populateShippingAddressSame);
    $('input#BillingAddress_Zipcode').on('blur', OrderDetail.populateShippingAddressSame);
}

OrderDetail.configureSelect2List = function () {
    $('select#CustomerId').select2();
    $('select#TierId').select2();
}

OrderDetail.configureProductPopup = function () {
    $('div#dvProductSelectionPopup').dialog(
        {
            autoOpen: false,
            hide: { duration: 500 },
            modal: true,
            show: { duration: 500 },
            width: 800,
            close: function (event, ui) {
                OrderDetail.resetControls();
            }
        }
    );
}

OrderDetail.loadProductData = function (isOpenPopup, pageNumber) {
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

OrderDetail.getCustomerById = function () {
    var ctrl = $('select#CustomerId');
    if (ctrl.length > 0) {
        var customerId = ctrl.find(":selected").val();
        var orderDetailData = $('div#orderDetailData');

        if (orderDetailData.length > 0) {
            var url = orderDetailData.data().getcustomerdetailsurl;
            var dataToPost = {
                customerId: customerId
            }

            $.ajax({
                url: url,
                type: "GET",
                data: dataToPost,
                success: function (result) {
                    if (result.success === true) {
                        OrderDetail.populateCustomerDetails(result);
                    }
                    else {
                        OrderDetail.showErrorMessage(result.errorMessage);
                    }
                },
                error: function (result) {
                    OrderDetail.showErrorMessage('Could not fetch the customer details.');
                }
            });
        }
    }
}

OrderDetail.updateTierinSession = function () {
    var ctrl = $('select#TierId');
    if (ctrl.length > 0) {
        var tierId = ctrl.find(":selected").val();
        var orderDetailData = $('div#orderDetailData');

        if (orderDetailData.length > 0) {
            var url = orderDetailData.data().updatepricetierinsessionurl;

            $.ajax({
                url: url,
                type: "POST",
                data: JSON.stringify({
                    tierId: tierId
                }),
                dataType: "json",
                contentType: 'application/json',
                success: function (data) {
                    if (data.success === true) {
                        $('div#dvOrderDetailItems').html(data.result);
                        $('div#dvOrderSummary').html(data.orderSummary);
                    }
                    else {
                        OrderDetail.showErrorMessage(data.errorMessage);
                    }
                },
                error: function (result) {
                    OrderDetail.showErrorMessage('Could not update the price tier details.');
                }
            });
        }
    }
}

OrderDetail.resetControls = function () {
    $('input#ProductNumber').val('');
    $('input#ProductName').val('');
    OrderDetail.loadProductData(false, 1);
    CheckboxSelection.resetSelection();
}

OrderDetail.populateCustomerDetails = function (response) {
    $('input#Phone').val(response.phoneNumber);
    $('input#CustomerName').val(response.customerName);
    $('input#CustomerEmail').val(response.customerEmail);
    $('input#BillingAddress_WellName').val(response.wellName);
    $('input#BillingAddress_City').val(response.city);
    $('select#BillingAddress_StateId').val(response.stateId);
    $('input#BillingAddress_County').val(response.county);
    $('input#BillingAddress_Zipcode').val(response.zipcode);
    OrderDetail.populateShippingAddressSame();
}

OrderDetail.populateShippingAddressSame = function () {
    var selectedOption = $("input[name='rbShippingAddress']:checked").val();
    var isOrderHeaderEditable = $('input#IsOrderHeaderEditable').val() === 'True';

    if (selectedOption == 'same') {
        $('input#ShippingAddress_WellName').val($('input#BillingAddress_WellName').val());
        $('input#ShippingAddress_City').val($('input#BillingAddress_City').val());
        $('select#ShippingAddress_StateId').val($('select#BillingAddress_StateId').find(":selected").val());
        $('input#ShippingAddress_StateId').val($('select#BillingAddress_StateId').find(":selected").val());
        $('input#ShippingAddress_County').val($('input#BillingAddress_County').val());
        $('input#ShippingAddress_Zipcode').val($('input#BillingAddress_Zipcode').val());

        $('input#ShippingAddress_WellName').focusout();
        $('input#ShippingAddress_City').focusout();
        $('select#ShippingAddress_StateId').focusout();
        $('input#ShippingAddress_County').focusout();
        $('input#ShippingAddress_Zipcode').focusout();

        $('input#ShippingAddress_WellName').attr("readonly", "readonly");
        $('input#ShippingAddress_City').attr("readonly", "readonly");
        $('select#ShippingAddress_StateId').attr("disabled", "disabled");
        $('input#ShippingAddress_County').attr("readonly", "readonly");
        $('input#ShippingAddress_Zipcode').attr("readonly", "readonly");
    }
    else if (isOrderHeaderEditable == true){
        $('input#ShippingAddress_WellName').val('');
        $('input#ShippingAddress_City').val('');
        $('select#ShippingAddress_StateId').val('');
        $('input#ShippingAddress_County').val('');
        $('input#ShippingAddress_Zipcode').val('');

        $('input#ShippingAddress_WellName').removeAttr("readonly", "readonly");
        $('input#ShippingAddress_City').removeAttr("readonly", "readonly");
        $('select#ShippingAddress_StateId').removeAttr("disabled", "disabled");
        $('input#ShippingAddress_County').removeAttr("readonly", "readonly");
        $('input#ShippingAddress_Zipcode').removeAttr("readonly", "readonly");
    }
}

OrderDetail.showErrorMessage = function (message) {
    var errorMsgCtrl = $('h3#divErrorMessage');
    if (errorMsgCtrl.length > 0) {
        errorMsgCtrl.html(message);
        errorMsgCtrl.show();
    }
}

OrderDetail.disableControlsForEdit = function () {
    var orderStatus = $('input#OrderStatus').val();
    if (orderStatus != 0) {
        $('select#CustomerId').attr("disabled", "disabled");
        $('select#TierId').attr("disabled", "disabled");
    }

    if (orderStatus != 0 && orderStatus != 1) {
        $('input#BillingAddress_WellName').attr("readonly", "readonly");
        $('input#BillingAddress_City').attr("readonly", "readonly");
        $('select#BillingAddress_StateId').attr("disabled", "disabled");
        $('input#BillingAddress_County').attr("readonly", "readonly");
        $('input#BillingAddress_Zipcode').attr("readonly", "readonly");

        $('input#ShippingAddress_WellName').attr("readonly", "readonly");
        $('input#ShippingAddress_City').attr("readonly", "readonly");
        $('select#ShippingAddress_StateId').attr("disabled", "disabled");
        $('input#ShippingAddress_County').attr("readonly", "readonly");
        $('input#ShippingAddress_Zipcode').attr("readonly", "readonly");
    }
}

OrderDetail.inoviceShippedItems = function () {
    var aOrderInvoice = $(this);

    if (confirm("Please confirm to Invoice the shipped item(s)?") === false)
        return;

    $('div#dvOrderItemsInvoicePopUp').dialog('close');

    $.ajax({
        url: aOrderInvoice.data().invoiceurl,
        type: "POST",
        data: JSON.stringify({
            orderId: $('input#OrderId').val(),
            taxAmount: $('input#txtInvoiceTax').val()
        }),
        dataType: "json",
        contentType: 'application/json',
        success: function (data) {
            $('div#dvOrderDetailItems').html(data.result);
            $('div#dvOrderSummary').html(data.orderSummaryHtml);
            if (data.success) {
                aOrderInvoice.hide();
                $('div#dvOrderActionButtons').find('#aViewShipmentReport').show();
                OrderDetailItems.configureProductPopup();
                $('a#aOrderInvoice').hide();
                OrderDetail.repopulateInvoicesList(data.invoicesList);
                OrderDetail.repopulateShipmentList(data.shippedItemsList);
            }
            else {
                console.log(data.message);
            }
        },
        error: function (result) {
            console.error("error: " + result);
        }
    });
}

OrderDetail.repopulateInvoicesList = function (invoicesList) {
    if (invoicesList.length > 0) {
        var ddlInvoices = $("select#InvoiceId");
        if (ddlInvoices.length == 0) {
            var invoicesListCtrl = $('div#dvInvoicesList');
            invoicesListCtrl.append('<div class="col col_25 pd_both"><div class="ctrl_group"><label for="InvoiceId">Invoices:</label><select id = "InvoiceId" name = "InvoiceId" ></select></div>');
        }

        ddlInvoices = $("select#InvoiceId");
        ddlInvoices.empty().append('<option selected="selected">Select to view</option>');
        $.each(invoicesList, function () {
            ddlInvoices.append($("<option></option>").val(this['Value']).html(this['Text']));
        });

        $('select#InvoiceId').on('change', OrderDetail.viewInvoice);
    }
}

OrderDetail.repopulateShipmentList = function (shipmentList) {
    if (shipmentList.length > 0) {
        var ddlShipments = $("select#ShipmentId");
        if (ddlShipments.length == 0) {
            var shipmentsListCtrl = $('div#dvShipmentList');
            shipmentsListCtrl.append('<div class="col col_25 pd_both"><div class="ctrl_group"><label for="ShipmentId">Shipments:</label><select id = "ShipmentId" name = "ShipmntId" ></select></div>');
        }

        ddlShipments = $("select#ShipmentId");
        ddlShipments.empty().append('<option selected="selected">Select to view</option>');
        $.each(shipmentList, function () {
            ddlShipments.append($("<option></option>").val(this['Value']).html(this['Text']));
        });

        $('select#ShipmentId').on('change', OrderDetail.viewShipment);
    }
}

OrderDetail.viewInvoice = function () {
    var orderDetailsDiv = $('div#dvOrderDetailItems');
    if (orderDetailsDiv.length > 0) {
        var invoiceId = $(this).val();
        var selectedOption = $("option:selected", this).text();
        if (invoiceId > 0) {
            if (selectedOption.length > 0) {
                var selectedData = selectedOption.split(' ');
                if (selectedData[0].trim() == 'Invoice') {
                    var params = '?invoiceId=' + invoiceId;
                    window.open(orderDetailsDiv.data().viewinvoicedetailsurl + params, "_blank");
                }
                else if (selectedData[0].trim() == 'Return') {
                    var params = '?creditMemoId=' + invoiceId;
                    window.open(orderDetailsDiv.data().viewreturnreporturl + params, "_blank");
                }
            }
        }
    }
}

OrderDetail.viewShipment = function () {
    var orderDetailsDiv = $('div#dvOrderDetailItems');
    if (orderDetailsDiv.length > 0) {
        var invoiceId = $(this).val();
        if (invoiceId > 0) {
            var params = '?invoiceId=' + invoiceId;
            window.open(orderDetailsDiv.data().viewshipmentdetailsurl + params, "_blank");
        }
    }
}

OrderDetail.viewCreditMemoReport = function () {
    var orderDetailsDiv = $('div#dvOrderDetailItems');
    if (orderDetailsDiv.length > 0) {
        var creditMemoId = $(this).val();
        if (creditMemoId > 0) {
            var params = '?creditMemoId=' + creditMemoId;
            window.open(orderDetailsDiv.data().viewreturnreporturl + params, "_blank");
        }
    }
}

OrderDetail.viewShipmentReport = function () {
    var aViewShipmentReport = $(this);
    var requestData = {
        orderId: $('input#OrderId').val()
    };

    var params = '?orderId=' + $('input#OrderId').val();
    window.open(aViewShipmentReport.data().viewshipmenturl + params, "_blank");
}

OrderDetail.configureOrderItemsPopup = function () {
    $('a#aShipMultiplePopUpOpener').hide();
    $('a#aInvoiceMultiplePopupOpener').hide();
    $('a#aCreditMemoMultiplePopupOpener').hide();

    console.log('configuring pop up');
    $('div#dvProcessMultipleItems,div#dvProcessReturnMultipleItems').dialog(
        {
            autoOpen: false,
            height: 300,
            hide: { duration: 500 },
            maxHeight: 300,
            maxWidth: 200,
            modal: true,
            show: { duration: 500 },
            width: 420
        }
    );
}

OrderDetail.processReturnItems = function () {
    var dvOrderDetailItems = $('div#dvOrderDetailItems');
    var currentSource = $(this);
    $('a#aProcessCreditMemoItems').hide();

    var requestData = {
        taxAmount: $('input#txtTaxAmount').val(),
    };

    $.ajax({
        url: dvOrderDetailItems.data().processcreditmemoitems,
        type: "POST",
        data: JSON.stringify(requestData),
        dataType: "json",
        contentType: 'application/json'
    }).done(function (data) {
        $('a#aProcessCreditMemoItems').show();
        if (data.success === true) {
            $('div#dvProcessReturnMultipleItems').dialog('close');
            window.location.href = dvOrderDetailItems.data().orderdetailurl + '?ct=' + Date.now() + '#dvItems';
        }
        else {
        }
    }).fail(function (result) {
        $('a#aProcessCreditMemoItems').show();
    });
}

$(document).ready(OrderDetail.init);