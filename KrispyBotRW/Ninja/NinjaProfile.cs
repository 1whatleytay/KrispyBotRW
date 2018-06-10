using System;
using System.Collections.Generic;

using Discord;
using Discord.WebSocket;

namespace KrispyBotRW.Ninja {
    internal class NinjaProfile {
        public readonly ulong UserId;
        public int MaxHP = 30, CurrentHP = 30;
        public int MaxStamina = 10, CurrentStamina = 10;
        public int HitMinimum = 2, HitMaximum = 5;
        public double Speed = 2.5;
        
        public int ExpLevel;
        public NinjaLevel Level;
        public readonly NinjaSkills Skills = new NinjaSkills();
        public string ChallengeMessage;

        public NinjaGame Game;

        private void CheckLevelIncreases(IMessageChannel battleChannel) {
            var nextLevel = NinjaLevel.Levels[Level.Number + 1];
            if (ExpLevel < nextLevel.ExpRequirements) return;
            ExpLevel = 0; Level = nextLevel;
            var loc = Skills.AddNewSkill();
            var levelUpMessage = "<@!" + UserId + "> has leveled up to a " + Level.Name + ".";
            if (loc != -1) {
                var skill = Skills[loc];
                if (skill.Level == 1)
                    levelUpMessage += " Obtained new skill **" + skill.BaseSkill.Name + "**!";
                else
                    levelUpMessage += " Upgraded skill to **" + skill.BaseSkill.Name + " " + skill.GetLevelText() + "**!";
            }
            battleChannel?.SendMessageAsync(levelUpMessage);
        }

        private void IncreaseStat() {
            var val = KrispyGenerator.Value();
            if (val >= 0 && val < 0.5)
            { MaxHP++; CurrentHP++; }
            else if (val >= 0.5 && val < 0.65)
                Speed *= 0.98;
            else if (val >= 0.65 && val < 0.80)
                MaxStamina++;
            else if (val >= 0.80 && val < 0.92)
                HitMaximum++;
            else if (val >= 0.92)
                if (HitMinimum++ >= HitMaximum)
                    HitMaximum += 4;
        }

        private void IncreaseStats(int amount) { for (var a = 0; a < amount; a++) IncreaseStat(); }
        
        public void GiveExp(bool didWin, int opponentLevel, int lengthOfBattle, IMessageChannel battleChannel) {
            var exp = Math.Max(
                (didWin ? 8 : 3) *
                Math.Max(opponentLevel - Level.Number, 0) * 5
                + 2 * lengthOfBattle - MaxStamina, 5) + KrispyGenerator.NumberBetween(5, 25);
            IncreaseStats(exp / 10);
            ExpLevel += exp;
            CheckLevelIncreases(battleChannel);
            Console.WriteLine("Exp: " + ExpLevel);
        }
        
        public void EndGame() {
            CurrentHP = Math.Min(Math.Max(CurrentHP, 0) + MaxHP / 2, MaxHP);
            CurrentStamina = MaxStamina;
            Skills.ResetSkillData();
            Game = null;
        }

        public Embed CreateEmbed(DiscordSocketClient client) {
            var user = client.GetUser(UserId);
            var skillDisplay = Skills.Count > 0 ? "Skills:\n" : "";
            foreach (var skill in Skills)
                skillDisplay += skill.BaseSkill.Name + " " + skill.GetLevelText() + "\n";
            return new EmbedBuilder()
                .WithColor(Color.DarkBlue)
                .WithTitle(user.Username + "#" + user.Discriminator + "'s Ninja")
                .WithDescription("Level: " + Level.Name + " (" + Level.Number + ") " +
                                 "[" + ExpLevel + "/" + NinjaLevel.Levels[Level.Number + 1].ExpRequirements + "]\n" +
                                 "HP: " + CurrentHP + "/" + MaxHP + "\n" +
                                 "Stamina: " + MaxStamina + "\n" +
                                 "Damage: " + HitMinimum + " - " + HitMaximum + "\n" +
                                 "Speed: " + Speed + "\n" +
                                 skillDisplay)
                .Build();
        }
        
        public static Dictionary<ulong, NinjaProfile> Profiles = new Dictionary<ulong, NinjaProfile>() {
            {414619400742633493, new NinjaProfile(414619400742633493) {
                Level = NinjaLevel.Levels[11],
                MaxHP = 900, CurrentHP = 900,
                MaxStamina = 50, CurrentStamina = 50,
                HitMinimum = 50, HitMaximum = 70,
                Speed = 0.25f,
                
                ChallengeMessage = "Who dares challenge the one and only God! I shall destroy you!"
            }},
            {189702078958927872, new NinjaProfile(189702078958927872) {
                Level = NinjaLevel.Levels[0],
                MaxHP = 10, CurrentHP = 10,
                MaxStamina = 1, CurrentStamina = 1,
                HitMinimum = 0, HitMaximum = 1,
                Speed = 2,
                
                ChallengeMessage = "Oh no im just a puny eris bot have mercy"
            }}
        };

        public static NinjaProfile GetOrCreate(ulong user) {
            if (!Profiles.ContainsKey(user)) Profiles.Add(user, new NinjaProfile(user));
            return Profiles[user];
        }

        public NinjaSaveProfile CreateSaveProfile() {
            return new NinjaSaveProfile(
                UserId,
                MaxHP, CurrentHP,
                MaxStamina, HitMinimum, HitMaximum,
                Speed, Skills.ToArray(),
                ExpLevel, Level, ChallengeMessage);
        }
        
        public NinjaProfile(ulong userId) {
            UserId = userId;
            Level = NinjaLevel.Levels[0];
            IncreaseStats(10); }
    }
}