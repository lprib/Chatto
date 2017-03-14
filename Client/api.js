function ChattoConnection(joinData) {
    this.joinData = joinData;
    var thisConnection = this;

    thisConnection.onOpen = function (joinResponse) {}
    thisConnection.onMessage = function (messageData) {}

    protobuf.load("https://raw.githubusercontent.com/JaxForReal/Chatto/master/chatto_proto.proto", function (err, root) {
        if (err) {
            throw err;
        }

        var MessageFromServer = root.lookup("chatto.MessageFromServer");
        var MessageToServer = root.lookup("chatto.MessageToServer");
        var JoinResponse = root.lookup("chatto.JoinResponse");
        var Join = root.lookup("chatto.Join");
        var Chat = MessageToServer.Chat;

        var webSocket = new WebSocket("ws://localhost:8080");

        webSocket.onopen = function () {
            //console.log("websocket opened");
            var Join = root.lookup("chatto.Join");
            var joinDataBinary = Join.encode(thisConnection.joinData).finish();

            webSocket.send(joinDataBinary);
        }

        webSocket.onmessage = function doHandshake(message) {
            toUint8Array(message.data,
                function (data) {
                    var response = JoinResponse.decode(data);
                    thisConnection.onOpen(response);
                });
            webSocket.onmessage = messageRecieved;
        }

        function messageRecieved(message) {
            toUint8Array(message.data, function (data) {
                var messageDecoded = MessageFromServer.decode(data);
                thisConnection.onMessage(messageDecoded);
            });
        }

        /*
            text: string
        */
        //TODO wtf
        thisConnection.sendChat = function (text) {
            console.log(MessageToServer, text);
            var messageTemplate = MessageToServer.from({
                chatToServer: {
                    text: text
                },
                time: 0
            });

            var encodedMessage = MessageToServer.encode(messageTemplate).finish()
            //console.log(MessageToServer.encode(messageTemplate).finish());

            webSocket.send(encodedMessage);
        }
    });
}

//converts a blob to arrayBuffer
//result is passed to callback
function toUint8Array(blob, callback) {
    var arrayBuffer;
    var fileReader = new FileReader();
    fileReader.onload = function () {
        arrayBuffer = this.result;
        callback(new Uint8Array(arrayBuffer));
    };
    fileReader.readAsArrayBuffer(blob);
}