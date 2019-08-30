using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Primbot_v._2.Uno_Score_Tracking {
    public static class SaveFiles_GlobalVariables {

        public static readonly DateTime startDate = new DateTime(2019, 1, 1, 0, 0, 0);
        public static readonly DateTime fnStartDate = new DateTime(2019, 8, 26, 0, 0, 0);
	public static readonly int EST_OFFSET = 4; //UTC would fluctuate between 4 and 5 depending on DST, EST would always be 0

        public static byte FORTNIGHT_NUMBER = 25;

        public static readonly byte GAME_TYPE_LENGTH = 2;
        public static readonly byte GAME_ITERATON_LENGTH = 5;
        public static readonly byte GAME_ID_LENGTH = 7; //type + iteration
        public static readonly byte DATE_LENGTH = 6;
        public static readonly byte USER_ID_LENGTH = 16;
        public static readonly byte SCORE_LENGTH = 2;
        public static readonly byte SEQUENCE_LENGTH = (byte)(GAME_ID_LENGTH + DATE_LENGTH + USER_ID_LENGTH
            + SCORE_LENGTH + 3);

        public static readonly byte MINESWEEPER_POINT_VALUE = 15 ;
        public static readonly byte CHESS_POINT_VALUE = 30 ;
        public static readonly byte IDLERPG_POINT_VALUE = 5 ;
        public static readonly byte POKEDUEL_POINT_VALUE = 5 ;
        public static readonly byte TOURNAMENT_POINT_VALUE = 200;
        public static readonly byte BUMP_POINT_VALUE = 3 ;
        public static readonly byte KNIGHTS_POINT_VALUE = 35 ;
        public static byte TRIVIA_POINT_VALUE = 10 ;
        public static readonly short TETRIS_SCORE_FOR_ONE_POINT = 200 ;

        public static readonly byte MINESWEEPER_DAILY_LIMIT = 2;
        public static readonly byte TETRIS_DAILY_LIMIT = 2;
        public static readonly byte POKEDUEL_DAILY_LIMIT = 6;
        public static readonly byte IDLERPG_DAILY_LIMIT = 6;
        public static readonly byte KNIGHTS_DAILY_LIMIT = 2;
        public static readonly byte CHESS_DAILY_LIMIT = 2;

        public static readonly ulong MY_ID = 263733973711192064;

        public static readonly string[] GameIden = new string[] {
            "non-standard", "uno", "cah", "minesweeper", "tetris", "pokeduel", "idlerpg",
            "bingo", "chess", "bumps", "event", "casino", "tourney", "knights", "trivia"
        };

        //METADATA
        public static readonly Dictionary<string, string> DefaultSaveFields = new Dictionary<string, string> {
            {"LIT-CachedUsername","-"},
            {"LIT-JoinedGuild","-"},
            {"LIT-Profilesymbol","-" },
            {"LIT-Profilecolor","-" },
            {"Balance","0"},
            {"LIT-Occupation","-" },
            {"LIT-AbstractCareerpath","-" },
            {"LIT-Randomevents","-" },
        };

        //UNO
        public static readonly Dictionary<string, string> UnoSaveFields = new Dictionary<string, string> {
            {"LIT-JOINDATE","-"}, {"LIT-TEAM","-"}, {"LIT-CUSTOMCOLOR","-"}, 
            {"POINTS-SERVER","0" },
            {"POINTS-UNO","0" }, {"HIGH-UNO","0" }, {"FIRST-UNO","0" }, {"ITER-UNO","0"},
            {"POINTS-CAH","0" }, {"FIRST-CAH","0" }, {"ITER-CAH","0" },
            {"POINTS-BINGO","0" }, {"HIGH-BINGO","0"}, {"ITER-BINGO","0"},
            {"POINTS-MS","0" }, {"ITER-MS","0" }, {"PLAYSTODAY-MS","0"},
            {"POINTS-TETRIS","0" }, {"HIGH-TETRIS","0" }, {"ITER-TETRIS","0" }, {"PLAYSTODAY-TETRIS","0"}, {"GAMESOVER10K-TETRIS","0"},
            {"POINTS-POKEDUEL","0" }, {"ITER-POKEDUEL","0" }, {"PLAYSTODAY-POKEDUEL","0"},
            {"POINTS-IDLERPG","0" }, {"ITER-IDLERPG","0" }, {"PLAYSTODAY-IDLERPG","0"},
            {"POINTS-CHESS","0" }, {"ITER-CHESS","0" }, {"PLAYSTODAY-CHESS","0"},
            {"POINTS-BUMPS","0" }, {"ITER-BUMPS","0" },
            {"POINTS-EVENTS","0" }, {"ITER-EVENTS","0" },
            {"POINTS-CASINO","0" }, {"ITER-CASINO","0" },
            {"POINTS-TOURNEY","0" }, {"ITER-TOURNEY","0" },
            {"POINTS-KNIGHTS","0" }, {"ITER-KNIGHTS","0" }, {"PLAYSTODAY-KNIGHTS","0"},
        };

        public static readonly List<string> LEADERBOARD_TYPES = new List<string> {
            "POINTS-SERVER", "POINTS-NS", "POINTS-UNO", "POINTS-CAH", "POINTS-BINGO", "POINTS-MS", "POINTS-TETRIS", "POINTS-POKEDUEL",
            "POINTS-IDLERPG", "POINTS-CHESS", "POINTS-BUMPS", "POINTS-EVENTS", "POINTS-CASINO", "POINTS-TOURNEY",
            "ITER-UNO", "HIGH-UNO", "FIRST-UNO",
            "ITER-CAH", "FIRST-CAH",
            "ITER-BINGO", "HIGH-BINGO",
            "ITER-MS",
            "ITER-TETRIS", "HIGH-TETRIS",
            "ITER-POKEDUEL", "ITER-IDLERPG", "ITER-CHESS", "ITER-BUMPS"
        };


        public static readonly string DEFAULT_SAVE_FILE_NAME = "Default.txt";
        public static readonly string UNO_SAVE_FILE_NAME = "Unoprofile.txt";
        public static readonly string PRIMBOT_SAVE_DIRECTORY = "..\\..";



        //---------------->COMMENT OUT BELOW WHEN UTILIZING TEST SERVER
        
        public static readonly ulong UNO_SERVER_ID = 469335072034652199;
        public static readonly ulong MAGI_SERVER_ID = 486364420847697930;
        public static readonly string RED_CARD_EMOJI = "<:UnoGreen:472812420952358922>";
        public static readonly string GREEN_CARD_EMOJI = "<:UnoRed:472812408986009600>";
        public static readonly string BLACK_CARD_EMOJI = "<:UnoADMIN:540770331426816001>";
        public static readonly string YELLOW_CARD_EMOJI = "<:UnoBlue:472812434059427850>";
        public static readonly string BLUE_CARD_EMOJI = "<:UnoYellow:472812390421889044> ";
        public static readonly string PASTEL_CARD_EMOJI = "<:UnoPastel:540518689821294603>";
        public static readonly string NEON_CARD_EMOJI = "<:UnoNeon:540518626730704906>";
        public static readonly string PASTEL_HEART = "<:pastelheart:540452749586989056>";
        public static readonly string NEON_HEART = "<:neonheart:540452749628669952>";


        public static readonly ulong TEAM_ONE_ID = 531166292724678660;
        public static readonly ulong TEAM_TWO_ID = 531166345879093249;
        public static readonly ulong GREEN_TEAM_ID = 472801252330176524;
        public static readonly ulong YELLOW_TEAM_ID = 472614570096328714;
        public static readonly ulong RED_TEAM_ID = 472614539087577099;
        public static readonly ulong BLUE_TEAM_ID = 472614593110474752;

        public static readonly ulong WILD_ID = 472613976161779722;



        public static readonly ulong REPORT_CHANNEL_ID = 537104363127046145;
        public static readonly ulong TRIVIA_CHANNEL_ID = 578026926778613760;
        
        public static List<ulong> UNO_SERVER_TEAMS = new List<ulong> {
            472801252330176524, 472614570096328714, 472614539087577099, 472614593110474752, 
        };

        public static readonly string DIR = "..\\..";
        public static readonly string UNO_SAVE_FILES_DIRECTORY = DIR + "\\Uno_Save_Files";
        public static readonly string USER_SAVE_DIRECTORY = DIR + "\\User_Save_Files";
        public static readonly string LEADERBOARD_DIRECTORY = UNO_SAVE_FILES_DIRECTORY + "\\Leaderboards";
        public static readonly string SAVEFILE_GAMEITERATIONS = UNO_SAVE_FILES_DIRECTORY + "\\Game_Iterations.txt";
        public static readonly string SAVEFILE_GAMELOGS = UNO_SAVE_FILES_DIRECTORY + "\\GameLogs.txt";
        public static readonly string SAVEFILE_FORTNIGHTDATES = UNO_SAVE_FILES_DIRECTORY + "\\Fortnight_Start_Dates.txt";
        public static readonly string COMMANDS_BY_DATE_DIRECTORY = DIR + "\\Server_Txt\\Cmdsused.txt";
        public static readonly string COMMANDS_TODAY_BACKUP_DIRECTORY = DIR + "\\Server_Txt\\Cmdexbackup.txt";
        public static readonly string MARRIAGE_FILE = DIR + "\\Server_Txt\\Marriages.txt";
        public static readonly string PURGED_SAVE_FILES_DIRECTORY = UNO_SAVE_FILES_DIRECTORY + "\\Purged";
        public static readonly string POKE = DIR + "\\Server_Txt\\Poke.txt";


        public static int cmdEx = 0;
        public static bool disableBananaBoy = false;
        public static bool cahtrack = false;
        public static bool spawntrack = true;
        public static bool spawntrackalicia = true;
        public static bool spawntrackdom = true;
        public static bool pingme = true;


        public static int slave = 0;
        public static ulong slavechannel = 0;
        public static int slavethreshold = 0;
        public static int slavevalue = 0;

        //---------------->COMMENT OUT BELOW WHEN UTILIZING UNO SERVER
        /*
        public static readonly ulong UNO_SERVER_ID = 506267529036169257;
        
        public static readonly ulong REPORT_CHANNEL_ID = 526077006270431233;
        public static List<ulong> UNO_SERVER_TEAMS = new List<ulong> {
            531667886062829579, 531667905079672852
        };

        public static readonly ulong TEAM_ONE_ID = 531667886062829579;
        public static readonly ulong TEAM_TWO_ID = 531667905079672852;
        public static readonly string RED_CARD_EMOJI = "<:unored:530193401925074944>";
        public static readonly string GREEN_CARD_EMOJI = "<:unogreen:530193380626530335>";
        public static readonly string BLACK_CARD_EMOJI = "<:unoblack:530193428919877653>";

        public static readonly string PASTEL_CARD_EMOJI = "<:unored:530193401925074944>";
        public static readonly string NEON_CARD_EMOJI = "<:unogreen:530193380626530335>";
        public static readonly string PASTEL_HEART = ":heart:";
        public static readonly string NEON_HEART = ":green_heart:";
        
        public static readonly string LEADERBOARD_DIRECTORY = "C:\\Users\\tombo\\source\\repos\\Primbot v.2\\Primbot v.2\\Uno_Save_Files_Test_Server\\Leaderboards";
        public static readonly string SAVEFILE_GAMEITERATIONS = "C:\\Users\\tombo\\source\\repos\\Primbot v.2\\Primbot v.2\\Uno_Save_Files_Test_Server\\Game_Iterations.txt";
        public static readonly string SAVEFILE_GAMELOGS = "C:\\Users\\tombo\\source\\repos\\Primbot v.2\\Primbot v.2\\Uno_Save_Files_Test_Server\\GameLogs.txt";
        public static readonly string SAVEFILE_FORTNIGHTDATES = "C:\\Users\\tombo\\source\\repos\\Primbot v.2\\Primbot v.2\\Uno_Save_Files_Test_Server\\Fortnight_Start_Dates.txt";
        public static readonly string USER_SAVE_DIRECTORY = "C:\\Users\\tombo\\source\\repos\\Primbot v.2\\Primbot v.2\\User_Save_Files_Test_Server"; 
         */
    }
}
