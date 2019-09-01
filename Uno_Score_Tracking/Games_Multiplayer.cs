using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static Primbot_v._2.Uno_Score_Tracking.Defs;
using static Primbot_v._2.Uno_Score_Tracking.SaveFiles_Mapped;
using Discord.WebSocket;

namespace Primbot_v._2.Uno_Score_Tracking {
    public static class Games_Multiplayer {
        //STANDARD SCORING SYSTEM
        private static readonly int MIN_NO_OF_PLAYERS = 1;
        private static readonly bool TWO_TEAM_REQUIREMENT = true;



        public static bool SaveFileUpdate(Tuple<ulong, byte> x, string gameType, bool revert = false, int dateHex = -1) {
            string overallPath = USER_SAVE_DIRECTORY + "\\" + x.Item1.ToString() + "\\" + UNO_SAVE_FILE_NAME;
            byte fnNO = FORTNIGHT_NUMBER;
            if (dateHex != -1) {
                DetermineFortnightNumber(dateHex);
            }
            string fortnightPath = USER_SAVE_DIRECTORY + "\\" + x.Item1.ToString() + "\\" + "FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME;

            int pointVal = x.Item2;
            int iter = 1;
            if (revert) {
                iter = -1;
                pointVal *= -1;
            }
            bool res = false;
            switch (gameType) {
                case "uno": res = UnoSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "cah": res = CAHSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
            }
            if (!res) {
                return false;
            }
            AddFieldValue("POINTS-SERVER", fortnightPath, pointVal.ToString());
            AddFieldValue("POINTS-SERVER", overallPath, pointVal.ToString());
            return true;
        }

        private static bool CAHSaveFileUpdates(string overallPath, string fortnightPath, int pointValue, int iteration) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-CAH", overallPath, iter);
            AddFieldValue("ITER-CAH", fortnightPath, iter);
            AddFieldValue("POINTS-CAH", overallPath, points);
            AddFieldValue("POINTS-CAH", fortnightPath, points);
            return true;
        }

        private static bool UnoSaveFileUpdates(string overallPath, string fortnightPath, int pointValue, int iteration) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-UNO", overallPath, iter);
            AddFieldValue("ITER-UNO", fortnightPath, iter);
            AddFieldValue("POINTS-UNO", overallPath, points);
            AddFieldValue("POINTS-UNO", fortnightPath, points);
            return true;
        }

        public static Task CAHLog(string message, bool force = false) {
            byte noOfPlayers = 0;
            if (Byte.TryParse(message.Trim().Substring(0, message.IndexOf(" ")), out byte ignore)) {
                noOfPlayers = Byte.Parse(message.Trim().Substring(0, message.IndexOf(" ")));
            } else if (message.IndexOf("\n") != -1) {
                if (Byte.TryParse(message.Trim().Substring(0, message.IndexOf("\n")), out byte ignore2)) {
                    noOfPlayers = Byte.Parse(message.Trim().Substring(0, message.IndexOf("\n")));
                }
            }
            List<Tuple<SocketGuildUser, bool>> users;
            try {
                users = GuildCache.InterpretUserInputWithTies(message);
            } catch (Exception e) { throw e; }
            if (users.Where(x => x.Item1 == null).Count() >= 1) {
                string list = "`";
                foreach (var user in users) {
                    list += (user.Item1?.Username ?? "NULL") + ", ";
                }
                list += "`";
                throw new Exception(":exclamation: At least one user was invalid.\n " + list);
            }
            if (noOfPlayers == 0) {
                noOfPlayers = (byte)users.Count();
            }
            if (!force) {
                string lastTeam = GuildCache.GetTeam(users[0].Item1);
                bool qualifiable = false;
                foreach (var keyvalue in users) {
                    var player = keyvalue.Item1;
                    if (GuildCache.GetTeam(player) != lastTeam) {
                        qualifiable = true;
                        break;
                    }
                }
                if (!qualifiable) {
                    throw new Exception("Game is not qualifiable for logging (one team).");
                }
            }
            List<Tuple<ulong, byte>> orderedPairs = new List<Tuple<ulong, byte>>();

            byte maxReward = (byte)(noOfPlayers * 5);
            byte lastReward = maxReward;

            for (int currIndex = 0; currIndex < users.Count(); currIndex++) {
                var pair = users[currIndex];
                if (pair.Item2 == false) {
                    //not a tie, return to normal
                    if (maxReward - (currIndex * 5) <= 0) {
                        lastReward = 5;
                    } else {
                        lastReward = (byte)(maxReward - (currIndex * 5));
                    }
                    orderedPairs.Add(new Tuple<ulong, byte>(pair.Item1.Id, (byte)(lastReward)));
                } else {
                    //tie, use last reward
                    orderedPairs.Add(new Tuple<ulong, byte>(pair.Item1.Id, (byte)(lastReward)));
                }
            }
            Bridge.LogGame("cah", orderedPairs);
            int q = 0;
            foreach (var orderedPair in orderedPairs) {
                SaveFileUpdate(orderedPair, "cah");
                if (q == 0) {
                    string saveDir = USER_SAVE_DIRECTORY + "\\" + orderedPair.Item1.ToString() +
                        "\\" + UNO_SAVE_FILE_NAME;
                    string FNsaveDir = USER_SAVE_DIRECTORY + "\\" + orderedPair.Item1.ToString() +
                        "\\" + "FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME;
                    AddFieldValue("FIRST-CAH", saveDir, "1");
                    AddFieldValue("FIRST-CAH", FNsaveDir, "1");
                }
                q++;
            }
            return Task.CompletedTask;
        }

        public static Task ManualUnoLog(string message, bool force = false, bool manual = false, byte manualnoofplayers = 3) { 

            string playerStrArr = message;
            byte noOfPlayers = manualnoofplayers;
            if (!manual) {
                playerStrArr = "";
                noOfPlayers = 0;
                if (Byte.TryParse(message.Trim().Substring(0, message.IndexOf(" ")), out byte ignore)) {
                    noOfPlayers = Byte.Parse(message.Trim().Substring(0, message.IndexOf(" ")));
                } else if (message.IndexOf("\n") != -1) {
                    if (Byte.TryParse(message.Trim().Substring(0, message.IndexOf("\n")), out byte ignore2)) {
                        noOfPlayers = Byte.Parse(message.Trim().Substring(0, message.IndexOf("\n")));
                    }
                }
                for (int i = 1; i <= 100; i++) {
                    if (message.IndexOf(i + ". ") != -1 && message.IndexOf((i + 1) + ". ") != -1) {
                        int start = message.IndexOf((i) + ". ") + ((i + 1) + ". ").Length;
                        int end = message.IndexOf(i + 1 + ". ");
                        var captureArea = message.Substring(start, end - start);
                        playerStrArr += captureArea + ",";
                        message = message.Substring(message.IndexOf(i + 1 + ". "));
                    } else if (message.IndexOf(i + ". ") != -1) {
                        int start = message.IndexOf((i) + ". ") + ((i + 1) + ". ").Length;
                        if (message.Contains("\n")) {
                            if (start < message.IndexOf("\n")) {
                                var captureArea = message.Substring(start, message.IndexOf("\n") - start);
                                playerStrArr += captureArea;
                            }
                            break;
                        } else if (message.Contains("This game lasted")) {
                            var captureArea = message.Substring(start, message.IndexOf("This game lasted") - start);
                            playerStrArr += captureArea;
                            break;
                        } else {
                            var captureArea = message.Substring(start);
                            playerStrArr += captureArea;
                            break;
                        }
                    }
                }
                playerStrArr = playerStrArr.Replace("\n", "");
                if (playerStrArr == "" || ((playerStrArr.Count(x => x == ',') < MIN_NO_OF_PLAYERS - 1) && noOfPlayers == 0)) {
                    //throw new Exception("Minimum number of players was not met for logging.");
                }
            }
            var users = GuildCache.InterpretUserInput(playerStrArr);
            if (GuildCache.IsWellFormattedListOfUsers(users) != "") {
                string list = "`";
                foreach(var user in users) {
                    list += (user?.Username ?? "NULL") + ", ";
                }
                list += "`";
                throw new Exception("Malformatted set of users: " + list);
            }
            List<Player> players = new List<Player>();
            foreach (var user in users) {
                players.Add(new Player(user.Id.ToString(), user.Username, user.Discriminator, true, 1, 0, GuildCache.GetTeam(user) ?? ""));
            }
            if (noOfPlayers == 0) {
                noOfPlayers = (byte)players.Count();
                string x = "hi";
            }
            players = DeterminePointDistribution(players, noOfPlayers);
            if (!force) {
                if (!IsGameQualifable(players)) {
                    throw new Exception("Each player was of the same team, rendering the game unqualifiable.");
                }
            }
            List<Tuple<ulong, byte>> orderedPairs = new List<Tuple<ulong, byte>>();
            foreach (Player p in players) {
                orderedPairs.Add(new Tuple<ulong, byte>(UInt64.Parse(p.ID), (byte)p.pointsEarned));
            }
            Bridge.LogGame("uno", orderedPairs);
            int q = 0;
            foreach (var orderedPair in orderedPairs) {
                SaveFileUpdate(orderedPair, "uno");
                if (q == 0) {
                    string saveDir = USER_SAVE_DIRECTORY + "\\" + players[0].ID.ToString() +
                        "\\" + UNO_SAVE_FILE_NAME;
                    string FNsaveDir = USER_SAVE_DIRECTORY + "\\" + players[0].ID.ToString() +
                        "\\" + "FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME;
                    AddFieldValue("FIRST-UNO", saveDir, "1");
                    AddFieldValue("FIRST-UNO", FNsaveDir, "1");
                    if (Int32.Parse(SearchValue(saveDir, "HIGH-UNO")) < players[0].pointsEarned) {
                        SetFieldValue("HIGH-UNO", saveDir, players[0].pointsEarned.ToString());
                    }
                    if (Int32.Parse(SearchValue(FNsaveDir, "HIGH-UNO")) < players[0].pointsEarned) {
                        SetFieldValue("HIGH-UNO", FNsaveDir, players[0].pointsEarned.ToString());
                    }
                }
                q++;
            }
            return Task.CompletedTask;
        }

        public static void UnoLog(string JSON_OUTPUT_PATH) {
            List<Player> players = JSONFileGenPlayers(JSON_OUTPUT_PATH);
            players = DeterminePlayerTeams(players);
            Console.WriteLine("Recording game with following players:");
            foreach (Player p in players) { Console.WriteLine(p.name + ": " + p.team); }
            if (IsGameQualifable(players)) {
                players = DeterminePointDistribution(players);
            } else {
                GuildCache.Uno_Cache.GetTextChannel(REPORT_CHANNEL_ID).SendMessageAsync("**Uno Game with incorrect number of players was attempted to be logged. Please check manually for now.**");
            }
            List<Tuple<ulong, byte>> orderedPairs = new List<Tuple<ulong, byte>>();
            foreach (Player p in players) {
                orderedPairs.Add(new Tuple<ulong, byte>(UInt64.Parse(p.ID), (byte)p.pointsEarned));
            }
            Bridge.LogGame("uno", orderedPairs);
        }

        private static bool IsGameQualifable(List<Player> players) {
            //if (players.Count < MIN_NO_OF_PLAYERS || players.Count > 7) {
            //Console.WriteLine("Game logged has " + players.Count() + " and is not yet rankable automatically.");
            //return false;
            //}
            //team requirements
            if (TWO_TEAM_REQUIREMENT) {
                int i = 0;
                string previousTeam = players[0].team;
                foreach (Player p in players) {
                    i++;
                    if (p.team != previousTeam) {
                        return true; //at least one other team
                    }
                    previousTeam = players[i - 1].team;
                }
                return false;
            }
            return false;
        }

        private static List<Player> JSONFileGenPlayers(string JSON_OUTPUT_PATH) {
            string unparsedJSONcon;
            if (File.Exists(JSON_OUTPUT_PATH)) {
                try {
                    string[] temp = File.ReadAllLines(JSON_OUTPUT_PATH);
                    unparsedJSONcon = string.Join("", temp);
                } catch (Exception e) {
                    throw e;
                }
            } else {
                return null;
            }
            List<Player> result = new List<Player>();
            try {
                var stats = JsonConvert.DeserializeObject<dynamic>(unparsedJSONcon);
                int i = 1;
                foreach (var player in stats.finished) {
                    result.Add(new Player(player.id.ToString(), player.name.ToString(),
                        player.discriminator.ToString(), true, (byte)i, (int)player.cardsPlayed.Value));
                    i++;
                }
                i = 1;
                foreach (var player in stats.quit) {
                    result.Add(new Player(player.id.ToString(), player.name.ToString(),
                        player.discriminator.ToString(), false, (byte)i, (int)player.cardsPlayed.Value));
                    i++;
                }
                //glitch has been observed where duplicates will occur - same player in won and lost. this preserves only the won.
                List<Player> distinctPlayers = result.GroupBy(p => p.ID).Select(g => g.First()).ToList();
                return distinctPlayers;
            } catch (Exception e) {
                throw e;
            }
        }

        private static List<Player> DeterminePlayerTeams(List<Player> o) {

            foreach (Player player in o) {
                ulong ID;
                try {
                    ID = UInt64.Parse(player.ID);
                } catch {
                    continue;
                }
                string rolesubs = GuildCache.GetTeam(ID) ?? null;
                if (rolesubs == null) {
                    player.team = "none";
                    continue;
                }
                player.team = rolesubs;
            }
            return o;
        }


        private static List<Player> DeterminePointDistribution(List<Player> players, int noOfPlayersTotal = 0) {
            int noOfPlayersWon = 0;
            foreach (var player in players) {
                if (player.won) {
                    noOfPlayersWon++;
                }
            }
            if (noOfPlayersTotal == 0) {
                noOfPlayersTotal = players.Count();
            }
            List<int> rewards = new List<int> { 5, 10, 15 }; //formerly: { 1, 5, 10 }
            int lastReward = 15;

            while ((noOfPlayersTotal - 3) > 0) {
                lastReward += 5;
                rewards.Add(lastReward);
                noOfPlayersTotal--;
            }

            foreach (var player in players) {
                if (player.won) {
                    player.pointsEarned = rewards[rewards.Count() - 1];
                    rewards.RemoveAt(rewards.Count() - 1);
                }
            }
            return players;
        }


        private class Player {
            public string ID;
            public string name;
            public bool won;
            public bool qualifiable; //to be used when stupid cat adds a var for 7+ player games
            public byte ranking;
            public int noOfCardsPlayed;
            public int pointsEarned;
            public string team;


            public Player(string ID, string username, string discrim, bool won, byte ranking, int cardsPlayed = 0, string team = "") {
                this.ID = ID;
                this.name = username + "#" + discrim;
                this.won = won;
                this.ranking = ranking;
                this.noOfCardsPlayed = cardsPlayed;
                if (team == "") {
                    team = GuildCache.GetTeam(GuildCache.GetUserByID(UInt64.Parse(ID))) ?? "?";
                } else {
                    this.team = team;
                }
            }
        }

    }
}
