using System;
using System.Text;

namespace KrispyBotRW {
    public static class KrispyGenerator {
        private static readonly Random Rng = new Random();

        public static bool Odds(int odds) { return Rng.Next(0, odds) == 0; }
        
        public static string GenerateBeeps() {
            var beepCount = Rng.Next(10, 100);
            var stringBuilder = new StringBuilder();
            for (var a = 0; a < beepCount; a++)
                stringBuilder.Append(Rng.Next(0, 2) == 0 ? "boop" : "beep");
            return stringBuilder.ToString();
        }

        public static int NumberBetween(int min, int max) { return Rng.Next(min, max); }
        public static double Value() { return Rng.NextDouble(); }
        
        public static string PickLine(string[] lines) { return lines[Rng.Next(0, lines.Length)]; }
    }
}