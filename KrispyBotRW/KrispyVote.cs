using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KrispyBotRW {
    public class KrispyVote : ModuleBase<SocketCommandContext> {
        private const ulong NullVoter = 0;
        private const int DefaultMaxVotes = 2;
        
        class ElectionProfile {
            public readonly ulong UserId;
            public int MaxVotes;
            
            public readonly List<ulong> VotedFor = new List<ulong>();
            public readonly List<ulong> WasVotedBy = new List<ulong>();

            public ElectionProfile(ulong id) {
                UserId = id;
                MaxVotes = DefaultMaxVotes;
            }
        }

        private static readonly KrispyDict<KrispyDict<ElectionProfile>> Profiles
            = new KrispyDict<KrispyDict<ElectionProfile>>(
                x => new KrispyDict<ElectionProfile>(
                    y => new ElectionProfile(y)));

        private static readonly KrispyDict<Dictionary<string, ulong>> Names
            = new KrispyDict<Dictionary<string, ulong>>(
                x => new Dictionary<string, ulong>());

        public static void GenNamesStatic(SocketGuild guild) {
            var serverNames = Names.GetOrCreate(guild.Id);
            serverNames.Clear();
            foreach (var user in guild.Users) {
                if (user.IsBot || user.Nickname == null || !user.Nickname.Contains('|')) continue;
                
                var first = KrispyNickname.FirstName(user.Nickname).ToLower();
                var nick = KrispyNickname.NickName(user.Nickname).ToLower();
                
                if (serverNames.ContainsKey(first)) serverNames[first] = NullVoter;
                else serverNames[first] = user.Id;

                if (serverNames.ContainsKey(nick)) serverNames[nick] = NullVoter;
                else serverNames[nick] = user.Id;
            }
        }
        
        [Command("vt-gen")]
        public async Task GenNames() {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("Only admins can use this command.");
                return;
            }

            GenNamesStatic(Context.Guild);

            await ReplyAsync("Names generated.");
        }

        [Command("vt-alias")]
        public async Task AliasName(string name, SocketUser alias) {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("Only admins can use this command.");
                return;
            }
            
            Names.GetOrCreate(Context.Guild.Id)[name.ToLower()] = alias.Id;
            await ReplyAsync("Aliased.");
        }

        [Command("vt-clear-names")]
        public async Task ClearNames() {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("Only admins can use this command.");
                return;
            }
            
            if (Names.ContainsKey(Context.Guild.Id)) Names.Remove(Context.Guild.Id);
            await ReplyAsync("Names cleared.");
        }

        [Command("vt-clear-profile")]
        public async Task ClearProfile(SocketUser user) {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("Only admins can use this command.");
                return;
            }
            
            var serverProfiles = Profiles.GetOrCreate(Context.Guild.Id);
            var profile = serverProfiles.GetOrCreate(user.Id);

            foreach (var voted in profile.VotedFor)
                if (voted != NullVoter)
                    serverProfiles.GetOrCreate(voted).WasVotedBy.Remove(user.Id);
            foreach (var voter in profile.WasVotedBy)
                if (voter != NullVoter)
                    serverProfiles.GetOrCreate(voter).VotedFor.Remove(user.Id);

            serverProfiles.Remove(user.Id);

            await ReplyAsync("Profile cleared.");
        }

        [Command("profile")]
        public async Task ForceProfile(SocketUser user) {
           var guildUser = (IGuildUser) user;
            var profile = Profiles.GetOrCreate(Context.Guild.Id).GetOrCreate(user.Id);
            var desc = new StringBuilder();

            desc.Append("Voted For:\n");
            foreach (var voted in profile.VotedFor) {
                if (voted == NullVoter) continue;
                var voteUser = Context.Guild.GetUser(voted);
                if (voteUser == null) continue;
                var nick = KrispyNickname.NickName(voteUser.Nickname);
                desc.Append("\t" + nick + "\n");
            }
            
            desc.Append("Was Voted By:\n");
            foreach (var voter in profile.WasVotedBy) {
                var nick = voter == NullVoter
                    ? "Krispy Bot"
                    : KrispyNickname.NickName(Context.Guild.GetUser(voter).Nickname);
                desc.Append("\t" + nick + "\n");
            }
            
            var embed = new EmbedBuilder()
                .WithTitle(guildUser.Nickname + "'s profile")
                .WithDescription(desc.ToString())
                .WithCurrentTimestamp()
                .WithImageUrl("https://cdn.discordapp.com/avatars/" + user.Id + "/" + user.AvatarId + ".png")
                .WithColor(Color.Red)
                .Build();

            await ReplyAsync("", false, embed);
        }
        
        [Command("profile")]
        public async Task ShowProfile(string candidate) {
            var user = await GetUserByCandidate(candidate);
            if (user != null) await ForceProfile(user);
        }

        [Command("vt-vote")]
        public async Task ForceVote(SocketUser user) {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("Only admins can use this command.");
                return;
            }
            
            Profiles.GetOrCreate(Context.Guild.Id).GetOrCreate(user.Id).WasVotedBy.Add(NullVoter);
        }

        [Command("vt-grant")]
        public async Task GrantVote(SocketUser user) {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("Only admins can use this command.");
                return;
            }
            
            Profiles.GetOrCreate(Context.Guild.Id).GetOrCreate(user.Id).MaxVotes++;

            await ReplyAsync("Vote granted.");
        }
        
        [Command("vt-clear")]
        public async Task ClearVotes() {
            if (!Context.User.IsAdmin()) {
                await ReplyAsync("Only admins can use this command.");
                return;
            }
            Profiles.Clear();
            await ReplyAsync("Votes cleared.");
        }

        private async Task<SocketUser> GetUserByCandidate(string user) {
            if (!Names.ContainsKey(Context.Guild.Id)) {
                await ReplyAsync("I can't tell who you're talking about. Have the admin run $vt-gen and see if it helps.");
                return null;
            }

            var compName = user.ToLower();
            
            var names = Names[Context.Guild.Id];
            if (names.ContainsKey(compName)) {
                var voted = names[compName];
                if (voted == NullVoter)
                    await ReplyAsync("Which " + user + " are you talking about? Try a nickname, or s-something. Baka!");
                else return Context.Guild.GetUser(voted);
            } else await ReplyAsync("S-sorry... I didn't find who you were talking about." +
                                    "Try entering their first name- or their full nickname!");
            return null;
        }
        
//        [Command("vote")]
//        public async Task Vote(SocketUser user) {
//            var serverProfile = Profiles.GetOrCreate(Context.Guild.Id);
//            var voter = serverProfile.GetOrCreate(Context.User.Id);
//            var voted = serverProfile.GetOrCreate(user.Id);
//            if (voter == voted) {
//                await ReplyAsync("Hey, d-don't vote for yourself... b-baka");
//            } else if (voter.VotedFor.Count >= voter.MaxVotes) {
//                await ReplyAsync("You already voted too many times, stupid!");
//            } else if (user.IsAdmin()) {
//                await ReplyAsync(user.Username + " is already an admin! You'll get me in trouble!");
//            } else if (user.Id == Context.Client.CurrentUser.Id) {
//                await ReplyAsync("Y-you want me!? *blushes* B-but I can't...");
//            } else if (user.IsBot) {
//                await ReplyAsync("You want another bot? :sob: I won't let you!");
//            } else {
//                voter.VotedFor.Add(voted.UserId);
//                voted.WasVotedBy.Add(voter.UserId);
//                //await Context.Message.DeleteAsync();
//                await ReplyAsync(KrispyGenerator.PickLine(KrispyLines.Votes));
//            }
//        }
//
//        [Command("vote")]
//        public async Task Vote(string candidate) {
//            var user = await GetUserByCandidate(candidate);
//            if (user != null) await Vote(user);
//        }

        [Command("vt-leaderboard")]
        public async Task ShowLeaderboard() {
            var builder = new StringBuilder();
            builder.Append("```\n");
            var serverProfiles = Profiles.GetOrCreate(Context.Guild.Id);
            var profileValues = new ElectionProfile[serverProfiles.Count];
            serverProfiles.Values.CopyTo(profileValues, 0);
            Array.Sort(profileValues, (x, y) => y.WasVotedBy.Count - x.WasVotedBy.Count);
            foreach (var profile in profileValues) {
                if (profile.WasVotedBy.Count < 1) continue;
                var user = Context.Guild.GetUser(profile.UserId);
                if (user == null) continue;
                var name = KrispyNickname.NickName(user.Nickname);
                builder.Append(name.PadRight(20) + " | " + profile.WasVotedBy.Count.ToString().PadLeft(4) + "\n");
            }
            builder.Append("```");
            await ReplyAsync(builder.ToString());
        }
    }
}