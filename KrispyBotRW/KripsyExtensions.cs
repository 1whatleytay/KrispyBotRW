using System;
using System.Linq;
using System.Collections.Generic;

using Discord;
using Discord.WebSocket;

namespace KrispyBotRW {
    public class KrispyDict<T> : Dictionary<ulong, T> {
        public delegate T Constructor(ulong id);

        private readonly Constructor cons;
        
        public T GetOrCreate(ulong id) {
            if (!ContainsKey(id)) {
                Add(id, cons(id));
            }
            return this[id];
        }
        
        public KrispyDict(Constructor constructor) { cons = constructor; }
    }
    
    public static class KrispyExtensions {
        public static ulong GuildId(this SocketMessage message) {
            return ((IGuildChannel) message.Channel).GuildId;
        }
        
        public static bool IsAdmin(this SocketUser user) {
            return ((IGuildUser) user).RoleIds.Contains(378339275189518336ul);
        }

        public static bool IsAdminOrMod(this SocketUser user) {
            var guildUser = (IGuildUser) user;
            return guildUser.RoleIds.Contains(378339275189518336ul) ||
                   guildUser.RoleIds.Contains(378339453166682112ul);
        }
    }
}