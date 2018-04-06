using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            if (text.Contains("hi"))
                await msg.Channel.SendMessageAsync("Hello " + KrispyGenerator.PickLine(KrispyLines.Emoticon));
            else if (text.Contains("hello"))
                await msg.Channel.SendMessageAsync("Hi " + KrispyGenerator.PickLine(KrispyLines.Emoticon));
            else return false;
            return true;
        }
    }
}