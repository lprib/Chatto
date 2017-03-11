﻿using System;
using System.Linq;
using Google.Protobuf;
using Proto;
using WebSocketSharp;

namespace Client
{
    class Program
    {
        private static WebSocket ws;

        static void Main()
        {
            ws = new WebSocket("ws://localhost:8080");
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
            ws.OnMessage += HandleJoinResponse;

            ws.Connect();

            while (true)
            {
                var chat = new MessageToServer()
                {
                    ChatToServer = new MessageToServer.Types.Chat
                    {
                        Text = Console.ReadLine(),
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

        private static void HandleJoinResponse(object o, MessageEventArgs args)
        {
            var response = JoinResponse.Parser.ParseFrom(args.RawData);
            if (response.ResponseCase == JoinResponse.ResponseOneofCase.Success)
            {
                Console.WriteLine(response.Success.OnlineUsers.Aggregate("Online Users: ",
                    (acc, next) => acc + ", " + next));
            }
            else if (response.ResponseCase == JoinResponse.ResponseOneofCase.Error)
            {
                Console.WriteLine(response.Error.ToString());
            }

            ws.OnMessage -= HandleJoinResponse;
            ws.OnMessage += HandleMessage;
        }

        private static void HandleMessage(object o, MessageEventArgs args)
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
                case MessageFromServer.MessageOneofCase.UserAction:
                    Console.WriteLine(message.UserAction.Name +
                                      (message.UserAction.ActionType ==
                                       MessageFromServer.Types.UserAction.Types.ActionType.Join
                                          ? " joined."
                                          : " left."));
                    break;
            }
        }
    }
}