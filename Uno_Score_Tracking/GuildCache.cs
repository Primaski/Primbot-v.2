using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using static Primbot_v._2.Uno_Score_Tracking.Defs;
using System.IO;
using System.Globalization;

namespace Primbot_v._2.Uno_Score_Tracking {
    public class GuildCache {

        public static List<SocketGuild> connectedGuilds = new List<SocketGuild>();
        public static SocketGuild Uno_Cache { get; private set; } //make private later
        public static SocketGuild Magi_Cache { get; private set; }
        public static SocketGuild Poke_Cache { get; private set; }

        public static int GREEN_TEAM_SCORE = 0;
        public static int YELLOW_TEAM_SCORE = 0;
        public static int RED_TEAM_SCORE = 0;
        public static int BLUE_TEAM_SCORE = 0;
        public static ulong TEAM_ONE_CHANNEL_ID = 475433210076069888;
        public static ulong TEAM_TWO_CHANNEL_ID = 475431960089591811;
        public static ulong GREEN_TEAM_CHANNEL_ID = 575531406382333962;
        public static ulong YELLOW_TEAM_CHANNEL_ID = 575531446434004992;
        public static ulong RED_TEAM_CHANNEL_ID = 575531354817822720;
        public static ulong BLUE_TEAM_CHANNEL_ID = 575531485570924573;
        public static List<ulong> yahtzeebois = new List<ulong>();

        public static List<Tuple<string, string>> x = null;


        private static List<AwaitedMessage> AWAITED_MESSAGES = new List<AwaitedMessage>();


        //returns the index of the array it was stored at
        public static int StoreAwaitedMessage(AwaitedMessage message) {
            AWAITED_MESSAGES.Add(message);
            return AWAITED_MESSAGES.Count()-1;
        }

        public static AwaitedMessage GetAwaitedMessage(int indexOfMessage) {
            if(indexOfMessage > AWAITED_MESSAGES.Count() - 1 || indexOfMessage < 0) {
                return null;
            }
            return AWAITED_MESSAGES[indexOfMessage];
        }

        public static bool DeleteAwaitedMessage(int indexOfMessage) {
            if (indexOfMessage > AWAITED_MESSAGES.Count() - 1 || indexOfMessage < 0) {
                return false;
            }
            AWAITED_MESSAGES.RemoveAt(indexOfMessage);
            return true;
        }

        public static void InitializeUnoServer(SocketGuild guild) {
            if (guild.Id == UNO_SERVER_ID) {
                Uno_Cache = guild;
            }
            return;
        }

        public static void InitializeMyServer(SocketGuild guild) {
            if (guild.Id == MAGI_SERVER_ID) {
                Magi_Cache = guild;
            }
            return;
        }

        public static SocketTextChannel GetChannel(ulong ID) {
            return Uno_Cache.GetTextChannel(ID) ?? null;
        }

        public static string GetUsername(ulong id) {
            string username = Uno_Cache.GetUser(id)?.Username;
            if(username == null) {
                username = SaveFiles_Mapped.SearchCachedUsername(id);
            }
            return username;
        }

        public static string GetTeam(ulong id) {
            List<string> LOCAL_UNO_SERVER_TEAMS = new List<string>();
            foreach(var team in UNO_SERVER_TEAMS) {
                LOCAL_UNO_SERVER_TEAMS.Add(team.ToString());
            }
            var x = ExtractRoleSubsetFromUser(GetUserByID(id), LOCAL_UNO_SERVER_TEAMS, true);
            if (x.Count() == 0) {
                return null;
            }
           return x[0];

        }

        public static string GetTeam(SocketGuildUser user) {
            List<string> LOCAL_UNO_SERVER_TEAMS = new List<string>();
            foreach (var team in UNO_SERVER_TEAMS) {
                LOCAL_UNO_SERVER_TEAMS.Add(team.ToString());
            }
            var x = ExtractRoleSubsetFromUser(user, LOCAL_UNO_SERVER_TEAMS, true);
            if (x.Count() == 0) {
                return null;
            }
            return x[0];
        }

        public static List<string> ExtractRoleSubsetFromUser(SocketGuildUser x, List<string> soughtRoles, bool byID = false) {
            var userRoles = x?.Roles;
            List<string> result = new List<string>();
            if(userRoles == null) {
                return result;
            }
            foreach (SocketRole role in userRoles) {
                if (!byID) {
                    if (soughtRoles.Contains(role.Name)) {
                        result.Add(role.Name);
                    }
                } else {
                    if (soughtRoles.Contains(role.Id.ToString())) {
                        result.Add(role.Id.ToString());
                    }
                }
            }
            return result;
        }


        public static bool HasRole(SocketGuildUser x, string roleName, bool byID = false) {
            List<string> roles = ExtractRoleSubsetFromUser(x, new List<string> { roleName }, byID);
            if (roles.Count() == 0) {
                return false;
            }
            return true;
        }

        public static List<string> ExtractRoleSubsetFromUser(ulong UserID, List<string> soughtRoles, bool byID = false) {
            var userRoles = Uno_Cache.GetUser(UserID)?.Roles;
            List<string> result = new List<string>();
            if (userRoles == null) {
                return result;
            }
            foreach (SocketRole role in userRoles) {
                if (!byID) {
                    if (soughtRoles.Contains(role.Name)) {
                        result.Add(role.Name);
                    }
                } else {
                    if (soughtRoles.Contains(role.Id.ToString())) {
                        result.Add(role.Id.ToString());
                    }
                }
            }
            return result;
        }

        internal static void InitializePokeCollectors(SocketGuild guild) {
            if (guild.Id == 597469488778182656) {
                Poke_Cache = guild;
            }
        }

        /// <summary>
        /// Whether the user passes in a username, ID, ping etc. this method will attempt to resolve a
        /// mapping to a socket user.
        /// </summary>
        /// <example>"USERNAME, PING, USERNAME, ID, USERNAME"</example>
        /// <returns>Username repeats if several interpretations. Bool represents if players are at "same
        /// "level", as in CAH, for a tie. Ranked set to false, equal standings (default) set to true.</returns>
        public static List<SocketGuildUser> InterpretUserInput(string expectedUsers, SocketGuild guild = null) {
            List<SocketGuildUser> users = new List<SocketGuildUser>();
            Regex IDcapture = new Regex(@"((?:\D|^|!|@|<)\d{16,18}(?: |,|\(|>|$))");
            if(guild == null) {
                guild = Uno_Cache;
            }

            if (!expectedUsers.Contains(",")) {
                expectedUsers.Trim();
                if (IDcapture.IsMatch(expectedUsers)) {
                    ulong ID = UInt64.Parse(Regex.Match(IDcapture.Match(expectedUsers).Value, @"\d{16,18}").Value);
                    users.Add(GetUserByID(ID, guild));
                } else {
                    if(expectedUsers.IndexOf("#") != -1) {
                        expectedUsers.Substring(0, expectedUsers.IndexOf("#"));
                    }
                    users.Add(GetUserByUsername(expectedUsers, guild));
                }
                return users;
            }

            string[] userList = expectedUsers.Split(',');
            foreach(string user in userList) {
                user.Trim();
                if (IDcapture.IsMatch(user)) {
                    ulong ID = UInt64.Parse(Regex.Match(IDcapture.Match(user).Value, @"\d{16,18}").Value);
                    users.Add(GetUserByID(ID));
                } else {
                    string userm = user;
                    if (user.IndexOf("#") != -1) {
                        userm = user.Substring(0, user.IndexOf("#"));
                    }
                    if (userm.ToLower() == "null") {
                        users.Add(GetUserByID(447487261760552982));
                        continue;
                    }
                    users.Add(GetUserByUsername(userm));
                }
            }
            return users;
        }

        public static void LogCMD() {
            string date = "";
            if (DateTime.Now.TimeOfDay.TotalMinutes > 180) {
                date = SaveFiles_Sequences.GenerateDateHex().Substring(0, 3);
            } else {
                date = (Int32.Parse(SaveFiles_Sequences.GenerateDateHex().Substring(0, 3), NumberStyles.HexNumber) - 0x1).ToString();
            }
            SaveFiles_Mapped.AddLine(COMMANDS_BY_DATE_DIRECTORY, date, cmdEx.ToString());
            cmdEx = 0;
            GuildCache.IncrementCMD(0);
        }

        public static bool NewDay(SocketTextChannel channel) {
            channel.SendMessageAsync("`Resetting user daily limits...`");
            List<string> PLAYSTODAY_TYPES = new List<string>();
            foreach (var keyValue in UnoSaveFields) {
                if (keyValue.Key.StartsWith("PLAYSTODAY")) {
                    PLAYSTODAY_TYPES.Add(keyValue.Key);
                }
            }
            var directories = Directory.EnumerateDirectories(USER_SAVE_DIRECTORY);
            foreach (var user in directories) {
                foreach (var playsToday in PLAYSTODAY_TYPES) {
                    try {
                        SaveFiles_Mapped.ModifyFieldValue(playsToday, user + "\\" + UNO_SAVE_FILE_NAME, "0");
                    }catch{ /*file did not exist, move on*/ }
                }
            }
            channel.SendMessageAsync("`Storing metadata statistics...`");
            GuildCache.LogCMD();
            channel.SendMessageAsync("`Complete.`");
            return true;
        }

        public static void IncrementCMD(int x = -1) {
            cmdEx++;
            if (File.Exists(COMMANDS_TODAY_BACKUP_DIRECTORY)) {
                File.Delete(COMMANDS_TODAY_BACKUP_DIRECTORY);
            }
                using (StreamWriter sw = new StreamWriter(COMMANDS_TODAY_BACKUP_DIRECTORY)) {
                if (x == -1) {
                    sw.WriteLine(cmdEx);
                } else {
                    sw.WriteLine(x);
                }
                sw.Close();
                }
            
            return;
        }
        internal static void SetTeamScore(int teamScore, byte teamNumber) {
            switch(teamNumber) {
                case 1:
                Uno_Cache.GetVoiceChannel(RED_TEAM_CHANNEL_ID).ModifyAsync(x => x.Name = "Team Red: " + teamScore.ToString());
                    break;
                case 2:
                Uno_Cache.GetVoiceChannel(YELLOW_TEAM_CHANNEL_ID).ModifyAsync(x => x.Name = "Team Yellow: " + teamScore.ToString());
                    break;
                case 3:
                    Uno_Cache.GetVoiceChannel(GREEN_TEAM_CHANNEL_ID).ModifyAsync(x => x.Name = "Team Green: " + teamScore.ToString());
                    break;
                case 4:
                    Uno_Cache.GetVoiceChannel(BLUE_TEAM_CHANNEL_ID).ModifyAsync(x => x.Name = "Team Blue: " + teamScore.ToString());
                    break;
                default:
                    break;
            }
            return;
        }

        //returns the index of the awaited message
        public static int SearchAwaitedMessage(ulong serverID, ulong channelID, ulong userID, string message) {
            if(AWAITED_MESSAGES.Count() == 0) {
                return -1;
            }
            for(int i = 0; i < AWAITED_MESSAGES.Count() - 1; i++) {
                if(AWAITED_MESSAGES[i].serverID == serverID && AWAITED_MESSAGES[i].channelID == channelID) {
                    if(AWAITED_MESSAGES[i].userID == userID) {
                        //if(message.StartsWith())
                    }
                }
            }
            return -1;
        }

        //bool TRUE if tied with person ABOVE
        public static List<Tuple<SocketGuildUser,bool>> InterpretUserInputWithTies(string expectedUsers) {
            List<Tuple<SocketGuildUser,bool>> users = new List<Tuple<SocketGuildUser,bool>>();
            Regex IDcapture = new Regex(@"((?:\D|^|!|@|<)\d{16,18}(?: |,|\(|>|$))");

            if (!expectedUsers.Contains(",")) {
                expectedUsers.Trim();
                if (IDcapture.IsMatch(expectedUsers)) {
                    ulong ID = UInt64.Parse(Regex.Match(IDcapture.Match(expectedUsers).Value, @"\d{16,18}").Value);
                    users.Add(Tuple.Create(GetUserByID(ID),false));
                } else {
                    if (expectedUsers.IndexOf("#") != -1) {
                        expectedUsers.Substring(0, expectedUsers.IndexOf("#"));
                    }
                    var userTuple = Tuple.Create(GetUserByUsername(expectedUsers), false);
                    users.Add(userTuple);
                }
                return users;
            }

            string[] userList = expectedUsers.Split(',');
            bool open = false;
            bool preparedToClose = false;
            bool firstParensUser = false;
            foreach (string user in userList) {
                string userm = user.Trim();
                if (userm.StartsWith("(")) {
                    if(open == true) {
                        throw new Exception("Nested parentheses are impossible.");
                    }
                    open = true; firstParensUser = true;
                    userm = userm.Substring(1);
                }
                if (userm.EndsWith(")")) {
                    if(open == false) {
                        throw new Exception("No open parens to match.");
                    }
                    preparedToClose = true;
                    userm = userm.Substring(0, userm.IndexOf(")"));
                }
                userm.Trim();
                if (IDcapture.IsMatch(userm)) {
                    ulong ID = UInt64.Parse(Regex.Match(IDcapture.Match(userm).Value, @"\d{16,18}").Value);
                    if (open && !firstParensUser) { 
                        //only if in parens AND we're not referring to the first one
                        users.Add(Tuple.Create(GetUserByID(ID), true));
                    } else {
                        users.Add(Tuple.Create(GetUserByID(ID), false));
                    }
                } else {
                    if (userm.IndexOf("#") != -1) {
                        userm = userm.Substring(0, user.IndexOf("#"));
                    }
                    if (open && !firstParensUser) {
                        if(userm.ToLower() == "null") {
                            users.Add(Tuple.Create(GetUserByID(447487261760552982),true));
                            continue;
                        }
                        users.Add(Tuple.Create(GetUserByUsername(userm),true));
                    } else {
                        if (userm.ToLower() == "null") {
                            users.Add(Tuple.Create(GetUserByID(447487261760552982), false));
                            continue;
                        }
                        users.Add(Tuple.Create(GetUserByUsername(userm), false));
                    }
                }
                if (preparedToClose) {
                    preparedToClose = false;
                    open = false;
                }
                if (firstParensUser) {
                    firstParensUser = !firstParensUser;
                }
            }
            return users;
        }

        public static string IsWellFormattedListOfUsers(List<SocketGuildUser> users) {
            string format = "`p*logX [USER]` OR `p*logX [USER], [USER], [USER]...`\n" +
    "whereby `X` is the type of game (like `ms` for Minesweeper), \n`[USER]` is their username, " +
    "ping, or ID, \nand `...` refers to any number of users <= 10";
            if (users?.Any() == false) {
                return (":exclamation: No valid users received. Ensure this format is used:\n" + format);
            } else if (users.Where(x => x == null).Count() >= 1) {
                return (":exclamation: At least one user was invalid. Ensure this format is used:\n" + format);
            }
            return "";
        }

        public static SocketGuildUser GetUserByID(ulong ID, SocketGuild guild = null) {
            if(guild == null) {
                guild = Uno_Cache;
            }
            SocketGuildUser user = guild.GetUser(ID);
            if(user == null) {
                return null;
            } else {
                return user;
            }
        }

        public static SocketGuildUser GetUserByUsername(string username, SocketGuild guild = null) {
            if (guild == null) {
                guild = Uno_Cache;
            }
            username = username.ToLower().Trim();
            var guildUsers = guild.Users;
            List<SocketGuildUser> possibleMatch = new List<SocketGuildUser>();
            foreach(SocketGuildUser user in guildUsers) {
                if (!user.IsBot) {
                    if (user.Username.ToLower() == username) {
                        return user;
                    }
                    if (user.Username.ToLower().StartsWith(username)) {
                        possibleMatch.Add(user);
                    }
                }
            }
            if(possibleMatch.Count() == 1) {
                return possibleMatch[0];
            }
            return null;
        }

        /// <summary>
        /// Bracketed elements return together - unbracketed are delimited by commas. Bool - equal level.
        /// </summary>
        internal static List<Tuple<string,bool>> GetElementsInUserList(string expr) {
            List<Tuple<string,bool>> result = new List<Tuple<string,bool>>();
            string[] splits;
            if (expr.Contains('(')) {
                string remainder = expr;
                while(remainder.Trim() != "") {
                    short openbracketIndex = (short)remainder.IndexOf('(');
                    short closedbracketIndex = (short)remainder.IndexOf(')');
                    if(openbracketIndex == -1 && closedbracketIndex == -1) { //end of str
                        break;
                    }
                    if((openbracketIndex > closedbracketIndex) || (openbracketIndex == -1)) { //malformatted
                        throw new Exception("Malformatted brackets when attempting to parse.\n" +
                            "Open bracket index number: " + openbracketIndex + "\nClosed bracket index" +
                            "number: " + closedbracketIndex);
                    }
                    splits = remainder.Substring(0, openbracketIndex).Split(',');
                    foreach (string split in splits) {
                        result.Add(Tuple.Create(split.Trim(),false));
                    }
                    splits = remainder.Substring(openbracketIndex + 1, closedbracketIndex - (openbracketIndex + 1)).Split(',');
                    foreach (string split in splits) {
                        result.Add(Tuple.Create(split.Trim(), true));
                    }
                    remainder = expr.Substring(closedbracketIndex+1);
                }
                //remainder doesn't have { }
                splits = expr.Split(',');
                foreach (string split in splits) {
                    result.Add(Tuple.Create(split.Trim(), false));
                }
            } else {
                splits = expr.Split(',');
                foreach(string split in splits) {
                    result.Add(Tuple.Create(split.Trim(),false));
                }
            }
            return result;
        }
    }

    public class AwaitedMessage {
        public ulong serverID;
        public ulong channelID;
        public ulong userID;
        public List<string> awaitedResponses;
        public string receivedResponse;
        public Status status = Status.Awaiting;
        public ResponseType responseType;

        public enum Status {
            Awaiting, Received
        }

        public enum ResponseType {
            Any, Match, StartsWith
        }

        public AwaitedMessage(ulong serverID, ulong channelID, ulong userID, List<string> awaitedResponses = null, ResponseType responseType = ResponseType.Match) {
            this.serverID = serverID;
            this.channelID = channelID;
            this.userID = userID;
            if(awaitedResponses == null) {
                responseType = ResponseType.Any;
                return;
            }
            this.responseType = responseType;
            this.awaitedResponses = awaitedResponses;
        }

        //public MatchesAwaitedResponse(string message) {
            //if()
        //}
    }
}
