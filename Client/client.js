protobuf.load("https://raw.githubusercontent.com/JaxForReal/Chatto/master/chatto_proto.proto",
    function(err, root) {
        if (err) {
            throw err;
        }

        var MessageFromServer = root.lookup("chatto.MessageFromServer");
        var MessageToServer = root.lookup("chatto.MessageToServer");
        var JoinResponse = root.lookup("chatto.JoinResponse");

        var webSocket = new WebSocket("ws://localhost:8080");

        webSocket.onopen = function() {
            //console.log("websocket opened");
            var Join = root.lookup("chatto.Join");
            var join = Join.encode({
                name: window.prompt("name"),
                password: window.prompt("pass"),
                room: window.prompt("room")
            }).finish();

            webSocket.send(join);
        }

        webSocket.onmessage = function(message) {
            toUint8Array(message.data,
                function(data) {
                    var response = JoinResponse.decode(data);
                    if (typeof response.success !== "undefined") {
                        var onlineUsers = "Online users: ";
                        response.success.onlineUsers.forEach(function(user) { onlineUsers += user + ", " });
                        addMessage(onlineUsers);
                    }
                });
            webSocket.onmessage = messageRecieved;
        }

        function messageRecieved(message) {
            toUint8Array(message.data,
                function(data) {
                    message = MessageFromServer.decode(data);
                    addMessage(JSON.stringify(message));
                });
        }

        //todo this is not triggering
        $("#message-input").keydown(function(e) {
            if (e.keyCode === 13) {
                console.log("trig");
                var chatMessage = MessageToServer.encode({
                    chat_to_server: MessageToServer.nested.Chat.encode({
                        text: $("#message-input").val()
                    }),
                    time: 0
                }).finish();

                webSocket.send(chatMessage);
                $("#message-input").val("");
            }
        });
    });

//converts a blob to arrayBuffer
//result is passed to callback
function toUint8Array(blob, callback) {
    var arrayBuffer;
    var fileReader = new FileReader();
    fileReader.onload = function() {
        arrayBuffer = this.result;
        callback(new Uint8Array(arrayBuffer));
    };
    fileReader.readAsArrayBuffer(blob);
}

function addMessage(message) {
    $("#messages").append("<p>" + message + "</p>");
}