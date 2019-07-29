using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using static Primbot_v._2.Uno_Score_Tracking.Bridge;
using static Primbot_v._2.Uno_Score_Tracking.SaveFiles_GlobalVariables;
using System.Text.RegularExpressions;
using Primbot_v._2.Uno_Score_Tracking;

namespace Primbot_v._2.Modules.Scoring {
    //commands of this class should not be ASYNC
    public class Leaderboard : ModuleBase<SocketCommandContext> {
        [Command("lb", RunMode = RunMode.Async)]
        public async Task LoadLb([Remainder] string args = "") {

            await LoadLeaderboard(args);
            return;
        }

        [Command("leaderboard", RunMode = RunMode.Async)]
        public async Task LoadLeaderboard([Remainder] string args = "") {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            string res = InterpretLeaderboardInputAndGetLeaderboard(args);
            await ReplyAsync(res);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Retrieved a leaderboard for " + Context.User.Username);
        }

        [Command("score", RunMode = RunMode.Async)]
        public async Task LoadScore([Remainder] string args = "") {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            string res = InterpretScoreInputAndGetPlacementInfo(args, Context.User.Id);
            await ReplyAsync(res);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Retrieved a score for " + Context.User.Username);
        }

        [Command("spc", RunMode = RunMode.Async)]
        public async Task SetcolorAbb([Remainder] string args = "") {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await Setcolor(args);
        }

        [Command("setprofilecolor", RunMode = RunMode.Async)]
        public async Task Setcolor([Remainder] string args = "") {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args == null) {
                await ReplyAsync("Please specify the color in hexadecimal."); return;
            }
            if(args.Trim().ToLower() == "default") {
                string patho = USER_SAVE_DIRECTORY + "\\" + Context.User.Id + "\\" + UNO_SAVE_FILE_NAME;
                try {
                    SaveFiles_Mapped.ModifyFieldValue("LIT-CUSTOMCOLOR", patho, "-");
                    await ReplyAsync("Success! Your new profile color is your team's color! Check it out with `p*profile`.");
                } catch (Exception e) {
                    await ReplyAsync(e.Message);
                }
            }
            args = args.Replace("0x", "");
            args = args.Replace("#", "");
            args = args.Trim();
            if(!Int32.TryParse(args, System.Globalization.NumberStyles.HexNumber, null, out int ignore)) {
                await ReplyAsync("Please specify a hex value between 0x000000 and 0xFFFFFF."); return;
            }
            int hexValue = Int32.Parse(args,System.Globalization.NumberStyles.HexNumber);
            if(hexValue > 0xFFFFFF) {
                await ReplyAsync("Please specify a hex value between 0x000000 and 0xFFFFFF."); return;
            }
            string path = USER_SAVE_DIRECTORY + "\\" + Context.User.Id + "\\" + UNO_SAVE_FILE_NAME;
            try {
                SaveFiles_Mapped.ModifyFieldValue("LIT-CUSTOMCOLOR", path, hexValue.ToString("X6"));
                await ReplyAsync("Success! Your new profile color is " + args + "! Check it out with `p*profile`."); 
            }catch(Exception e) {
                await ReplyAsync(e.Message);
            }
            return;
        }

        [Command("profile", RunMode = RunMode.Async)]
        public async Task GetProfile([Remainder] string args = "") {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args == "") {
                var x = SaveFiles_Mapped.CreateSavedProfileObject(Context.User.Id).GetEmbed();
                await ReplyAsync("", false, x);
            } else {
                List<SocketGuildUser> w = GuildCache.InterpretUserInput(args);
                if (w == null) {
                    await ReplyAsync("Not a valid user!"); return;
                }else if(w[0] == null) {
                    await ReplyAsync(args.Substring(0,1).ToUpper() + args.Substring(1) + " is not a valid user!");
                    if (args.Contains("overall")) {
                        await ReplyAsync("Pssst, user profiles are overall by default.");
                    }
                    return;
                }
                var x = SaveFiles_Mapped.CreateSavedProfileObject(w[0].Id).GetEmbed();
                await ReplyAsync("", false, x);
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Retrieved a profile for " + Context.User.Username);
        }
    }
}
