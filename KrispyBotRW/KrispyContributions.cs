using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord.Commands;
using Discord.WebSocket;

namespace KrispyBotRW {
    public class KrispyContributions : ModuleBase<SocketCommandContext> {
        private class ContributionProfile {
            private const int TimeoutTime = 30;

            public readonly ulong UserId;
            private int Messages, Images, Gifted;
            private DateTime Timeout = DateTime.Now;

            private bool CheckTime() {
                if (Timeout > DateTime.Now) return false;
                Timeout = DateTime.Now + new TimeSpan(0, 0, 0, TimeoutTime);
                return true;
            }

            public void NewMessage(int withImages) {
                Images += withImages;
                if (!CheckTime()) return;
                Messages++;
            }
            public void GiftPoints(int points) { Gifted += points; }

            public int GetScore() { return Messages + Images + Gifted; }

            public ContributionProfile(ulong userId) { UserId = userId; }
        }

        private static readonly Dictionary<ulong, ContributionProfile> Profiles
            = new Dictionary<ulong, ContributionProfile>();

        private static readonly ulong[] BlockedChannels = {
            378337613087637505, // #bot-commands
            423902578217189377, // #duckhunt
            438712122059259905, // #tatsugotchi
        };
        
        public static void ProcessMessage(SocketMessage message) {
            if (BlockedChannels.Contains(message.Channel.Id)) return;
            var userId = message.Author.Id;
            if (!Profiles.ContainsKey(userId)) Profiles.Add(userId, new ContributionProfile(userId));
            Profiles[userId].NewMessage(message.Attachments.Count);
        }

        [Command("ct-gift")]
        public async Task GiftPoints(SocketUser user, int number) {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            if (!Profiles.ContainsKey(user.Id)) Profiles.Add(user.Id, new ContributionProfile(user.Id));
            Profiles[user.Id].GiftPoints(number);
            await ReplyAsync(";)");
        }

        [Command("ct-clean")]
        public async Task CleanContributions(SocketUser user) {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            if (!Profiles.ContainsKey(user.Id)) await ReplyAsync("I couldn't find the user you were talking about :(");
            else {
                Profiles.Remove(user.Id);
                await ReplyAsync("I removed all their contrbutions " + KrispyGenerator.PickLine(KrispyLines.Emoticon));
            }
        }

        [Command("ct-reset")]
        public async Task ResetContributions() {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            Profiles.Clear();
            await ReplyAsync("All contributions reset!");
        }

        [Command("ct-leaderboard")]
        public async Task ShowLeaderboard() {
            var builder = new StringBuilder("```\n");
            var profileValues = new ContributionProfile[Profiles.Values.Count];
            Profiles.Values.CopyTo(profileValues, 0);
            Array.Sort(profileValues, (x, y) => y.GetScore() - x.GetScore());
            foreach (var profile in profileValues) {
                var user = Context.Guild.GetUser(profile.UserId);
                if (user == null) continue;
                bool isAdminOrMod;
                foreach (var role in Context.Guild.Roles)
                    if (role.Id == 378339275189518336 || role.Id == 378339453166682112) {
                        isAdminOrMod = true;
                        break;
                    }
                if (isAdminOrMod) return;
                builder.Append(
                    (user.Username + "#" + user.Discriminator).PadRight(30) + " | " +
                    profile.GetScore().ToString().PadLeft(10) + "\n");
            }
            if (Profiles.Count == 0) builder.Append("Nothing yet!");
            builder.Append("```");
            await ReplyAsync(builder.ToString());
        }
    }
}