using System;
using System.Collections.Generic;

namespace KrispyBotRW.Ninja {
    [Serializable]
    internal class NinjaSaveProfile {
        private readonly ulong UserId;
        private readonly int MaxHP, CurrentHP;
        private readonly int MaxStamina;
        private readonly int HitMinimum, HitMaximum;
        private readonly double Speed;
        
        private readonly int ExpLevel;
        private readonly int LevelIndex;
        private readonly List<NinjaSaveSkill> Skills = new List<NinjaSaveSkill>();
        
        private readonly string ChallengeMessage;

        public NinjaProfile CreateProfile() {
            var profile = new NinjaProfile(UserId) {
                MaxHP = MaxHP,
                CurrentHP = CurrentHP,
                MaxStamina = MaxStamina,
                CurrentStamina = MaxStamina,
                HitMinimum = HitMinimum,
                HitMaximum = HitMaximum,
                Speed = Speed,
                ExpLevel = ExpLevel,
                Level = NinjaLevel.Levels[LevelIndex],
                ChallengeMessage = ChallengeMessage
            };
            foreach (var save in Skills) {
                profile.Skills.Add(save.CreateSkill());
            }
            return profile;
        }

        public NinjaSaveProfile(
            ulong userId, int maxHP, int cHP, int stamina, int hitMin, int hitMax, double speed,
            IEnumerable<NinjaSkill> skills, int expLevel, NinjaLevel level, string challengeMessage) {
            UserId = userId;
            MaxHP = maxHP;
            CurrentHP = cHP;
            MaxStamina = stamina;
            HitMinimum = hitMin;
            HitMaximum = hitMax;
            Speed = speed;
            ExpLevel = expLevel;
            LevelIndex = level.Number;
            ChallengeMessage = challengeMessage;
            foreach (var skill in skills) Skills.Add(skill.CreateSaveSkill());
        }
    }
}