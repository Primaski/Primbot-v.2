/*using Newtonsoft.Json;
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
                case "pokeduel": res = PokeduelSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "idlerpg": res = IdleSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "chess": res = ChessSaveFileUpdates(overallPath, fortnightPath, pointVal, iter); break;
                case "uno1v1": res = Uno1v1SaveFilesUpdates(overallPath, fortnightPath, pointVal, iter); break;
                default: break;
            }
            if (!res) {
                return false;
            }
            AddFieldValue("POINTS-SERVER", fortnightPath, pointVal.ToString());
            AddFieldValue("POINTS-SERVER", overallPath, pointVal.ToString());
            return true;
        }

        private static bool PokeduelSaveFileUpdates(string[] overallPaths, string[] fortnightPaths, int[] pointVals, int[] iterations) {
            if(!(overallPaths.Length == fortnightPaths.Length && 
                overallPaths.Length == pointVals.Length &&
                overallPaths.Length == iterations.Length)) {
                throw new Exception("In 1v1 Savefile updates, inconsistent array lengths, no operation was perfomed.");
            }
            for (int i = 0; i < overallPaths.Length; i++) {
                string overallPath = overallPaths[i];
                string fortnightPath = fortnightPaths[i];
                string points = pointVals[i].ToString();
                string iter = iterations[i].ToString();
                AddFieldValue("ITER-POKEDUEL", overallPath, iter);
                AddFieldValue("ITER-POKEDUEL", fortnightPath, iter);
                AddFieldValue("POINTS-POKEDUEL", overallPath, points);
                AddFieldValue("POINTS-POKEDUEL", fortnightPath, points);
                AddFieldValue("PLAYSTODAY-POKEDUEL", overallPath, iter);
            }
            return true;
        }

        private static bool IdleSaveFileUpdates(string[] overallPaths, string[] fortnightPaths, int[] pointVals, int[] iterations) {
            if (!(overallPaths.Length == fortnightPaths.Length &&
                overallPaths.Length == pointVals.Length &&
                overallPaths.Length == iterations.Length)) {
                throw new Exception("In 1v1 Savefile updates, inconsistent array lengths, no operation was perfomed.");
            }
            for (int i = 0; i < overallPaths.Length; i++) {
                string overallPath = overallPaths[i];
                string fortnightPath = fortnightPaths[i];
                string points = pointVals[i].ToString();
                string iter = iterations[i].ToString();
                AddFieldValue("ITER-IDLERPG", overallPath, iter);
                AddFieldValue("ITER-IDLERPG", fortnightPath, iter);
                AddFieldValue("POINTS-IDLERPG", overallPath, points);
                AddFieldValue("POINTS-IDLERPG", fortnightPath, points);
                AddFieldValue("PLAYSTODAY-IDLERPG", overallPath, iter);
            }
            return true;
        }

        private static bool ChessSaveFileUpdates(string[] overallPaths, string[] fortnightPaths, int[] pointVals, int[] iterations) {
            if (!(overallPaths.Length == fortnightPaths.Length &&
                overallPaths.Length == pointVals.Length &&
                overallPaths.Length == iterations.Length)) {
                throw new Exception("In 1v1 Savefile updates, inconsistent array lengths, no operation was perfomed.");
            }
            for (int i = 0; i < overallPaths.Length; i++) {
                string overallPath = overallPaths[i];
                string fortnightPath = fortnightPaths[i];
                string points = pointVals[i].ToString();
                string iter = iterations[i].ToString();
                AddFieldValue("ITER-CHESS", overallPath, iter);
                AddFieldValue("ITER-CHESS", fortnightPath, iter);
                AddFieldValue("POINTS-CHESS", overallPath, points);
                AddFieldValue("POINTS-CHESS", fortnightPath, points);
                AddFieldValue("PLAYSTODAY-CHESS", overallPath, iter);
            }
            return true;
        }

    }
}
*/