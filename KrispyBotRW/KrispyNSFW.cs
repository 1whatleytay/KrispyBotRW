using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace KrispyBotRW {
    public class KrispyNSFW : ModuleBase<SocketCommandContext> {
        [Command("nsfw")]
        public async Task AddNSFW() {
            await ((IGuildUser) Context.User).AddRoleAsync(Context.Guild.GetRole(378355640273207296));
            await ReplyAsync(KrispyLines.NSFW);
        }
    }
}