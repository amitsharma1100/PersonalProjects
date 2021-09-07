var ManageOrders = new Object();

ManageOrders.init = function () {
    ManageOrders.configureDatePicker();
    $('input#btnSearch').on('click', ManageOrders.search);
    $('input#btnReset').on('click', ManageOrders.resetControls);
    $('input[data-numeric]').blur(ManageOrders.validateNumericValues);
    ManageOrders.resetControls();
}

ManageOrders.search = function (pageNumber) {
    var page = 1;
    if (pageNumber && isNaN(pageNumber) == false) {
        page = pageNumber;
    }

    var searchData = $('div#searchData');
    if (searchData.length > 0) {
        var url = searchData.data().getorderdetailsurl;
        var searchRequest = {};
        searchRequest.OrderId = $('input#OrderId').val();
        searchRequest.OrderDateFrom = $('input#OrderDateFrom').val();
        searchRequest.OrderDateTo = $('input#OrderDateTo').val();
        searchRequest.ProductNumber = $('input#ProductNumber').val();
        searchRequest.ProductName = $('input#ProductName').val();
        searchRequest.CustomerNumber = $('input#CustomerNumber').val();
        searchRequest.CustomerName = $('input#CustomerName').val();
        searchRequest.OrderStatus = $('select#OrderStatus').val();
        searchRequest.Page = page;

        $.ajax({
            url: url,
            type: "GET",
            data: searchRequest,
            success: function (result) {
                $('div#searchResults').html(result);
            },
            error: function (result) {
                $('div#searchResults').html('Could not fetch the orders list ');
            }
        });
    }
}

ManageOrders.validateNumericValues = function (request) {
    var txtBox = $(this);
    if (txtBox.val() == '')
        return;

    if ($.isNumeric(txtBox.val()) === false || txtBox.val() < 0) {
        txtBox.val('');
        txtBox.select();
        txtBox.bounce();
    }
}

ManageOrders.resetControls = function () {
    $('input#OrderId').val('');
    $('input#ProductNumber').val('');
    $('input#ProductName').val('');
    $('input#CustomerNumber').val('');
    $('input#CustomerName').val('');
    $('input#OrderDateFrom').val('');
    $('input#OrderDateTo').val('');
    $('select#OrderStatus').val('0');
    ManageOrders.search(1);
}

ManageOrders.configureDatePicker = function () {
    var calenderImageUrl = $('div#searchData').data().calenderimageurl;

    var fromDate = $('input#OrderDateFrom').datepicker({
        showOn: "both",
        buttonImageOnly: true,
        buttonImage: calenderImageUrl,
        buttonText: "Select From date"
    })
        .on("change", function () {
            toDate.datepicker("option", "minDate", getDate(this));
        }),
        toDate = $('input#OrderDateTo').datepicker({
            showOn: "both",
            buttonImageOnly: true,
            buttonImage: calenderImageUrl,
            buttonText: "Select To date"
        })
            .on("change", function () {
                fromDate.datepicker("option", "maxDate", getDate(this));
            });
}

function getDate(element) {
    var date;
    try {
        date = $.datepicker.parseDate("mm/dd/yy", element.value);
        console.log(date);
    } catch (error) {
        date = null;
    }

    return date;
}

$(ManageOrders.init);