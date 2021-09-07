var ProductInventory = new Object();

ProductInventory.init = function () {
    ProductInventory.updateInventoryBox();
    $('input[type="radio"][name="changeType"]').click(ProductInventory.updateInventoryBox);
    $('input.txtTransferred').click(ProductInventory.manageTrasnferInventory);    
    $('div#dvInventorySection').find('input[type="number"]').blur(ProductInventory.validateInventory);
    $('div#dvInventorySection').find('input[type="number"]').focus(ProductInventory.configureQuantityBoxes);
}

ProductInventory.configureQuantityBoxes = function () {
    $(this).select();
}

ProductInventory.manageTrasnferInventory = function () {
    var currentRow = $(this).closest('div.row');
    var selectedtxtTransferred = currentRow.find('input.txtTransferred');
    var notSelectedRow = currentRow.siblings('div.row');
    var notSelectedtxtTransferred = notSelectedRow.find('input.txtTransferred');

    notSelectedRow.fadeTo("slow", 0.5);
    notSelectedtxtTransferred.attr('readonly', 'readonly');
    notSelectedtxtTransferred.val(0);

    currentRow.fadeTo("slow", 1);
    selectedtxtTransferred.removeAttr('readonly');
}

ProductInventory.resetTransferInventoryAppearance = function () {
    var dvRows = $('div#dvProductInventory').find('div.row');
    dvRows.fadeTo("fast", 1);
};

ProductInventory.updateInventoryBox = function () {
    ProductInventory.resetTransferInventoryAppearance();
    var changeTypeRadio = $('input[type="radio"][name="changeType"]:checked');
    var changeType = '';
    if (changeTypeRadio.length) {
        changeType = changeTypeRadio.val();
    }
    var tbl = $('#dvProductInventory');
    var dvTransfer = $('#dvProductTransferInventory');
    switch (changeType) {
        case 'Created':
            {
                dvTransfer.hide();
                tbl.show();

                tbl.find('#headingTransferTo').hide();
                tbl.find('.colTransferTo').hide();
                tbl.find('#headingTransferredInventory').hide();
                tbl.find('.colTransferredInventory').hide();

                tbl.find('#headingIncreasedInventory').hide();
                tbl.find('.colIncreasedInventory').hide();

                tbl.find('#headingDecreasedInventory').hide();
                tbl.find('.colDecreasedInventory').hide();

                tbl.find('#headingRemarks').hide();
                tbl.find('.colRemarks').hide();
                break;
            }
        case 'Increased':
            {
                dvTransfer.hide();
                tbl.show();

                tbl.find('#headingTransferTo').hide();
                tbl.find('.colTransferTo').hide();
                tbl.find('#headingTransferredInventory').hide();
                tbl.find('.colTransferredInventory').hide();

                tbl.find('#headingIncreasedInventory').show();
                tbl.find('.colIncreasedInventory').show();

                tbl.find('#headingDecreasedInventory').hide();
                tbl.find('.colDecreasedInventory').hide();

                tbl.find('#headingRemarks').show();
                tbl.find('.colRemarks').show();
                break;
            }
        case 'Decreased':
            {
                dvTransfer.hide();
                tbl.show();

                tbl.find('#headingTransferTo').hide();
                tbl.find('.colTransferTo').hide();
                tbl.find('#headingTransferredInventory').hide();
                tbl.find('.colTransferredInventory').hide();

                tbl.find('#headingIncreasedInventory').hide();
                tbl.find('.colIncreasedInventory').hide();

                tbl.find('#headingDecreasedInventory').show();
                tbl.find('.colDecreasedInventory').show();

                tbl.find('#headingRemarks').show();
                tbl.find('.colRemarks').show();
                break;
            }
        case 'Transferred':
            {
                //tbl.hide();
                dvTransfer.show();


                tbl.find('#headingTransferTo').show();
                tbl.find('.colTransferTo').show();
                tbl.find('#headingTransferredInventory').show();
                tbl.find('.colTransferredInventory').show();

                tbl.find('#headingIncreasedInventory').hide();
                tbl.find('.colIncreasedInventory').hide();

                tbl.find('#headingDecreasedInventory').hide();
                tbl.find('.colDecreasedInventory').hide();

                tbl.find('#headingRemarks').show();
                tbl.find('.colRemarks').show();

                break;
            }
        default:
            {
                break;
            }
    }
}

ProductInventory.validateInventory = function () {
    var txtBox = $(this);
    var newQuantity = parseInt(txtBox.val());
    
    if (newQuantity < 0) {
        ProductInventory.resetQuantityBox(txtBox);
    }
    else {

        var changeType = txtBox.data().changetype;
        if (changeType === "decreased" || changeType === "transferred") {
            var currentQuantity = parseInt(txtBox.closest('div.row').data().currentquantity);
            if (newQuantity > currentQuantity) {
                ProductInventory.resetQuantityBox(txtBox);
            }
        }
    }
};

ProductInventory.resetQuantityBox = function (txtBox) {
    if (txtBox.length) {
        txtBox.val('0');
        txtBox.bounce();
        txtBox.select();
        txtBox.closest('div#dvInventorySection').find('div#dvQuantityMessage').fadeIn(1000).delay(2000).fadeOut(3000);
    }
}

$(document).ready()
{
    ProductInventory.init();
};