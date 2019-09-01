using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using Discord.WebSocket;
using static Primbot_v._2.Uno_Score_Tracking.SaveFiles_GlobalVariables;

namespace Primbot_v._2.Uno_Score_Tracking {
    public static class SaveFiles_Mapped {

        /// <summary>
        /// For files that have fields of the format: [fieldname]:VALUE.
        /// </summary>
        public static string SetFieldValue(string fieldname, string filepath, string value) {
            return ModifyFieldValue(fieldname, filepath, value);
        }


        /// <summary>
        /// For files that have fields of the format: [fieldname]:VALUE. Value must be an integer.
        /// </summary>
        /// <param name="intBase">Replace what you would put in .ToString(). For example, if hex, "X". If hex with 4 digits, "X4".</param>
        public static string AddFieldValue(string fieldname, string filepath, string value, string intBase = "", bool permitNegatives = false) {
            return ModifyFieldValue(fieldname, filepath, value, '+', intBase, permitNegatives);
        }

        /// <summary>
        /// Modify mapped [key,value] save files.
        /// </summary>
        /// <param name="operation">Must be "." for a non-int literal.</param>
        /// <returns></returns>
        public static string ModifyFieldValue(string fieldname, string filepath, string value, char operation = '.', string intBase = "", bool permitNegatives = false) {
            string resultVal = "";
            string stdErrorMsg = "Attempted to set field value: " + value + " at " + fieldname + " in " + filepath + " with operation " + operation + ". ";
            char[] acceptedFieldOps = { '.', '+' };
            if (!acceptedFieldOps.Contains(operation)) {
                throw new Exception(stdErrorMsg + "Invalid operation was provided.");
            }
            Console.WriteLine(filepath);
            if (File.Exists(filepath)) {
                //Console.WriteLine("exists!");
                int lineToEdit = -1;
                string[] fileLines;
                fileLines = File.ReadAllLines(filepath);
                for (int i = 0; i < fileLines.Length; i++) {
                    if (fileLines[i].StartsWith(fieldname + ":")) {
                        lineToEdit = i;
                        break;
                    }
                }
                if (lineToEdit == -1)
                    return "";
                if (operation == '.') {
                    resultVal = value;
                    fileLines[lineToEdit] = fieldname + ":" + value;
                } else {
                    //HEXADECIMAL INT
                    string originVal = fileLines[lineToEdit].Replace(fieldname + ":", "").TrimEnd();
                    if (intBase.Contains("X")) {
                        if (Int32.TryParse(originVal, NumberStyles.HexNumber, null, out int ignore) && Int32.TryParse(value, NumberStyles.HexNumber, null, out int ignore2)) {
                            int original = Int32.Parse(originVal, NumberStyles.HexNumber);
                            int differential = Int32.Parse(value, NumberStyles.HexNumber);
                            if ((original + differential) < 0 && !permitNegatives) {
                                throw new Exception(stdErrorMsg + "A negative value was returned, and was rejected by the handler.");
                            }
                            original += differential;
                            resultVal = original.ToString(intBase);
                            fileLines[lineToEdit] = fieldname + ":" + original.ToString(intBase);
                        } else {
                            throw new Exception(stdErrorMsg + "Operation was meant for an int, but either " + originVal + " or " + value + " was not parsable.");
                        }
                    } else {
                        //DECIMAL
                        if (Int32.TryParse(originVal, out int ignore) && Int32.TryParse(value, out int ignore2)) {
                            int original = Int32.Parse(originVal);
                            int differential = Int32.Parse(value);
                            original += differential;
                            resultVal = original.ToString();
                            fileLines[lineToEdit] = fieldname + ":" + original.ToString();
                        } else {
                            throw new Exception(stdErrorMsg + "Operation was meant for an int, but either " + originVal + " or " + value + " was not parsable.");
                        }
                    }
                }

                File.WriteAllLines(filepath, fileLines);

                //leaderboard
                if (filepath.Contains(USER_SAVE_DIRECTORY) && !fieldname.StartsWith("LIT")) {
                    string userID = new DirectoryInfo(Path.GetDirectoryName(filepath)).Name;
                    if (filepath.Contains("\\FN")) {
                        UpdateLeaderboardFile(fieldname, FORTNIGHT_NUMBER, null, new Dictionary<string, string> { { userID, resultVal } });
                    } else {
                        UpdateLeaderboardFile(fieldname, -1, null,
                            new Dictionary<string, string> { { userID, resultVal } });
                    }
                    
                }
                return fileLines[lineToEdit].Substring(fileLines[lineToEdit].IndexOf(":") + 1);
            }
            throw new FileNotFoundException();
        }

        /// <summary>
        /// If file doesn't exist, will generate one
        /// </summary>
        public static bool CreateMappedSaveFileFields(string filepath, Dictionary<string, string> mapping) {
            string[] fields = new string[mapping.Count];
            int i = 0;
            foreach (KeyValuePair<string, string> entry in mapping) {
                fields[i] = entry.Key + ":" + entry.Value;
                i += 1;
            }
            try {
                if (!File.Exists(filepath)) {
                    File.WriteAllLines(filepath, fields);
                } else {
                    File.AppendAllLines(filepath, fields);
                }
            } catch (Exception e) {
                throw e;
            }
            return true;
        }

        /// <summary>
        /// To be used to track number of wins in the Uno Server as well. Local save files should be created:
        /// Fortnight specific, and overall.
        /// </summary>
        /// <returns></returns>
        public static bool CreateNewGameIterationSaveLog(string filepath) {
            Dictionary<string, string> x = new Dictionary<string, string>();
            for (short i = 0; i <= Byte.MaxValue; i++) {
                x.Add(i.ToString("X2"), "00000");
            }
            return CreateMappedSaveFileFields(filepath, x);
        }

        public static int GetKeyLineNumber(string path, string key) {
            if (!File.Exists(path)) {
                return -1;
            }
            string[] fileLines;
            fileLines = File.ReadAllLines(path);
            for (int i = 0; i < fileLines.Length; i++) {
                if (fileLines[i].StartsWith(key + ":")) {
                    return i;
                }
            }
            return -1;
        }

        public static bool CreateUserSaveFolder(ulong SnowflakeID, ulong GuildID, string username = "", string UnoTeam = "", bool isBot = false) {
            bool unoServer = false;
            if (GuildID == UNO_SERVER_ID) {
                unoServer = true;
                if (UnoTeam == "")
                    return false;
            }
            if (isBot) {
                return false;
            }
            string directory = USER_SAVE_DIRECTORY + "\\" + SnowflakeID;
            if (Directory.Exists(directory)) {
                return false;
            }
            try {
                Directory.CreateDirectory(directory);
            } catch (Exception e) {
                throw e;
            }
            CreateMappedSaveFileFields(directory + "\\" + DEFAULT_SAVE_FILE_NAME, DefaultSaveFields);
            ModifyFieldValue("LIT-JoinedGuild", directory + "\\" + DEFAULT_SAVE_FILE_NAME, GuildID.ToString());
            if (username != "") {
                ModifyFieldValue("LIT-CachedUsername", directory + "\\" + DEFAULT_SAVE_FILE_NAME, username);
            }
            if (unoServer) {
                CreateMappedSaveFileFields(directory + "\\" + UNO_SAVE_FILE_NAME, UnoSaveFields);
                CreateMappedSaveFileFields(directory + "\\FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME, UnoSaveFields);
                ModifyFieldValue("LIT-TEAM", directory + "\\" + DEFAULT_SAVE_FILE_NAME, UnoTeam);
                ModifyFieldValue("LIT-TEAM", directory + "\\FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME, UnoTeam);
            }
            return true;
        }

        public static string SearchCachedTeam(ulong ID) {
            if(ID == 299578425390268416) {
                Console.WriteLine("here");
            }
            string res = SearchMappedSaveFile(USER_SAVE_DIRECTORY + "\\" + ID.ToString() + "\\" + UNO_SAVE_FILE_NAME, "LIT-TEAM") ?? "-";
            if(res == "-") {
                return null;
            }
            return res;
        }

        public static string SearchCachedUsername(ulong ID) {
            string res = SearchMappedSaveFile(USER_SAVE_DIRECTORY + "\\" + ID.ToString() + "\\" + DEFAULT_SAVE_FILE_NAME, "LIT-CachedUsername") ?? "-";
            if (res == "-") {
                return null;
            }
            return res;
        }

        /// <summary>
        /// Returns value for a key.
        /// </summary>
        public static string SearchMappedSaveFile(string path, string key) {
            if (!File.Exists(path)) {
                throw new FileNotFoundException();
            }
            using (StreamReader sr = new StreamReader(path)) {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    if (line.StartsWith(key + ":")) {
                        return line.Substring(line.IndexOf(":") + 1);
                    }
                }
            }
            return null;
        }

        public static void NewLineMappedSaveFile(string path, string key, string val) {
            using (StreamWriter sw = File.AppendText(path)) {
                sw.WriteLine(key + ":" + val);
                sw.Close();
            }
            return;
        }

        public static SavedProfile CreateSavedProfileObject(ulong ID) {
            string path = USER_SAVE_DIRECTORY + "\\" + ID.ToString() + "\\" + UNO_SAVE_FILE_NAME;

            string[] result;
            if (File.Exists(path)) {
                result = File.ReadAllLines(path);
                SavedProfile profile = new SavedProfile(result, ID);
                return profile;
            }
            return null;
        }

        /// <summary>
        /// Assumes well-formatted.
        /// </summary>
        public static SavedLeaderboard CreateLeaderboardObject(string key, short fortnightNo = -1, ulong teamspecific = 0) {
            string path;
            if (fortnightNo == -1) {
                path = LEADERBOARD_DIRECTORY + "\\" + key + ".txt";
            } else {
                path = LEADERBOARD_DIRECTORY + "\\" + "FN" + fortnightNo.ToString() + "_" + key + ".txt";
            }
            if (!File.Exists(path)) {
                if(!CreateLeaderboardFile(key, fortnightNo)) {
                    return null;
                }
            }
            string[] result = File.ReadAllLines(path);

            SavedLeaderboard.Scoring type = SavedLeaderboard.Scoring.Points;
            if (key.StartsWith("ITER") || key.StartsWith("FIRST")) {
                type = SavedLeaderboard.Scoring.Wins;
            }
            if (key.StartsWith("HIGH")) {
                type = SavedLeaderboard.Scoring.Highscore;
            }
            SavedLeaderboard leaderboard = new SavedLeaderboard(key, result, type, fortnightNo, teamspecific);
            return leaderboard;
        }

        public static bool CreateUserSaveFolder(SocketGuildUser user) {
            bool unoServer = false;
            if (user.Guild.Id == UNO_SERVER_ID) {
                unoServer = true;
            }
            //if (user.IsBot) {
                //return false;
            //}
            string directory = USER_SAVE_DIRECTORY + "\\" + user.Id;
            if (Directory.Exists(directory)) {
                if (unoServer && !File.Exists(directory + "\\" + UNO_SAVE_FILE_NAME)) {
                    CreateUnoSaveFiles(user, directory);
                    return true;
                }
                return false;
            }
            try {
                Directory.CreateDirectory(directory);
            } catch (Exception e) {
                throw e;
            }
            CreateMappedSaveFileFields(directory + "\\" + DEFAULT_SAVE_FILE_NAME, DefaultSaveFields);
            ModifyFieldValue("LIT-JoinedGuild", directory + "\\" + DEFAULT_SAVE_FILE_NAME, user.Guild.Id.ToString());
            ModifyFieldValue("LIT-CachedUsername", directory + "\\" + DEFAULT_SAVE_FILE_NAME, user.Username);
            if (unoServer) {
                CreateUnoSaveFiles(user, directory);
            }
            return true;
        }

        public static bool CreateUnoSaveFiles(SocketGuildUser user, string directory) {
            CreateMappedSaveFileFields(directory + "\\" + UNO_SAVE_FILE_NAME, UnoSaveFields);
            CreateMappedSaveFileFields(directory + "\\FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME, UnoSaveFields);
            ModifyFieldValue("LIT-TEAM", directory + "\\" + UNO_SAVE_FILE_NAME, GuildCache.GetTeam(user));
            ModifyFieldValue("LIT-TEAM", directory + "\\FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME, GuildCache.GetTeam(user));
            return true;
        }

            /// <summary>
            /// Creates leaderboard file if there are none. Only updates values of specific ID's if they exist.
            /// </summary>
            /// <path> Leaderboard directory ONLY. If left null, default. </path>
            public static bool UpdateLeaderboardFile(string key, short fortnightNo = -1, string lbdirectory = null, Dictionary<string, string> newVals = null) {

            if (lbdirectory == null) {
                lbdirectory = LEADERBOARD_DIRECTORY;
            }
            string FNno = "";
            if (fortnightNo != -1) {
                FNno = "FN" + fortnightNo.ToString() + "_";
            }
            string fullPath = lbdirectory + "\\" + FNno + key + ".txt";
            if (!File.Exists(fullPath)) {
                return CreateLeaderboardFile(key, fortnightNo, lbdirectory);
            }
            if (newVals == null) { //do it for ALL users - much more computation time
                CreateLeaderboardFile(key, fortnightNo, lbdirectory);
            } else {
                foreach (var val in newVals) {
                    string res = ModifyFieldValue(val.Key, fullPath, val.Value); //update the score of each user specified - expected to derive from their save file
                    if (res == "") {
                        //user not found, attempt to search save file
                        string originVal = SearchMappedSaveFile(USER_SAVE_DIRECTORY + "\\" + val.Key + "\\" + UNO_SAVE_FILE_NAME, key) ?? "";
                        if (originVal == "")
                            continue; //file or field does not exist
                        string desiredLine = CreateLeaderboardLine(val.Key, val.Value);
                        File.AppendAllLines(fullPath, new string[] { desiredLine });
                    }
                }
                try {
                    return SortLeaderboard(key, fortnightNo, lbdirectory);
                } catch (Exception e) {
                    throw e;
                }
            }
            return true;
        }

        public static byte DetermineFortnightNumber(int dateHex) {
            int fortnightDateHex = GetFortnightStartDateHex(13);
            if (dateHex < fortnightDateHex) {
                throw new Exception("Cannot update previous to Fortnight 13.");
            }
            byte fortnight = 14;
            for (; dateHex > fortnightDateHex; fortnight++) {
                //iterates until finds a date LATER, then determines last FN had to be the one
                fortnightDateHex = GetFortnightStartDateHex(fortnight);
            }
            if (dateHex < fortnightDateHex) {
                //if false, latest FN is correct one, no need to decrement
                fortnight--;
            }
            return fortnight;
        }

        public static int GetFortnightStartDateHex(byte FNno) { 
            if(FNno < 13 || FNno > FORTNIGHT_NUMBER+1) {
                throw new Exception("Fortnight does not exist yet.");
            }
            string key = SearchMappedSaveFile(SAVEFILE_FORTNIGHTDATES, FNno.ToString("X2"));
            try {
                return Int32.Parse(key, NumberStyles.HexNumber);
            }catch {
                throw new Exception(key + " is not a number, Fortnight startdate could not be determined");
            }
        }

        public static DateTime GetFortnightStartDateTime(byte FNno) {
            if (FNno < 13 || FNno > FORTNIGHT_NUMBER) {
                throw new Exception("Fortnight does not exist yet.");
            }
            string stringRep = SearchMappedSaveFile(SAVEFILE_FORTNIGHTDATES, FNno.ToString("X2"));
            try {
                int dateHex = Int32.Parse(stringRep, NumberStyles.HexNumber);
                return SaveFiles_Sequences.TranslateDateHex(dateHex);
            } catch {
                throw new Exception(stringRep + " is not a number, Fortnight startdate could not be determined");
             }
        }

        /// <summary>
        /// Assumes well-formatted, will contain uncaught exceptions.
        /// </summary>
        public static bool SortLeaderboard(string key, short fortnightNo = -1, string path = null) {
            if (path == null)
                path = LEADERBOARD_DIRECTORY;
            string FNno = "";
            if (fortnightNo != -1) {
                FNno = "FN" + fortnightNo.ToString() + "_";
            }
            string destination = path +  "\\" + FNno + key + ".txt";
            string[] unsorted = File.ReadAllLines(destination);
            Dictionary<string, int> x = new Dictionary<string, int>();
            try {
                foreach (string str in unsorted) {
                    x.Add(str.Substring(0, str.IndexOf(":")), Int32.Parse(str.Substring(str.IndexOf(":") + 1)));
                }
            } catch (Exception e) {
                throw e;
            }
            List<KeyValuePair<string, int>> sortedLb = x.ToList();
            sortedLb.Sort((b, a) => a.Value.CompareTo(b.Value));
            string[] final = new string[sortedLb.Count()];
            for (int i = 0; i < final.Count(); i++) {
                final[i] = sortedLb[i].Key + ":" + sortedLb[i].Value;
            }
            File.WriteAllLines(destination, final);
            return true;
        }

        public static bool CreateLeaderboardFile(string key, short fortnightNo = -1, string path = null) {
            if (path == null)
                path = LEADERBOARD_DIRECTORY;
            if (key.StartsWith("LIT-"))
                return false;
            var userSaveFiles = Directory.EnumerateDirectories(USER_SAVE_DIRECTORY);
            Dictionary<string, string> leaderboardEntries = new Dictionary<string, string>();

            foreach (string userSaveFilePath in userSaveFiles) {
                string res;
                if (fortnightNo == -1) {
                    if (File.Exists(userSaveFilePath + "\\" + UNO_SAVE_FILE_NAME)) {
                        res = SearchMappedSaveFile(userSaveFilePath + "\\" + UNO_SAVE_FILE_NAME, key) ?? null;
                    } else {
                        continue;
                    }
                } else {
                    if (File.Exists(userSaveFilePath + "\\FN" + fortnightNo + "_" + UNO_SAVE_FILE_NAME)) {
                        res = SearchMappedSaveFile(userSaveFilePath + "\\FN" + fortnightNo + "_" + UNO_SAVE_FILE_NAME, key) ?? null;
                    } else {
                        continue;
                    }
                }
                if (res != null) {
                    leaderboardEntries.Add(new DirectoryInfo(userSaveFilePath).Name, res); //ID:<point val>
                }
            }

            string FNno = "";
            if(fortnightNo != -1) {
                FNno = "FN" + fortnightNo.ToString() + "_";
            }
            string destination = path + "\\" + FNno + key + ".txt";
            List<string> linesPreRes = new List<string>();
            if(leaderboardEntries.Count() == 0) {
                return false;
            }
            foreach (var userSaveLine in leaderboardEntries) {
                linesPreRes.Add(CreateLeaderboardLine(userSaveLine.Key, userSaveLine.Value));
            }
            string[] lines = linesPreRes.ToArray();
            File.WriteAllLines(destination, lines);

            return true;
        }

        public static string CreateLeaderboardLine(string SnowflakeID, string value) {
            return (SnowflakeID + ":" + value);
        }

        public static bool CreateAllLeaderboardFiles(string path = null, short fortnightNo = -1, List<string> reference = null) {
            path = path ?? LEADERBOARD_DIRECTORY;
            reference = reference ?? LEADERBOARD_TYPES;
            foreach (string type in reference) {
                CreateLeaderboardFile(type, fortnightNo, path);
            }
            return true;
        }
    }
}
