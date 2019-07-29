using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Primbot_v._2;

namespace Primbot_v._2.Server {
    public static class Killswitch {
        public enum Status {
            Off, Mute, Kick, Ban
        }
        public static Status state = Status.Off;


        public static void SetStatus(Status newState) {
            state = newState;
        }

        public static Status GetState() {
            return state;
        }

        public static string GetHelpMenu() {
            return "Killswitch (used for raids):\n" +
                "`p*killswitch off` - turns killswitch off (default)\n" +
                "`p*killswitch state` - returns the state of the killswitch" +
                "`p*killswitch mute` - mutes all users upon joining\n" +
                "`p*killswitch kick` - kicks all users upon joining\n" +
                "`p*killswitch ban` - bans all users upon joining\n";
        }
    }
}
