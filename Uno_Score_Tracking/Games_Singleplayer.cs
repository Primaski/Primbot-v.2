using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static Primbot_v._2.Uno_Score_Tracking.SaveFiles_Mapped;
using static Primbot_v._2.Uno_Score_Tracking.Defs;

namespace Primbot_v._2.Uno_Score_Tracking {

    /*
     * STEPS TO ADD A NEW SINGLE PLAYER GAME:
     * 0. In Defs, update POINT_VALUE byte, DAILY_LIMIT byte, GameIden string <ONLY APPEND TO END>, and add to the GetDailyLimit method. Add to the UnoSaveFields and create applicable LB Save types.
     * 1. Create method in this class called [GameName]SaveFileUpdates(string overallPath, string fortnightPath, int pointValue = 0, int iteration = 1)
     *   1a. Convert and store iteration and point values as a string
     *   1b. AddFieldValue(x,y,z) where y alternates between the FN and overall path, z is the point value or iteration, and x is "ITER-"/"POINTS-" or for limited events "PLAYSTODAY-"
     * 2. Add to the switch case in this class `case "name of game": res = MethodName(params); break;"
     * 3. In Modules/Scoring/Log.cs, set up the instructions on how to log the game
     *   3a. Check that is Point Manager, InterpretUserInput, IsWellFormattedListOfUsers, create a list of Tuples for each scorer - 
            first value as ulong ID, second as score. Ensure before sending to the Bridge that the play limit was not hit if exists.
            Send it to Bridge.LogGame, and if this is successful, send it to Games_SinglePlayer for save file updates. Report back to the user whether the operation was a success.
     */
    public static class Games_Singleplayer {

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
                case "non-standard": res = NonStandardSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "minesweeper": res = MinesweeperSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "tetris": res = TetrisSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "bumps": res = BumpsSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "event": res = EventSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "casino": res = CasinoSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "tourney": res = TourneySaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "knights": res = KnightsSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "trivia": res = TriviaSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "daily": res = DailySaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                default: break;
            }
            if (!res) {
                return false;
            }
            AddFieldValue("POINTS-SERVER", fortnightPath, pointVal.ToString());
            AddFieldValue("POINTS-SERVER", overallPath, pointVal.ToString());
            return true;
        }

        private static bool NonStandardSaveFileUpdates(string overallPath, string fortnightPath, int pointValue = 0, int iteration = 1) {
            //only overall point vals for now
            return true;
        }

        private static bool MinesweeperSaveFileUpdates(string overallPath, string fortnightPath, int pointValue = 0, int iteration = 1) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-MS", overallPath, iter);
            AddFieldValue("ITER-MS", fortnightPath, iter);
            AddFieldValue("POINTS-MS", overallPath, points);
            AddFieldValue("POINTS-MS", fortnightPath, points);
            AddFieldValue("PLAYSTODAY-MS", overallPath, iter);
            return true;
        }
        private static bool DailySaveFileUpdates(string overallPath, string fortnightPath, int pointValue = 0, int iteration = 1) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-DAILY", overallPath, iter);
            AddFieldValue("ITER-DAILY", fortnightPath, iter);
            AddFieldValue("POINTS-DAILY", overallPath, points);
            AddFieldValue("POINTS-DAILY", fortnightPath, points);
            AddFieldValue("PLAYSTODAY-DAILY", overallPath, iter);
            return true;
        }

        private static bool TetrisSaveFileUpdates(string overallPath, string fortnightPath, int pointValue = 0, int iteration = 1) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-TETRIS", overallPath, iter);
            AddFieldValue("ITER-TETRIS", fortnightPath, iter);
            AddFieldValue("POINTS-TETRIS", overallPath, points);
            AddFieldValue("POINTS-TETRIS", fortnightPath, points);
            if (pointValue > 50) {
                if (iteration == -1) {
                    iter = "-2";
                } else {
                    iter = "2";
                }
            }
            AddFieldValue("PLAYSTODAY-TETRIS", overallPath, iter);
            return true;
        }


        private static bool BumpsSaveFileUpdates(string overallPath, string fortnightPath, int pointValue = 0, int iteration = 1) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-BUMPS", overallPath, iter);
            AddFieldValue("ITER-BUMPS", fortnightPath, iter);
            AddFieldValue("POINTS-BUMPS", overallPath, points);
            AddFieldValue("POINTS-BUMPS", fortnightPath, points);
            return true;
        }

        private static bool EventSaveFileUpdates(string overallPath, string fortnightPath, int pointValue = 0, int iteration = 1) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-EVENTS", overallPath, iter);
            AddFieldValue("ITER-EVENTS", fortnightPath, iter);
            AddFieldValue("POINTS-EVENTS", overallPath, points);
            AddFieldValue("POINTS-EVENTS", fortnightPath, points);
            return true;
        }

        private static bool CasinoSaveFileUpdates(string overallPath, string fortnightPath, int pointValue = 0, int iteration = 1) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-CASINO", overallPath, iter);
            AddFieldValue("ITER-CASINO", fortnightPath, iter);
            AddFieldValue("POINTS-CASINO", overallPath, points);
            AddFieldValue("POINTS-CASINO", fortnightPath, points);
            return true;
        }

        private static bool TourneySaveFileUpdates(string overallPath, string fortnightPath, int pointValue = 0, int iteration = 1, bool first = false) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-TOURNEY", overallPath, iter);
            AddFieldValue("ITER-TOURNEY", fortnightPath, iter);
            AddFieldValue("POINTS-TOURNEY", overallPath, points);
            AddFieldValue("POINTS-TOURNEY", fortnightPath, points);
            if (first) {
                AddFieldValue("POINTS-TOURNEY", fortnightPath, points);
            }
            return true;
        }

        private static bool KnightsSaveFileUpdates(string overallPath, string fortnightPath, int pointValue = 0, int iteration = 1) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-KNIGHTS", overallPath, iter);
            AddFieldValue("ITER-KNIGHTS", fortnightPath, iter);
            AddFieldValue("POINTS-KNIGHTS", overallPath, points);
            AddFieldValue("POINTS-KNIGHTS", fortnightPath, points);
            AddFieldValue("PLAYSTODAY-KNIGHTS", overallPath, iter);
            return true;
        }

        private static bool TriviaSaveFileUpdates(string overallPath, string fortnightPath, int pointValue, int iteration) {
            string iter = iteration.ToString();
            string points = pointValue.ToString();
            AddFieldValue("ITER-TRIVIA", overallPath, iter);
            AddFieldValue("ITER-TRIVIA", fortnightPath, iter);
            AddFieldValue("POINTS-TRIVIA", overallPath, points);
            AddFieldValue("POINTS-TRIVIA", fortnightPath, points);
            return true;
        }
    }
}