using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KrispyBotRW {
    public class KrispyTeams: ModuleBase<SocketCommandContext> {
        enum Teams : ulong {
            Green = 474735488692125716,
            Blue = 474735600415670273,
        }
        
        public static async Task RemoveTeamRoles(SocketGuild guild, IGuildUser user) {
            foreach (var team in Enum.GetValues(typeof(Teams)))
                if (user.RoleIds.Contains((ulong) team)) await user.RemoveRoleAsync(guild.GetRole((ulong) team));
        }
        
        [Command("green")]
        public async Task JoinTeamGreen() {
            await RemoveTeamRoles(Context.Guild, (IGuildUser)Context.User);
            await ((IGuildUser) Context.User).AddRoleAsync(Context.Guild.GetRole((ulong)Teams.Green));
            await ReplyAsync("Joined Team Green. " + String.Format(KrispyGenerator.PickLine(KrispyLines.Opposing), "Blue"));
        }
        
        [Command("blue")]
        public async Task JoinTeamBlue() {
            await RemoveTeamRoles(Context.Guild, (IGuildUser)Context.User);
            await ((IGuildUser) Context.User).AddRoleAsync(Context.Guild.GetRole((ulong)Teams.Blue));
            await ReplyAsync("Joined Team Blue. " + String.Format(KrispyGenerator.PickLine(KrispyLines.Opposing), "Green"));
        }

        [Command("member")]
        public async Task LeaveTeams() {
            await RemoveTeamRoles(Context.Guild, (IGuildUser)Context.User);
            await ReplyAsync("Yeah, you didn't need a team anyway.");
        }
    }
}