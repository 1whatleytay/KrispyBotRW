using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace KrispyBotRW {
    public class KrispyNickname : ModuleBase<SocketCommandContext> {
        public static string FirstName(string nickname) {
            return nickname.Contains('|') ? nickname.Substring(0, nickname.IndexOf('|')).Trim() : nickname;
        }

        public static string NickName(string nickname) {
            return nickname.Contains('|') ? nickname.Substring(nickname.IndexOf('|') + 1).Trim() : nickname;
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
                        "That's a long nickname. Try to keep it under " + (32 - (firstName.Length + 3)) + " characters."
                    );
            else {
                await user.ModifyAsync(x => { x.Nickname = finalNickname; });
                await ReplyAsync("I changed your nickname " + KrispyGenerator.PickLine(KrispyLines.Emoticon));
            }
        }
    }
}