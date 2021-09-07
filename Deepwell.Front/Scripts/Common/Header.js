var Header = new Object();

Header.init = function () {
   // Header.configureProductPopup();
}

Header.configureProductPopup = function () {
    $('#productDialog').dialog(
        {
            autoOpen: false,
            height: 100,
            hide: { duration: 500 },
            modal: true,
            show: { duration: 500 },
            width: 350
        }
    );

    $('#productDialogOpener').click(function () {
        $('#productDialog').dialog('open');
    });
}

$(document).ready(Header.init);