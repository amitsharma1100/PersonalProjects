var ManageUser = new Object();

ManageUser.search = function (pageNumber) {
    var page = 1;
    if (pageNumber && isNaN(pageNumber) == false) {
        page = pageNumber;
    }

    $('input#Search').attr("disabled", true);
    var searchData = $('div#searchData');
    if (searchData.length > 0) {
        var url = searchData.data().searchurl;
        var dataToPost = {
            FirstName: $('input#FirstName').val(),
            LastName: $('input#LastName').val(),
            EmployeeId: $('input#EmployeeId').val(),
            IsActive: $('select#IsActiveOptions').find(":selected").text(),
            page: page,
        }

        $.ajax({
            url: url,
            type: "GET",
            data: dataToPost,
            success: function (result) {
                $('div#searchResults').html(result);
                $('input#Search').attr("disabled", false);
            },
            error: function (result) {
                $('div#searchResults').html('Could not fetch the user list ');
                $('input#Search').attr("disabled", false);
            }
        });
        return false;
    }
}

ManageUser.internalSearch = function (pageNumber) {
    $('div.top_msgs').find('h4').html('');
    ManageUser.search(pageNumber);
}

ManageUser.ResetControls = function () {
    $('input#FirstName').val('');
    $('input#LastName').val('');
    $('input#EmployeeId').val('');
    $('select#IsActiveOptions').val('All');
    ManageUser.search(1);
}

ManageUser.init = function () {
    $('input#Search').on('click', ManageUser.internalSearch);
    $('input#Reset').on('click', ManageUser.ResetControls);
    ManageUser.ResetControls();
    ManageUser.search(1);
}

$(ManageUser.init);