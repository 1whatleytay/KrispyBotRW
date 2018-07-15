using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace KrispyBotRW {
    public class KrispySchedule : ModuleBase<SocketCommandContext> {
//        private readonly ulong[] _classes = {
//            413834803884982274,
//            413834851611836416,
//            413834751661572096,
//            413835032264704014,
//            413835097007980545,
//            413835011666739220,
//            413835556909219841,
//            413835679437422593,
//            413835819074191361,
//            413835787755323392,
//            413835737826328588,
//            463437914928578560
//        };
//
//        private enum ClassName {
//            Math,
//            Geo,
//            Science,
//            Tech,
//            PhysEd,
//            English,
//            French,
//            Strings,
//            Music,
//            Drama,
//            VisualArts,
//            Civics
//        }

        [Command("civics")]
        public async Task AddCivics() {
            await ((IGuildUser) Context.User).AddRoleAsync(Context.Guild.GetRole(463437914928578560));
            await ReplyAsync(KrispyGenerator.PickLine(KrispyLines.Civics));
        }

        [Command("civic")]
        public async Task AddCivic() { AddCivics(); }

//        public async Task RemoveScheduleRoles() {
//            var discardRoles = new List<SocketRole>();
//            foreach (var removeId in _classes)
//                foreach (var roleId in ((IGuildUser)Context.User).RoleIds)
//                    if (roleId == removeId) discardRoles.Add(Context.Guild.GetRole(removeId));
//            await ((IGuildUser)Context.User).RemoveRolesAsync(discardRoles);
//        }

//        [Command("schedule-clear")]
//        public async Task ClearSchedule() {
//            await RemoveScheduleRoles();
//            await ReplyAsync("Schedule cleared! You won't be receiving any of those nasty notifs anymore.");
//        }
//        
//        [Command("schedule")]
//        public async Task Reschedule([Remainder] string schedule) {
//            schedule = schedule.ToLower();
//            var roles = new List<SocketRole>();
//            if (schedule.Contains("math"))    roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Math]));
//            if (schedule.Contains("geo"))     roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Geo]));
//            if (schedule.Contains("sci"))     roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Science]));
//            if (schedule.Contains("tech"))    roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Tech]));
//            if (schedule.Contains("phys") ||
//                schedule.Contains("pys") ||
//                schedule.Contains("gym"))     roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.PhysEd]));
//            if (schedule.Contains("eng"))     roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.English]));
//            if (schedule.Contains("french"))  roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.French]));
//            if (schedule.Contains("strings")) roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Strings]));
//            if (schedule.Contains("music"))   roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Music]));
//            if (schedule.Contains("civic"))   roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Civics]));
//            if (schedule.Contains("art")) {
//                roles.Add(schedule.Contains("theatre")
//                    ? Context.Guild.GetRole(_classes[(int) ClassName.Drama])
//                    : Context.Guild.GetRole(_classes[(int) ClassName.VisualArts]));
//            } else if (schedule.Contains("theatre") || schedule.Contains("drama"))
//                roles.Add(Context.Guild.GetRole(_classes[(int)ClassName.Drama]));
//
//            switch (roles.Count) {
//                case 0:
//                    await ReplyAsync("You didn't specify a single class... or maybe you did a typo?");
//                    break;
//                case 1:
//                    if (roles[0].Id == 463437914928578560) {
//                        await RemoveScheduleRoles();
//                        await ((IGuildUser)Context.User).AddRoleAsync(roles[0]);
//                        await ReplyAsync(KrispyGenerator.PickLine(CivicsFunLines));
//                    }
//                    else await ReplyAsync("I only saw one class. What kind of underachiever are you? Try again, buddy.");
//                    break;
//                case 2:
//                    await ReplyAsync("Okay... two classes isn't alot. I don't reeeeeaaaaally believe you though. Maybe there's a typo?");
//                    break;
//                default:
//                    if (roles.Count > 5) await ReplyAsync(
//                        "Seriously? " + roles.Count + " classes? Does the school even allow that? Try again, maybe I read something wrong."
//                        );
//                    else {
//                        await RemoveScheduleRoles();
//                        await ((IGuildUser)Context.User).AddRolesAsync(roles);
//                        await ReplyAsync("I've refreshed your schedule " + KrispyGenerator.PickLine(KrispyLines.Emoticon));
//                    }
//                    break;
//            }
//        }
    }
}