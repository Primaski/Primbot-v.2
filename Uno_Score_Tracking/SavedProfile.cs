using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Primbot_v._2.Uno_Score_Tracking.Defs;

namespace Primbot_v._2.Uno_Score_Tracking {
    public class SavedProfile {

        public Dictionary<string, string> stats = new Dictionary<string, string>();
        public List<string> badges;
        Dictionary<string, int> placement = new Dictionary<string, int>();
        public static readonly Dictionary<string,string> desiredFields = new Dictionary<string,string>{
            {"LIT-TEAM","Team Name" },
            {"LIT-CUSTOMCOLOR","Profile Color" },
            {"POINTS-SERVER","Server Points" },
            {"ITER-UNO",BLACK_CARD_EMOJI + "Uno Games" },
            {"FIRST-UNO",YELLOW_CARD_EMOJI + "Uno Wins" },
            {"ITER-CAH",":black_joker: CAH Games" },
            {"HIGH-TETRIS","<:tetris:538223213357039617> Tetris High Score" },
            {"GAMESOVER10K-TETRIS",":books: 10,000+ Tetris Games" },
            {"ITER-MS","<:ms:538224603214905344> Minesweeper Wins" },
            {"ITER-IDLERPG",":crossed_swords: IdleRPG Wins" },
            {"ITER-POKEDUEL","<:pokeball:538222986571022346> Pokeduel Wins" },
            {"ITER-CHESS","<:chess:538223894293643264> Chess Wins" },
            {"ITER-KNIGHTS", ":crossed_swords: Knights Path Wins" },
            {"ITER-BUMPS", ":fist: Bumps" },
        };
        public ulong USER_ID;

        public SavedProfile(string[] allMappings, ulong userID, List<string> badges = null) {
            string[] keyValue;
            USER_ID = userID;
            foreach(string str in allMappings) {
                keyValue = str.Split(':');
                if(keyValue.Count() != 2) {
                    throw new Exception("False key-value mapping pair");
                }
                if (desiredFields.ContainsKey(keyValue[0])) {
                    stats.Add(keyValue[0], keyValue[1]);
                }
            }
            if(badges == null) {
                return;
            }
        }

        public Embed GetEmbed(bool includeBadges = false) {
            EmbedBuilder emb = new EmbedBuilder();
            GetPlacements();
            if (stats.ContainsKey("LIT-TEAM")) {
                if (stats["LIT-TEAM"] == TEAM_ONE_ID.ToString()) {
                    emb.WithColor(255, 179, 186);
                } else if (stats["LIT-TEAM"] == TEAM_TWO_ID.ToString()) {
                    emb.WithColor(8, 255, 0);
                } else if (stats["LIT-TEAM"] == WILD_ID.ToString()) {
                    emb.WithColor(0, 0, 0);
                } else {
                    var team = GuildCache.GetTeam(USER_ID);
                    if (team == TEAM_ONE_ID.ToString()) {
                        emb.WithColor(255, 179, 186);
                    } else if (team == TEAM_TWO_ID.ToString()) {
                        emb.WithColor(8, 255, 0);
                    } else if (team == WILD_ID.ToString()) {
                        emb.WithColor(0, 0, 0);
                    } else {
                        emb.WithColor(178, 178, 178);
                    }
                }
            }

            if (stats.ContainsKey("LIT-CUSTOMCOLOR")) {
                var hexColor = stats["LIT-CUSTOMCOLOR"];
                if (hexColor != "-" && Int32.TryParse(hexColor,System.Globalization.NumberStyles.HexNumber,null,out int ignore)) {
                    if (Int32.Parse(hexColor,System.Globalization.NumberStyles.HexNumber) < 0xFFFFFF) {
                        emb.WithColor(Byte.Parse(hexColor.Substring(0,2),System.Globalization.NumberStyles.HexNumber), 
                            Byte.Parse(hexColor.Substring(2,2), System.Globalization.NumberStyles.HexNumber), Byte.Parse(hexColor.Substring(4,2), 
                            System.Globalization.NumberStyles.HexNumber));
                    }
                }
            }

            var url = GuildCache.GetUserByID(USER_ID)?.GetAvatarUrl();
            emb.WithThumbnailUrl(url);
            StringBuilder toBeEarned = new StringBuilder();
            foreach(var stat in stats) {
                if (!stat.Key.StartsWith("LIT")) {
                    if (placement.ContainsKey(stat.Key)) {
                        if(stat.Value == "0") {
                            string add = RemoveEmojis(desiredFields[stat.Key]);
                            toBeEarned.Append(", " + add);
                            continue;
                        }
                        //emb.AddField(desiredFields[stat.Key], stat.Value + "(" + (placement[stat.Key] + 1) + "th)",true);
                        emb.AddField(desiredFields[stat.Key], stat.Value, true);
                    } else {
                        if (stat.Value == "0") {
                            string add = RemoveEmojis(desiredFields[stat.Key]);
                            toBeEarned.Append(", " + add);
                            continue;
                        }
                        emb.AddField(desiredFields[stat.Key], stat.Value,true);
                    }
                }
            }

            string tBE = toBeEarned.ToString();
            if (tBE.Length > 1) {
                emb.WithFooter("Still to be earned: " + tBE.Substring(1));
            }

            return emb.Build();
        }

        private string RemoveEmojis(string v) {
            bool openCol = false;
            bool openBrack = false;
            StringBuilder w = new StringBuilder();
            for(int i = 0; i < v.Length; ++i) {
                switch (v[i]) {
                    case ':':
                        openCol = !openCol; break;
                    case '<':
                        openBrack = true; break;
                    case '>':
                        openBrack = false; break;
                    default:
                        if (!(openBrack || openCol)) {
                            w.Append(v[i]);
                        } break;
                }
            }
            return w.ToString();
        }

        private void GetPlacements() {
            foreach (KeyValuePair<string, string> field in desiredFields) {
                if (stats.ContainsKey(field.Key)) {
                    int lineNo = SaveFiles_Mapped.GetKeyLineNumber(LEADERBOARD_DIRECTORY + "\\" + field.Key + ".txt", USER_ID.ToString());
                    if (lineNo != -1) {
                        placement.Add(field.Key, lineNo + 1); //ex: on the 0th line of save file implies 1st place
                    }
                }
            }
        }
    }
}
