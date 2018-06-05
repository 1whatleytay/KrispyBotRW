using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KrispyBotRW {
    public class KrispyVote : ModuleBase<SocketCommandContext> {
        private const int MaxVotes = 3, MaxSubmissions = 2;

        private class UserProfile {
            public int CustomMaxVotes = MaxVotes;
            public int CustomMaxSubmissions = MaxSubmissions;
        }
        
        private class Candidate {
            public readonly IGuildUser Poster;
            public readonly string Url;
            public readonly string Name;
            public readonly DateTime Time;
            public readonly int Index;
            public bool Enabled = true;

            public readonly List<IGuildUser> Votes = new List<IGuildUser>();
            
            public readonly Embed FullEmbed;

            public Candidate(string name, string url, IGuildUser poster, int index = -1) {
                Name = name; Url = url; Poster = poster; Index = index;
                Time = DateTime.Now;
                
                FullEmbed = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithTitle("New Submission By: " + KrispyNickname.FirstName(Poster.Nickname))
                    .WithDescription("Name: " + Name + "\nVote Id: " + Index + "\nSubmitted at: " + Time)
                    .WithImageUrl(Url)
                    .Build();
            }
        }
        
        private class CandidateGroup : List<Candidate> {
            public int At(string emojiName) {
                for (var a = 0; a < Count; a++)
                    if (this[a].Name == emojiName)
                        return a;
                return -1;
            }

            public CandidateGroup Candidates(ulong user) {
                var group = new CandidateGroup();
                foreach (var candidate in this)
                    if (candidate.Poster.Id == user)
                        group.Add(candidate);
                return group;
            }

            public List<Candidate> Votes(ulong user) {
                var votes = new List<Candidate>();
                foreach (var candidate in this)
                    foreach (var vote in candidate.Votes)
                        if (vote.Id == user)
                            votes.Add(candidate);
                return votes;
            }

            public void RemoveUser(ulong user) {
                foreach (var candidate in this) {
                    for (var a = 0; a < candidate.Votes.Count; a++) {
                        if (candidate.Votes[a].Id == user)
                            candidate.Votes.RemoveAt(a);
                    }
                    if (candidate.Poster.Id == user)
                        candidate.Enabled = false;
                }
            }
        }
        
        private static readonly Dictionary<ulong, UserProfile> profiles = new Dictionary<ulong, UserProfile>();
        private static readonly CandidateGroup candidates = new CandidateGroup();
        
        [Command("show-votes")]
        public async Task ShowCandidates() {
            var text = new StringBuilder();
            foreach (var candidate in candidates) {
                if (candidate.Enabled)
                    text.Append(
                        ":" + candidate.Name + ": by " + KrispyNickname.FirstName(candidate.Poster.Nickname)
                        + " (Vote Id: " + candidate.Index + ")\n" + candidate.Url + "\n");
            }
            await ReplyAsync(text.ToString());
        }
        
        [Command("clean-vote")]
        public async Task DeleteCandidate(int voteId) {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User))
                await ReplyAsync("Sorry, only admins can use this command.");
            if (voteId >= candidates.Count || voteId < 0)
                await ReplyAsync("Sorry, I couldn't find the vote you were lookign for.");
            else if (!candidates[voteId].Enabled)
                await ReplyAsync("This submission has already been deleted.");
            else {
                candidates[voteId].Enabled = false;
                await ReplyAsync(
                    ":" + candidates[voteId].Name + ": by " +
                    KrispyNickname.FirstName(candidates[voteId].Poster.Nickname) + "successfully deleted.");
            }
        }

        [Command("clean-vote")]
        public async Task DeleteCandidate(string emojiName) { await DeleteCandidate(candidates.At(emojiName)); }
        
        [Command("show-profile")]
        public async Task ShowProfile(SocketUser user) {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            var ccands = candidates.Candidates(user.Id);
            var cvotes = candidates.Votes(user.Id);
            var maxVotesDesc = profiles.ContainsKey(user.Id)
                ? "Max Votes: " + profiles[user.Id].CustomMaxVotes
                : "";
            var maxSubmissionsDesc = profiles.ContainsKey(user.Id)
                ? "Max Submissions: " + profiles[user.Id].CustomMaxSubmissions
                : "";
            var submissionsDesc = new StringBuilder("Submissions:\n");
            foreach (var cand in ccands)
                if (cand.Enabled)
                    submissionsDesc.Append("\t:" + cand.Name + ": (" + cand.Index + ")");
            var votesDesc = new StringBuilder("Votes:\n");
            foreach (var vote in cvotes) {
                if (vote.Enabled)
                    votesDesc.Append("\t:" + vote.Name + ": (" + vote.Index + ")");
            }
            var profile = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle(((IGuildUser)user).Nickname)
                .WithDescription(
                    maxVotesDesc + "\n" +
                    maxSubmissionsDesc + "\n" +
                    submissionsDesc + "\n" +
                    votesDesc)
                .Build();
            await ReplyAsync("", false, profile);
        }

        [Command("clean-profile")]
        public async Task CleanProfile(SocketUser user) {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            if (profiles.ContainsKey(user.Id)) profiles.Remove(user.Id);
            candidates.RemoveUser(user.Id);
            await ReplyAsync("User " + user.Username + "#" + user.Discriminator + " successfully cleaned!");
        }

        [Command("grant-vote")]
        public async Task GrantVote(SocketUser user) {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            if (profiles.ContainsKey(user.Id)) profiles[user.Id].CustomMaxVotes++;
            else profiles.Add(user.Id, new UserProfile { CustomMaxVotes = MaxVotes + 1 });
            await ReplyAsync("Granted vote to " + user.Username + "#" + user.Discriminator + ".");
        }

        [Command("grant-submission")]
        public async Task GrantSubmission(SocketUser user) {
            if (!KrispyCommands.UserIsKrispyAdmin(Context.User)) {
                await ReplyAsync("Sorry, only admins can use this command.");
                return;
            }
            if (profiles.ContainsKey(user.Id)) profiles[user.Id].CustomMaxSubmissions++;
            else profiles.Add(user.Id, new UserProfile { CustomMaxSubmissions = MaxSubmissions + 1 });
            await ReplyAsync("Granted submission to " + user.Username + "#" + user.Discriminator + ".");
        }
        
        [Command("vote")]
        public async Task Vote(int voteId) {
            if (voteId >= candidates.Count || voteId < 0) {
                await ReplyAsync("I couldn't find the submission you were talking about... try again?");
                return;
            }
            var candidate = candidates[voteId];
            if (candidate.Enabled) {
                if (candidate.Poster.Id == Context.User.Id) await ReplyAsync("Hey... you can't vote for yourself!");
                else {
                    var cvotes = profiles.ContainsKey(Context.User.Id)
                        ? profiles[Context.User.Id].CustomMaxVotes
                        : MaxVotes;
                    if (candidates.Votes(Context.User.Id).Count >= cvotes) {
                        await ReplyAsync("You've already voted " + cvotes + " time" + (cvotes == 1 ? "" : "s") + ".");
                    } else {
                        candidate.Votes.Add((IGuildUser)Context.User);
                        await ReplyAsync("I counted your vote!");
                    }
                }
            } else await ReplyAsync("Sorry! This post has been deleted.");
        }
        
        [Command("vote")]
        public async Task Vote(string emojiName) { await Vote(candidates.At(emojiName)); }

        [Command("submit")]
        public async Task Submit() { await ReplyAsync("You might want to add a name to your submission. Use `@Krispy Bot submit <name>` and attach your image."); }

        [Command("submit")]
        public async Task Submit([Remainder] string emojiName) {
            if (Context.Message.Attachments.Count < 1)
                await ReplyAsync("... you didn't submit anything. Try again, but upload an image. I don't think you can paste links either.");
            else {
                var cmaxSubmissions = (profiles.ContainsKey(Context.User.Id)
                    ? profiles[Context.User.Id].CustomMaxSubmissions
                    : MaxSubmissions);
                if (candidates.Candidates(Context.User.Id).Count >= cmaxSubmissions)
                    await ReplyAsync("You've already posted " + cmaxSubmissions + " submissions!");
                else {
                    var cname = KrispyLines.Emojify(emojiName);
                    var existingSubmission = candidates.At(cname);
                    if (existingSubmission == -1) {
                        var candidate = new Candidate(
                            KrispyLines.Emojify(emojiName),
                            Context.Message.Attachments.First().Url,
                            (IGuildUser) Context.User,
                            candidates.Count);
                        candidates.Add(candidate);
                        await ReplyAsync("", false, candidate.FullEmbed);
                    } else {
                        var submission = candidates[existingSubmission];
                        var poster = submission.Poster.Id == Context.User.Id
                            ? "You"
                            : KrispyNickname.FirstName(submission.Poster.Nickname);
                        await ReplyAsync("Sorry. " + poster + " already posted a submission with the same name. Try another name!");
                    }
                }
            }
        }
    }
}