using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.IO;
using Discord.WebSocket;
using System.Threading;
using static Primbot_v._2.Uno_Score_Tracking.Defs;

namespace Primbot_v._2.Modules.Information {
    public class Pings : ModuleBase<SocketCommandContext> {
        [Command("unoping")]
        public async Task PingMe() {
            if(Uno_Score_Tracking.SaveFiles_Entries.EntryExists(UNO_PING_LOG, Context.User.Id.ToString())){
                await ReplyAsync("I already know to ping you!");
                return;
            }
            Uno_Score_Tracking.SaveFiles_Entries.AddEntry(UNO_PING_LOG, Context.User.Id.ToString());
            await ReplyAsync("Added you to the ping list!");
        }
        [Command("unodontping")]
        public async Task DontPingMe() {
            bool worked = Uno_Score_Tracking.SaveFiles_Entries.DeleteEntry(UNO_PING_LOG, Context.User.Id.ToString());
            if (worked) {
                await ReplyAsync("Removed you from the ping list!");
            } else {
                await ReplyAsync("Didn't find you on the ping list!");
            }
        }
    }
}