using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace KrispyBotRW {
    public class KrispyNickname : ModuleBase<SocketCommandContext> {
        public static string FirstName(string nickname) {
            return nickname.Substring(0, nickname.IndexOf('|')).Trim();
        }

        public static string NickName(string nickname) {
            return nickname.Substring(nickname.IndexOf('|') + 1).Trim();
        }
        
        [Command("nickname")]
        public async Task NewNickname([Remainder] string nickname) {
            var user = (IGuildUser) Context.User;
            var fullNickname = user.Nickname;
            var firstName = fullNickname.Substring(0, fullNickname.IndexOf('|')).Trim();
            var newNickname = KrispyLines.CapitalizeAll(nickname);
            var finalNickname = firstName + " | " + newNickname;
            if (finalNickname.Length > 32)
                await ReplyAsync(
                    string.Format(KrispyLines.Nickname[0], 32 - (firstName.Length + 3))
                    );
            else {
                await user.ModifyAsync(x => { x.Nickname = finalNickname; });
                await ReplyAsync(
                    string.Format(KrispyLines.Nickname[1], KrispyGenerator.PickLine(KrispyLines.Emoticon))
                    );
            }
        }
    }
}