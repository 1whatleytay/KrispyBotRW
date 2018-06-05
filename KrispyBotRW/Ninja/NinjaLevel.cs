namespace KrispyBotRW.Ninja {
    internal class NinjaLevel {
        public readonly string Name;
        public readonly int Number, ExpRequirements;
        private NinjaLevel(int number, string name, int exp) { Number = number; Name = name; ExpRequirements = exp; }
        
        public static readonly NinjaLevel[] Levels = {
            new NinjaLevel(0, "Punching Bag", 0), 
            new NinjaLevel(1, "Ninja Wannabe", 20),
            new NinjaLevel(2, "Apprentice Ninja", 80),
            new NinjaLevel(3, "Stealthy Threat", 200),
            new NinjaLevel(4, "Lethal Threat", 440),
            new NinjaLevel(5, "Worthy Opponent", 850),
            new NinjaLevel(6, "Contract Killer", 1000),
            new NinjaLevel(7, "God, None Can Oppose.", int.MaxValue),
            new NinjaLevel(8, "Reverse Punching Bag", int.MaxValue),
        };
    }
}