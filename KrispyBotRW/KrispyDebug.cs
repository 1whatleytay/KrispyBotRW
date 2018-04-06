using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace KrispyBotRW {
    public class KrispyDebug : ModuleBase<SocketCommandContext> {
        [Command("dummy")]
        public async Task Dummy([Remainder] string text) {
            Console.WriteLine(text);
            await ReplyAsync(
                string.Format(KrispyLines.Debug[0], 
                    KrispyGenerator.PickLine(KrispyLines.Emoticon)));
        }

        [Command("error")]
        public Task ThrowError() {
            throw new Exception(KrispyGenerator.GenerateBeeps());
        }
        
        [Command("version")]
        public async Task Version() {
            await ReplyAsync(
                string.Format(KrispyLines.Debug[1],
                    KrispyCore.VersionMajor, KrispyCore.VersionMinor, KrispyCore.Revision));
        }
    }
}