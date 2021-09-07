(function () {
    //ADD ACTIVE CLASS
    var url = location.pathname.split("/");

    if (typeof url[1] !== "undefined") {
        $("#main_nav [data-url='" + url[1].toLowerCase().trim() + "']").addClass("active");
    }

    //OPEN POPUP
    $(document).on("click", "[data-role='popup']", function () {
        $("body").css("overflow", "hidden");
        var $popup_id = $(this).attr("data-target");
        $("#" + $popup_id).addClass("show");
    });

    //CLOSE POPUP
    $(document).on("click", ".close_this_popup", function () {
        $("body").css("overflow", "auto");
        $(this).closest(".popup_wrapper").removeClass("show");
    });

    //SUBMENU DROPDOWN
    $(document).on("click", "li.has_submenu > a", function () {
        var is_opened = $(this).parent().hasClass("active_submenu");

        $(".submenu").slideUp("fast");
        $("li.has_submenu").removeClass("active_submenu");

        if (!is_opened) {
            $(this).parent().addClass("active_submenu");
            $(this).next(".submenu").slideDown("fast");
        }
    });

    $(document).on("click", ".tab_form .tabs li", function () {
        $(".tab_content").removeClass("active");
        $(".tab_form .tabs li").removeClass("active");

        var content_id = $(this).attr("data-panel");
        $("#" + content_id).addClass("active");
        $(this).addClass("active");
    });
}());