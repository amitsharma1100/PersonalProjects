var ManageCustomer = new Object();

ManageCustomer.search = function (pageNumber) {
    var page = 1;
    if (pageNumber && isNaN(pageNumber) == false)
    {
        page = pageNumber;
    }

    var searchData = $('div#searchData');
    if (searchData.length > 0) {
        var url = searchData.data().searchurl;
        var dataToPost = {
            CustomerNumber : $('input#CustomerNumber').val(),
            Name : $('input#Name').val(),
            Status: $('select#Status').find(":selected").text(),
            Type: $('select#Type').find(":selected").text(),
            Page: page,
            Taxable: $('select#Taxable').find(":selected").text(),
        }

        $.ajax({
            url: url,
            type: "GET",
            data: dataToPost,
            success: function (result) {
                $('div#searchResults').html(result);
            },
            error: function (result) {
                $('div#searchResults').html('Could not fetch the customer list');
            }
        });
        return false;
    }
}

ManageCustomer.internalSearch = function (pageNumber) {
    $('div.top_msgs').find('h4').html('');
    ManageCustomer.search(pageNumber);
}

ManageCustomer.ResetControls = function () {
    $('input#CustomerNumber').val('');
    $('input#Name').val('');
    $('select#Status').val('0');
    $('select#Type').val('0');
    $('select#Taxable').val('0');
    ManageCustomer.search(1);
}

ManageCustomer.init = function () {
    $('input#Search').on('click', ManageCustomer.internalSearch);
    $('input#Reset').on('click', ManageCustomer.ResetControls);
    ManageCustomer.search(1);
}

$(ManageCustomer.init);