$(document).ready(function() {
    $("#join-form").submit(function (e) {
        //prevent GET in url
        e.preventDefault();
        $("#join-form").hide();
        doConnect();
        return false;
    });
});

function doConnect() {
    var connection = new ChattoConnection({
        name: $("#join-form input[name=name]").val(),
        password: $("#join-form input[name=password]").val(),
        room: $("#join-form input[name=room]").val(),
    });

    connection.onOpen = function (joinResponse) {
        if (typeof joinResponse.success !== "undefined") {
            var online = "Online users: ";
            joinResponse.success.onlineUsers.forEach(function (user) {
                online += user + ", ";
            });
            addMessage("Room: " + $("#join-form input[name=room]").val());
            addMessage(online);
        }
    }

    connection.onMessage = function (message) {
        if (typeof message.chatFromServer !== "undefined") {
            var chat = message.chatFromServer;
            addMessage(chat.name + " " + chat.trip + ": " + chat.text);
        }
    }

    function addMessage(text) {
        $("#messages").append("<p>" + text + "</p>");
    }

    $("#message-input").keydown(function (e) {
        console.log("asd");
        if (e.keyCode === 13 && !e.shiftKey) {
            e.preventDefault();
            connection.sendChat($("#message-input").val());
            $("#message-input").val("");
        }
    });
}