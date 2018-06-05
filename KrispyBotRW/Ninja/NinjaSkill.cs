using System.Text;

namespace KrispyBotRW.Ninja {
    public class NinjaSkillBase {
        private static int IncrementId = 0;
        public readonly int Id;
        public readonly string Name;
        private NinjaSkillBase(string name) {
            Id = IncrementId++;
            Name = name;
        }

        public static readonly NinjaSkillBase[] Skills = {
            new NinjaSkillBase("Dodge"), // A 5% chance to completely dodge an attack
            new NinjaSkillBase("Critical"), // A 5% chance to deal twice as much damage
            new NinjaSkillBase("Ambush"), // Start the battle with 5 less of the enemy health
            new NinjaSkillBase("Last Stand"), // Lets your ninja survive one more attack before dying
            new NinjaSkillBase("Resist"), // Your ninja takes 1 less damage every turn
            new NinjaSkillBase("Last Ditch Effort"), // Your ninja does up to 30% more damage as it's health gets lower 
        };

        public static NinjaSkillBase Random() { return Skills[(int)(KrispyGenerator.Value() * Skills.Length)]; }
    }
    
    public class NinjaSkill {
        public readonly NinjaSkillBase BaseSkill;
        public int Level = 1;

        public string GetLevelText() {
            var builder = new StringBuilder();
            for (var a = 0; a < Level; a++) builder.Append("I");
            return builder.ToString();
        }

        public NinjaSkill(NinjaSkillBase baseSkill) {
            BaseSkill = baseSkill;
        }
    }
}