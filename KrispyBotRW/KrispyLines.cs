using System;
using System.IO;
using System.Linq;
using System.Text;

namespace KrispyBotRW {
    public static class KrispyLines {
        public static readonly string[] Unknown = File.ReadAllLines("Lines/unknown.txt");
        public static readonly string[] Question = File.ReadAllLines("Lines/question.txt");
        public static readonly string[] Emoticon = File.ReadAllLines("Lines/emoticon.txt");

        public static readonly string[] Nickname = File.ReadAllLines("Lines/nickname.txt");
        public static readonly string[] Schedule = File.ReadAllLines("Lines/schedule.txt");
        public static readonly string[] Games = File.ReadAllLines("Lines/games.txt");
        public static readonly string[] Debug = File.ReadAllLines("Lines/debug.txt");

        public static readonly string Help = File.ReadAllText("Lines/help.txt");
        public static readonly string NSFW = File.ReadAllText("Lines/nsfw.txt");
        
        public static string CapitalizeAll(string name) {
            var nameWords = name.Split(" ");
            var newName = nameWords.Aggregate("", (current, nameWord) => current + nameWord.First().ToString().ToUpper() + nameWord.Substring(1) + " ");
            return newName.Trim();
        }
    }

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
        
        public static string PickLine(string[] lines) { return lines[Rng.Next(0, lines.Length)]; }
    }
}