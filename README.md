# Chatto
A chat server and client that uses Google Protobuf over websocket for communication, written in C#.

## Protocol
Defined in the [protobuf definition](https://github.com/JaxForReal/Chatto/blob/master/chatto_proto.proto).

#### Connection and handshake
- Client connects to server websocket
- Client sends a `Join` message with name, room, etc.
- Server sends a `JoinResponse` message
    - If the join is successful, it will contain a `JoinResponseSuccessful` message, with the room's information
    - Otherwise, will contain an `Error` enum

#### When another user sends a message:
- Server will emit a `MessageFromServer` message
    - It will contain a `Chat` message with the sender's info, and `time` in unix epoch ticks
- The client should reply with a `MessageToServer`
    - It should contain a `ReadReciept` message with required information
    - It should contain a `time` value in unix epoch ticks

#### When another user does an action other than chat (eg. join or leave)
- Server will emit a `MessageFromServer` message
    - It will contain a `UserAction` message, and `time`
    - The action is shown in the `UserAction.action_type` field
    - It will be of type enum `ActionType`

#### If there is an error
- Server will emit a `MessageFromServer` message
    - It will contain an `Error` enum, denoting the type
    - will contain `time`

#### Broadcast from server
- Server will emit a `MessageServer` message
    - will contain a string `server_broadcast` and `time`