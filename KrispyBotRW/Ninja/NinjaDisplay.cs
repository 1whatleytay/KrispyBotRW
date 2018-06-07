using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Discord;
using Discord.WebSocket;

namespace KrispyBotRW.Ninja {
    internal class NinjaDisplay {
        private readonly IUserMessage Message;
        private readonly ulong Challenger, Defender;
        private readonly string ChallengerNN, DefenderNN;

        private bool ShowHealthBar, ShowKOMessage;

        private bool HasHealthBarValues;
        private int ChallengerHP, DefenderHP, ChallengerMaxHP, DefenderMaxHP;
        public void WithHealthBar(int challengerHP, int defenderHP, int challengerMaxHP, int defenderMaxHP) {
            ShowHealthBar = true;
            HasHealthBarValues = true;
            ChallengerHP = challengerHP;
            ChallengerMaxHP = challengerMaxHP;
            DefenderHP = defenderHP;
            DefenderMaxHP = defenderMaxHP;
        }

        public void WithHealthBar() { if (HasHealthBarValues) ShowHealthBar = true; }

        private bool ChallengerKO, DefenderKO;
        public void WithKOMessage(bool challengerKO, bool defenderKO) {
            ShowKOMessage = true;
            ChallengerKO = challengerKO;
            DefenderKO = defenderKO;
        }
        
        private class AttackQueue {
            private readonly int QueueSize;
            private readonly LinkedList<string> Queue = new LinkedList<string>();
            
            public void AddToQueue(string attack) {
                Queue.AddLast(attack);
                if (Queue.Count > QueueSize) Queue.RemoveFirst();
            }

            public string this[int i] => Queue.ElementAt(i);
            public int Count => Queue.Count;

            public AttackQueue(int queueSize = 8) { QueueSize = queueSize; }
        }

        private readonly AttackQueue Attacks = new AttackQueue();
        
        public enum Participant { Challenger, Defender }

        private string GetTimesMessage(int times) {
            switch (times) {
                case 2: return "twice";
                case 3: return "thrice";
                case 4: return "quatrice";
                case 5: return "cinquifice";
                default: return times + " times";
            }
        }

        private void WithCustom(string message) { Attacks.AddToQueue(message); }

        public void WithCustom(Participant participant, string message) {
            WithCustom(string.Format(message,
                participant == Participant.Challenger ? ChallengerNN : DefenderNN,
                participant == Participant.Defender ? ChallengerNN : DefenderNN));
        }

        public void WithDamage(Participant participant, int damage, int times = 1, int criticalTimes = 0) {
            string attacker = participant == Participant.Challenger ? ChallengerNN : DefenderNN,
                defender = participant == Participant.Defender ? ChallengerNN : DefenderNN;

            if (times == 1) WithCustom(string.Format(
                                       KrispyGenerator.PickLine(KrispyLines.NinjaSingle),
                                       attacker, defender) + " [" + damage + " damage]");
            else WithCustom(string.Format(
                                        KrispyGenerator.PickLine(KrispyLines.NinjaMultiple),
                                        attacker, defender, GetTimesMessage(times), times) + " [" + damage + " " +
                            (criticalTimes > 0 ? "CRITICAL" + (criticalTimes > 1 ? "x" + criticalTimes : "") : "damage") +
            "]");
        }

        public void ResetDisplay() {
            ShowHealthBar = false;
            ShowKOMessage = false;
        }

        public async Task UpdateDisplay() {
            if (!ShowHealthBar && !ShowKOMessage && Attacks.Count <= 0) return;
            
            var builder = new StringBuilder();
            
            if (ShowHealthBar || Attacks.Count > 0) {
                builder.Append("```\n");
                if (ShowHealthBar) {
                    var maxLength = Math.Max(Math.Max(ChallengerNN.Length, DefenderNN.Length), 8);
                    builder.Append(ChallengerNN.PadRight(maxLength) + " | " + DefenderNN.PadLeft(maxLength) + "\n");
                    builder.Append((ChallengerHP + "/" + ChallengerMaxHP).PadRight(maxLength) + " | " +
                                   (DefenderHP + "/" + DefenderMaxHP).PadLeft(maxLength) + "\n");
                }
                if (Attacks.Count > 0) {
                    if (ShowHealthBar) builder.Append("\n");
                    for (var a = 0; a < Attacks.Count; a++) {
                        builder.Append(Attacks[a] + "\n");
                    }
                }
                builder.Append("```\n");
            }
            
            if (ShowKOMessage) {
                if (ChallengerKO && DefenderKO)
                    builder.Append("You were both knocked out!\n");
                else if (ChallengerKO)
                    builder.Append("<@!" + Defender + ">! You have defeated <@!" + Challenger + ">!\n");
                else if (DefenderKO)
                    builder.Append("<@!" + Challenger + ">! You have defeated <@!" + Defender + ">!\n");
            }
            await Message.ModifyAsync(x => x.Content = builder.ToString());
        }
        

        public NinjaDisplay(IMessageChannel messageChannel, ulong challenger, ulong defender, string challengeMessage) {
            Challenger = challenger;
            Defender = defender;
            
            var guild = (messageChannel as SocketGuildChannel).Guild;
            IGuildUser ch = guild.GetUser(Challenger), de = guild.GetUser(Defender);
            ChallengerNN = KrispyNickname.NickName(ch.Nickname ?? ch.Username);
            DefenderNN = KrispyNickname.NickName(de.Nickname ?? de.Username);
            
            Message = messageChannel.SendMessageAsync(
                challengeMessage ?? 
                DefenderNN + "! You are being attacked by " + ChallengerNN + ". Defend yourself!"
            ).GetAwaiter().GetResult();
        }
    }
}