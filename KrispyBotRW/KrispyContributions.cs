using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
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
                if (CheckTime()) Messages++;
            }
            public void GiftPoints(int points) { Gifted += points; }

            public int GetScore() { return Messages + Images + Gifted; }

            public ContributionProfile(ulong userId) { UserId = userId; }
        }

        private static readonly KrispyDict<KrispyDict<ContributionProfile>> Profiles
            = new KrispyDict<KrispyDict<ContributionProfile>>(
                x => new KrispyDict<ContributionProfile>(
                    y => new ContributionProfile(y)));

        private static readonly ulong[] BlockedChannels = {
            378337613087637505, // #bot-commands
            423902578217189377, // #duckhunt
            438712122059259905, // #tatsugotchi
        };
        
        public static void ProcessMessage(SocketMessage message) {
            if (BlockedChannels.Contains(message.Channel.Id)) return;
            Profiles.GetOrCreate(message.GuildId())
                .GetOrCreate(message.Author.Id)
                .NewMessage(message.Attachments.Count);
        }

        [Command("dj")]
        public async Task MakeDJ() {
            var djRoleId = 378948854000648193ul;
            var djRole = Context.Guild.GetRole(djRoleId);
            var user = ((IGuildUser) Context.User);
            if (user.RoleIds.Contains(djRoleId)) {
                await user.RemoveRoleAsync(djRole);
                await ReplyAsync("You are no longer a DJ. DRINNGGGG *that's my best impression of mozart*");
            } else {
                await user.AddRoleAsync(djRole);
                await ReplyAsync("You are a DJ. Congrats!");
            }
        }
        
        [Command("streamer")]
        public async Task MakeStreamer() {
            var streamerRoleId = 443517231268233226ul;
            var streamerRole = Context.Guild.GetRole(streamerRoleId);
            var user = ((IGuildUser) Context.User);
            if (user.RoleIds.Contains(streamerRoleId)) {
                await user.RemoveRoleAsync(streamerRole);
                await ReplyAsync("You are not a streamer. Never was, never will be.");
            } else {
                await user.AddRoleAsync(streamerRole);
                await ReplyAsync("You are a Streamer. Congrats!");
            }
        }

        [Command("ct-gift")]
        public async Task GiftPoints(SocketUser user, int number) {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            Profiles.GetOrCreate(Context.Guild.Id).GetOrCreate(user.Id).GiftPoints(number);
            await ReplyAsync(";)");
        }

        [Command("ct-clean")]
        public async Task CleanContributions(SocketUser user) {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            if (!Profiles.ContainsKey(user.Id)) await ReplyAsync("I couldn't find the user you were talking about :(");
            else {
                Profiles.Remove(user.Id);
                await ReplyAsync("I removed all their contributions " + KrispyGenerator.PickLine(KrispyLines.Emoticon));
            }
        }

        [Command("ct-reset")]
        public async Task ResetContributions() {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            Profiles.Clear();
            await ReplyAsync("All contributions reset!");
        }

        [Command("leaderboard")]
        public async Task ShowLeaderboard() {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("The leaderboard is currently disabled.");
                return;
            }
            var builder = new StringBuilder("```\n");
            var serverProfiles = Profiles.GetOrCreate(Context.Guild.Id);
            var profileValues = new ContributionProfile[serverProfiles.Count];
            serverProfiles.Values.CopyTo(profileValues, 0);
            Array.Sort(profileValues, (x, y) => y.GetScore() - x.GetScore());
            foreach (var profile in profileValues) {
                var user = Context.Guild.GetUser(profile.UserId);
                if (user == null) continue;
                if (user.IsBot) continue;
                if (user.IsAdmin()) continue;
                builder.Append(
                    (user.Username + "#" + user.Discriminator).PadRight(30) + " | " +
                    profile.GetScore().ToString().PadLeft(10) + "\n");
            }
            builder.Append("```");
            await ReplyAsync(builder.ToString());
        }
    }
}