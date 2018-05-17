using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

namespace KrispyBotRW {
    public class KrispyCore {
        public const int VersionMajor = 0, VersionMinor = 3, Revision = 0;
        
        private readonly DiscordSocketClient _client = new DiscordSocketClient();
        private readonly CommandService _commands = new CommandService();
        private IServiceProvider _services;
        
        private readonly Timer _statusUpdates = new Timer { Interval = 10 * 60 * 1000 };

        private string _token;
        
        public delegate void KrispyMessageCallback(SocketMessage message, object userData);

        public readonly Dictionary<ulong, KrispyMessageCallback> UserCallbacks =
            new Dictionary<ulong, KrispyMessageCallback>();
        
        private static Task Log(LogMessage msg) {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        private async void UpdateStatus(object sender, ElapsedEventArgs args) {
            if (!KrispyGenerator.Odds(4)) return;
            var result = new KrispyStatusLine();
            await _client.SetGameAsync(result.status, null, result.type);
        }

        private async Task CheckMessage(SocketMessage msg) {
            if (!(msg is SocketUserMessage userMsg)) return;

            if (UserCallbacks.ContainsKey(msg.Author.Id)) {
                UserCallbacks[msg.Author.Id](msg, null);
            }
            
            KrispyContributions.ProcessMessage(msg);

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
                            "Hehehe... We've hit an error!\n```\n" + result.ErrorReason + "\n```");
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

            _statusUpdates.Elapsed += UpdateStatus;
            _statusUpdates.Enabled = true;
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