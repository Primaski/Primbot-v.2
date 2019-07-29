using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Globalization;
using static Primbot_v._2.Uno_Score_Tracking.SaveFiles_GlobalVariables;
using Primbot_v._2.Uno_Score_Tracking;
using System.IO;

namespace Primbot_v._2.Modules.Information {
    public class Testing : ModuleBase<SocketCommandContext> {

        [Command("polol")]
        public async Task Polol() {
            DirectoryInfo dir = new DirectoryInfo(USER_SAVE_DIRECTORY);
            var paths = dir.EnumerateDirectories();
            foreach(var path in paths) {
                Console.WriteLine(path.FullName);
                if(File.Exists(path.FullName + "\\Unoprofile.txt")) {
                    Console.WriteLine("Found it.");
                    string[] fileLines = File.ReadAllLines(path.FullName + "\\Unoprofile.txt");
                    for(int i = 0; i < fileLines.Length; i++) {
                        if (fileLines[i].StartsWith("HIGH-CAH")) {
                            Console.WriteLine("HIGH-CAH FOUND");
                            fileLines[i] = fileLines[i].Replace("HIGH-CAH", "FIRST-CAH");
                        }
                    }
                    File.WriteAllLines(path.FullName + "\\Unoprofile.txt", fileLines);
                }
                if (File.Exists(path.FullName + "\\FN13_Unoprofile.txt")) {
                    Console.WriteLine("Found it!");
                    string[] fileLines = File.ReadAllLines(path.FullName + "\\FN13_Unoprofile.txt");
                    for (int i = 0; i < fileLines.Length; i++) {
                        if (fileLines[i].StartsWith("HIGH-CAH")) {
                            Console.WriteLine("HIGH-CAH FOUND");
                            fileLines[i] = fileLines[i].Replace("HIGH-CAH", "FIRST-CAH");
                        }
                    }
                    File.WriteAllLines(path.FullName + "\\FN13_Unoprofile.txt", fileLines);
                }
            }
            Console.WriteLine("complete");
        }

        [Command("test", RunMode = RunMode.Async)]
        public async Task Manual() {
            await ReplyAsync(":ok_hand:");
        }
        
        [Command("anaa")]
        public async Task Owo() {
            string NAME_OF_GAME = "pokeduel";
            string[] FIELD_VALUES = { "ITER-POKEDUEL", "POINTS-POKEDUEL" };
            int pointValForThisGame = 10;


            string[] msResults = File.ReadAllLines("C:\\avatars\\" + NAME_OF_GAME + ".txt");
            string[] username = new string[msResults.Count()];
            int[] score = new int[msResults.Count()];
            for (int i = 0; i < msResults.Count(); i++) {
                string msResult = msResults[i];
                try {
                    username[i] = msResult.Substring(msResult.IndexOf(". ") + 2, msResult.IndexOf("- ") - (msResult.IndexOf(". ") + 2));
                    score[i] = Int32.Parse(msResult.Substring(msResult.IndexOf("- ") + 2));
                } catch (Exception e) {
                    Console.WriteLine(msResult + "\n" + e.Message);
                }
            }

            List<SocketGuildUser> users = new List<SocketGuildUser>();
            foreach (string usern in username) {
                users.Add(GuildCache.GetUserByUsername(usern) ?? null);
                if (users[users.Count() - 1] == null) {
                    Console.WriteLine("Problem in retrieving user " + usern + " provide alternate name?");
                    users.Remove(users[users.Count() - 1]);
                    string response = Console.ReadLine();
                    if (response == "no") {
                        continue;
                    } else {
                        users.Add(GuildCache.GetUserByUsername(response) ?? null);
                        if (users[users.Count() - 1] == null) {
                            Console.WriteLine("Problem in retrieving user " + response + " again. Continuing.");
                        }
                    }
                }
            }

            int j = -1;
            foreach (var user in users) {
                j++;
                string userSavePath = USER_SAVE_DIRECTORY + "\\" + user.Id.ToString();
                if (!Directory.Exists(userSavePath)) {
                    Console.WriteLine("User Directory does not exist. Continue?");
                    if (Console.ReadLine() == "no") {
                        return;
                    }
                }
                if (!File.Exists(userSavePath + "\\" + UNO_SAVE_FILE_NAME)) {
                    Console.WriteLine("User SaveFile does not exist. Continue?");
                    if (Console.ReadLine() == "no") {
                        return;
                    }
                }
                string fullPath = userSavePath + "\\" + UNO_SAVE_FILE_NAME;
                foreach (var fieldname in FIELD_VALUES) {
                    if (fieldname.StartsWith("POINTS")) {
                        SaveFiles_Mapped.ModifyFieldValue(fieldname, fullPath, (score[j] * pointValForThisGame).ToString());
                        Console.WriteLine("Changed " + user + "'s field value to " + SaveFiles_Mapped.SearchMappedSaveFile(fullPath, fieldname));
                    } else {
                        SaveFiles_Mapped.ModifyFieldValue(fieldname, fullPath, score[j].ToString());
                        Console.WriteLine("Changed " + user + "'s field value to " + SaveFiles_Mapped.SearchMappedSaveFile(fullPath, fieldname));
                    }
                }
            }
        }
        /*
        [Command("erbherherhresher", RunMode = RunMode.Async)]
        public async Task GameIterFiles() {
            SaveFiles_Mapped.CreateNewGameIterationSaveLog(SaveFiles_GlobalVariables.SAVEFILE_GAMEITERATIONS);
            await ReplyAsync("Game_iteration file created.");
        }

        [Command("sihjgierjherihjreijher")]
        public async Task IDSeed([Remainder] string args = null) {
            if (args != null && Byte.TryParse(args,out byte ignore)) {
                byte type = Byte.Parse(args);
                string ID = SaveFiles_Sequences.LogGame(type, Context.User.Id, 15);
                await ReplyAsync(ID);
            }
            return;
        }

        [Command("giroiwgiwioweo")]
        public async Task Translate([Remainder] string args = null) {
            Dictionary<string, string> x = new Dictionary<string, string>();
            if (args != null) {
                //x = SaveLogger.TranslateSaveSeed(args);
            }
            var result = new EmbedBuilder();

            result.WithTitle("Game Statistics");
            result.AddField("Game: ", x["Game"]);
            //result.AddField("Iteration: ", x["Iteration"]);
            result.AddField("Date: ", x["Date"]);
            result.AddField("Time: ", x["Time"]);
            result.AddField("User ID: ", x["User ID"]);
            result.AddField("Point Val: ", x["Points"]);
            result.AddField("Game ID: ", x["Game ID"]);
            result.WithColor(Color.Red);

            Embed embed = result.Build();

            await Context.Channel.SendMessageAsync("", false, embed);
            return;
        }

        [Command("seekID")]
        public async Task seekID([Remainder] string args = null) {
            Dictionary<string, string> x = new Dictionary<string, string>();
            if (args != null) {
                List<string> w = SaveFiles_Sequences.SearchSaveFileForID(Int32.Parse(args));
                //x = SaveLogger.TranslateSaveSeed(w[0]);
            }
            var result = new EmbedBuilder();

            result.WithTitle("Game Statistics");
            result.AddField("Game: ", x["Game"]);
            //result.AddField("Iteration: ", x["Iteration"]);
            result.AddField("Date: ", x["Date"]);
            result.AddField("Time: ", x["Time"]);
            result.AddField("User ID: ", x["User ID"]);
            result.AddField("Point Val: ", x["Points"]);
            result.AddField("Game ID: ", x["Game ID"]);
            result.WithColor(Color.Red);

            Embed embed = result.Build();

            await Context.Channel.SendMessageAsync("", false, embed);
            return;
        }

        [Command("bingoboard")]
        public async Task bingo([Remainder] string args = null) {
            var result = new EmbedBuilder();

            result.WithTitle("Board");
            result.AddField("╔════╦════╦════╦════╦════╗", "_ _");
            result.AddField("║ XX ║ 15 ║ 25 ║ 16 ║ 51 ║", "_ _");
            result.AddField("╠════╬════╬════╬════╬════╣", "_ _");
            result.AddField("║ 11 ║ 95 ║ XX ║ XX ║ XX ║", "_ _");
            result.AddField("╠════╬════╬════╬════╬════╣", "_ _");
            result.AddField("║ XX ║ XX ║ FR ║ 51 ║ 22 ║", "_ _");
            result.AddField("╠════╬════╬════╬════╬════╣", "_ _");
            result.AddField("║ 13 ║ XX ║ XX ║ 49 ║ 28 ║", "_ _");
            result.AddField("╠════╬════╬════╬════╬════╣", "_ _");
            result.AddField("║ 29 ║ 11 ║ 29 ║ 02 ║ 01 ║", "_ _");
            result.AddField("╚════╩════╩════╩════╩════╝", "_ _");
            result.WithColor(Color.Red);

            Embed embed = result.Build();
            await Context.Channel.SendMessageAsync("", false, embed);
            return;
        }


        [Command("scoring")]
        public async Task scoring([Remainder] string args = null) {
            var result = new EmbedBuilder();

            result.WithTitle("Uno! - Scoring Procedure");
            result.AddField("For a three player game:", "1st: 10\n 2nd: 5,\n 3rd: 1", true);
            result.AddField("For a four player game:", "1st: 15\n 2nd: 10,\n 3rd: 5\n 4th: 1", true);
            result.AddField("5 or 6 player game:", "1st: 20\n 2nd: 15\n 3rd: 10\n 4th: 5\n 5th,6th: 1", true);
            result.AddField("_ _", "For every two additional players after that, everyone's scores are bumped up to the next 5-point tier (as was the difference between 4 and 5 player games).", false);
            result.AddField("Specifications:", "1. Only players who stay to the end will receive points. Games will be counted on a basis of how many" +
                "people joined initially, unless the game contains 7+ players, in which case it will be based on how many remained after the first winner.\n" +
                "2. A game must contain 3 people, 2 of which are from different teams to be considered valid.\n3. If the bot autokicks you for inactivity, it is not our fault.");
            result.WithColor(Color.Red);

            Embed embed = result.Build();

            await Context.Channel.SendMessageAsync("", false, embed);
            return;
        }
        */
    }
}
