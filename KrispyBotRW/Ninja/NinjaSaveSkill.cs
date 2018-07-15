using System;

namespace KrispyBotRW.Ninja {
    [Serializable]
    internal class NinjaSaveSkill {
        private readonly int SkillIndex, SkillLevel;

        public NinjaSkill CreateSkill() {
            var skill = new NinjaSkill(NinjaSkillBase.Skills[SkillIndex]) {
                Level = SkillLevel
            };
            return skill;
        }
        
        public NinjaSaveSkill(int skillIndex, int skillLevel) {
            SkillIndex = skillIndex;
            SkillLevel = skillLevel;
        }
    }
}