﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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
        public async Task Help() { await ReplyAsync(KrispyLines.Help); }

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

        private static int currentPresentI = 0, currentPresentJ = 0;

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
            else if (components.Contains("open")) {
                if (msg.Author.Id == 199158747904475136)
                    switch (currentPresentI++) {
                        case 0:
                            await msg.Channel.SendMessageAsync("*You find a balloon. I think you can wear it on your head.*\nGo ahead, put it on :balloon::tophat:");
                            await msg.Channel.SendMessageAsync("There's one more present left...");
                            break;
                        case 1:
                            await msg.Channel.SendMessageAsync("*You find an empty salt container. There's a note beside it with \"for all your league of legends games.\" written on it.*");
                            await msg.Channel.SendMessageAsync("I realize these presents were pretty awful (and not really real but I mean), and I'm sorry... I'm not really great at giving out presents. But anyway, hope you have a happy birthday!");
                            break;
                        default:
                            await msg.Channel.SendMessageAsync("I didn't get you more than two presents... how much money do you think I have?");
                            break;
                    }
                else if (msg.Author.Id == 271076385009696769)
                    switch (currentPresentJ++) {
                        case 0:
                            await msg.Channel.SendMessageAsync("*You find a roomba. Another one to add to your collection, I guess.*");
                            await msg.Channel.SendMessageAsync("More cute cat pictures please :)");
                            break;
                        default:
                            await msg.Channel.SendMessageAsync("That was your only present... sorry it took a while for the roomba to ship.");
                            break;
                    }
                else await msg.Channel.SendMessageAsync("Hey... leave the presents for the birthday boy, okay?");
            }
            else return false;
            return true;
        }
    }
}