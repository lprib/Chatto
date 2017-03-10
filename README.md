# Chatto
A chat server and client that uses Google Protobuf over websocket for communication, written in C#.

#Client<->Server communication:
First, connect to the server's websocket.
Then, serialize a `join` message, as found in
[the proto definition](https://github.com/JaxForReal/Chatto/blob/master/chatto_proto.proto).
This will assign you a channel. Clients can send `ChatToServer` messages to the server, and will recieve `ChatFromServer` replies.
Your message will always get echoed back to you.
