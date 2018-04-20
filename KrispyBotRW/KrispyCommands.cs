using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        
        [Command("big")]
        public async Task Big([Remainder] string text) {
            text = text.ToLower();
            var endText = new StringBuilder();
            foreach (var c in text)
                if (c >= 'a' && c <= 'z')
                    endText.Append(":regional_indicator_" + c + ":");
                else if (c == '?')
                    endText.Append(":question:");
                else if (c == '!')
                    endText.Append(":exclamation:");
                else
                    endText.Append(c);
            await ReplyAsync(endText.ToString());
        }

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
            else if (text.Contains("donut") || text.Contains("doughnut"))
                await msg.Channel.SendMessageAsync(":doughnut:");
            else return false;
            return true;
        }
    }
}