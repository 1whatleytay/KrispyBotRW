using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace KrispyBotRW {
    public class KrispySchedule : ModuleBase<SocketCommandContext> {
        private readonly ulong[] _classes = {
            413834803884982274,
            413834851611836416,
            413834751661572096,
            413835032264704014,
            413835097007980545,
            413835011666739220,
            413835556909219841,
            413835679437422593,
            413835819074191361,
            413835787755323392,
            413835737826328588
        };

        private enum ClassName {
            Math,
            Geo,
            Science,
            Tech,
            PhysEd,
            English,
            French,
            Strings,
            Music,
            Drama,
            VisualArts,
        }
        
        [Command("schedule")]
        public async Task Reschedule([Remainder] string schedule) {
            var roles = new List<SocketRole>();
            if (schedule.Contains("math"))    roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Math]));
            if (schedule.Contains("geo"))     roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Geo]));
            if (schedule.Contains("sci"))     roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Science]));
            if (schedule.Contains("tech"))    roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Tech]));
            if (schedule.Contains("phys") ||
                schedule.Contains("pys") ||
                schedule.Contains("gym"))     roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.PhysEd]));
            if (schedule.Contains("eng"))     roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.English]));
            if (schedule.Contains("french"))  roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.French]));
            if (schedule.Contains("strings")) roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Strings]));
            if (schedule.Contains("music"))   roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Music]));
            if (schedule.Contains("art")) {
                roles.Add(schedule.Contains("theatre")
                    ? Context.Guild.GetRole(_classes[(int) ClassName.Drama])
                    : Context.Guild.GetRole(_classes[(int) ClassName.VisualArts]));
            } else if (schedule.Contains("theatre") || schedule.Contains("drama"))
                roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Drama]));

            switch (roles.Count) {
                case 0:
                    await ReplyAsync(KrispyLines.Schedule[0]);
                    break;
                case 1:
                    await ReplyAsync(KrispyLines.Schedule[1]);
                    break;
                case 2:
                    await ReplyAsync(KrispyLines.Schedule[2]);
                    break;
                default:
                    if (roles.Count > 5) await ReplyAsync(string.Format(KrispyLines.Schedule[2], roles.Count));
                    else {
                        var discardRoles = new List<SocketRole>();
                        foreach (var removeId in _classes)
                            foreach (var roleId in ((IGuildUser)Context.User).RoleIds)
                                if (roleId == removeId) discardRoles.Add(Context.Guild.GetRole(removeId));
                        await ((IGuildUser)Context.User).RemoveRolesAsync(discardRoles);
                        await ((IGuildUser)Context.User).AddRolesAsync(roles);
                        await ReplyAsync(string.Format(KrispyLines.Schedule[4],
                            KrispyGenerator.PickLine(KrispyLines.Emoticon)));
                    }
                    break;
            }
        }
    }
}