using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

namespace KrispyBotRW {
    public class KrispyCore {
        public const int VersionMajor = 0, VersionMinor = 1, Revision = 0;
        
        private readonly DiscordSocketClient _client = new DiscordSocketClient();
        private readonly CommandService _commands = new CommandService();
        private IServiceProvider _services;

        private string _token;

        private static Task Log(LogMessage msg) {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        private async Task CheckMessage(SocketMessage msg) {
            if (!(msg is SocketUserMessage userMsg)) return;

            int mentionPos = 0, charPos = 0;
            bool passMention = false, passChar = false;

            if (userMsg.HasMentionPrefix(_client.CurrentUser, ref mentionPos)) passMention = true;
            else if (userMsg.HasCharPrefix('$', ref charPos)) passChar = true;

            if (passMention || passChar) {
                var pos = passMention ? mentionPos : charPos;
                if (await KrispyCommands.Fun(_client, msg, pos)) return;
                var context = new SocketCommandContext(_client, userMsg);
                var result = await _commands.ExecuteAsync(context, pos, _services);
                if (!result.IsSuccess) {
                    if (result.Error == CommandError.UnknownCommand) {
                        var chosenLine = KrispyGenerator.PickLine(KrispyLines.Unknown);
                        if (chosenLine.Contains("{0}")) chosenLine =
                            string.Format(chosenLine, KrispyNickname.FirstName(((IGuildUser)msg.Author).Nickname));
                        await context.Channel.SendMessageAsync(chosenLine);
                    }
                    else
                        await context.Channel.SendMessageAsync(
                            (KrispyGenerator.Odds(10) ? "Oof. Doublé. Not a fan." : "Hehehe... We've hit an error!")
                            + "\n```\n" + result.ErrorReason + "\n```");
                }
            }
        } 

        private async Task KrispyAsync() {
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.Log += Log;
            _client.MessageReceived += CheckMessage;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            try {
                _token = File.ReadAllText("token.txt");
            } catch (FileNotFoundException) {
                Console.WriteLine(
                    "Could not find bot token. Please start program with arguments `token <token>` to set the current token.");
                return;
            }

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
        
        public static void Main(string[] args) {
            if (args.Length >= 2 && args[0] == "token") {
                File.WriteAllText("token.txt", args[1]);
                Console.WriteLine("Token added!");
                return;
            }
            new KrispyCore().KrispyAsync().GetAwaiter().GetResult();
        }
    }
}