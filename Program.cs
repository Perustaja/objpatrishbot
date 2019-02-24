﻿using System;
using System.Threading.Tasks;
using Discord;

namespace objpatrishbot
{
    class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var client = new DiscordSocetClient();

            client.Log += Log;

            client.MessageReceived += MessageReceived;

            string token = args[1];
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            // Block this task until the program is closed.
	        await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content == "!ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
        }
    }
}
