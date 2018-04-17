using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KrispyBotRW {
    public class KrispyCommands : ModuleBase<SocketCommandContext> {
        [Command("shutdown")]
        public async Task Shutdown() {
            await Context.Client.StopAsync();
            await Context.Client.LogoutAsync();
        }

        [Command("help")]
        public async Task Help() { await ReplyAsync(KrispyLines.Help); }

        public static async Task<bool> Fun(DiscordSocketClient client, SocketMessage msg, int msgLoc) {
            var text = msg.ToString().Substring(msgLoc);
            var components = text.Split(" ");
            if (components.Contains("hi"))
                await msg.Channel.SendMessageAsync("Hello " + KrispyGenerator.PickLine(KrispyLines.Emoticon));
            else if (components.Contains("hello"))
                await msg.Channel.SendMessageAsync("Hi " + KrispyGenerator.PickLine(KrispyLines.Emoticon));
            else if (text.Contains("what") && (components.Contains("time") ||
                                               components.Contains("day") ||
                                               components.Contains("date")))
                await msg.Channel.SendMessageAsync("It is February " +
                                                   (int) (DateTime.Now - new DateTime(2018, 2, 1)).TotalDays +
                                                   ", 2018.");
            else return false;
            return true;
        }
    }
}