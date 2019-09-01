using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static Primbot_v._2.Uno_Score_Tracking.Defs;

namespace Primbot_v._2.Uno_Score_Tracking {
    public class Score_Tracker_Exceptions {
        internal static string WriteNewException(Exception e) {
            return ":exclamation: **Critical Error: " + e.Message + "**\n\n" + e.StackTrace;
        }
    }

    public class InvalidSaveSequenceException : Exception {
        [FlagsAttribute] enum field {Type, Iteration, ID, Date,
        Time, Timestamp, Snowflake, Points};

        public InvalidSaveSequenceException() : base(CorrectFormat()) {
            return;
        }

        public InvalidSaveSequenceException(string passedInValue, string message) : base(Errorval(message,passedInValue)) {
            return;
        }

        public InvalidSaveSequenceException(string message) : base(Errorval(message)) {
            return;
        }

        public static string CorrectFormat() {
            return "The correct format for a save sequence is as follows:\n" +
                "TTIIIII:DDDMMM-SSSSSSSSSSSSSSSS_PP\n" +
                "All values hex. T -> game type, I -> game iteration, D -> date (days from 2019/1/1)," +
                "M -> minute of day (max val 59F), S -> snowflake ID, P -> score value";
        }

        public static string Errorval(string message, string passedInVal = "") {
            string x = "The correct format for a save sequence is as follows:\n" +
                "TTIIIII:DDDMMM-SSSSSSSSSSSSSSSS_PP\n" +
                "All values hex. T -> game type, I -> game iteration, D -> date (days from 2019/1/1)," +
                "M -> minute of day (max val 59F), S -> snowflake ID, P -> score value\n\n";
            if(message != "") {
                x += "Additional details: " + message + "\n";
            }
            if(passedInVal != "") {
                x += "Passed in value: " + passedInVal;
            }
            return x;
        }
    }
}
