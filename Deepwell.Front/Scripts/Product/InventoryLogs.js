var InventoryLogs = new Object();

InventoryLogs.init = function () {
    InventoryLogs.configureDatePicker();
    $('input#Search').on("click", InventoryLogs.showLog);
    $('li#liInventoryChangeLogTab').on("click", InventoryLogs.showLog);
    $('input#Reset').on("click", InventoryLogs.reset);
}

InventoryLogs.configureDatePicker = function () {
    var calenderImageUrl = $('#liInventoryChangeLogTab').data().calenderimageurl;

    var fromDate = $('input#FromDate').datepicker({
        showOn: "both",
        buttonImageOnly: true,
        buttonImage: calenderImageUrl,
        buttonText: "Select From date"
    })
    .on("change", function () {
        toDate.datepicker("option", "minDate", getDate(this));
    }),
    toDate = $('input#ToDate').datepicker({
        showOn: "both",
        buttonImageOnly: true,
        buttonImage: calenderImageUrl,
        buttonText: "Select To date"
    })
    .on("change", function () {
        fromDate.datepicker("option", "maxDate", getDate(this));
    });
}

InventoryLogs.showLog = function () {
    var logData = $('div#inventoryLog');
    if (logData.length > 0) {
        var url = logData.data().showlogurl;
        var request = {
            ProductId: $('input#ProductId').val(),
            ChangeType: $('select#ChangeTypeSelected').val(),
            Location: $('select#LocationSelected').val(),
            FromDate: $('input#FromDate').val(),
            ToDate: $('input#ToDate').val(),
        }

        $.ajax({
            url: url,
            type: "GET",
            data: request,
            success: function (result) {
                logData.html(result);
            },
            error: function (result) {
                logData.html('Could not fetch the log data.');
            }
        });
    }
}

InventoryLogs.reset = function () {
    $('select#ChangeTypeSelected').val('');
    $('select#LocationSelected').val('');
    $('input#FromDate').val();
    $('input#ToDate').val();
    InventoryLogs.showLog();
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

$(document).ready(InventoryLogs.init);