using System;
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

            //TODO handle joinResponse messages
            ws.OnMessage += (o, args) =>
            {
                var message = MessageFromServer.Parser.ParseFrom(args.RawData);
                switch (message.MessageCase)
                {
                    case MessageFromServer.MessageOneofCase.ChatFromServer:
                        var chat = message.ChatFromServer;
                        Console.WriteLine($"{chat.Name} {chat.Trip} : {chat.Text}");
                        break;
                    case MessageFromServer.MessageOneofCase.Error:
                        Console.WriteLine(message.Error.ToString());
                        break;
                    case MessageFromServer.MessageOneofCase.UserJoinLeave:
                        Console.WriteLine(message.UserJoinLeave.Name +
                                          (message.UserJoinLeave.ActionType ==
                                           MessageFromServer.Types.UserAction.Types.ActionType.Join
                                              ? " joined."
                                              : " left."));
                        break;
                }
            };

            ws.Connect();

            while (true)
            {
                var chat = new MessageToServer()
                {
                    ChatToServer = new MessageToServer.Types.Chat
                    {
                        Text = Prompt(">"),
                    }
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