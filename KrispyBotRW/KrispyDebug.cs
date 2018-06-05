using System;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace KrispyBotRW {
    public class KrispyDebug : ModuleBase<SocketCommandContext> {
        [Command("dummy")]
        public async Task Dummy([Remainder] string text) {
            Console.WriteLine(text);
            await ReplyAsync("Ditto " + KrispyGenerator.PickLine(KrispyLines.Emoticon));
        }

        [Command("error")]
        public Task ThrowError() {
            throw new Exception(KrispyGenerator.GenerateBeeps());
        }
        
        [Command("probe")]
        public async Task Probe() {
            var builder = new StringBuilder();
            foreach (var role in Context.Guild.Roles)
                builder.Append(role.Name.PadRight(30) + " " + role.Id + "\n");
            await ReplyAsync("```\n" + builder.ToString() + "\n```");
        }
        
        [Command("version")]
        public async Task Version() {
            await ReplyAsync("Hi :D I'm Krispy Bot (RW)™ version: " +
                             KrispyCore.VersionMajor + "." + KrispyCore.VersionMinor + "." + KrispyCore.Revision);
        }
    }
}