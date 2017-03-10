using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Proto;

namespace Client
{
    class Program
    {
        static void Main()
        {
            var ws = new WebSocketSharp.WebSocket("ws://localhost:8080");
            ws.OnError += (sender, args) =>
            {
                //Console.WriteLine(args.Exception.ToString());
            };
            ws.OnOpen += (o, args) =>
            {
                var join = new Join
                {
                    Name = Prompt("name: "),
                    Password = Prompt("pass: "),
                    Room = Prompt("room: ")
                };
                ws.Send(join.ToByteArray());
            };

            ws.OnMessage += (o, args) => Console.WriteLine(ChatFromServer.Parser.ParseFrom(args.RawData).ToString());

            ws.Connect();

            while (true)
            {
                var chat = new ChatToServer
                {
                    Text = Prompt("")
                };
                ws.Send(chat.ToByteArray());
            }
        }

        private static string Prompt(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }
    }
}
