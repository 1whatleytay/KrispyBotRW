namespace KrispyBotRW.Ninja {
    public class NinjaLevel {
        private static int IncrementId;
        public readonly string Name;
        public readonly int Number, ExpRequirements;
        private NinjaLevel(string name, int exp) { Number = IncrementId++; Name = name; ExpRequirements = exp; }
        
        public static readonly NinjaLevel[] Levels = {
            new NinjaLevel("Punching Bag", 0),
            new NinjaLevel("Ninja Wannabe", 40),
            new NinjaLevel("Apprentice Ninja", 120),
            new NinjaLevel("Stealthy Threat", 400),
            new NinjaLevel("Lethal Threat", 600),
            new NinjaLevel("Worthy Opponent", 720),
            new NinjaLevel("Professional Ninja", 880),
            new NinjaLevel("Shadow Walker", 1000), 
            new NinjaLevel("Master of Stealth", 1200),
            new NinjaLevel("Hidden Warrior", 1450), 
            new NinjaLevel("Black Belt in Ninja Star Making", 1750),
            new NinjaLevel("Warrior of the Night", 2000), 
            new NinjaLevel("As Fast as Lightning", 2250),
            new NinjaLevel("Contract Killer", 2500),
            new NinjaLevel("God, None Can Oppose.", int.MaxValue),
            new NinjaLevel("Reverse Punching Bag", int.MaxValue),
        };
    }
}