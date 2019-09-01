using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using static Primbot_v._2.Uno_Score_Tracking.Defs;
using System.Threading.Tasks;

namespace Primbot_v._2.Uno_Score_Tracking {
    public static class SaveFiles_Sequences {

        /// <summary>
        /// Calls a Save Sequence generation method, and writes to file. Returns Save Sequence.
        /// </summary>
        public static string LogGame(int GameID, ulong SnowflakeID, int score) {
            try {
                string saveSequence = CreateSaveSequence(GameID, SnowflakeID, score);
                WriteSequenceToSaveFile(saveSequence);
                return saveSequence;
            } catch (Exception e) {
                throw e;
            }
            
        }

        private static string CreateSaveSequence(int GameID, ulong ID, int score) {
            string DateGen = "";
            if(GameID < 0x0 || GameID > 0xFFFFFFF) {
                throw new Exception(":exclamation: Error: Invalid Game ID provided.");
            }
            try {
                DateGen = GenerateDateHex();
            }catch(Exception e) {
                throw e;
            }
            
            return GameID.ToString("X7") + ":" + DateGen + "-" + ID.ToString("X16") + "_" + score.ToString("X2");
        }

        /// <summary>
        /// Reverts all savefiles by ID. All score values are set to 0x00, which the system should interpret as invalid.
        /// </summary>
        public static List<string> RevertbyID(int gameID) {
            if(gameID < 0x0 || gameID > 0xFFFFFFF) {
                throw new Exception("Invalid ID.");
            }
            List<string> toReplace = SearchSaveFileForID(gameID);
            if(toReplace.Count() == 0) {
                throw new NullReferenceException();
            }
            string toReplaceWith;
            foreach (string sequence in toReplace) {
                try {
                    toReplaceWith = (sequence.Substring(0, SEQUENCE_LENGTH - SCORE_LENGTH) + "00");
                    ReplaceSaveFileSequence(sequence, toReplaceWith);
                } catch(Exception e) {
                    throw e;
                }
            }
            return toReplace;
        }

        private static Task ReplaceSaveFileSequence(string sequence, string toReplaceWith, string path = "") {
            if(path == "") {
                path = SAVEFILE_GAMELOGS;
            }
            try {
                string[] saveFile = File.ReadAllLines(path);
                int line_to_edit = Array.IndexOf(saveFile, sequence);
                saveFile[line_to_edit] = toReplaceWith;
                File.WriteAllLines(path, saveFile);
                return Task.CompletedTask;
            } catch(Exception e) {
                return Task.FromException(e);
            }
        }

        public static string GenerateDateHex() {
            double differential = (DateTime.Now - Defs.startDate).TotalDays;
            int diffInt = (int)differential;
            if(diffInt > 0xFFF || diffInt < 0x0) {
                throw new Exception(":exclamation: Error: Date Hex must be of format 0x000 between 000 and FFF, but instead was: " + diffInt.ToString());
            }
            string dateString = diffInt.ToString("X3");

            differential = (DateTime.Now - DateTime.Today).TotalMinutes;
            diffInt = (int)differential;
            if(diffInt > 0x59F || diffInt < 0x0) {
                throw new Exception(":exclamation: Error: Time Hex must be between 0x0 and 0x53B, as it is a state of minutes in a day, but instead was: " + diffInt.ToString("X"));
            }
            return dateString + diffInt.ToString("X3");
        }

        /// <summary>
        /// Returns new unique ID. ID format - [game ID] [game iteration]. Every ID is unique, a bijection from games to numbers. Once an ID is used, it can never be reused.
        /// Ensure it is stored in a save file.
        /// </summary>
        public static string GenerateID(byte gameID) {
            string gameIDHex = gameID.ToString("X2");

            try {
                string fieldValue = SaveFiles_Mapped.AddFieldValue(gameIDHex, SAVEFILE_GAMEITERATIONS, "1", "X5"); //returns "" if not found
                fieldValue = gameIDHex + fieldValue;
                if (fieldValue == "") {
                    throw new Exception(":exclamation: Error: You might want to implement a method now to append lines to a file, since I couldn't find one, jackass.");
                }
                if (fieldValue.Length != GAME_ID_LENGTH) {
                    throw new Exception(":exclamation: Error: Field value length in Game Iterations was not of format TT:IIIII. Was instead: " + fieldValue);
                }
                if(!Int32.TryParse(fieldValue, NumberStyles.HexNumber, null, out int ignore)) {
                    throw new Exception(":exclamation: Error: Game Iteration File returned a non-" + GAME_ID_LENGTH + "-digit length value.");
                } else {
                    return fieldValue;
                }
            } catch (Exception e) {
                throw e;
            }
        }

        private static void WriteSequenceToSaveFile(string SaveSequence) {
            if (!IsValidSequence(SaveSequence)) {
                throw new InvalidSaveSequenceException(":exclamation: Error: Passed in-> " + SaveSequence, "");
            }
            using (StreamWriter sw = new StreamWriter(SAVEFILE_GAMELOGS,true)) {
                sw.WriteLine("\n" + SaveSequence);
            }
            return;
        }

        /// <summary>
        /// Returns list of all lines matching identification - distinction will be player/point tuples. Returns empty list if null.
        /// </summary>
        public static List<string> SearchSaveFileForID(int gameID) {
            string gameIden = gameID.ToString("X7");
            List<string> result = new List<string>();
            string[] saveFile = File.ReadAllLines(SAVEFILE_GAMELOGS);
            foreach(string line in saveFile) {
                if (line.StartsWith(gameIden)) {
                    result.Add(line);
                }
            }
            return result;
        }

        public static DateTime TranslateDateHex(int dateHex) {
            if (dateHex > 0xFFFFFF || dateHex < 0x0) {
                throw new Exception("Invalid Save Sequence. Max sequence int value is 0xFFFFFF, min is 0x0.");
            }
            string stringRep = dateHex.ToString("X6");
            int dateInfo = Int32.Parse(stringRep.Substring(0, 3), NumberStyles.HexNumber);
            int timeInfo = Int32.Parse(stringRep.Substring(3, 3), NumberStyles.HexNumber);
            return startDate.AddDays((double)dateInfo).AddMinutes((double)timeInfo);
        }

        public static bool IsValidSequence(string sequence) {
            //expected format: TTIIIII:DDDMMM-SSSSSSSSSSSSSSSS_PP (7+6+16+2+3=34)
            //T > game type, I > game iteration, D > game day, M > game minute (59F max val), S > snowflake ID, P > point value
            if(sequence.Length != SEQUENCE_LENGTH) {
                return false;
            }
            if(!(sequence[GAME_ID_LENGTH] == ':' && sequence[GAME_ID_LENGTH+DATE_LENGTH+1] == '-' 
                && sequence[GAME_ID_LENGTH+DATE_LENGTH+USER_ID_LENGTH+2] == '_')) {
                return false;
            }
            if(!(Int32.TryParse(sequence.Substring(0,GAME_ID_LENGTH), NumberStyles.HexNumber, null, out int ignore) && 
                Int32.TryParse(sequence.Substring(GAME_ID_LENGTH+1,DATE_LENGTH), NumberStyles.HexNumber, null, out int ignore2) &&
                UInt64.TryParse(sequence.Substring(GAME_ID_LENGTH+DATE_LENGTH+2,USER_ID_LENGTH), NumberStyles.HexNumber, null, out ulong ignore3) && 
                Int16.TryParse(sequence.Substring(GAME_ID_LENGTH+DATE_LENGTH+USER_ID_LENGTH+3, SCORE_LENGTH), NumberStyles.HexNumber, null, out short ignore4))){
                return false;
            }
            int minute = Int32.Parse(sequence.Substring(11, 3), NumberStyles.HexNumber);
            if(minute > 0x59F) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Must all have the same ID.
        /// </summary>
        public static SavedGame ConvertSequencesToObject(List<string> sequences) {
            foreach (string sequence in sequences) {
                if (!IsValidSequence(sequence)) {
                    throw new InvalidSaveSequenceException(":exclamation: Error: Passed in-> " + sequence, "");
                }
            }
            SavedGame game = null;
            try {
                game = new SavedGame(sequences);
            }catch(Exception e) {
                throw e;
            }
            return game;
        }
    }

}
