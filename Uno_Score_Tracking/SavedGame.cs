using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using static Primbot_v._2.Uno_Score_Tracking.SaveFiles_GlobalVariables;
/// <summary>
/// FIX UP
/// </summary>
namespace Primbot_v._2.Uno_Score_Tracking {
    public class SavedGame {
        private static readonly DateTime startDate = new DateTime(2019, 1, 1, 0, 0, 0);
        static readonly byte GAME_TYPE_LENGTH = 2;
        static readonly byte GAME_ITERATON_LENGTH = 5;
        static readonly byte GAME_ID_LENGTH = 7; //type + iteration
        static readonly byte DATE_LENGTH = 6;
        static readonly byte USER_ID_LENGTH = 16;
        static readonly byte SCORE_LENGTH = 2;


        string gameID = "";
        byte gameType;
        string gameName;
        int gameIteration;

        enum Status { Initialized, Uninitialized };
        Status status;

        DateTime playedTime;
        List<string> allSequences;

        public List<Tuple<ulong, byte>> Players = new List<Tuple<ulong,byte>>(); //(player ID, point value)

        public SavedGame(List<string> allSequences) {
            status = Status.Uninitialized;
            for(int i = 0; i < allSequences.Count(); i++) {
                string currentSequence = allSequences[i];
                if (!SaveFiles_Sequences.IsValidSequence(currentSequence)) {
                    throw new InvalidSaveSequenceException("Passed in: " + currentSequence, "");
                }

                if (i == 0) {
                    TranslateSaveSequence(currentSequence);
                } else {
                    //discard if not same ID
                    if(GetGameID(currentSequence) != this.gameID) {
                        throw new Exception("Sequences with different IDs were passed in. Ensure one SavedGame Object is created per Game ID.");
                    }
                    TranslateUserIDAndScore(currentSequence);
                }
            }
            this.allSequences = allSequences;
            status = Status.Initialized;
            return;
        }

        public void TranslateSaveSequence(string fullSequence) {
            TranslateGameIDAndDateHex(fullSequence);
            TranslateUserIDAndScore(fullSequence);
            return;
        }

        public void TranslateGameIDAndDateHex(string fullSequence) {
            int ID = -1;
            int date = -1;
            try {
                ID = Int32.Parse(GetGameID(fullSequence), NumberStyles.HexNumber);
                date = Int32.Parse(GetDateHex(fullSequence), NumberStyles.HexNumber);
            } catch (Exception e) {
                throw e;
            }
            TranslateID(ID);
            TranslateDateHex(date);
            return;
        }

        public void TranslateUserIDAndScore(string fullSequence) {
            ulong userID = 0;
            byte score = 0;
            try {
                userID = UInt64.Parse(GetUserID(fullSequence), NumberStyles.HexNumber);
                score = Byte.Parse(GetScore(fullSequence), NumberStyles.HexNumber);
            } catch (Exception e) {
                throw e;
            }
            Players.Add(new Tuple<ulong,byte>(userID, score));
            return;
        }

        public void TranslateID(int GameID) {
            if (GameID > 0xFFFFFFF || GameID < 0x0) {
                throw new Exception("Invalid Save Sequence. Max Sequence int value is 0xFFFFFFF, min is 0x0.");
            }
            string stringRep = GameID.ToString("X7");
            string gameType = stringRep.Substring(0, GAME_TYPE_LENGTH);
            string gameIteration = stringRep.Substring(GAME_TYPE_LENGTH, GAME_ITERATON_LENGTH);
            this.gameID = stringRep;
            this.gameType = Byte.Parse(gameType, NumberStyles.HexNumber);
            this.gameName = GameIden[this.gameType];
            this.gameIteration = Int32.Parse(gameIteration, NumberStyles.HexNumber);
            return;
        }

        public void TranslateDateHex(int dateHex) {
            if (dateHex > 0xFFFFFF || dateHex < 0x0) {
                throw new Exception("Invalid Save Sequence. Max sequence int value is 0xFFFFFF, min is 0x0.");
            }
            string stringRep = dateHex.ToString("X6");
            int dateInfo = Int32.Parse(stringRep.Substring(0,3), NumberStyles.HexNumber);
            int timeInfo = Int32.Parse(stringRep.Substring(3,3), NumberStyles.HexNumber);
            this.playedTime = startDate.AddDays((double)dateInfo).AddMinutes((double)timeInfo);
            return;
        }

        /// <summary>
        /// Translates Sequence contents into human readable formats and Decimal representations. Contains game ID, but not sequence.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,string> MakeHumanReadable(bool getUsername = false) {
            if(this.status == Status.Uninitialized) {
                return null;
            }
            Dictionary<string, string> x = new Dictionary<string, string>();
            x.Add("Game", gameName[0].ToString().ToUpper() + gameName.Substring(1));
            //x.Add("Iteration", gameIteration.ToString());
            x.Add("Game ID", gameID);
            x.Add("Time", playedTime.ToString("yyyy MMMM dd") + ", " + playedTime.ToString("HH:mm") + " EST");
            int i = 1;
            foreach(var player in Players) {
                if (getUsername) {
                    x.Add("Player " + i, GuildCache.GetUsername(player.Item1) + " (" + player.Item1.ToString() + ")");
                } else {
                    x.Add("Player " + i + " ID", player.Item1.ToString());
                }
                x.Add("Player " + i + " Points", player.Item2.ToString());
                i++;
            }
            return x;
        }

        public string GetGameID(string sequence) {
            return sequence.Substring(0, GAME_ID_LENGTH);
        }

        public string GetDateHex(string sequence) {
            return sequence.Substring(sequence.IndexOf(":")+1, DATE_LENGTH);
        }

        public string GetUserID(string sequence) {
            return sequence.Substring(sequence.IndexOf("-")+1, USER_ID_LENGTH);
        }

        public string GetScore(string sequence) {
            return sequence.Substring(sequence.IndexOf("_")+1, SCORE_LENGTH);
        }
    }
}