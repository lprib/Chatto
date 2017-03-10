using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Fleck;
using Proto;

namespace Server
{
    class Server
    {
        private WebSocketServer server;
        private List<Room> rooms = new List<Room>();
        private Dictionary<IWebSocketConnection, User> joinedUsers = new Dictionary<IWebSocketConnection, User>();

        public Server()
        {
            FleckLog.Level = LogLevel.Info;
            server = new WebSocketServer("ws://0.0.0.0:8080");
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

        public void OnSocketClose(IWebSocketConnection connection)
        {
        }

        public void OnSocketMessage(IWebSocketConnection connection, byte[] data)
        {
            //if user has already logged in
            if (joinedUsers.ContainsKey(connection))
            {
                var message = ChatToServer.Parser.ParseFrom(data);
                var user = joinedUsers[connection];

                var outgoingChat = new ChatFromServer
                {
                    Name = user.Name,
                    Text = message.Text,
                    Trip = user.Trip
                };
                Log.LogInfo($"{user.Name} @{user.Room.Name} {user.Trip} : {message.Text}");

                //send message to all users in that person's room
                joinedUsers[connection].Room.Send(outgoingChat);
            }
            else
            {
                var joinMessage = Join.Parser.ParseFrom(data);
                //the room the the new user is joining
                var room = GetRoom(joinMessage.Room);
                var user = new User(connection, joinMessage.Name, joinMessage.Password, GetRoom(joinMessage.Room));
                room.AddUser(user);
                joinedUsers.Add(connection, user);

                Log.LogInfo($"{user.Name} @{user.Room.Name} {user.Trip} joined");
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