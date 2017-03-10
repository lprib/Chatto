using System.Collections.Generic;
using Fleck;
using Google.Protobuf;
using Proto;

namespace Server
{
    class Room
    {
        public string Name { get; }
        public List<User> Users { get; } = new List<User>();

        public Room(string name)
        {
            Name = name;
        }

        public void AddUser(User user)
        {
            Users.Add(user);
        }

        public void Send(ChatFromServer outgoingChat)
        {
            foreach (var user in Users)
            {
                user.Connection.Send(outgoingChat.ToByteArray());
            }
        }
    }
}
