using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Globalization;
using Discord.Commands;
using static Primbot_v._2.Uno_Score_Tracking.Defs;
using Discord;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace Primbot_v._2.Uno_Score_Tracking {
    /// <summary>
    /// Bridge between SaveFiles and Guild.
    /// </summary>
    public static class Bridge {

        /// <summary>
        /// Assumes IDpointcorr is well-formatted. Ensure this is complete to avoid ID nullification.
        /// </summary>
        public static Task LogGame(string GameName, List<Tuple<ulong, byte>> IDpointcorr, SocketCommandContext context = null) {
            ISocketMessageChannel reportingChannel = (ISocketMessageChannel)GuildCache.Uno_Cache.GetChannel(REPORT_CHANNEL_ID);
            byte GameType;
            string GameID = "";
            int attemptedGameType = Array.IndexOf(GameIden, GameName);
            if (attemptedGameType == -1 || attemptedGameType > Byte.MaxValue) {
                throw new Exception("Game Identification not found.");
            } else {
                GameType = (byte)attemptedGameType;
            }
            try {
                GameID = SaveFiles_Sequences.GenerateID(GameType);
            } catch (Exception e) {
                throw e;
            }
            List<string> SaveSequences = new List<string>(); //for creating an object to return in human readable form
            foreach (var player in IDpointcorr) {
                try {
                    SaveFiles_Sequences.LogGame(Int32.Parse(GameID, NumberStyles.HexNumber), player.Item1, player.Item2);
                } catch (Exception e) {
                    //revert
                    SaveFiles_Sequences.RevertbyID(Int32.Parse(GameID, NumberStyles.HexNumber));
                    throw e;
                }
            }
            Embed em = ReportLoggedGame(GameID); 
            reportingChannel.SendMessageAsync("", false, em);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Expects game to already be in save file
        /// </summary>=
        public static Embed ReportLoggedGame(string GameID, bool revert = false) {
            int x = -1;
            try {
                x = Int32.Parse(GameID, NumberStyles.HexNumber);
                if (x < 0x0 || x > 0xFFFFFFF) {
                    throw new Exception("Incorrectly formatted Game ID.");
                }
                List<string> matchingSequences = SaveFiles_Sequences.SearchSaveFileForID(x);
                SavedGame gameObject = new SavedGame(matchingSequences);
                Dictionary<string, string> embedContents = gameObject.MakeHumanReadable(true);
                if (revert) {
                    return GenerateLogEmbed("Game Statistics:", embedContents, revert);
                }
                return GenerateLogEmbed("Game Statistics:", embedContents);
            } catch (Exception e) {
                throw e;
            }
        }

        private static Embed GenerateLogEmbed(string Title, Dictionary<string, string> embedContents, bool revert = false) {
            var result = new EmbedBuilder();
            if (!revert) {
                result.WithTitle(Title);
            } else {
                result.WithTitle("REVERTED GAME ->");
            }
            int i = 0;
            string avatarID = "";
            string imageID = "https://thumbs.gfycat.com/AnguishedLikableJackrabbit-small.gif";
            bool imageShown = false;
            if (embedContents.ContainsKey("Player 1") && !embedContents.ContainsKey("Player 2")) {
                var key = embedContents["Player 1"];
                string ID = key.Substring(key.IndexOf("(") + 1, key.IndexOf(")") - (key.IndexOf("(") + 1));
                if(UInt64.TryParse(ID,out ulong temp)) {
                    try {
                        avatarID = GuildCache.Uno_Cache.GetUser(UInt64.Parse(ID)).GetAvatarUrl();
                    } catch {}
                }
            }
            if (avatarID != "") {
                result.WithThumbnailUrl(avatarID);
            } else {
                result.WithImageUrl(imageID);
            }
            foreach (KeyValuePair<string, string> dict in embedContents) {
                if(avatarID == "" && imageShown == false) {
                    imageShown = true;
                }
                if (dict.Key.StartsWith("Player") || i < 2) {
                    result.AddField(dict.Key, dict.Value, true);
                } else {
                    result.AddField(dict.Key, dict.Value, false);
                }
                i++;
            }
            if (!revert) {
                result.WithColor(Color.Red);
            } else {
                result.WithColor(Color.DarkGrey);
            }

            Embed embed = result.Build();
            return embed;
        }

        public static string GetLeaderboard(string gameAbbreviation, string name = "ITER", string humanName = "",
            short fortnight = -1, short startIndex = 0, byte noOfEntries = 10, ulong teamspecific = 0) {
            string key = name + "-" + gameAbbreviation; //ITER-UNO example
            SavedLeaderboard lb = SaveFiles_Mapped.CreateLeaderboardObject(key, fortnight, teamspecific);
            string result = lb.MakeHumanReadable(humanName, startIndex, noOfEntries);
            return result;
        }

        public static string GetUserScore(string gameAbbreviation, ulong soughtID, string name = "ITER", string humanName = "",
                short fortnight = -1, bool local = false) {
            string key = name + "-" + gameAbbreviation; //ITER-UNO example
            SavedLeaderboard lb = SaveFiles_Mapped.CreateLeaderboardObject(key, fortnight);
            string result = lb.GetHumanReadableScore(soughtID,humanName,local);
            return result;
        }

        public static string InterpretScoreInputAndGetPlacementInfo(string args, ulong requesterID = 0) {
            args.ToLower();
            string GAME_TYPE = "";
            short FORTNIGHT = FORTNIGHT_NUMBER;
            try {
                if (args.Contains("overall")) {
                    FORTNIGHT = -1;
                    args = args.Replace("overall", "");
                } else if (Regex.IsMatch(args, @"(fn|fortnight)\s?(\d{1,2})(\D|$)", RegexOptions.IgnoreCase)) {
                    string fnNo = Regex.Match(args, @"(fn|fortnight)\s?(\d{1,2})(\D|$)").Groups[2].ToString();
                    FORTNIGHT = Int16.Parse(fnNo);
                    if (FORTNIGHT >= 0 && FORTNIGHT < 13) {
                        return "Individual tracking before Fortnight 13 is not available. Reference <#490300319729713172>.";
                    }
                    if (FORTNIGHT > FORTNIGHT_NUMBER) {
                        return "The server is on Fortnight " + FORTNIGHT_NUMBER + ", and does not have tracking for future fortnights yet, silly goose.";
                    }
                    args = args.Replace("fn" + fnNo, "");
                    args = args.Replace("fn " + fnNo, "");
                }

                string[] gameTypes = { "cah", "uno", "ms", "tetris", "bumps", "idlerpg", "pokeduel", "bingo", "chess",
            "tourney", "tournament", "casino", "minesweeper", "idle", "poke", "knight", "knights", "trivia"};
                foreach (string gameType in gameTypes) {
                    if (args.Contains(gameType)) {
                        GAME_TYPE = gameType;
                        args = args.Replace(gameType, "");
                        break;
                    }
                }
                if (GAME_TYPE == "")
                    GAME_TYPE = "server";

                List<SocketGuildUser> x = new List<SocketGuildUser>();
                bool local = false;
                if (args != "") {
                    x = GuildCache.InterpretUserInput(args);
                } else if (requesterID != 0) {
                    x = GuildCache.InterpretUserInput(requesterID.ToString());
                    local = true;
                } else {
                    throw new Exception();
                }

                if (x == null) {
                    return "User not found.";
                }

                if (x.Count() == 0) {
                    return "User not found.";
                }
                ulong userID = x[0].Id;

                string res;
                try {
                    switch (GAME_TYPE) {
                        case "server":
                            res = GetUserScore("SERVER", userID, "POINTS", "Server", FORTNIGHT, local); break;
                        case "uno":
                            res = GetUserScore("UNO", userID, "FIRST", "First-place Uno!", FORTNIGHT, local); break;
                        case "cah":
                            res = GetUserScore("CAH", userID, "FIRST", "CAH", FORTNIGHT, local); break;
                        case "bingo":
                            res = GetUserScore("BINGO", userID, "HIGH", "Bingo", FORTNIGHT, local); break;
                        case "ms":
                        case "minesweeper":
                            res = GetUserScore("MS", userID, "ITER", "Minesweeper", FORTNIGHT, local); break;
                        case "tetris":
                            res = GetUserScore("TETRIS", userID, "HIGH", "Tetris", FORTNIGHT, local); break;
                        case "pokeduel":
                        case "poke":
                            res = GetUserScore("POKEDUEL", userID, "ITER", "Pokeduels", FORTNIGHT, local); break;
                        case "idlerpg":
                        case "idle":
                            res = GetUserScore("IDLERPG", userID, "ITER", "IdleRPG duels", FORTNIGHT, local); break;
                        case "chess":
                            res = GetUserScore("CHESS", userID, "ITER", "Chess", FORTNIGHT, local); break;
                        case "bumps":
                            res = GetUserScore("BUMPS", userID, "ITER", "Bumps", FORTNIGHT, local); break;
                        case "casino":
                            res = GetUserScore("CASINO", userID, "POINTS", "Casino points", FORTNIGHT, local); break;
                        case "tourney":
                        case "tournament":
                            res = GetUserScore("TOURNEY", userID, "POINTS", "Tourney points", FORTNIGHT, local); break;
                        case "knight":
                        case "knights":
                            res = GetUserScore("KNIGHTS", userID, "ITER", "Knights", FORTNIGHT, local); break;
                        case "trivia":
                            res = GetUserScore("KNIGHTS", userID, "ITER", "Knights", FORTNIGHT, local); break;
                        default:
                            throw new Exception("Game not found");
                    }
                    if (res == null) {
                        throw new Exception("Leaderboard not found");
                    }
                } catch (Exception e) {
                    return e.Message + "\n" + e.StackTrace;
                }
                return res;
            } catch(Exception e) {
                return e.Message + "\n" + e.StackTrace;
            }
            
        }

        public static string InterpretLeaderboardInputAndGetLeaderboard(string args) {
            args.ToLower();
            string GAME_TYPE = "";
            bool fortnightProvided = false;
            short FORTNIGHT = FORTNIGHT_NUMBER;
            short STARTING_INDEX = 0;
            if (args.Contains("overall")) {
                FORTNIGHT = -1;
                args.Replace("overall", "");
            } else if (Regex.IsMatch(args, @"(fn|fortnight)\s?(\d{1,2})(\D|$)", RegexOptions.IgnoreCase)) {
                string fnNo = Regex.Match(args, @"(fn|fortnight)\s?(\d{1,2})(\D|$)").Groups[2].ToString();
                FORTNIGHT = Int16.Parse(fnNo);
                if (FORTNIGHT >= 0 && FORTNIGHT < 13) {
                    return "Individual tracking before Fortnight 13 is not available.";
                }
                if (FORTNIGHT > FORTNIGHT_NUMBER) {
                    return "The server is on Fortnight " +FORTNIGHT_NUMBER + ", and does not have tracking for future fortnights yet, silly goose.";
                }
                fortnightProvided = true;
                args.Replace(fnNo, "");
            }

            string[] gameTypes = { "cah", "uno", "ms", "iteruno", "tetris", "bumps", "idlerpg", "pokeduel", "bingo", "chess",
            "tourney", "tournament", "casino", "minesweeper", "idle", "poke", "knight", "10ktetris", "knights", "trivia"};
            foreach (string gameType in gameTypes) {
                if (args.Contains(gameType)) {
                    GAME_TYPE = gameType;
                    args.Replace(gameType, "");
                }
            }
            if (GAME_TYPE == "")
                GAME_TYPE = "server";
            if (Regex.IsMatch(args, @"(page|p|pg)\s?(\d{1,2})", RegexOptions.IgnoreCase)) {
                string page = Regex.Match(args, @"(page|p|pg)\s?(\d{1,2})").Groups[2].ToString();
                short temp = Int16.Parse(page);
                STARTING_INDEX = (short)((temp - 1) * 10);
                args.Replace(page, "");
            }

            ulong teamspecific = 0;
            if (args.Contains("yellow")) {
                args.Replace("yellow", "");
                teamspecific = YELLOW_TEAM_ID;
            }else if (args.Contains("green")) {
                args.Replace("green", "");
                teamspecific = GREEN_TEAM_ID;
            }else if (args.Contains("blue")) {
                args.Replace("blue", "");
                teamspecific = BLUE_TEAM_ID;
            }else if (args.Contains("red")) {
                args.Replace("red", "");
                teamspecific = RED_TEAM_ID;
            }

            if(!fortnightProvided && GAME_TYPE != "server") {
                FORTNIGHT = -1;
            }
            string lb;
            Console.WriteLine(FORTNIGHT);
            try {
                switch (GAME_TYPE) {
                    case "server":
                        lb = GetLeaderboard("SERVER", "POINTS", "Server", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "uno":
                        lb = GetLeaderboard("UNO", "FIRST", "First-place Uno!", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "iteruno":
                        lb = GetLeaderboard("UNO", "ITER", "Uno Games Played", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "cah":
                        lb = GetLeaderboard("CAH", "FIRST", "CAH", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "bingo":
                        lb = GetLeaderboard("BINGO", "HIGH", "Bingo", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "ms":
                    case "minesweeper":
                        lb = GetLeaderboard("MS", "ITER", "Minesweeper", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "tetris":
                        lb = GetLeaderboard("TETRIS", "HIGH", "Tetris", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "10ktetris":
                        lb = GetLeaderboard("TETRIS", "GAMESOVER10K", "Tetris 10k+ games", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "pokeduel":
                    case "poke":
                        lb = GetLeaderboard("POKEDUEL", "ITER", "Pokeduels", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "idlerpg":
                    case "idle":
                        lb = GetLeaderboard("IDLERPG", "ITER", "IdleRPG duels", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "chess":
                        lb = GetLeaderboard("CHESS", "ITER", "Chess", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "bumps":
                        lb = GetLeaderboard("BUMPS", "ITER", "Bumps", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "casino":
                        lb = GetLeaderboard("CASINO", "POINTS", "Casino points", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "tourney":
                    case "tournament":
                        lb = GetLeaderboard("TOURNEY", "POINTS", "Tourney points", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    case "knight":
                    case "knights":
                        lb = GetLeaderboard("KNIGHTS", "ITER", "Knights Path", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    //case "trivia":
                        //lb = GetLeaderboard("TRIVIA", "ITER", "Knights Path", FORTNIGHT, STARTING_INDEX, 10, teamspecific); break;
                    default:
                        throw new Exception("Game not found");
                }
                if(lb == null) {
                    throw new Exception("Leaderboard not found");
                }
            } catch (Exception e) {
                return e.Message + "\n" + e.StackTrace;
            }
            return lb;
        }

        public static Embed GetDailyLimitEmbed(ulong ID) {
            EmbedBuilder emb = new EmbedBuilder();
            try {
                string filePath = USER_SAVE_DIRECTORY + "\\" + ID + "\\Unoprofile.txt";
                var limitPairs = SaveFiles_Mapped.GetAllValues(filePath, "PLAYSTODAY-");
<<<<<<< HEAD
                if(limitPairs == null || limitPairs.Count() == 0) {
=======
                if (limitPairs == null || limitPairs.Count() == 0) {
>>>>>>> games1v1
                    return emb.Build();
                }
                int alreadyPlayed, maxDaily, playsLeft, msknights;
                alreadyPlayed = maxDaily = playsLeft = 0;
                msknights = MINESWEEPER_DAILY_LIMIT;
<<<<<<< HEAD
                foreach(var tuple in limitPairs) {
                    alreadyPlayed = Int32.Parse(tuple.Item2);
                    maxDaily = Defs.GetDailyLimit(tuple.Item1.ToLower());
                    playsLeft = maxDaily - alreadyPlayed;
                    if (tuple.Item1 == "MS" || tuple.Item1 == "KNIGHTS") { msknights -= alreadyPlayed; continue; }
                    emb.AddField(tuple.Item1[0] + tuple.Item1.Substring(1).ToLower(), playsLeft.ToString() + " more game(s)", true);
                }
                emb.AddField("MS/Knights", msknights.ToString() + " more game(s)", true);
=======
                foreach (var tuple in limitPairs) {
                    alreadyPlayed = Int32.Parse(tuple.Item2);
                    maxDaily = Defs.GetDailyLimit(tuple.Item1.ToLower());
                    playsLeft = maxDaily - alreadyPlayed;
                    playsLeft = (playsLeft < 0) ? 0 : playsLeft;
                    if (tuple.Item1 == "MS" || tuple.Item1 == "KNIGHTS") { msknights -= alreadyPlayed; continue; }
                    emb.AddField(tuple.Item1[0] + tuple.Item1.Substring(1).ToLower(), playsLeft.ToString() + " more game(s)", true);
                }
                emb.AddField("MS/Knights", ((msknights < 0) ? "0" : msknights.ToString()) + " more game(s)", true);
>>>>>>> games1v1
                emb.WithTitle("Your remaining games available today:");
                emb.WithColor(Color.Purple);
                TimeSpan beforeReset = Defs.TimeUntilMidnight();
                emb.WithFooter("Your daily limits reset in " + beforeReset.Hours + " hours and " + beforeReset.Minutes + " minutes");
                emb.WithCurrentTimestamp();
                return emb.Build();
            } catch (FileNotFoundException) {
                return emb.WithTitle("You don't have an Uno account!").Build();
            } catch (Exception e) {
                throw e;
            }
        }
    }
}
