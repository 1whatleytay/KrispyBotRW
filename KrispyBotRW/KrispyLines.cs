using System.IO;
using System.Linq;

using Discord;

namespace KrispyBotRW {
    public static class KrispyLines {
        public static readonly string[] Unknown = File.ReadAllLines("Lines/unknown.txt");
        public static readonly string[] Emoticon = File.ReadAllLines("Lines/emoticon.txt");
        public static readonly string[] Status = File.ReadAllLines("Lines/status.txt");
        public static readonly string[] NinjaSingle = File.ReadAllLines("Lines/ninja-attacks-single.txt");
        public static readonly string[] NinjaMultiple = File.ReadAllLines("Lines/ninja-attacks-multiple.txt");
        public static readonly string[] NinjaDodges = File.ReadAllLines("Lines/ninja-dodges.txt");

        public static readonly string Help = File.ReadAllText("Lines/help.txt");
        public static readonly string NSFW = File.ReadAllText("Lines/nsfw.txt");

        public static string CapitalizeAll(string name) {
            var nameWords = name.Split(" ");
            var newName = nameWords.Aggregate("",
                (current, nameWord) => current + nameWord.First().ToString().ToUpper() + nameWord.Substring(1) + " ");
            return newName.Trim();
        }

        public static string Emojify(string name) {
            return name.ToLower().Replace(" ", "-").Replace(@"\s+", "");
        }
    }
    
    public class KrispyStatusLine {
        public readonly string Status;
        public readonly ActivityType Type = ActivityType.Playing;

        public KrispyStatusLine() {
            var toParse = KrispyGenerator.PickLine(KrispyLines.Status);
            var isPlaying = toParse.StartsWith("!p");
            var isWatching = toParse.StartsWith("!w");
            var isListening = toParse.StartsWith("!l");
            var isStreaming = toParse.StartsWith("!s");
            if (isPlaying || isWatching || isListening || isStreaming) {
                if (isPlaying) Type = ActivityType.Playing;
                else if (isWatching) Type = ActivityType.Watching;
                else if (isListening) Type = ActivityType.Listening;
                else Type = ActivityType.Streaming;
                Status = toParse.Substring(3);
            } else Status = toParse;
        }
    }
}