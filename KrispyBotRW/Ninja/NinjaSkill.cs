using System.Text;
using System.Collections.Generic;

namespace KrispyBotRW.Ninja {
    internal class NinjaSkillBase {
        private static int IncrementId;
        public readonly int Id;
        public readonly string Name;
        public readonly int DefaultData;
        private NinjaSkillBase(string name, int defaultData = 0) {
            Id = IncrementId++;
            Name = name;
            DefaultData = defaultData;
        }

        public static readonly NinjaSkillBase[] Skills = {
            new NinjaSkillBase("Dodge"), // A +5% chance to completely dodge an attack
            new NinjaSkillBase("Critical"), // A 5% chance to deal twice as much damage
            new NinjaSkillBase("Ambush"), // Start the battle with 5% less of the enemy health
            new NinjaSkillBase("Last Stand", 1), // Lets your ninja survive one more attack before dying
            new NinjaSkillBase("Resist"), // Your ninja takes 1 less damage every turn
            new NinjaSkillBase("Last Ditch Effort"), // Your ninja does up to 30% more damage as it's health gets lower
        };

        public static NinjaSkillBase Random() { return Skills[(int)(KrispyGenerator.Value() * Skills.Length)]; }
    }
    
    internal class NinjaSkill {
        public readonly NinjaSkillBase BaseSkill;
        public int Level = 1;
        public int ExtraData;

        public string GetLevelText() {
            var builder = new StringBuilder();
            for (var a = 0; a < Level; a++) builder.Append("I");
            return builder.ToString();
        }

        public NinjaSaveSkill CreateSaveSkill() {
            return new NinjaSaveSkill(BaseSkill.Id, Level);
        }

        public NinjaSkill(NinjaSkillBase baseSkill) {
            BaseSkill = baseSkill;
            ExtraData = BaseSkill.DefaultData;
        }
    }
    
    internal class NinjaSkills : List<NinjaSkill> {
        public int CheckForSkill(int skill) {
            for (var a = 0; a < Count; a++)
                if (this[a].BaseSkill.Id == skill) return a;
            return -1;
        }

        public int CheckForLevel(int skill) {
            var skillIndex = CheckForSkill(skill);
            if (skillIndex == -1) return 0;
            return this[skillIndex].Level;
        }


        public bool SkillExists(int skill) { return CheckForSkill(skill) != -1; }
        public NinjaSkill GetSkill(int skill) { var loc = CheckForSkill(skill); return loc == -1 ? null : this[loc]; }
            
        public int AddNewSkill() {
            var baseSkill = NinjaSkillBase.Random();
            var skillLoc = CheckForSkill(baseSkill.Id);
            if (skillLoc == -1) { skillLoc = Count; Add(new NinjaSkill(baseSkill)); }
            else if (this[skillLoc].Level == 3) this[skillLoc].Level++;
            return skillLoc;
        }

        public void ResetSkillData() { foreach (var skill in this) skill.ExtraData = skill.BaseSkill.DefaultData; }
    }
}