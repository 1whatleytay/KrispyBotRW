using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace KrispyBotRW {
    public class KrispySchedule : ModuleBase<SocketCommandContext> {
        private enum ClassName : ulong {
            English = 487315578156220416,
            Math = 487315641745932298,
            History = 487315862227779634,
            Science = 487319372428476417,
            Tech = 487319407920414720,
            Foods = 487608334233370635,
            PhysEd = 487315912098054164,
            French = 487316015299035169,
            Strings = 487316120420745216,
            Business = 487442657891581954,
            Drama = 487316161327792129,
            VisualArts = 487316241711890432,
            CivicsCareers = 487316213786083329,
            Biology = 542090296826265614,
            Chemistry = 542090306569633794,
            Physics = 542090309249662976,
        }

        public static List<ulong> FindRoleIds(string text) {
            var roles = new List<ulong>();
            // I should come up with rules for this instead of a bunch of text.Contains...
            if (text.Contains("math"))    roles.Add((ulong)ClassName.Math);
            if (text.Contains("hist"))    roles.Add((ulong)ClassName.History);
            if (text.Contains("sci"))     roles.Add((ulong)ClassName.Science);
            if (text.Contains("tech"))    roles.Add((ulong)ClassName.Tech);
            if ((text.Contains("phys") ||
                 text.Contains("pys")) &&
                 text.Contains("ed") ||
                 text.Contains("gym"))    roles.Add((ulong)ClassName.PhysEd);
            if ((text.Contains("phys") ||
                 text.Contains("pys")) &&
                 (!text.Contains("ed") ||
                 Regex.Matches(text, "phys").Count >= 2 ||
                 Regex.Matches(text, "pys").Count >= 2))
                                                roles.Add((ulong)ClassName.Physics);
            if (text.Contains("eng"))     roles.Add((ulong)ClassName.English);
            if (text.Contains("french"))  roles.Add((ulong)ClassName.French);
            if (text.Contains("string"))  roles.Add((ulong)ClassName.Strings);
            if (text.Contains("food"))    roles.Add((ulong)ClassName.Foods);
            if (text.Contains("civic") ||
                text.Contains("career"))  roles.Add((ulong)ClassName.CivicsCareers);
            if (text.Contains("bu") &&
                text.Contains("s"))       roles.Add((ulong)ClassName.Business);
            if (text.Contains("bio"))     roles.Add((ulong)ClassName.Biology);
            if (text.Contains("chem"))    roles.Add((ulong)ClassName.Chemistry);
            if (text.Contains("art")) {
                if (text.Contains("theatre") || text.Contains("drama")) roles.Add((ulong)ClassName.Drama);
                else roles.Add((ulong)ClassName.VisualArts);
            }
            if (text.Contains("theatre") || text.Contains("drama"))
                roles.Add((ulong)ClassName.Drama);
            if (text.Contains("visual"))
                roles.Add((ulong)ClassName.VisualArts);
            return roles;
        }

        public static List<SocketRole> FindRoles(string text, SocketGuild guild) {
            var roleIds = FindRoleIds(text);
            var roles = new List<SocketRole>();
            foreach (var id in roleIds) roles.Add(guild.GetRole(id));
            return roles;
        }
        
        public static async Task RemoveScheduleRoles(SocketGuild guild, IGuildUser user) {
            var discardRoles = new List<SocketRole>();
            foreach (var removeId in Enum.GetValues(typeof(ClassName)))
                foreach (var roleId in user.RoleIds)
                    if (roleId == (ulong)removeId) discardRoles.Add(guild.GetRole((ulong)removeId));
            await user.RemoveRolesAsync(discardRoles);
        }

        [Command("schedule-clear")]
        public async Task ClearSchedule() {
            await RemoveScheduleRoles(Context.Guild, (IGuildUser)Context.User);
            await ReplyAsync("Schedule cleared! You won't be receiving any of those nasty notifications anymore.");
        }
        
        [Command("schedule")]
        public async Task Reschedule([Remainder] string schedule) {
            schedule = schedule.ToLower();
            var roles = FindRoles(schedule, Context.Guild);
            var guildUser = (IGuildUser) Context.User;
            switch (roles.Count) {
                case 0:
                    await ReplyAsync(KrispyGenerator.PickLine(KrispyLines.Disappointed)
                                     + " If you want to clear your schedule use $schedule-clear.");
                    break;
                case 1:
                    await guildUser.AddRolesAsync(roles);
                    await Context.Channel.SendMessageAsync("You're taking " + roles[0].Name + "? I've added that role for you.");
                    break;
                default:
                    await RemoveScheduleRoles(Context.Guild, guildUser);
                    await guildUser.AddRolesAsync(roles);
                    var response = new StringBuilder("Here's your new schedule " +
                                                     KrispyGenerator.PickLine(KrispyLines.Emoticon) + "\n```\n");
                    foreach (var role in roles) response.Append("\t- " + role.Name + "\n");
                    response.Append("```");
                    await ReplyAsync(response.ToString());
                    break;
            }
        }
    }
}