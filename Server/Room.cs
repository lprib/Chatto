using System.Collections.Generic;
using Fleck;
using Google.Protobuf;
using Proto;
using static Proto.MessageFromServer.Types.UserAction.Types;

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

        public void AddUser(User newUser)
        {
            //send a join message to the room
            var userJoinBroadcast = new MessageFromServer();
            userJoinBroadcast.UserAction = new MessageFromServer.Types.UserAction
            {
                Name = newUser.Name,
                ActionType = ActionType.Join
            };
            Send(userJoinBroadcast);

            Users.Add(newUser);
        }

        public void RemoveUser(User leavingUser)
        {
            Users.Remove(leavingUser);
            //broadcast to room that the user left
            var userLeaveBroadcast = new MessageFromServer();
            userLeaveBroadcast.UserAction = new MessageFromServer.Types.UserAction
            {
                Name = leavingUser.Name,
                ActionType = ActionType.Leave
            };

            Send(userLeaveBroadcast);
        }

        public void Send(MessageFromServer outgoingChat)
        {
            foreach (var user in Users)
            {
                user.Connection.Send(outgoingChat.ToByteArray());
            }
        }
    }
}
