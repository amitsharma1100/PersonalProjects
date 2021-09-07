var ManageProduct = new Object();

ManageProduct.search = function (pageNumber) {
    var page = 1;
    if (pageNumber && isNaN(pageNumber) == false) {
        page = pageNumber;
    }

    var productsData = $('div#productsData');
    if (productsData.length > 0) {
        var url = productsData.data().searchurl;
        var dataToPost = {
            ProductNumber : $('input#ProductNumber').val(),
            ProductName: $('input#ProductName').val(),
            //Taxable: $('select#Taxable').find(":selected").text(),
            Active: $('select#IsActiveOptions').val(),
            ProductType: $('select#ProductType').val(),
            page: page,
        }

        $.ajax({
            url: url,
            type: "GET",
            data: dataToPost,
            success: function (result) {
                $('div#searchResults').html(result);
            },
            error: function (result) {
                $('div#searchResults').html('Could not fetch the product list ');
            }
        });
        return false;
    }
}

ManageProduct.internalSearch = function (pageNumber) {
    $('div.top_msgs').find('h4').html('');
    ManageProduct.search(pageNumber);
}

ManageProduct.deleteProduct = function (id) {
    if (id) {
        var productsData = $('div#productsData');
        if (productsData.length > 0) {
            var url = productsData.data().deleteproducturl;
            var dataToPost = {};
            dataToPost.id = id;

            $.ajax({
                url: url,
                dataType: "json",
                type: "GET",
                data: dataToPost,
                success: function (result) {
                    ManageProduct.search(1);
                },
                error: function (xhr) {
                    ManageProduct.search(1);
                }
            });

            return false;
        }
    }
}

ManageProduct.resetControls = function () {
    $('input#ProductNumber').val('');
    $('input#ProductName').val('');
    $('select#IsActiveOptions').val('All');
    $('select#ProductType').val('All');
    ManageProduct.search(1);
}

ManageProduct.init = function () {
    $('input#Search').on("click", ManageProduct.internalSearch);
    $('input#Reset').on("click", ManageProduct.resetControls);
    ManageProduct.resetControls();
    ManageProduct.search(1);
}

$(ManageProduct.init);