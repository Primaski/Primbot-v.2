using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using static Primbot_v._2.Uno_Score_Tracking.SaveFiles_GlobalVariables;
using System.Text;

namespace Primbot_v._2.Uno_Score_Tracking {
    public class SavedLeaderboard {

        string KEY;
        short FORTNIGHT_NO; //-1 = overall
        int FIRST_ZERO_INDEX;
        public readonly List<LBUser> RESULTS;
        public ulong teamspecific = 0;

        //public int TEAM_1_SCORE, TEAM_2_SCORE, WILD_SCORE;
        public int GREEN_TEAM_SCORE, BLUE_TEAM_SCORE, RED_TEAM_SCORE, YELLOW_TEAM_SCORE, WILD_SCORE;

        public enum Scoring {
            Points, Highscore, Wins
        };
        Scoring scoringType;

        /*Dictionary<string, string> TEAM_CARD = new Dictionary<string, string>(){
            {TEAM_ONE_ID.ToString(), PASTEL_CARD_EMOJI}, {TEAM_TWO_ID.ToString(), NEON_CARD_EMOJI},
            {WILD_ID.ToString(), BLACK_CARD_EMOJI }
            };*/

        Dictionary<string, string> TEAM_CARD = new Dictionary<string, string>(){
            {GREEN_TEAM_ID.ToString(), GREEN_CARD_EMOJI}, {BLUE_TEAM_ID.ToString(), BLUE_CARD_EMOJI},
            {RED_TEAM_ID.ToString(), RED_CARD_EMOJI}, {YELLOW_TEAM_ID.ToString(), YELLOW_CARD_EMOJI},
            {WILD_ID.ToString(), BLACK_CARD_EMOJI }
            };

        //string TEAM_1_MESSAGE = PASTEL_HEART + "**Pastel: **";
        //string TEAM_2_MESSAGE = NEON_HEART + "**Neon: **";
        string GREEN_TEAM_MESSAGE = ":green_heart: **Green: **";
        string YELLOW_TEAM_MESSAGE = ":yellow_heart: **Yellow: **";
        string RED_TEAM_MESSAGE = ":heart: **Red: **";
        string BLUE_TEAM_MESSAGE = ":blue_heart: **Blue: **";
        string WILD_MESSAGE = ":black_heart:" + "**Admin: **";


        public SavedLeaderboard(string key, string[] leaderboardFile, Scoring type = Scoring.Points, short fortnightNo = -1, ulong teamspecific = 0) {
            this.scoringType = type;
            this.KEY = key;
            string[] temp;
            string team, username;
            team = "";
            username = "";
            int score = 0;
            this.FORTNIGHT_NO = fortnightNo;
            RESULTS = new List<LBUser>();
            this.teamspecific = teamspecific;
            try {
                foreach (string str in leaderboardFile) {

                    temp = str.Split(':');
                    if (temp.Count() != 2) {
                        throw new Exception();
                    }
                    ulong ID = 0;
                    ID = UInt64.Parse(temp[0]);
                    if(ID == 447487261760552982) {
                        //dummy account which defaults to null, shouldn't appear on lb
                        continue;
                    }
                    team = GuildCache.GetTeam(ID) ?? "?";
                    if (team == "?")
                        team = SaveFiles_Mapped.SearchCachedTeam(ID) ?? "?";
                    if (teamspecific != 0) {
                        if(team != teamspecific.ToString()) {
                            continue;
                        }
                    }
                    username = GuildCache.GetUsername(ID) ?? "?";
                    score = Int32.Parse(temp[1]);
                    RESULTS.Add(new LBUser(ID, username, team, score));
                }
            } catch (Exception e) {
                throw e;
            }
            CalculateTeamScores();
            if (key == "POINTS-SERVER" && fortnightNo == FORTNIGHT_NUMBER && teamspecific == 0) {
                GuildCache.SetTeamScore(RED_TEAM_SCORE, 1);
                GuildCache.SetTeamScore(YELLOW_TEAM_SCORE, 2);
                GuildCache.SetTeamScore(GREEN_TEAM_SCORE, 3);
                GuildCache.SetTeamScore(BLUE_TEAM_SCORE, 4);
            }
            SortLeaderboard();
        }

        private void SortLeaderboard() {
            RESULTS.Sort((x, y) => y.score.CompareTo(x.score));
            int i = 0;
            foreach (var user in RESULTS) {
                if (user.score == 0) {
                    FIRST_ZERO_INDEX = i;
                    return;
                }
                i++;
            }
            FIRST_ZERO_INDEX = i;
            return;
        }

        private void CalculateTeamScores() {
            foreach (var result in RESULTS) {
                int worth = result.score;
                if (result.team == GREEN_TEAM_ID.ToString()) {
                    GREEN_TEAM_SCORE += result.score;
                } else if (result.team == BLUE_TEAM_ID.ToString()) {
                    BLUE_TEAM_SCORE += result.score;
                } else if (result.team == YELLOW_TEAM_ID.ToString()) {
                    YELLOW_TEAM_SCORE += result.score;
                } else if (result.team == RED_TEAM_ID.ToString()) {
                    RED_TEAM_SCORE += result.score;
                } else if (result.team == WILD_ID.ToString()) {
                    WILD_SCORE += result.score;
                }
            }
            RED_TEAM_MESSAGE += RED_TEAM_SCORE.ToString();
            YELLOW_TEAM_MESSAGE += YELLOW_TEAM_SCORE.ToString();
            GREEN_TEAM_MESSAGE += GREEN_TEAM_SCORE.ToString();
            BLUE_TEAM_MESSAGE += BLUE_TEAM_SCORE.ToString();
            WILD_MESSAGE += WILD_SCORE.ToString();
        }

        public string GetHumanReadableScore(ulong ID, string nameOfKey = "", bool local = false) {
            int score = 0;
            int placement = 0;
            string username = "";
            if (nameOfKey == "") {
                nameOfKey = KEY;
            }
            string inFortnight = "in Fortnight " + FORTNIGHT_NO;
            if (FORTNIGHT_NO == -1) {
                inFortnight = "overall";
            }
            if (FORTNIGHT_NO == FORTNIGHT_NUMBER) {
                inFortnight = "this Fortnight";
            }
            string emojiName = BLACK_CARD_EMOJI;
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < RESULTS.Count(); i++) {
                var user = RESULTS[i];
                if (user.ID == ID) {
                    username = user.username;
                    if (TEAM_CARD.ContainsKey(user.team)) {
                        emojiName = TEAM_CARD[user.team];
                    }
                    score = user.score;
                    placement = ++i;
                }
            }
            result.Append(emojiName);
            if (!local) {
                result.Append(username);
                switch (scoringType) {
                    case Scoring.Highscore: result.Append("'s " + nameOfKey + " highscore is " + score); break;
                    case Scoring.Wins: result.Append(" has " + score + " " + nameOfKey + " wins"); break;
                    default: result.Append(" has " + score + " " + nameOfKey + " points"); break;
                }
            } else {
                result.Append("You");
                switch (scoringType) {
                    case Scoring.Highscore: result.Append("r " + nameOfKey + " highscore is " + score); break;
                    case Scoring.Wins: result.Append(" have " + score + " " + nameOfKey + " wins"); break;
                    default: result.Append(" have " + score + " " + nameOfKey + " points"); break;
                }
            }
            result.Append(" " + inFortnight + "! " + emojiName + "\n:tada: ");
            if (score != 0) {
                if (!local) {
                    result.Append(username + " is in ");
                } else {
                    result.Append("You are in ");
                }
                int mod = placement % 10;
                if (placement < 10 || placement > 13) {
                    switch (mod) {
                        case 1: result.Append(placement + "st place out of " + (FIRST_ZERO_INDEX) + " users! :tada:"); break;
                        case 2: result.Append(placement + "nd place out of " + (FIRST_ZERO_INDEX) + " users! :tada:"); break;
                        case 3: result.Append(placement + "rd place out of " + (FIRST_ZERO_INDEX) + " users! :tada:"); break;
                        default: result.Append(placement + "th place out of " + (FIRST_ZERO_INDEX) + " users! :tada:"); break;
                    }
                } else {
                    result.Append(placement + "th place out of " + (FIRST_ZERO_INDEX) + " users! :tada:");
                }
            }
            return result.ToString();
        }

        public string MakeHumanReadable(string nameOfKey = "", short startingIndex = 0, byte noToShow = 10) {
            StringBuilder stringres = new StringBuilder();
            if (startingIndex < 0 || noToShow < 1) {
                return "";
            }
            if (nameOfKey == "")
                nameOfKey = KEY;
            switch (scoringType) {
                case Scoring.Highscore: nameOfKey += " High scores"; break;
                case Scoring.Wins: nameOfKey += " Wins"; break;
                default: nameOfKey += " Points"; break;
            }
            if (FORTNIGHT_NO != -1) {
                stringres.AppendLine("**Fortnight " + FORTNIGHT_NO.ToString() + " " + nameOfKey + "**:");
            } else {
                stringres.AppendLine("**Overall " + nameOfKey + "**:");
            }
            int lastScore = 0;
            int lastIndex = 0;
            if (startingIndex < RESULTS.Count()) {
                for (int i = startingIndex; (i < startingIndex + noToShow) && (i < RESULTS.Count()) && RESULTS[i].score != 0; i++) {
                    var result = RESULTS[i];
                    string emojiName = ":warning:";
                    if (TEAM_CARD.ContainsKey(result.team)) {
                        emojiName = TEAM_CARD[result.team];
                    }
                    if (lastScore != result.score) {
                        lastIndex = i;
                        lastScore = result.score;
                    }
                    stringres.AppendLine((lastIndex + 1).ToString() + ". " + emojiName + " **" + result.username + "** - " + result.score.ToString());

                }
            }
            stringres.AppendLine(".==.==.==.==.==.==.==.==.==.==.");
            List<Tuple<string, int>> pairs = new List<Tuple<string, int>>(); //msg , sorting methodology
            pairs.Add(Tuple.Create(GREEN_TEAM_MESSAGE, GREEN_TEAM_SCORE));
            pairs.Add(Tuple.Create(YELLOW_TEAM_MESSAGE, YELLOW_TEAM_SCORE));
            pairs.Add(Tuple.Create(RED_TEAM_MESSAGE, RED_TEAM_SCORE));
            pairs.Add(Tuple.Create(BLUE_TEAM_MESSAGE, BLUE_TEAM_SCORE));
            pairs.Add(Tuple.Create(WILD_MESSAGE, WILD_SCORE));
            pairs.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            foreach (var pair in pairs) {
                stringres.AppendLine(pair.Item1);
            }
            if (FORTNIGHT_NO != -1) {
                stringres.AppendLine("Showing " + (startingIndex + 1).ToString() + " to " + (startingIndex + noToShow).ToString() + ". " +
                    "Type `p*lb [type] page" + (((startingIndex + 1) / 10) + 2).ToString() + "` for more. Wanted to see overall " +
                    "scores instead of Fortnight scores? Try typing `p*lb [type] overall`!");
            } else {
                stringres.AppendLine("Showing " + (startingIndex + 1).ToString() + " to " + (startingIndex + noToShow).ToString() + ". " +
                    "Type `p*lb [type] page" + (((startingIndex + 1) / 10) + 2).ToString() + "` for more. Wanted to see this fortnight's " +
                    "scores instead of overall scores? Try typing `p*lb [type] fn[number]`!");
            }
            return stringres.ToString();
        }

    }

    public class LBUser {
        public ulong ID;
        public string username, team;
        public int score;

        public LBUser(ulong ID, string username, string team, int score) {
            this.ID = ID;
            this.username = username;
            this.team = team;
            this.score = score;
        }
    }
}
