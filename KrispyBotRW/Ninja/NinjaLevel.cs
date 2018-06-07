namespace KrispyBotRW.Ninja {
    internal class NinjaLevel {
        private static int IncrementId;
        public readonly string Name;
        public readonly int Number, ExpRequirements;
        private NinjaLevel(string name, int exp) { Number = IncrementId++; Name = name; ExpRequirements = exp; }
        
        public static readonly NinjaLevel[] Levels = {
            new NinjaLevel("Punching Bag", 0),
            new NinjaLevel("Ninja Wannabe", 20),
            new NinjaLevel("Apprentice Ninja", 80),
            new NinjaLevel("Stealthy Threat", 200),
            new NinjaLevel("Lethal Threat", 320),
            new NinjaLevel("Worthy Opponent", 650),
            new NinjaLevel("Professional Ninja", 800),
            new NinjaLevel("Master of Stealth", 1000),
            new NinjaLevel("Black Belt in Ninja Star Making", 1200),
            new NinjaLevel("As Fast as Lightning", 1420), 
            new NinjaLevel("Contract Killer", 1500),
            new NinjaLevel("God, None Can Oppose.", int.MaxValue),
            new NinjaLevel("Reverse Punching Bag", int.MaxValue),
        };
    }
}