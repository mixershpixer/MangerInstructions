$(document).ready(function () {
    $("#changetheme").click(function () {

        if ($(this).hasClass("fa-moon")) {
            $(this).removeClass("fa-moon").addClass("fa-sun");
            $("#themestyle").prop("href", "/lib/bootstrap/dist/css/bootstrap-dark.css");
        }
        else {
            $(this).removeClass("fa-sun").addClass("fa-moon");
            $("#themestyle").prop("href", "/lib/bootstrap/dist/css/bootstrap.css");
        }

        var theme;
        if ($(this).hasClass("fa-moon"))
            theme = "light";
        else
            theme = "dark";

        $.post("/Home/SetTheme",
            {
                theme: theme
            });
    });

    var setting = $("#tags").tagsinput({
        maxTags: 8,
        confirmKeys: [13, 32, 44],
        trimValue: true
    });

    $("#nameEdit").change(function () {
        $.post("/Account/ChangeUserName",
            {
                name: this.value
            });
    });

    $("#textComment").on("input", function () {
        var text = $(this).val().replace(/\s/g, '');
        if (text == "")
            $("#buttonComment").attr("disabled", true);
        else
            $("#buttonComment").attr("disabled", false);
    });

    

});

function readURL(input, step) {
    $("#_imageContainer_" + step).hide();
    $("#imageContainer_" + step).empty();
    for (var i = 0; i < 3; i++) {
        var htmlAdd = '<div class="thumbnail col-xs-4 col-sm-4 col-md-4 col-lg-4">';
        if (input.files && input.files[i]) {
            var reader = new FileReader();
            htmlAdd += '<img id="image_' + step + '_' + i + '" /></div>';
            $("#imageContainer_" + step).append(htmlAdd);
            reader.onload = (function (step, index) {
                return function (e) {
                    $("#image_" + step + "_" + index).attr("src", e.target.result);
                };
            })(step, i);
            reader.readAsDataURL(input.files[i]);
        }
    }
}

var getVotes = function (idInstruction, rateSpanId, ratingOutputId) {
    $.post("/Home/GetVotes",
        {
            idInstruction: idInstruction
        }
    ).done(function (data) {
        $("#" + ratingOutputId).text(data);
        var rating = parseInt(data) + 1;
        for (var i = 1; i < rating; ++i) {
            $("#" + rateSpanId + i).removeClass().addClass("glyphicon glyphicon-star");
            $("#" + rateSpanId + i).css("color", "gold");
        }
    });
}

function remove(id, button) {
    $.post("/Home/RemoveComment",
        {
            idComment: id
        });
    button.disabled = true;
}

var setRateEvents = function (idInstruction, rateSpanId, ratingOutputId) {
    $("#" + rateSpanId + "1,#" + rateSpanId + "2,#" + rateSpanId + "3,#" + rateSpanId + "4,#" + rateSpanId + "5").on("click", function () {
        var elementNumber = parseInt($(this).attr("id").substr(-1));
        for (var i = elementNumber; i > 0; --i) {
            $("#" + rateSpanId + i).removeClass().addClass("glyphicon glyphicon-star");
            $("#" + rateSpanId + i).css("color", "gold");
        }
        for (var i = elementNumber + 1; i < 6; ++i) {
            $("#" + rateSpanId + i).removeClass().addClass("glyphicon glyphicon-star-empty");
            $("#" + rateSpanId + i).css("color", "black");
        }
        $.post("/Home/SetVote",
            {
                idInstruction: idInstruction,
                rating: elementNumber
            })
            .done(function (data) {
                $("#" + ratingOutputId).text(data);
            });
    });
}