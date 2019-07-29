using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Threading;

namespace Primbot_v._2.Modules.Just_For_Fun {
    public class KnightsSolver : ModuleBase<SocketCommandContext> {

        [Command("cahtrack")]
        public async Task CT([Remainder] string args = null) {
            Uno_Score_Tracking.SaveFiles_GlobalVariables.cahtrack = !Uno_Score_Tracking.SaveFiles_GlobalVariables.cahtrack;
        }

        /*[Command("kni")]
        public async Task Solver([Remainder] string args = null) {
            var channel = (SocketTextChannel)Uno_Score_Tracking.GuildCache.Uno_Cache.GetChannel(499765861146689552);

            while (Uno_Score_Tracking.SaveFiles_GlobalVariables.cahtrack) {
                ulong prevMsg = 0;
                IEnumerable<IMessage> w = channel.GetMessagesAsync(2).FlattenAsync().Result;
                for (int i = 0; i < w.Count(); ++i) {
                    if (w.ElementAt(i).Author.Id == 383995098754711555 && w.ElementAt(i).Id != prevMsg) {
                        try {
                            prevMsg = w.ElementAt(i).Id;
                            string fullMsg = w.ElementAt(i).ToString();
                            string fullMsgTest = w.ElementAt(i).Content.ToString();
                            if (fullMsg.IndexOf(":black_large_square:") != -1) {
                                string cropped = fullMsg.Substring(fullMsg.IndexOf(":black_large_square:"));
                                Console.WriteLine(cropped);
                            } else {
                                Console.WriteLine(fullMsg);
                                Console.WriteLine(fullMsgTest);
                            }
                        } catch {
                        }
                    }
                }
            }
            return;
        }
        */
    }
}