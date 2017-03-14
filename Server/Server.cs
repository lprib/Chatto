using System;
using System.Collections.Generic;
using System.Linq;
using Fleck;
using Google.Protobuf;
using Proto;
using Join = Proto.Join;

namespace Server
{
    class Server
    {
        private readonly WebSocketServer server;
        private readonly List<Room> rooms = new List<Room>();

        private readonly Dictionary<IWebSocketConnection, User> joinedUsers =
            new Dictionary<IWebSocketConnection, User>();

        public Server()
        {
            FleckLog.Level = LogLevel.Debug;
            server = new WebSocketServer("ws://0.0.0.0:6060");
            //server.Certificate = new X509Certificate2();
        }

        public void Start()
        {
            server.Start(socket =>
            {
                socket.OnOpen = () => OnSocketOpen(socket);
                socket.OnBinary = data => OnSocketMessage(socket, data);
                socket.OnClose = () => OnSocketClose(socket);
                socket.OnMessage = message => socket.Send("echooo" + message);
            });
        }

        public void OnSocketOpen(IWebSocketConnection connection)
        {
        }

        public void OnSocketClose(IWebSocketConnection leavingConnection)
        {
            if (!joinedUsers.ContainsKey(leavingConnection)) return;

            var leavingUser = joinedUsers[leavingConnection];
            leavingUser.Room.RemoveUser(leavingUser);
        }

        public void OnSocketMessage(IWebSocketConnection connection, byte[] data)
        {
            //if user has already logged in
            if (joinedUsers.ContainsKey(connection))
            {
                var message = MessageToServer.Parser.ParseFrom(data);
                var user = joinedUsers[connection];

                //if message is of type Chat
                if (message.MessageCase == MessageToServer.MessageOneofCase.ChatToServer)
                {
                    var outgoingChat = new MessageFromServer();
                    outgoingChat.ChatFromServer = new MessageFromServer.Types.Chat
                    {
                        Name = user.Name,
                        Trip = user.Trip,
                        Text = message.ChatToServer.Text
                    };
                    Log.LogInfo($"{user.Name} @{user.Room.Name} {user.Trip} : {message.ChatToServer}");

                    //send message to all users in that person's room
                    joinedUsers[connection].Room.Send(outgoingChat);
                }
            }
            else
            {
                //this is a new user joining for the first time
                var joinMessage = Join.Parser.ParseFrom(data);

                //the room the the new user is joining
                var room = GetRoom(joinMessage.Room);
                var newUser = new User(connection, joinMessage.Name, joinMessage.Password, GetRoom(joinMessage.Room));
                joinedUsers.Add(connection, newUser);
                room.AddUser(newUser);

                //send a success response back to the new client
                var joinResponse = new JoinResponse {Success = new JoinResponseSuccessful()};
                joinResponse.Success.OnlineUsers.Add(room.Users.Select(roomUser => roomUser.Name).ToList());
                Console.WriteLine("size: " + joinResponse.CalculateSize());
                connection.Send(joinResponse.ToByteArray());

                Log.LogInfo($"{newUser.Name} @ {newUser.Room.Name}  {newUser.Trip} joined");
            }
        }

        private Room GetRoom(string name)
        {
            //if room already exists, return it
            if (rooms.Any(r => r.Name == name))
            {
                return rooms.First(r => r.Name == name);
            }
            //else create a new room
            var newRoom = new Room(name);
            rooms.Add(newRoom);
            return newRoom;
        }
    }
}