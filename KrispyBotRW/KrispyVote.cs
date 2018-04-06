using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace KrispyBotRW {
    public class KrispyVote : ModuleBase<SocketCommandContext> {
        [Command("vote")]
        public async Task Vote([Remainder] string user) {
            await ReplyAsync("There isn't an active vote at the moment :(");
            //TODO implement voting system after rewrite
        }
    }
}