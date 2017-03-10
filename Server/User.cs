using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server
{
    class User
    {
        private static readonly MD5 Md5 = new MD5Cng();

        public IWebSocketConnection Connection { get; }
        public string Name { get; }
        public string Trip { get; }
        public Room Room { get; }

        public User(IWebSocketConnection connection, string name, string password, Room room)
        {
            Connection = connection;
            Name = name;
            Trip = Convert.ToBase64String(Md5.ComputeHash(Encoding.Unicode.GetBytes(password))).Substring(0, 5);
            Room = room;
        }
    }
}
