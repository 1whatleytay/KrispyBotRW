using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KrispyBotRW {
    public class KrispyCommands : ModuleBase<SocketCommandContext> {
        public static bool UserIsKrispyAdmin(SocketUser user) {
            return ((IGuildUser) user).RoleIds.Contains(378339275189518336ul);
        }
        
        [Command("shutdown")]
        public async Task Shutdown() {
            await Context.Client.StopAsync();
            await Context.Client.LogoutAsync();
        }

        [Command("help")]
        public async Task Help() {
            await ReplyAsync(KrispyLines.Help);
        }

        private static string DigitName(char name) {
            switch (name) {
                case '0': return "zero";
                case '1': return "one";
                case '2': return "two";
                case '3': return "three";
                case '4': return "four";
                case '5': return "five";
                case '6': return "six";
                case '7': return "seven";
                case '8': return "eight";
                case '9': return "nine";
                default: return "hmm"; // not sure how you'd trigger this but why not
            }
        }
        
        [Command("big")]
        public async Task Big([Remainder] string text) {
            await Context.Message.DeleteAsync();
            text = text.ToLower();
            var endText = new StringBuilder();
            foreach (var c in text)
                if (c >= 'a' && c <= 'z')
                    endText.Append(":regional_indicator_" + c + ":");
                else if (c >= '0' && c <= '9')
                    endText.Append(":" + DigitName(c) + ":");
                else if (c == '?')
                    endText.Append(":question:");
                else if (c == '!')
                    endText.Append(":exclamation:");
                else if (c == '.')
                    endText.Append(":record_button:");
                else
                    endText.Append(c);
            await ReplyAsync(endText.ToString());
        }

        private static bool DadJokesEnabled;
        
        [Command("dad-jokes")]
        public async Task DadJokes(bool state) {
            DadJokesEnabled = state;
            await ReplyAsync("Dad Jokes: " + (state ? "Enabled!" : "Disabled!"));
        }

        private static async Task DadJokes(SocketMessage msg) {
            // Dad jokes are fun
            if (!DadJokesEnabled) return;
            var messageText = msg.Content.ToLower() + " ";
            bool iAm = messageText.Contains("i am"), imA = messageText.Contains("i'm"), im = messageText.Contains("im");
            if ((iAm || imA || im) && !msg.Author.IsBot) {
                var firstStr = iAm ? "i am" : (imA ? "i'm" : "im");
                int beg = messageText.IndexOf(firstStr) + firstStr.Length + 1, last = messageText.IndexOf(' ', beg);
                await msg.Channel.SendMessageAsync("Hi, " + messageText.Substring(beg, last - beg) +
                                             ". I'm dad.");
            }
        }

        private static readonly Emoji ThumbsUp = new Emoji("👍"), ThumbsDown = new Emoji("👎");

        public static async Task MonitorMessages(SocketMessage msg) {
            await DadJokes(msg);
            KrispyContributions.ProcessMessage(msg);

            if (msg.Channel.Id == 434090042408042506) {
                var eventMessage = (SocketUserMessage)msg;
                await eventMessage.AddReactionAsync(ThumbsUp);
                await eventMessage.AddReactionAsync(ThumbsDown);
            }
        }

        [Command("add")]
        public async Task AddRole([Remainder] string text) {
            if (KrispySchedule.FindRoleIds(text).Count > 0) {
                var roles = KrispySchedule.FindRoles(text, Context.Guild);
                await ((IGuildUser) Context.User).AddRolesAsync(roles);
                await ReplyAsync("Schedule updated! Your new classes have been added.");
            } else {
                await KrispyGames.AddGamesContext(Context, text);
            }
        }

        [Command("remove")]
        public async Task RemoveRole([Remainder] string text) {
            if (KrispySchedule.FindRoleIds(text).Count > 0) {
                var roles = KrispySchedule.FindRoles(text, Context.Guild);
                await ((IGuildUser) Context.User).RemoveRolesAsync(roles);
                await ReplyAsync("Schedule updated! Your old classes have been removed.");
            } else {
                await KrispyGames.RemoveGamesContext(Context, text);
            }
        }

        public static async Task<bool> Fun(DiscordSocketClient client, SocketMessage msg, int msgLoc) {
            var text = msg.ToString().ToLower().Substring(msgLoc);
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
           else if (text.Contains("what") && components.Contains("love"))
                await msg.Channel.SendMessageAsync("Baby don't hurt me.");
            else if (components.Contains("donut") || components.Contains("doughnut"))
                await msg.Channel.SendMessageAsync(":doughnut:");
            else if (components.Contains("mispell"))
                await msg.Channel.SendMessageAsync("I mean... that's how I spelled it. That's not actually right.");
            else if (components.Contains("misspell"))
                await msg.Channel.SendMessageAsync("Correct! Good job... Here's a doughnut: :doughnut:.");
            else if (components.Contains("inspire")) {
                string inspiroBotUrl;
                var request = (HttpWebRequest)WebRequest.Create("http://inspirobot.me/api?generate=true&oy=vey");
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream)) {
                    inspiroBotUrl = reader.ReadToEnd();
                }
                await msg.Channel.SendMessageAsync(inspiroBotUrl);
            }
            else if (components.Contains("bitch"))
                await msg.Channel.SendMessageAsync("Look who's talking.");
            else if (components.Contains("ur") && components.Contains("mom") && components.Contains("gay"))
                await msg.Channel.SendMessageAsync("no u");
            else if (KrispySchedule.FindRoleIds(components[0]).Count == 1) {
                var role = KrispySchedule.FindRoles(components[0],
                    (SocketGuild)((IGuildChannel) msg.Channel).Guild)[0];
                await ((IGuildUser) msg.Author).AddRoleAsync(role);
                await msg.Channel.SendMessageAsync("Added " + role.Name + " to your schedule "
                                                   + KrispyGenerator.PickLine(KrispyLines.Emoticon));
            } else return false;
            return true;
        }
    }
}