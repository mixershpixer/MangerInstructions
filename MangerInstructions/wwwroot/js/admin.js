
function changeUserName(userId, element) {
    $.post("/Admin/ChangeUserName",
        {
            Id: userId,
            name: element.value
        });
}

function changeUserEmail(userId, element) {
    $.post("/Admin/ChangeUserEmail",
        {
            Id: userId,
            email: element.value
        });
}

