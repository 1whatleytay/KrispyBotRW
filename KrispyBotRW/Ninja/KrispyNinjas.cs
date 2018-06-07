using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;

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
                profile.CurrentHP = Math.Min(profile.MaxHP, profile.CurrentHP + 1);
        }

        [Command("ninja-message")]
        public async Task ChangeNinjaMessage([Remainder] string message) {
            NinjaProfile.GetOrCreate(Context.User.Id).ChallengeMessage = message;
        }
        
        [Command("expose")]
        public async Task ExposeNinja([Remainder] SocketUser user) {
            await ReplyAsync("", false, NinjaProfile.GetOrCreate(user.Id).CreateEmbed(Context.Client));
        }

        [Command("ninja")]
        public async Task Ninja([Remainder] SocketUser user) {
            ulong challenger = Context.User.Id, defender = user.Id;
            
            if (NinjaProfile.GetOrCreate(challenger).Game != null || NinjaProfile.GetOrCreate(defender).Game != null)
                await ReplyAsync("One of you are still engaged in combat. Wait for your next chance to attack!");
            else NinjaGame.Games.Add(new NinjaGame(challenger, defender, Context.Channel));
        }
    }
}