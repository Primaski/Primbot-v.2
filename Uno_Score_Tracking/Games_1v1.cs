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
    public static class Games_1v1 {
        public static bool SaveFileUpdate(List<Tuple<ulong, byte>> earners, string gameType, bool revert = false, int dateHex = -1) {

            if(earners.Count() != 2) {
                throw new Exception("Not a 1v1 game Exception.");
            }

            byte fnNO = FORTNIGHT_NUMBER;
            if (dateHex != -1) {
                DetermineFortnightNumber(dateHex);
            }

            string[] overallPath = new string[earners.Count()];
            string[] fortnightPath = new string[earners.Count()];
            int[] pointVal = new int[earners.Count()];
            int iter = (revert) ? -1 : 1;
            for(int i = 0; i < earners.Count(); i++) {
                var earner = earners[i];
                overallPath[i] = USER_SAVE_DIRECTORY + "\\" + earner.Item1.ToString() + "\\" + UNO_SAVE_FILE_NAME;
                fortnightPath[i] = USER_SAVE_DIRECTORY + "\\" + earner.Item1.ToString() + "\\" + "FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME;
                int relPointVal = earner.Item2;
                relPointVal = (revert) ? -relPointVal : relPointVal;
                pointVal[i] = relPointVal;
            }

            bool res = false;
            switch (gameType) {
                case "pokeduel": res = PokeduelSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "idlerpg": res = IdleSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "chess": res = ChessSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "uno1v1": res = Uno1v1SaveFilesUpdates(overallPath, fortnightPath, pointVal, iter); break;
                default: break;
            }
            if (!res) {
                return false;
            }
            for (int i = 0; i < overallPath.Length; i++) {
                AddFieldValue("POINTS-SERVER", fortnightPath[i], pointVal[i].ToString());
                AddFieldValue("POINTS-SERVER", overallPath[i], pointVal[i].ToString());
            }
            return true;
        }

        private static bool Uno1v1SaveFilesUpdates(string[] overallPaths, string[] fortnightPaths, int[] pointVals, int iterations) {
            if (!(overallPaths.Length == fortnightPaths.Length &&
                overallPaths.Length == pointVals.Length)) {
                throw new Exception("In 1v1 Savefile updates, inconsistent array lengths, no operation was perfomed.");
            }
            string pathWinner = overallPaths[0];
            string fnPathWinner = fortnightPaths[0];
            string pointsWinner = pointVals[0].ToString();
            string iter = iterations.ToString();
            AddFieldValue("ITER-UNO1V1", pathWinner, iter);
            AddFieldValue("ITER-UNO1V1", fnPathWinner, iter);
            AddFieldValue("POINTS-UNO1V1", pathWinner, pointsWinner);
            AddFieldValue("POINTS-UNO1V1", fnPathWinner, pointsWinner);
            AddFieldValue("FIRST-UNO1V1", fnPathWinner, iter);
            AddFieldValue("PLAYSTODAY-UNO1V1", pathWinner, iter);
            AddFieldValue("PLAYSTODAY-UNO1V1", overallPaths[1], iter);
            return true;
        }

        private static bool PokeduelSaveFileUpdates(string[] overallPaths, string[] fortnightPaths, int[] pointVals, int iterations) {
            if (!(overallPaths.Length == fortnightPaths.Length &&
                overallPaths.Length == pointVals.Length)) {
                throw new Exception("In 1v1 Savefile updates, inconsistent array lengths, no operation was perfomed.");
            }
            string pathWinner = overallPaths[0];
            string fnPathWinner = fortnightPaths[0];
            string pointsWinner = pointVals[0].ToString();
            string iter = iterations.ToString();
            AddFieldValue("ITER-POKEDUEL", pathWinner, iter);
            AddFieldValue("ITER-POKEDUEL", fnPathWinner, iter);
            AddFieldValue("POINTS-POKEDUEL", pathWinner, pointsWinner);
            AddFieldValue("POINTS-POKEDUEL", fnPathWinner, pointsWinner);
            AddFieldValue("PLAYSTODAY-POKEDUEL", pathWinner, iter);
            AddFieldValue("PLAYSTODAY-POKEDUEL", overallPaths[1], iter);
            return true;
        }

        private static bool IdleSaveFileUpdates(string[] overallPaths, string[] fortnightPaths, int[] pointVals, int iterations) {
            if (!(overallPaths.Length == fortnightPaths.Length &&
                overallPaths.Length == pointVals.Length)) {
                throw new Exception("In 1v1 Savefile updates, inconsistent array lengths, no operation was perfomed.");
            }
            string pathWinner = overallPaths[0];
            string fnPathWinner = fortnightPaths[0];
            string pointsWinner = pointVals[0].ToString();
            string iter = iterations.ToString();
            AddFieldValue("ITER-IDLERPG", pathWinner, iter);
            AddFieldValue("ITER-IDLERPG", fnPathWinner, iter);
            AddFieldValue("POINTS-IDLERPG", pathWinner, pointsWinner);
            AddFieldValue("POINTS-IDLERPG", fnPathWinner, pointsWinner);
            AddFieldValue("PLAYSTODAY-IDLERPG", pathWinner, iter);
            AddFieldValue("PLAYSTODAY-IDLERPG", overallPaths[1], iter);
            return true;
        }

        private static bool ChessSaveFileUpdates(string[] overallPaths, string[] fortnightPaths, int[] pointVals, int iterations) {
            if (!(overallPaths.Length == fortnightPaths.Length &&
                overallPaths.Length == pointVals.Length)) {
                throw new Exception("In 1v1 Savefile updates, inconsistent array lengths, no operation was perfomed.");
            }
            string pathWinner = overallPaths[0];
            string fnPathWinner = fortnightPaths[0];
            string pointsWinner = pointVals[0].ToString();
            string iter = iterations.ToString();
            AddFieldValue("ITER-CHESS", pathWinner, iter);
            AddFieldValue("ITER-CHESS", fnPathWinner, iter);
            AddFieldValue("POINTS-CHESS", pathWinner, pointsWinner);
            AddFieldValue("POINTS-CHESS", fnPathWinner, pointsWinner);
            AddFieldValue("PLAYSTODAY-CHESS", pathWinner, iter);
            AddFieldValue("PLAYSTODAY-CHESS", overallPaths[1], iter);
            return true;
        }

        internal static void Uno1v1Log(List<ulong> list) {
            if(list.Count() != 2) { throw new Exception("Exactly 2 players required for a 1v1 Uno log."); }
            List<Tuple<ulong, byte>> playersAndPoints = new List<Tuple<ulong, byte>>();
            playersAndPoints.Add(Tuple.Create(list[0],UNO1V1_WINNER_POINT_VALUE));
            playersAndPoints.Add(Tuple.Create(list[1],UNO1V1_LOSER_POINT_VALUE));
            try {
                Bridge.LogGame("uno1v1", playersAndPoints);
            }catch(Exception e) {
                throw e;
            }
            try {
                bool success = SaveFileUpdate(playersAndPoints, "uno1v1");
                if (!success) {
                    throw new Exception("Unsuccessful Save File update.");
                }
            }catch(Exception e) {
                throw e;
            }
        }
    }
}