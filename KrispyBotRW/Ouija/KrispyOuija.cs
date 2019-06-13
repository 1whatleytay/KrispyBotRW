using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord.Rest;
using Discord.WebSocket;

namespace KrispyBotRW.Ouija {
    public static class KrispyOuija {
        private const ulong OuijaId = 505823091927023616;
        
        private class Ouija {
            private readonly ulong _author;
            private ulong _last;
            private readonly string _question;
            private string _response = "";
            private RestUserMessage _message;

            private static string CreateQuestion(string question) {
                return new Regex("[_]{1,}", RegexOptions.None).Replace(question, "{0}");
            }

            private string Begin() {
                return "<@" + _author + "> asks: ";
            }

            public string GetResult() {
                return string.Format(_question, "**" + _response + "**");
            }

            public string GetTemplate() {
                return Begin() + string.Format(_question, "**___**");
            }

            private async Task UpdateMessage() {
                await _message.ModifyAsync(x => x.Content = Begin() + GetResult());
            }

            public bool CanModify(ulong id) {
                return id != _last && id != _author;
            }
            
            public async Task AddCharacter(ulong by, char append) {
                if (!CanModify(by)) return;

                if (append == '-') _response += ' ';
                else _response += append;
                _last = by;

                await UpdateMessage();
            }
            
            public void SetMessage(RestUserMessage message) {
                _message = message;
            }
        
            public Ouija(ulong author, string question) {
                _author = author;
                _last = author;
                _question = CreateQuestion(question);
            }
        }

        private static Ouija _openOuija;
        
        // This code is messy as heck.
        public static async Task CheckOuija(SocketUserMessage message) {
            if (message.Channel.Id == OuijaId) {
                if (message.Content.Length == 1) {
                    await _openOuija.AddCharacter(message.Author.Id, message.Content[0]);
                } else if (message.Content.ToLower().Contains("goodbye")) {
                    if (_openOuija != null) {
                        await message.Channel.SendMessageAsync(_openOuija.GetResult());
                        _openOuija = null;
                    }
                } else if (message.Content.Contains("_")) {
                    if (_openOuija == null) {
                        await message.DeleteAsync();
                        _openOuija = new Ouija(message.Author.Id, message.Content);
                        var krispyMessage = await message.Channel.SendMessageAsync(_openOuija.GetTemplate());
                        _openOuija.SetMessage(krispyMessage);
                    }
                }
            }
        }
    }
}