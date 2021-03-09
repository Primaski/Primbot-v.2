using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Globalization;
using static Primbot_v._2.Uno_Score_Tracking.Defs;
using Primbot_v._2.Uno_Score_Tracking;
using System.Threading;

namespace Primbot_v._2.Modules.Scoring {
    //commands of this class should not be ASYNC
    public class Log : ModuleBase<SocketCommandContext> {

        /* SINGLE PLAYER */
        string format = "`p*logX [USER]` OR `p*logX [USER], [USER], [USER]...`\n" +
            "whereby `X` is the type of game (like `ms` for Minesweeper), \n`[USER]` is their username, " +
            "ping, or ID, \nand `...` refers to any number of users <= 10";

        List<ulong> pmID = new List<ulong> {
            263733973711192064, //prim
            328641116884959235, //lucky
            494274144159006731, //ava
            485120582866698242, //biggus
            439187379547537418, //mihael
            339095826183749632, //jenna
            467381662582308864, //swinub
            557320114110726204, //koro
            770517667940794409, //tane
        };

        [Command("logms", RunMode = RunMode.Async)]
        public async Task LogMineSweeper([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            List<Tuple<ulong, byte>> scorer;
            foreach (SocketUser user in users) {
                string playsTodayStr = SaveFiles_Mapped.SearchValue(USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() + "\\" + UNO_SAVE_FILE_NAME, "PLAYSTODAY-MS") ?? "";
                string playsTodayStrKnigh = SaveFiles_Mapped.SearchValue(USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() + "\\" + UNO_SAVE_FILE_NAME, "PLAYSTODAY-KNIGHTS") ?? "";
                if (!Int32.TryParse(playsTodayStr, out int ignore) && !Int32.TryParse(playsTodayStrKnigh, out int ignore2)) {
                    await ReplyAsync("Unknown error in determining how many games were played today.");
                    continue;
                }
                int playsToday = Int32.Parse(playsTodayStr);
                int playsTodayKnigh = Int32.Parse(playsTodayStrKnigh);
                if (playsToday + playsTodayKnigh >= MINESWEEPER_DAILY_LIMIT) {
                    await ReplyAsync(user.Username + " has hit their daily limit for this minigame, and their win cannot be logged.");
                    continue;
                }
                Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, MINESWEEPER_POINT_VALUE);
                scorer = new List<Tuple<ulong, byte>> { scorerTuple };
                try {
                    Task result = Uno_Score_Tracking.Bridge.LogGame("minesweeper", scorer, Context);
                    if (result.IsCompleted) {
                        if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "minesweeper")) {
                            await ReplyAsync("Successfully logged " + user.Username + "'s Minesweeper win! <#537104363127046145>");
                        } else {
                            await ReplyAsync(":exlamation: Error in updating " + user.Username + "'s save file.");
                        }
                    }
                } catch (Exception e) {
                    await ReplyAsync(e.Message + "\n" + e.StackTrace);
                    return;
                }
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged MS for " + Context.User.Username);
        }

        [Command("logtet", RunMode = RunMode.Async)]
        public async Task LogTet([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await LogTetris(args);
        }

        [Command("logtetris", RunMode = RunMode.Async)]
        public async Task LogTetris([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`."); return;
            }
            string score = args.Substring(0, args.IndexOf(" "));
            args = args.Substring(args.IndexOf(" ") + 1);
            if (!Int32.TryParse(score, out int ignore)) {
                await ReplyAsync("Format: `p*logtetris [amount] [user]`");
                return;
            }
            int tetrisScore = Int32.Parse(score);
            if (tetrisScore < 200 || tetrisScore % 100 != 0) {
                await ReplyAsync("Please input the tetris SCORE, not point value.");
                return;
            }
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            if (users.Count() > 1) {
                await ReplyAsync("Only one game log at a time is permitted for Tetris.");
                return;
            }
            List<Tuple<ulong, byte>> scorer;
            var user = users[0];
            string playsTodayStr = SaveFiles_Mapped.SearchValue(USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() + "\\" + UNO_SAVE_FILE_NAME, "PLAYSTODAY-TETRIS") ?? "";
            if (!Int32.TryParse(playsTodayStr, out int ignoreme)) {
                await ReplyAsync("Unknown error in determining how many games were played today.");
                return;
            }
            int playsToday = Int32.Parse(playsTodayStr);
            if (playsToday >= TETRIS_DAILY_LIMIT) {
                await ReplyAsync(user.Username + " has hit their daily limit for this minigame, and their win cannot be logged.");
                return;
            }
            byte reward = (byte)(tetrisScore / TETRIS_SCORE_FOR_ONE_POINT);
            if (reward > 50) {
                if (playsToday > TETRIS_DAILY_LIMIT - 2) {
                    reward = 50;
                } else {
                    byte secondVal = (byte)(reward - 50);
                    if (secondVal > 50) {
                        secondVal = 50;
                    }
                    Emoji one = new Emoji("1\u20e3");
                    Emoji two = new Emoji("2\u20e3");
                    var msg = await Context.Channel.SendMessageAsync("Logging a tetris game for " + user.Username + " with a score of " + tetrisScore + ". However, this exceeds the 50 points maximum " +
                        "for a single Tetris game. You have the option to count this as one game towards the daily limit for (50) points, or two games for (" +
                        (secondVal + 50).ToString() + "). How many?");

                    await msg.AddReactionAsync(one);
                    await msg.AddReactionAsync(two);
                    bool oneReacc = false;
                    bool twoReacc = false;

                    for (int i = 0; i < 100 && !oneReacc && !twoReacc; i++) {
                        var oneReactions = await msg.GetReactionUsersAsync(one, 5);
                        var twoReactions = await msg.GetReactionUsersAsync(two, 5);
                        foreach (var reactionUser in oneReactions) {
                            if (reactionUser.Id == Context.User.Id) {
                                oneReacc = true;
                                break;
                            }
                        }
                        foreach (var reactionUser in twoReactions) {
                            if (reactionUser.Id == Context.User.Id) {
                                twoReacc = true;
                                break;
                            }
                        }
                        Thread.Sleep(100);
                        i++;
                    }
                    if (!(oneReacc || twoReacc)) {
                        await ReplyAsync("You did not choose in time."); return;
                    }
                    if (oneReacc) {
                        reward = 50;
                        await ReplyAsync("You chose 1 for 50 points.");
                    } else {
                        reward = (byte)(50 + secondVal);
                        await ReplyAsync("You chose 2 for " + reward + " points.");
                    }
                }
            }
            Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, reward);
            scorer = new List<Tuple<ulong, byte>> { scorerTuple };
            try {
                Task result = Uno_Score_Tracking.Bridge.LogGame("tetris", scorer, Context);
                if (result.IsCompleted) {
                    if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "tetris")) {
                        await ReplyAsync("Successfully logged " + user.Username + "'s Tetris win! <#537104363127046145>");
                        string saveDir = USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() +
                            "\\" + UNO_SAVE_FILE_NAME;
                        string FNsaveDir = USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() +
                            "\\" + "FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME;
                        if (Int32.Parse(SaveFiles_Mapped.SearchValue(saveDir, "HIGH-TETRIS")) < tetrisScore) {
                            SaveFiles_Mapped.SetFieldValue("HIGH-TETRIS", saveDir, tetrisScore.ToString());
                            SaveFiles_Mapped.SetFieldValue("HIGH-TETRIS", FNsaveDir, tetrisScore.ToString());
                        }else if(Int32.Parse(SaveFiles_Mapped.SearchValue(FNsaveDir, "HIGH-TETRIS")) < tetrisScore) {
                            SaveFiles_Mapped.SetFieldValue("HIGH-TETRIS", FNsaveDir, tetrisScore.ToString());
                        }
                        if(tetrisScore >= 10000) {
                            SaveFiles_Mapped.AddFieldValue("GAMESOVER10K-TETRIS", FNsaveDir, "1");
                            SaveFiles_Mapped.AddFieldValue("GAMESOVER10K-TETRIS", saveDir, "1");
                        }
                    } else {
                        await ReplyAsync(":exclamation: Error in updating " + user.Username + "'s save file.");
                    }
                }
            } catch (Exception e) {
                await ReplyAsync(e.Message + "\n" + e.StackTrace);
                return;
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Tetris for " + Context.User.Username);
        }
        /*
        [Command("logpoke", RunMode = RunMode.Async)]
        public async Task LogPokeduel([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_I
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            List<Tuple<ulong, byte>> scorer;
            foreach (SocketUser user in users) {
                string playsTodayStr = SaveFiles_Mapped.SearchValue(USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() + "\\" + UNO_SAVE_FILE_NAME, "PLAYSTODAY-POKEDUEL") ?? "";
                if (!Int32.TryParse(playsTodayStr, out int ignore)) {
                    await ReplyAsync("Unknown error in determining how many games were played today.");
                    continue;
                }
                int playsToday = Int32.Parse(playsTodayStr);
                if (playsToday >= POKEDUEL_DAILY_LIMIT) {
                    await ReplyAsync(user.Username + " has hit their daily limit for this minigame, and their win cannot be logged.");
                    continue;
                }
                Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, POKEDUEL_POINT_VALUE);
                scorer = new List<Tuple<ulong, byte>> { scorerTuple };
                try {
                    Task result = Uno_Score_Tracking.Bridge.LogGame("pokeduel", scorer, Context);
                    if (result.IsCompleted) {
                        if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "pokeduel")) {
                            await ReplyAsync("Successfully logged " + user.Username + "'s Pokeduel win! <#537104363127046145>");
                        } else {
                            await ReplyAsync(":exlamation: Error in updating " + user.Username + "'s save file.");
                        }
                    }
                } catch (Exception e) {
                    await ReplyAsync(e.Message + "\n" + e.StackTrace);
                    return;
                }
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Pokeduel for " + Context.User.Username);
        }

        [Command("logidle", RunMode = RunMode.Async)]
        public async Task LogIdleduel([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            List<Tuple<ulong, byte>> scorer;
            foreach (SocketUser user in users) {
                string playsTodayStr = SaveFiles_Mapped.SearchValue(USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() + "\\" + UNO_SAVE_FILE_NAME, "PLAYSTODAY-IDLERPG") ?? "";
                if (!Int32.TryParse(playsTodayStr, out int ignore)) {
                    await ReplyAsync("Unknown error in determining how many games were played today.");
                    continue;
                }
                int playsToday = Int32.Parse(playsTodayStr);
                if (playsToday >= IDLERPG_DAILY_LIMIT) {
                    await ReplyAsync(user.Username + " has hit their daily limit for this minigame, and their win cannot be logged.");
                    continue;
                }
                Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, IDLERPG_POINT_VALUE);
                scorer = new List<Tuple<ulong, byte>> { scorerTuple };
                try {
                    Task result = Uno_Score_Tracking.Bridge.LogGame("idlerpg", scorer, Context);
                    if (result.IsCompleted) {
                        if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "idlerpg")) {
                            await ReplyAsync("Successfully logged " + user.Username + "'s IdleRPG win! <#537104363127046145>");
                        } else {
                            await ReplyAsync(":exlamation: Error in updating " + user.Username + "'s save file.");
                        }
                    }
                } catch (Exception e) {
                    await ReplyAsync(e.Message + "\n" + e.StackTrace);
                    return;
                }
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Idlerpg for " + Context.User.Username);
        }

        [Command("logchess", RunMode = RunMode.Async)]
        public async Task LogChess([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            List<Tuple<ulong, byte>> scorer;
            foreach (SocketUser user in users) {
                string playsTodayStr = SaveFiles_Mapped.SearchValue(USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() + "\\" + UNO_SAVE_FILE_NAME, "PLAYSTODAY-CHESS") ?? "";
                if (!Int32.TryParse(playsTodayStr, out int ignore)) {
                    await ReplyAsync("Unknown error in determining how many games were played today.");
                    continue;
                }
                int playsToday = Int32.Parse(playsTodayStr);
                if (playsToday >= CHESS_DAILY_LIMIT) {
                    await ReplyAsync(user.Username + " has hit their daily limit for this minigame, and their win cannot be logged.");
                    continue;
                }
                Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, CHESS_POINT_VALUE);
                scorer = new List<Tuple<ulong, byte>> { scorerTuple };
                try {
                    Task result = Uno_Score_Tracking.Bridge.LogGame("chess", scorer, Context);
                    if (result.IsCompleted) {
                        if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "chess")) {
                            await ReplyAsync("Successfully logged " + user.Username + "'s Chess win! <#537104363127046145>");
                        } else {
                            await ReplyAsync(":exlamation: Error in updating " + user.Username + "'s save file.");
                        }
                    }
                } catch (Exception e) {
                    await ReplyAsync(e.Message + "\n" + e.StackTrace);
                    return;
                }
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged chess for " + Context.User.Username);
        }
        */

        [Command("logpoke", RunMode = RunMode.Async)]
        public async Task LogPokeduel([Remainder] string args = null) {
            GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            if(users.Count() != 2) {
                await ReplyAsync("Format: `p*logpoke [winner], [loser]`");
                return;
            }


            List<Tuple<ulong, byte>> scorers = new List<Tuple<ulong, byte>>();
            try {
                scorers.Add(Tuple.Create(users[0].Id,
                    Defs.HasUserHitDailyLimit(users[0].Id, "pokeduel") ? (byte)0 : POKEDUEL_WINNER_POINT_VALUE));
                scorers.Add(Tuple.Create(users[1].Id,
                    Defs.HasUserHitDailyLimit(users[1].Id, "pokeduel") ? (byte)0 : POKEDUEL_LOSER_POINT_VALUE));
            } catch(Exception e) { throw e; }

            try {
                Task result = Bridge.LogGame("pokeduel", scorers, Context);
                if (result.IsCompleted) {
                    if (Games_1v1.SaveFileUpdate(scorers,"pokeduel")) {
                        await ReplyAsync("Successfully logged the Pokeduel! <#537104363127046145>");
                    } else {
                        await ReplyAsync(":exlamation: Error in updating save files.");
                    }
                }
            } catch(Exception e) {
                await ReplyAsync(e.Message + "\n" + e.StackTrace);
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Pokeduel for " + Context.User.Username);
        }

        [Command("logidle", RunMode = RunMode.Async)]
        public async Task LogIdleduel([Remainder] string args = null) {
            GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            if (users.Count() != 2) {
                await ReplyAsync("Format: `p*logidle [winner], [loser]`");
                return;
            }

            List<Tuple<ulong, byte>> scorers = new List<Tuple<ulong, byte>>();
            try {
                scorers.Add(Tuple.Create(users[0].Id,
                    Defs.HasUserHitDailyLimit(users[0].Id, "idlerpg") ? (byte)0 : IDLERPG_WINNER_POINT_VALUE));
                scorers.Add(Tuple.Create(users[1].Id,
                    Defs.HasUserHitDailyLimit(users[1].Id, "idlerpg") ? (byte)0 : IDLERPG_LOSER_POINT_VALUE));
            } catch (Exception e) { throw e; }

            try {
                Task result = Bridge.LogGame("idlerpg", scorers, Context);
                if (result.IsCompleted) {
                    if (Games_1v1.SaveFileUpdate(scorers, "idlerpg")) {
                        await ReplyAsync("Successfully logged the IdleRPG duel! <#537104363127046145>");
                    } else {
                        await ReplyAsync(":exlamation: Error in updating save files.");
                    }
                }
            } catch (Exception e) {
                await ReplyAsync(e.Message + "\n" + e.StackTrace);
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Pokeduel for " + Context.User.Username);
        }

        [Command("logchess", RunMode = RunMode.Async)]
        public async Task LogChess([Remainder] string args = null) {
            GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            if (users.Count() != 2) {
                await ReplyAsync("Format: `p*logchess [winner], [loser]`");
                return;
            }


            List<Tuple<ulong, byte>> scorers = new List<Tuple<ulong, byte>>();
            try {
                scorers.Add(Tuple.Create(users[0].Id,
                    Defs.HasUserHitDailyLimit(users[0].Id, "chess") ? (byte)0 : CHESS_WINNER_POINT_VALUE));
                scorers.Add(Tuple.Create(users[1].Id,
                    Defs.HasUserHitDailyLimit(users[1].Id, "chess") ? (byte)0 : CHESS_LOSER_POINT_VALUE));
            } catch (Exception e) { throw e; }

            try {
                Task result = Bridge.LogGame("chess", scorers, Context);
                if (result.IsCompleted) {
                    if (Games_1v1.SaveFileUpdate(scorers, "chess")) {
                        await ReplyAsync("Successfully logged the Chess game! <#537104363127046145>");
                    } else {
                        await ReplyAsync(":exlamation: Error in updating save files.");
                    }
                }
            } catch (Exception e) {
                await ReplyAsync(e.Message + "\n" + e.StackTrace);
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Pokeduel for " + Context.User.Username);
        }

        [Command("logbump", RunMode = RunMode.Async)]
        public async Task LogBump([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            List<Tuple<ulong, byte>> scorer;
            foreach (SocketUser user in users) {
                Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, BUMP_POINT_VALUE);
                scorer = new List<Tuple<ulong, byte>> { scorerTuple };
                try {
                    Task result = Uno_Score_Tracking.Bridge.LogGame("bumps", scorer, Context);
                    if (result.IsCompleted) {
                        if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "bumps")) {
                            await ReplyAsync("Successfully logged " + user.Username + "'s Bump! <#537104363127046145>");
                        } else {
                            await ReplyAsync(":exlamation: Error in updating " + user.Username + "'s save file.");
                        }
                    }
                } catch (Exception e) {
                    await ReplyAsync(e.Message + "\n" + e.StackTrace);
                    return;
                }
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged bump for " + Context.User.Username);
        }

        [Command("logknight", RunMode = RunMode.Async)]
        public async Task LogKnight([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await LogKnights(args);
        }

        [Command("logknights", RunMode = RunMode.Async)]
        public async Task LogKnights([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            List<Tuple<ulong, byte>> scorer;
            foreach (SocketUser user in users) {
                string playsTodayStr = SaveFiles_Mapped.SearchValue(USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() + "\\" + UNO_SAVE_FILE_NAME, "PLAYSTODAY-MS") ?? "";
                string playsTodayStrKnigh = SaveFiles_Mapped.SearchValue(USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() + "\\" + UNO_SAVE_FILE_NAME, "PLAYSTODAY-KNIGHTS") ?? "";
                if (!Int32.TryParse(playsTodayStr, out int ignore) && !Int32.TryParse(playsTodayStrKnigh, out int ignore2)) {
                    await ReplyAsync("Unknown error in determining how many games were played today.");
                    continue;
                }
                int playsToday = Int32.Parse(playsTodayStr);
                int playsTodayKnigh = Int32.Parse(playsTodayStrKnigh);
                if (playsToday + playsTodayKnigh >= KNIGHTS_DAILY_LIMIT) {
                    await ReplyAsync(user.Username + " has hit their daily limit for this minigame, and their win cannot be logged.");
                    continue;
                }
                Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, KNIGHTS_POINT_VALUE);
                scorer = new List<Tuple<ulong, byte>> { scorerTuple };
                try {
                    Task result = Uno_Score_Tracking.Bridge.LogGame("knights", scorer, Context);
                    if (result.IsCompleted) {
                        if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "knights")) {
                            await ReplyAsync("Successfully logged " + user.Username + "'s Minesweeper Knights win! <#537104363127046145>");
                        } else {
                            await ReplyAsync(":exlamation: Error in updating " + user.Username + "'s save file.");
                        }
                    }
                } catch (Exception e) {
                    await ReplyAsync(e.Message + "\n" + e.StackTrace);
                    return;
                }
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged knights for " + Context.User.Username);
        }

        [Command("logns", RunMode = RunMode.Async)]
        public async Task LogNonStandard([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            string score = args.Substring(0, args.IndexOf(" "));
            args = args.Substring(args.IndexOf(" ") + 1);
            if (!Int32.TryParse(score, out int ignore)) {
                await ReplyAsync("Format: `p*logns [amount] [user]`");
                return;
            } else if (Int32.Parse(score) > Byte.MaxValue) {
                await ReplyAsync("255 is the maximum value for an individual log. Try splitting it up.");
                return;
            }
            byte NSscore = Byte.Parse(score);
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            if (users.Count() > 1) {
                await ReplyAsync("Only one game log at a time is permitted for Non-standard logs.");
                return;
            }
            var user = users[0];
            List<Tuple<ulong, byte>> scorer;
            Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, NSscore);
            scorer = new List<Tuple<ulong, byte>> { scorerTuple };
            try {
                Task result = Uno_Score_Tracking.Bridge.LogGame("non-standard", scorer, Context);
                if (result.IsCompleted) {
                    if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "non-standard")) {
                        await ReplyAsync("Successfully logged " + user.Username + "'s Non-standard score! <#537104363127046145>");
                    } else {
                        await ReplyAsync(":exclamation: Error in updating " + user.Username + "'s save file.");
                    }
                }
            } catch (Exception e) {
                await ReplyAsync(e.Message + "\n" + e.StackTrace);
                return;
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged NS for " + Context.User.Username);
        }

        [Command("logtourney", RunMode = RunMode.Async)]
        public async Task LogTourney([Remainder] string args = null) {
            GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            List<Tuple<ulong, byte>> scorer;
            foreach (SocketUser user in users) {
                Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, TOURNAMENT_POINT_VALUE);
                scorer = new List<Tuple<ulong, byte>> { scorerTuple };
                try {
                    Task result = Uno_Score_Tracking.Bridge.LogGame("tourney", scorer, Context);
                    if (result.IsCompleted) {
                        if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "tourney")) {
                            await ReplyAsync("Successfully logged " + user.Username + "'s Tournament win! <#537104363127046145>");
                        } else {
                            await ReplyAsync(":exlamation: Error in updating " + user.Username + "'s save file.");
                        }
                    }
                } catch (Exception e) {
                    await ReplyAsync(e.Message + "\n" + e.StackTrace);
                    return;
                }
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Pokeduel for " + Context.User.Username);
        }

        [Command("revert")]
        public async Task Revert([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync("Format: `p*revert [Game ID]`");
                return;
            }
            if (!Int32.TryParse(args, NumberStyles.HexNumber, null, out int ignore)) {
                await ReplyAsync(":exclamation: Please provide a valid Game ID.");
                return;
            }
            string IDString = args.Trim();
            int ID = Int32.Parse(args, NumberStyles.HexNumber);

            try {
                List<string> reversions = SaveFiles_Sequences.RevertbyID(ID);
                if (reversions != null) {
                    //List<string> seqs = SaveFiles_Sequences.SearchSaveFileForID(ID);
                    string gameName = GameIden[Byte.Parse(IDString.Substring(0, GAME_TYPE_LENGTH), NumberStyles.HexNumber)];
                    int i = 0;
                    int date;
                    List<Tuple<ulong, byte>> affected = new List<Tuple<ulong, byte>>();
                    foreach (var rev in reversions) {
                        ulong userID = UInt64.Parse(rev.Substring(rev.IndexOf("-") + 1, USER_ID_LENGTH), NumberStyles.HexNumber);
                        byte pointVal = Byte.Parse(rev.Substring(rev.IndexOf("_") + 1, SCORE_LENGTH), NumberStyles.HexNumber);
                        date = Int32.Parse(rev.Substring(rev.IndexOf(":") + 1, 6), NumberStyles.HexNumber);
                        bool multi = Games_Multiplayer.SaveFileUpdate(new Tuple<ulong, byte>(userID, pointVal), gameName, true, date);
                        if (gameName == "uno" && i == 0 && multi) {
                            SaveFiles_Mapped.AddFieldValue("FIRST-UNO", USER_SAVE_DIRECTORY + "\\" + userID.ToString() +
                        "\\" + "FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME,"-1");
                            SaveFiles_Mapped.AddFieldValue("FIRST-UNO", USER_SAVE_DIRECTORY + "\\" + userID.ToString() +
                        "\\" + UNO_SAVE_FILE_NAME, "-1");
                        }
                        Games_Singleplayer.SaveFileUpdate(new Tuple<ulong, byte>(userID, pointVal), gameName, true, date);
                        affected.Add(new Tuple<ulong, byte>(userID, pointVal));
                        i++;
                    }
                    
                    date = Int32.Parse(reversions[0].Substring(reversions[0].IndexOf(":") + 1, 6), NumberStyles.HexNumber);
                    Games_1v1.SaveFileUpdate(affected, gameName, true, date);


                    var channel = GuildCache.GetChannel(REPORT_CHANNEL_ID);
                    var embed = Bridge.ReportLoggedGame(IDString, true);

                    await channel.SendMessageAsync("", false, embed);
                    await ReplyAsync("Reversion of Game " + args.Trim() + " successful.");
                } else {
                    await ReplyAsync("Critical failure in reversion of game."); return;
                }
            } catch (Exception e) {
                await ReplyAsync(e.Message);
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Reverted game for " + Context.User.Username);
        }

        [Command("loguno", RunMode = RunMode.Async)]
        public async Task LogUno([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync("Format: `p*loguno [Game Message]`");
                return;
            }
            try {
                await Games_Multiplayer.ManualUnoLog(args);
            }catch(Exception e) {
                await ReplyAsync(e.Message);
                return;
            }
            await ReplyAsync("Successful manual log of the Uno game! <#537104363127046145>");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Uno Game for " + Context.User.Username);
        }

        [Command("forceloguno", RunMode = RunMode.Async)]
        public async Task ForceLogUno([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync("Format: `p*loguno [Game Message]`");
                return;
            }
            try {
                await Games_Multiplayer.ManualUnoLog(args,true);
            } catch (Exception e) {
                await ReplyAsync(e.Message);
                return;
            }
            await ReplyAsync("Successful manual log of the Uno game! <#537104363127046145>");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Uno Game for " + Context.User.Username);
        }

        [Command("forceforceloguno", RunMode = RunMode.Async)]
        public async Task ForceForceLogUno([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync("Format: `p*loguno [Game Message]`");
                return;
            }
            try {
                await Games_Multiplayer.ManualUnoLog(args, true);
            } catch (Exception e) {
                await ReplyAsync(e.Message);
                return;
            }
            await ReplyAsync("Successful manual log of the Uno game! <#537104363127046145>");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Uno Game for " + Context.User.Username);
        }

        [Command("logcah", RunMode = RunMode.Async)]
        public async Task LogCAH([Remainder] string args = null) {
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync("Format: `p*logcah *[List of Users]`" +
                    "\n* - Players who tied put in parentheses, separated by commas.");
                return;
            }
            try {
                await Games_Multiplayer.CAHLog(args);
            } catch (Exception e) {
                await ReplyAsync(e.Message);
                return;
            }
            await ReplyAsync("Successful manual log of the CAH game! <#537104363127046145>");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Game for " + Context.User.Username);
        }

        [Command("logonw", RunMode = RunMode.Async)]
        public async Task LogONW([Remainder] string args = null) {
            /*** Untested: 
             * Do not attempt to run this command until the appropriate key/pair values have been added to users' Fortnight and overall save files respectively.
             * Those key/pair values being:
             * POINTS, ITER, FIRST
             * ***/
            return Task.CompletedTask; //safeguard
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync("Format: `p*logonw ([List of Winners]) ([List of Losers])`" +
                    "\nexample: (Biggus, Jenna, TheLuckyRobot) (Primaski, swinub, Thembi)");
                return;
            }
            try {
                await Games_Multiplayer.ONWLog(args);
            } catch (Exception e) {
                await ReplyAsync(e.Message);
                return;
            }
            await ReplyAsync("Successful manual log of the ONW game! <#537104363127046145>");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Game for " + Context.User.Username);
        }

        [Command("logevent", RunMode = RunMode.Async)]
        public async Task LogEvent([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (Context != null) {
                if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                    await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                        "as it was designated for Point Managers in the Uno Server.");
                    return;
                }
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required fields: `[amount]` `[user]`.");
                return;
            }
            if(args.Split(' ').Count() < 2) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            string score = args.Substring(0, args.IndexOf(" "));
            args = args.Substring(args.IndexOf(" ") + 1);
            if (!Int32.TryParse(score, out int ignore)) {
                await ReplyAsync("Format: `p*logevent [amount] [user]`");
                return;
            } else if (Int32.Parse(score) > Byte.MaxValue) {
                await ReplyAsync("255 is the maximum value for an individual log. Try splitting it up.");
                return;
            }
            byte NSscore = Byte.Parse(score);
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            if (users.Count() > 1) {
                await ReplyAsync("Only one game log at a time is permitted for Event logs.");
                return;
            }
            var user = users[0];
            List<Tuple<ulong, byte>> scorer;
            Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, NSscore);
            scorer = new List<Tuple<ulong, byte>> { scorerTuple };
            try {
                Task result = Uno_Score_Tracking.Bridge.LogGame("event", scorer, Context);
                if (result.IsCompleted) {
                    if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "event")) {
                        await ReplyAsync("Successfully logged " + user.Username + "'s Event score! <#537104363127046145>");
                    } else {
                        await ReplyAsync(":exclamation: Error in updating " + user.Username + "'s save file.");
                    }
                }
            } catch (Exception e) {
                await ReplyAsync(e.Message + "\n" + e.StackTrace);
                return;
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Event for " + Context.User.Username);
        }

        [Command("logtrivia", RunMode = RunMode.Async)]
        public async Task LogTrivia([Remainder] string args = null) {
            GuildCache.IncrementCMD();
            if (!HasRole("Point Manager", (SocketGuildUser)Context.User, true) && Context.User.Id != MY_ID
                && !pmID.Contains(Context.User.Id)) {
                await ReplyAsync(":exclamation: You do not have appropriate permissions to log this game, " +
                    "as it was designated for Point Managers in the Uno Server.");
                return;
            }
            if (args == null) {
                await ReplyAsync(":exclamation: Missing required field: `user(s)`.");
                return;
            }
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(args);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            List<Tuple<ulong, byte>> scorer;
            foreach (SocketUser user in users) {
                Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, TRIVIA_POINT_VALUE);
                scorer = new List<Tuple<ulong, byte>> { scorerTuple };
                try {
                    Task result = Uno_Score_Tracking.Bridge.LogGame("trivia", scorer, Context);
                    if (result.IsCompleted) {
                        if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "trivia")) {
                            await ReplyAsync("Successfully logged " + user.Username + "'s Trivia win! <#537104363127046145>");
                        } else {
                            await ReplyAsync(":exclamation: Error in updating " + user.Username + "'s save file.");
                        }
                    }
                } catch (Exception e) {
                    await ReplyAsync(e.Message + "\n" + e.StackTrace);
                    return;
                }
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged Trivia for " + Context.User.Username);
        }

        [Command("daily", RunMode = RunMode.Async)]
        public async Task LogDaily([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (Context.Guild.Id != UNO_SERVER_ID) {
                await ReplyAsync(":exclamation: You need to be in the Uno server to do this.");
                return;
            }
            List<SocketGuildUser> users = Uno_Score_Tracking.GuildCache.InterpretUserInput(Context.User.Id.ToString());
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                await ReplyAsync(GuildCache.IsWellFormattedListOfUsers(users));
                return;
            }
            //FROM HERE
            List<Tuple<ulong, byte>> scorer;
            foreach (SocketUser user in users) {
                string playsTodayStr = SaveFiles_Mapped.SearchValue(USER_SAVE_DIRECTORY + "\\" + user.Id.ToString() + "\\" + UNO_SAVE_FILE_NAME, "PLAYSTODAY-DAILY") ?? "";
                if (!Int32.TryParse(playsTodayStr, out int ignore)) {
                    await ReplyAsync("Unknown error in determining how many games were played today.");
                    continue;
                }
                int playsToday = Int32.Parse(playsTodayStr);
                if (playsToday >= DAILY_DAILY_LIMIT) {
                    await ReplyAsync(user.Mention + ", you've already claimed your daily today! You may claim again in " + TimeUntilMidnight().TotalHours + " hours and " + 
                        TimeUntilMidnight().TotalMinutes + " minutes!");
                    continue;
                }
                Tuple<ulong, byte> scorerTuple = Tuple.Create(user.Id, DAILY_POINT_VALUE);
                scorer = new List<Tuple<ulong, byte>> { scorerTuple };
                try {
                    Task result = Uno_Score_Tracking.Bridge.LogGame("daily", scorer, Context);
                    if (result.IsCompleted) {
                        if (Uno_Score_Tracking.Games_Singleplayer.SaveFileUpdate(scorerTuple, "daily")) {
                            Byte[] b = new Byte[3];
                            random.NextBytes(b);
                            await ReplyAsync("",false,new EmbedBuilder().WithColor(b[0],b[1],b[2]).WithThumbnailUrl(Context.User.GetAvatarUrl())
                                .WithAuthor("Congratulations, " + Context.User.Mention + "!").WithDescription("You've earned " + scorerTuple.Item2.ToString() + " points! " +
                                "Come back tomorrow for more.").Build());
                            await ReplyAsync("Successfully logged " + user.Username + "'s Daily! <#537104363127046145>");
                        } else {
                            await ReplyAsync(":exlamation: Error in updating " + user.Username + "'s save file.");
                        }
                    }
                } catch (Exception e) {
                    await ReplyAsync(e.Message + "\n" + e.StackTrace);
                    return;
                }
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Logged MS for " + Context.User.Username);
        }


        public bool HasRole(string role, SocketGuildUser user, bool checkIfPartOfUnoServer = false) {
            if (checkIfPartOfUnoServer) {
                if (user.Guild.Id != UNO_SERVER_ID) {
                    return false;
                }
            }
            if (GuildCache.ExtractRoleSubsetFromUser(user, new List<string> { role }).Count() >= 1) {
                return true;
            }
            return false;
        }
    }

}
