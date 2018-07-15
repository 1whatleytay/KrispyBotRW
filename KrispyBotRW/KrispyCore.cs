using System;
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
        public const int VersionMajor = 0, VersionMinor = 4, Revision = 1;
        
        private readonly DiscordSocketClient Client = new DiscordSocketClient();
        private readonly CommandService Commands = new CommandService();
        private IServiceProvider Services;
        
        private readonly Timer StatusUpdates = new Timer { Interval = 600000, Enabled = true };
        private readonly Timer NinjaUpdates = new Timer { Interval = 3000, Enabled = true };
        private readonly Timer HealUpdates = new Timer { Interval = 20000, Enabled = true };

        private string Token;
        
        public delegate void KrispyMessageCallback(SocketMessage message, object userData);
        
        private static Task Log(LogMessage msg) {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        private async void UpdateStatus(object sender, ElapsedEventArgs args) {
            if (!KrispyGenerator.Odds(4)) return;
            var result = new KrispyStatusLine();
            await Client.SetGameAsync(result.Status, null, result.Type);
        }

        private static void UpdateNinjas(object sender, ElapsedEventArgs args) { Ninja.KrispyNinjas.AdvanceGames(); }
        private static void UpdateHeals(object sender, ElapsedEventArgs args) { Ninja.KrispyNinjas.RestoreHP(); }
        
        private async Task CheckMessage(SocketMessage msg) {
            if (!(msg is SocketUserMessage userMsg)) return;

            await KrispyCommands.MonitorMessages(msg);

            int mentionPos = 0, charPos = 0;
            bool passMention = false, passChar = false;

            if (userMsg.HasMentionPrefix(Client.CurrentUser, ref mentionPos)) passMention = true;
            else if (userMsg.HasCharPrefix('$', ref charPos)) passChar = true;

            if (passMention || passChar) {
                var pos = passMention ? mentionPos : charPos;
                if (await KrispyCommands.Fun(Client, msg, pos)) return;
                var context = new SocketCommandContext(Client, userMsg);
                var result = await Commands.ExecuteAsync(context, pos, Services);
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
            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(Commands)
                .BuildServiceProvider();

            Client.Log += Log;
            Client.MessageReceived += CheckMessage;
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly());

            try {
                Token = File.ReadAllText("token.txt");
            } catch (FileNotFoundException) {
                Console.WriteLine(
                    "Could not find bot token. Please start program with arguments `token <token>` to set the current token.");
                return;
            }

            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();

            StatusUpdates.Elapsed += UpdateStatus;
            NinjaUpdates.Elapsed += UpdateNinjas;
            HealUpdates.Elapsed += UpdateHeals;

//            System.Threading.Thread.Sleep(4000);
//
//            await ((IMessageChannel) Client.GetChannel(378337310074339350)).SendMessageAsync(
//                ":kiwi:");

//            for (var i = 0; i < Client.Guilds.Count; i++) {
//                Console.WriteLine("Guild Name: " + Client.Guilds[i]);
//            }
            
            await Task.Delay(-1);
        }
        
        public static void Main(string[] args) {
            if (args.Length >= 2 && args[0] == "token") {
                File.WriteAllText("token.txt", args[1]);
                Console.WriteLine("Token added!");
            }
            new KrispyCore().KrispyAsync().GetAwaiter().GetResult();
        }
    }
}