using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KrispyBotRW.Ninja {
    public class KrispyNinjas : ModuleBase<SocketCommandContext> {
        public static void AdvanceGames() {
            var removeGames = new List<NinjaGame>();
            foreach (var game in NinjaGame.Games) {
                var result = game.Step();
                if (result == NinjaGame.StepResult.End) removeGames.Add(game);
            }
            foreach (var game in removeGames) NinjaGame.Games.Remove(game);
        }

        public static void RestoreHP() {
            foreach (var profile in NinjaProfile.Profiles.Values)
                profile.CurrentHP = Math.Min(profile.MaxHP, profile.CurrentHP + 3);
        }
        
        [Command("nj-load")]
        public async Task NinjaLoad() {
            NinjaProfile.Profiles.Clear();
            var formatter = new BinaryFormatter();
            var fs = new FileStream("ninjas.bin", FileMode.Open, FileAccess.Read);
            var saveProfiles = (List<NinjaSaveProfile>) formatter.Deserialize(fs);
            foreach (var save in saveProfiles) {
                var cprof = save.CreateProfile();
                NinjaProfile.Profiles.Add(cprof.UserId, cprof);
            }
            fs.Close();
        }

        [Command("nj-save")]
        public async Task NinjaSave() {
            var formatter = new BinaryFormatter();
            var fs = new FileStream("ninjas.bin", FileMode.Create, FileAccess.Write);
            var saveProfiles = new List<NinjaSaveProfile>();
            foreach (var profile in NinjaProfile.Profiles.Values)
                saveProfiles.Add(profile.CreateSaveProfile());
            formatter.Serialize(fs, saveProfiles);
            fs.Close();
            await ReplyAsync("Everything is saved. Feel free to reset the bot.");
        }
        
        [Command("nj-message")]
        public async Task NinjaChangeMessage([Remainder] string message) {
            NinjaProfile.GetOrCreate(Context.User.Id).ChallengeMessage = message;
            await ReplyAsync("Your challenge message has been changed!");
        }
        
        [Command("nj-show")]
        public async Task NinjaShow(SocketUser user) {
            try {
                await ReplyAsync("", false, NinjaProfile.GetOrCreate(user.Id).CreateEmbed(Context.Client));
            } catch (Exception e) {
                await ReplyAsync(e.Message + "\n```\n" + e.StackTrace + "\n```");
            }
        }

        [Command("nj")]
        public async Task Nj(SocketUser user) { await Ninja(user); }
        
        [Command("ninja")]
        public async Task Ninja(SocketUser user) {
            ulong challenger = Context.User.Id, defender = user.Id;

            if (challenger == defender)
                await ReplyAsync("Hey... you can't challenge yourself!");
            else if (NinjaProfile.GetOrCreate(challenger).Game != null ||
                     NinjaProfile.GetOrCreate(defender).Game != null)
                await ReplyAsync("One of you are still engaged in combat. Wait for your next chance to attack!");
            else NinjaGame.Games.Add(new NinjaGame(challenger, defender, Context.Channel));
        }
        
        [Command("beta")]
        public async Task OptIn() {
            await ((IGuildUser) Context.User).AddRoleAsync(Context.Guild.GetRole(454255214606811146));
            await ReplyAsync("Now you're part of the ninja beta :D");
        }
    }
}