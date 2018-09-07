using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KrispyBotRW {
    public class KrispyGames : ModuleBase<SocketCommandContext> {
        private static async Task<List<SocketRole>> GetGameRoles(SocketCommandContext context, string games) {
            games = games.ToLower();
            var roles = new List<SocketRole>();
            bool isPc = games.Contains("pc"), isConsole = games.Contains("console");
            if (games.Contains("paladins")) {
                if (isConsole) roles.Add(context.Guild.GetRole(409730879775047701));
                else roles.Add(context.Guild.GetRole(409730824146124800));
            }
            if (games.Contains("brawl")) {
                if (isConsole) roles.Add(context.Guild.GetRole(409773879343579137));
                else roles.Add(context.Guild.GetRole(409731094817275905));
            }
            if (games.Contains("fortnite"))
                roles.Add(context.Guild.GetRole(430116935724826636));
            if (games.Contains("lol") || games.Contains("league") || games.Contains("legends"))
                roles.Add(context.Guild.GetRole(409730970556694538));
            if (games.Contains("tera") || games.Contains("terra"))
                roles.Add(context.Guild.GetRole(433825760898187265));
            if (games.Contains("counter") || games.Contains("strike") || games.Contains("cs"))
                roles.Add(context.Guild.GetRole(433828628980170753));
            if (games.Contains("player") || games.Contains("unknown") ||
                games.Contains("battleground") || games.Contains("pubg"))
                roles.Add(context.Guild.GetRole(487408127239651364));
            if (games.Contains("dec"))
                roles.Add(context.Guild.GetRole(452965439916605440));
            if (games.Contains("over") || games.Contains("watch"))
                roles.Add(context.Guild.GetRole(487392446016127006));
            if (games.Contains("admin")) {
                await context.Channel.SendMessageAsync("Sneaky... I'll just pretend I didn't see that.");
                return null;
            }
            return roles;
        }

        public static async Task AddGamesContext(SocketCommandContext context, string games) {
            var gameRoles = await GetGameRoles(context, games);
            if (gameRoles == null) return;
            switch (gameRoles.Count) {
                case 0:
                    await context.Channel.SendMessageAsync("You didn't specify a single game... try again?");
                    break;
                case 1:
                    await ((IGuildUser)context.User).AddRoleAsync(gameRoles[0]);
                    await context.Channel.SendMessageAsync("You play " + gameRoles[0].Name + "? I never knew. Here, I'll add that role for you.");
                    break;
                default:
                    await ((IGuildUser)context.User).AddRolesAsync(gameRoles);
                    await context.Channel.SendMessageAsync("Nice! I've added those games to your role list if you don't mind.");
                    break;
            }
        }
        
        [Command("play")]
        public async Task AddGames([Remainder] string games) {
            await AddGamesContext(Context, games);
        }

        [Command("plays")] public async Task AddGamesAlias([Remainder] string games) { await AddGames(games); }

        public static async Task RemoveGamesContext(SocketCommandContext context, string games) {
            var gameRoles = await GetGameRoles(context, games);
            if (gameRoles == null) return;
            switch (gameRoles.Count) {
                case 0:
                    await context.Channel.SendMessageAsync(KrispyGenerator.PickLine(KrispyLines.Disappointed));
                    break;
                case 1:
                    await ((IGuildUser) context.User).RemoveRoleAsync(gameRoles[0]);
                    await context.Channel.SendMessageAsync(string.Format("Sad to see you leave {0}. Here, I'll remove that role from you.", gameRoles[0].Name));
                    break;
                default:
                    await ((IGuildUser) context.User).RemoveRolesAsync(gameRoles);
                    await context.Channel.SendMessageAsync("Well, I've removed those games from your role list.");
                    break;
            }
        }
        
        [Command("leave")]
        public async Task RemoveGames([Remainder] string games) {
            await RemoveGamesContext(Context, games);
        }

        [Command("nplays")] public async Task RemoveGamesAlias([Remainder] string games) { await AddGames(games); }
    }
}