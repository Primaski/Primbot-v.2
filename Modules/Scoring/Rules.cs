using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Globalization;
using static Primbot_v._2.Uno_Score_Tracking.SaveFiles_GlobalVariables;
using Primbot_v._2.Uno_Score_Tracking;
using System.Threading;

namespace Primbot_v._2.Modules.Information {
    public class Rules : ModuleBase<SocketCommandContext> {

        Dictionary<string, Embed> rules = new Dictionary<string, Embed> {
            { "uno" , uno },
            { "cah" , cah },
            { "ms" , ms },
            { "knights" , knights },
            { "tetris" , tetris },
            { "poke" , poke },
            { "idle" , idle },
            { "chess" , chess },
            { "bumps" , bumps },
            { "casino" , casino },
        };

        public static Embed uno = new EmbedBuilder().WithAuthor("Uno!")
            .WithTitle("Uno! - Scoring Procedure")
            .AddField("3 players:", "1st: 10\n 2nd: 5\n 3rd: 1", true)
            .AddField("4 players:", "1st: 15\n 2nd: 10\n 3rd: 5\n 4th: 1", true)
            .AddField("_ _", "For every additional player after that, everyone's scores are bumped up to the next 5-point tier. Start with `uno join`.", false)
            .AddField("Specifications:", "1. Only players who stay in the game until the end shall receive points.\n2. Two teams must be in the game at the START a game for it to be " +
            "considered qualifiable.\n3. Above 7 players, scoring only counts as the number of people who are still active UPON THE FIRST WINNER, with a strict floor of 7. (example: " +
            "if 9 begin, and only 6 are left upon the first person winning, it will be counted as a 7 player game.)\n4. Certain actions that prolong the game purposefully, " +
            "like using `uno callout` repeatedly are prohibited.\n5. Team or cross-team collaboration is permitted.")
            .WithColor(Color.Red).Build();
        public static Embed cah = new EmbedBuilder().WithTitle("Cards Against Humanity")
            .AddField("3 players:", "1st: 10\n 2nd: 5\n 3rd: 1", true)
            .AddField("4 players:", "1st: 15\n 2nd: 10\n 3rd: 5\n 4th: 1", true)
            .AddField("_ _", "For every additional player after that, everyone's scores are bumped up to the next 5-point tier. Start with `-cah c`.", false)
            .AddField("Specifications:", "1. Unlike Uno, this game only counts for the amount of people who finished AND anyone who achieved 5 or more points. Once you achieve 5 points, even if you leave, you're counted in the tally." +
            "\n2. Also unlike Uno, collaboration is not permitted in any form.\n" +
            "3. Ties will be rounded up to the highest scoring player in the bunch.\n4. Pinging is permitted when someone is czar, but it is preferred you use `p*cah` which automatically pings the czar.", false)
            .WithColor(Color.Red).Build();
        public static Embed ms = new EmbedBuilder().WithTitle("Minesweeper")
            .AddField("Value:", "15 points")
            .AddField("_ _", "There are 12 bombs. Type in the tile name to mark it as clear, or with an F after it to mark it. Start with `;rms`, and click the 1 emoji.")
            .AddField("Specifications:", "1. Only two games BETWEEN Regular Minesweeper (15 pts) and Minesweeper Knights (35 pts) are permitted to be submitted daily\n" +
            "2. Collaboration is permitted.")
            .WithColor(Color.Red).Build();
        public static Embed knights = new EmbedBuilder().WithTitle("Minesweeper Knights")
            .AddField("Value:", "35 points")
            .AddField("_ _", "There are 12 bombs. Type in the tile name to mark it as clear, or with an F after it to mark it. Start with `;rms`, and click the 2 emoji.")
            .AddField("Specifications:", "1. Only two games BETWEEN Regular Minesweeper (15 pts) and Minesweeper Knights (35 pts) are permitted to be submitted daily\n" +
            "2. Collaboration is permitted.")
            .WithColor(Color.Red).Build();
        public static Embed tetris = new EmbedBuilder().WithTitle("Tetris")
            .AddField("Value:", "1 point per 200 Tetris points")
            .AddField("_ _", " Start with `?game tetris`.")
            .AddField("Specifications:", "1. Only two games are permitted to be submitted daily.\n2. A maximum of 50 server points (10,000 in Tetris) can be earned from " +
            "a single game. If 10,000 is exceeded, the submitter has the option to count it as one or two games. If counted as two games, they will earn points for all extra " +
            "points above 10,000 up to 20,000, but void their ability to submit another one for the day.2")
            .WithColor(Color.Red).Build();
        public static Embed chess = new EmbedBuilder().WithTitle("Tetris")
            .AddField("Value:", "15 points (winner only)")
            .AddField("_ _", " Start with `|ng [PING]`.")
            .AddField("Specifications:", "1. Only two games are permitted to be submitted daily.\n2. This is the ONLY multiplayer minigame where two of the same team make a game eligible\n" +
            "3. Resignations are considered validly qualifaible games IF the game has progressed sufficiently, arbitrarily defined as 15 chess piece value points lost by either side.\n" +
            "4. Usage of engines is strictly prohibited, and collaboration is strongly frowned upon, but not illegal.")
            .WithColor(Color.Red).Build();
        public static Embed poke = new EmbedBuilder().WithTitle("Pokeduel")
            .AddField("Value:", "5 points (winner only)")
            .AddField("_ _", " Start with `p!duel [PING]`.")
            .AddField("Specifications:", "1. The opponent must be of the other team to qualify for points\n2. Only the winner will receive a reward after submission.\n" +
            "\n3. A maximum of six games can be submitted daily, whether you had won or lost the battle. For example, if you battle your friend 6 times, you win 4, your friend wins 2, " +
            "you've both reached your daily limit for submission.")
            .WithColor(Color.Red).Build();
        public static Embed idle = new EmbedBuilder().WithTitle("IdleRPG Duel")
            .AddField("Value:", "5 points (winner only)")
            .AddField("_ _", " Start with `+activebattle [amount] [PING]`.")
            .AddField("Specifications:", "1. The opponent must be of the other team to qualify for points\n2. Only the winner will receive a reward after submission.\n" +
            "\n3. A maximum of six games can be submitted daily, whether you had won or lost the battle. For example, if you battle your friend 6 times, you win 4, your friend wins 2, " +
            "you've both reached your daily limit for submission.\n4. Only activebattles count for points.")
            .WithColor(Color.Red).Build();
        public static Embed bumps = new EmbedBuilder().WithTitle("Bumps")
            .AddField("Value:", "3 points")
            .AddField("_ _", "Bumps our server for Disboard or similar services. Use either `!d bump` or `dlm!bump`.")
            .AddField("Specifications:", "1. Points go SPECIFICALLY to the one the bot pings or responds to upon a successful bump, no exceptions.\n2. Bumps do not need to be reported, and are the " +
            "only game scores not reported in <#483379609723863051>.")
            .WithColor(Color.Red).Build();
        public static Embed casino = new EmbedBuilder().WithTitle("Casino")
             .AddField("Value:", "First place: 100 points\nSecond place: 80 points\nThird place: 60 points")
            .AddField("_ _", "In #casino, use commands like `+work`, `+slut`, `+roulette [amount] [color]` etc. to earn or gamble money.")
            .AddField("Specifications:", "1. Points are awarded strictly at the end of a fortnight to the INDIVIDUALS in the top three.\n2. Exchanging or robbing money from active teammates is considered " +
            "cheating (suspected collusion to boost one teammate).\n3. Upon Casino reward distribution at the beginning of a fortnight (check #announcements), robberies are not permitted of the winning team " +
            "for 24 hours after distribution.")
            .WithColor(Color.Red).Build();


        [Command("scoring")]
        public async Task Scoring([Remainder] string args = null) {
            await Rule(args);
        }

        [Command("rules")]
        public async Task Rule([Remainder] string args = null) {
            string format = "Please enter a valid rule category: Uno, Cah, Minesweeper, Knights, Casino, " +
                    "Tetris, Pokeduel, IdleRPG, Chess, Bumps.";
            if (args == null) {
                await ReplyAsync(format); return;
            }
            try {
                args = args.ToLower().Trim();
                args.Replace("pokeduel", "poke");
                args.Replace("idlerpg", "idle");
                args.Replace("minesweeper", "ms");
                args.Replace("cards against humanity", "cah");
                Embed reply = rules[args];
                await ReplyAsync("", false, reply);
            } catch {
                await ReplyAsync(format);
            }
        }
    }
}
