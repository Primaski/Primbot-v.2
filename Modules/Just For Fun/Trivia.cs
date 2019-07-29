using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using  static Primbot_v._2.Uno_Score_Tracking.SaveFiles_GlobalVariables;

namespace Primbot_v._2.Modules.Just_For_Fun {
    public class Trivia : ModuleBase<SocketCommandContext> {

        [Command("tcommands", RunMode = RunMode.Async)]
        public async Task Commands() {
            await ReplyAsync(
                "`p*tstart` -> `p*techo [str]` -> `p*tnew [category]` -> `p*tsetlb [ID] [newVal]` -> " +
                "`p*taddlb [ID] [newVal]` -> `p*tend`"
                );
        }

        [Command("tstart", RunMode = RunMode.Async)]
        public async Task Start() {
            await ReplyAsync("on");
            if (Context.User.Id != MY_ID) {
                await ReplyAsync("You do not have permission to execute this command."); return;
            }
            try {
                var w = (SocketTextChannel)Uno_Score_Tracking.GuildCache.Uno_Cache.GetChannel(TRIVIA_CHANNEL_ID);
                int x = Server.Trivia.Start(w);
            } catch {
                await ReplyAsync("Cache loaded incorrectly, reload the bot.");
            }
            return;
        }

        [Command("techo", RunMode = RunMode.Async)]
        public async Task Echo([Remainder] string args = null) {
            if (Context.User.Id != MY_ID) {
                await ReplyAsync("You do not have permission to execute this command."); return;
            }
            var w = (SocketTextChannel)Uno_Score_Tracking.GuildCache.Uno_Cache.GetChannel(TRIVIA_CHANNEL_ID);
            if (Server.Trivia.Say(args, null, w) == null) {
                await ReplyAsync("failed");
            } else {
                await ReplyAsync("succeeded");
            }
            return;
        }

        [Command("tnew", RunMode = RunMode.Async)]
        public async Task NewQuestion([Remainder] string args = null) {
            if (Context.User.Id != MY_ID) {
                await ReplyAsync("You do not have permission to execute this command."); return;
            }
            if (args == null) {
                await ReplyAsync("Provide a category"); return;
            }
            bool x = Server.Trivia.NewQuestion(args);
            await ReplyAsync(x.ToString());
            return;
        }

        [Command("taddlb", RunMode = RunMode.Async)]
        public async Task AddLB([Remainder] string args = null) {
            await EditLB(true, args);
            return;
        }

        [Command("tsetlb", RunMode = RunMode.Async)]
        public async Task EditLB(bool add = false, [Remainder] string args = null) {
            if (Context.User.Id != MY_ID) {
                await ReplyAsync("You do not have permission to execute this command."); return;
            }
            string format = "Format: `p*teditlb [User ID] [newVal]` (p*taddlb else)";
            if (args == null) {
                await ReplyAsync(format); return;
            } else if (args.Split(' ').Count() < 2) {
                await ReplyAsync(format); return;
            }
            string strID = args.Split(' ')[0];
            string strval = args.Split(' ')[1];

            try {
                ulong ID = UInt64.Parse(strID);
                int val = Int32.Parse(strval);
                Server.Trivia.EditLeaderboard(ID, val);
                await ReplyAsync("Success.");
            } catch {
                await ReplyAsync(format);
            }
            return;
        }

        [Command("tbrute", RunMode = RunMode.Async)]
        public async Task BruteInitalize() {
            if (Context.User.Id != MY_ID) {
                await ReplyAsync("You do not have permission to execute this command."); return;
            }
            try {
                var w = (SocketTextChannel)Uno_Score_Tracking.GuildCache.Uno_Cache.GetChannel(TRIVIA_CHANNEL_ID);
                int x = Server.Trivia.Preface(w, null, false);
            } catch (Exception e) {
                await ReplyAsync("Cache loaded incorrectly, reload the bot.");
            }
        }

        [Command("tend", RunMode = RunMode.Async)]
        public async Task End() {

            if (Context.User.Id != MY_ID) {
                await ReplyAsync("You do not have permission to execute this command."); return;
            }
            Server.Trivia.End();
        }
    }
}