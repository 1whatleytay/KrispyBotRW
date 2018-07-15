using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KrispyBotRW {
    public class KrispyGames : ModuleBase<SocketCommandContext> {
        private async Task<List<SocketRole>> GetGameRoles(string games, bool isPlaying = true) {
            games = games.ToLower();
            var roles = new List<SocketRole>();
            bool isPc = games.Contains("pc"), isConsole = games.Contains("console");
            if (games.Contains("paladins")) {
                if (isPc) roles.Add(Context.Guild.GetRole(409730824146124800));
                if (isConsole) roles.Add(Context.Guild.GetRole(409730879775047701));
                if (!isPc && !isConsole) {
                    await ReplyAsync((isPlaying ?
                        "Oh great! You play Paladins? I never knew. I'm a little curious on what you play it on." :
                        "Sad to see you leave Paladins... I just want to know on which platform.") +
                                     " Be sure to add \"PC\" or \"Console\" to your request.");
                    return null;
                }
            }
            if (games.Contains("brawl")) {
                if (isPc) roles.Add(Context.Guild.GetRole(409731094817275905));
                if (isConsole) roles.Add(Context.Guild.GetRole(409773879343579137));
                if (!isPc && !isConsole) {
                    await ReplyAsync((isPlaying ?
                        "Sooo... you play Brawlhalla? But on what?" :
                        "Brawlhalla says bye bye. But where? On your desktop monitor or your TV screen?") +
                                     " Be sure to add \"PC\" or \"Console\" to your request.");
                    return null;
                }
            }
            if (games.Contains("fortnite"))
                roles.Add(Context.Guild.GetRole(430116935724826636));
            if (games.Contains("lol") || games.Contains("league") || games.Contains("legends"))
                roles.Add(Context.Guild.GetRole(409730970556694538));
            if (games.Contains("tera") || games.Contains("terra"))
                roles.Add(Context.Guild.GetRole(433825760898187265));
            if (games.Contains("counter") || games.Contains("strike") || games.Contains("cs"))
                roles.Add(Context.Guild.GetRole(433828628980170753));
            if (games.Contains("player") || games.Contains("unknown") ||
                games.Contains("battlegrounds") || games.Contains("pubg"))
                roles.Add(Context.Guild.GetRole(433825483558354957));
            if (games.Contains("dec"))
                roles.Add(Context.Guild.GetRole(452965439916605440));
            if (games.Contains("admin")) {
                await ReplyAsync("Sneaky... I'll just pretend I didn't see that.");
                return null;
            }
            return roles;
        }
        
        [Command("plays")]
        public async Task AddGames([Remainder] string games) {
            var gameRoles = await GetGameRoles(games);
            if (gameRoles == null) return;
            switch (gameRoles.Count) {
                case 0:
                    await ReplyAsync("You didn't specify a single game... try again?");
                    break;
                case 1:
                    await ((IGuildUser)Context.User).AddRoleAsync(gameRoles[0]);
                    await ReplyAsync("You play " + gameRoles[0].Name + "? I never knew. Here, I'll add that role for you.");
                    break;
                default:
                    await ((IGuildUser)Context.User).AddRolesAsync(gameRoles);
                    await ReplyAsync("Nice! I've added those games to your role list if you don't mind.");
                    break;
            }
        }
        
        [Command("nplays")]
        public async Task RemoveGames([Remainder] string games) {
            var gameRoles = await GetGameRoles(games, false);
            if (gameRoles == null) return;
            switch (gameRoles.Count) {
                case 0:
                    await ReplyAsync("You didn't specify a single game... try again?");
                    break;
                case 1:
                    await ((IGuildUser) Context.User).RemoveRoleAsync(gameRoles[0]);
                    await ReplyAsync(string.Format("Sad to see you leave {0}. Here, I'll remove that role from you.", gameRoles[0].Name));
                    break;
                default:
                    await ((IGuildUser) Context.User).RemoveRolesAsync(gameRoles);
                    await ReplyAsync("Well, I've removed those games from your role list.");
                    break;
            }
        }
    }
}