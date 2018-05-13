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
                if (!CheckTime()) return;
                Messages++;
                Images += withImages;
            }
            public void GiftPoints(int points) { Gifted += points; }

            public int GetScore() { return Messages + Images * 3 + Gifted; }

            public ContributionProfile(ulong userId) { UserId = userId; }
        }

        private static readonly Dictionary<ulong, ContributionProfile> profiles
            = new Dictionary<ulong, ContributionProfile>();

        private static readonly ulong[] blockedChannels = {
            378337613087637505, // #bot-commands
            423902578217189377, // #duckhunt
            438712122059259905, // #tatsugotchi
        };
        
        public static void ProcessMessage(SocketMessage message) {
            if (blockedChannels.Contains(message.Channel.Id)) return;
            var userId = message.Author.Id;
            if (!profiles.ContainsKey(userId)) profiles.Add(userId, new ContributionProfile(userId));
            profiles[userId].NewMessage(message.Attachments.Count);
        }

        [Command("gift-points")]
        public async Task GiftPoints(SocketUser user, int number) {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            if (!profiles.ContainsKey(user.Id)) profiles.Add(user.Id, new ContributionProfile(user.Id));
            profiles[user.Id].GiftPoints(number);
            await ReplyAsync(";)");
        }

        [Command("clean-contributions")]
        public async Task CleanContributions(SocketUser user) {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            if (!profiles.ContainsKey(user.Id)) await ReplyAsync("I couldn't find the user you were talking about :(");
            else {
                profiles.Remove(user.Id);
                await ReplyAsync("I removed all their contrbutions " + KrispyGenerator.PickLine(KrispyLines.Emoticon));
            }
        }

        [Command("reset-contributions")]
        public async Task ResetContributions() {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            profiles.Clear();
            await ReplyAsync("All contributions reset!");
        }

        [Command("leaderboard")]
        public async Task ShowLeaderboard() {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            var builder = new StringBuilder("```\n");
            var profileValues = new ContributionProfile[profiles.Values.Count];
            profiles.Values.CopyTo(profileValues, 0);
            Array.Sort(profileValues, (x, y) => y.GetScore() - x.GetScore());
            foreach (var profile in profileValues) {
                var user = Context.Client.GetUser(profile.UserId);
                if (user == null) continue;
                builder.Append(
                    (user.Username + "#" + user.Discriminator).PadRight(30) + " | " +
                    profile.GetScore().ToString().PadLeft(10) + "\n");
            }
            if (profiles.Count == 0) builder.Append("Nothing yet!");
            builder.Append("```");
            await ReplyAsync(builder.ToString());
        }
    }
}