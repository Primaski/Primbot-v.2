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
            { "trivia", trivia }
        };

        public static Embed uno = new EmbedBuilder().WithAuthor("Uno!")
            .WithTitle("Uno! - Scoring Procedure")
            .AddField("3 players:", "1st: 15\n 2nd: 10\n 3rd: 5", true)
            .AddField("4 players:", "1st: 20\n 2nd: 15\n 3rd: 10\n 4th: 5", true)
            .AddField("_ _", "For every additional player after that, everyone's scores are bumped up to the next 5-point tier. Start with `uno join`.", false)
            .AddField("Specifications:", 
            "1. ALL game Pings MUST be sepearated by 30 minutes.\n" +
            "2. Only players who stay in the game until the end shall receive points.\n" +
            "3. Two teams must be in the game at the START a game for it to be " +
            "considered qualifiable.\n" +
            "4. Above 7 players, scoring only counts as the number of people who are still active UPON THE FIRST WINNER, with a strict floor of 7. (example: " +
            "if 9 begin, and only 6 are left upon the first person winning, it will be counted as a 7 player game.)\n" +
            "5. Certain actions that prolong the game purposefully, like using `uno callout` repeatedly are prohibited.\n6. Team or cross-team collaboration is permitted.")
            .WithColor(Color.Red).Build();
        public static Embed cah = new EmbedBuilder().WithTitle("Cards Against Humanity")
            .AddField("3 players:", "1st: 15\n 2nd: 10\n 3rd: 5", true)
            .AddField("4 players:", "1st: 20\n 2nd: 15\n 3rd: 10\n 4th: 5", true)
            .AddField("_ _", "For every additional player after that, everyone's scores are bumped up to the next 5-point tier. Start with `-cah c`.", false)
            .AddField("Specifications:", "1.ALL game Pings MUST be sepearated by 30 minutes.\n" +
            "2. Unlike Uno, this game only counts for the amount of people who finished AND anyone who achieved 5 or more points. Once you achieve 5 points, even if you leave, you're counted in the tally.\n" +
            "3. Also unlike Uno, collaboration is not permitted in any form.\n" +
            "4. Ties will be rounded up to the highest scoring player in the bunch.\n" +
            "5. Pinging is permitted when someone is czar, but it is preferred you use `p*cah` which automatically pings the czar.", false)
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
            .AddField("Specifications:", "1. Only two games are permitted to be submitted daily.\n" +
            "2. A maximum of 50 server points (10,000 in Tetris) can be earned from " +
            "a single game. If 10,000 is exceeded, the submitter has the option to count it as one or two games. If counted as two games, they will earn points for all extra " +
            "points above 10,000 up to 20,000, but void their ability to submit another one for the day.2")
            .WithColor(Color.Red).Build();
        public static Embed chess = new EmbedBuilder().WithTitle("Chess")
            .AddField("Value:", "30 points (winner), 10 points (loser)")
            .AddField("_ _", " Start with `|ng [PING]`.")
            .AddField("Specifications:", "1. Only two games are permitted to be submitted daily. Both players must be of different teams\n" +
            "2. The loser will receive 10 points (for now, point managers, use " +
            "`p*logns`. A draw also counts as 10 points for each player. \n" +
            "3. Resignations are considered validly qualifaible games IF the game has progressed sufficiently, arbitrarily defined as 15 chess piece value points lost by either side.\n" +
            "4. Usage of engines is strictly prohibited, and collaboration is strongly frowned upon, but not illegal.")
            .WithColor(Color.Red).Build();
        public static Embed poke = new EmbedBuilder().WithTitle("Pokeduel")
            .AddField("Value:", "5 points (winner only)")
            .AddField("_ _", " Start with `p!duel [PING]`.")
            .AddField("Specifications:", "1. The opponent must be of the other team to qualify for points\n" +
            "2. Only the winner will receive a reward after submission.\n" +
            "3. A maximum of six games can be submitted daily, whether you had won or lost the battle. For example, if you battle your friend 6 times, you win 4, your friend wins 2, " +
            "you've both reached your daily limit for submission.")
            .WithColor(Color.Red).Build();
        public static Embed idle = new EmbedBuilder().WithTitle("IdleRPG Duel")
            .AddField("Value:", "5 points (winner only)")
            .AddField("_ _", " Start with `$activebattle [amount] [PING]`.")
            .AddField("Specifications:", "1. The opponent must be of the other team to qualify for points\n" +
            "2. Only the winner will receive a reward after submission.\n" +
            "3. A maximum of six games can be submitted daily, whether you had won or lost the battle. For example, if you battle your friend 6 times, you win 4, your friend wins 2, " +
            "you've both reached your daily limit for submission.\n" +
            "4. Only activebattles count for points.")
            .WithColor(Color.Red).Build();
        public static Embed bumps = new EmbedBuilder().WithTitle("Bumps")
            .AddField("Value:", "3 points")
            .AddField("_ _", "Bumps our server for Disboard or similar services. Use either `!d bump` or `dlm!bump`.")
            .AddField("Specifications:", "1. Points go SPECIFICALLY to the one the bot pings or responds to upon a successful bump, no exceptions.\n" +
            "2. Bumps do not need to be reported, and are the " +
            "only game scores not reported in <#537104363127046145>.")
            .WithColor(Color.Red).Build();
        public static Embed casino = new EmbedBuilder().WithTitle("Casino")
             .AddField("Value:", "First place: 100 points\nSecond place: 80 points\nThird place: 60 points")
            .AddField("_ _", "In #casino, use commands like `+work`, `+slut`, `+roulette [amount] [color]` etc. to earn or gamble money.")
            .AddField("Specifications:", "1. Points are awarded strictly at the end of a fortnight to the INDIVIDUALS in the top three.\n" +
            "2. Exchanging or robbing money from active teammates is considered " +
            "cheating (suspected collusion to boost one teammate).\n3. Upon Casino reward distribution at the beginning of a fortnight (check #announcements), robberies are not permitted of the winning team " +
            "for 24 hours after distribution.")
            .WithColor(Color.Red).Build();
        public static Embed tourneygameplay1 = new EmbedBuilder().WithTitle("Tourney")
            .AddField("Value:", "Point value varies depending on host. Typically, it will look similar to this, however: Winner - 200 points, Runnerup - 100 points, Semifinalists - 50 points")
            .AddField("_ _","Special event that occurs typically with a 2 to 6 week gap, often with Nitro giveaways. Channels for contest are at the bottom.")
            .AddField("Specifications (1):", 
            "1. Bracketting size and methodology is chosen by the host, typically single-elimination with 8 participants, and 2 players from each team.\n" +
            "2. Preliminatory rounds are held between members of each team beforehand, by playing a SINGLE uno match. The number of winners that qualify depends on bracket size. This, as all matches apart from the final round, are bounded by a TIME CONSTRAINT held " +
            "by the host, and if an insufficient number of winners has been dictated after time has elapsed, the players with the FEWEST CARDS in descending order will be chosen. If there are ties, cards will be played until there are no longer ties.\n"
         + "3. Regular rounds are 1v1's that are also held to time constraints, and follows the same system as preliminary rounds. While the number of matches played in regular rounds is dictated strictly by the host, it is usually best 2 out of 3.\n").WithColor(Color.Red).Build();
        public static Embed tourneygameplay2 = new EmbedBuilder().AddField("Specifications (2):", 
            "4. The final round is also 1v1, and typically best 3 out of 5. The winner will hoist a special role, the largest sum of points, and Nitro if it is being offered.\n" 
            ).WithColor(Color.Red).Build();
        public static Embed trivia = new EmbedBuilder().WithTitle("Trivia")
            .AddField("Value:", "8 points per correct answer (may vary by trivia)")
            .AddField("_ _", "Special event that occurs biweekly in a channel near the top of the server, hosted almost entirely by a bot. Answers need only be shouted out, categories sought by `p*categories`.")
            .AddField("Specifications:",
            "1. SIGNING UP DOES __NOT__ DEMAND ATTENDANCE. It is only for a ping. You cannot be warned for not showing up.\n" +
            "2. You can enter at any time, whether you signed up or not, and immediately be qualifiable.\n" +
            "3. Please ensure you shout out the raw answer, devoid of spelling errors and with no punctuation. The bot dictates who has the correct answer, and overrides are dictated by moderators, but are done on a basis of bot failure." +
            "4. Points may be rewarded after the fact to account for bot errors." +
            "5. PLEASE DO NOT CHEAT BY LOOKING UP ANSWERS.")
            .WithColor(Color.Red).Build();
        public static Embed tourney = new EmbedBuilder().WithTitle("Tourney").AddField("Specifications (1):",
            "1. SIGNING UP __ABSOLUTELY REQUIRES__ THAT YOU SHOW UP. Not showing up OR being late (also considered \"no-show\") messes up bracketting - there will be pings leading up to it for anyone that signs up (gives you a special role), so there is no excuse. " +
            "Your first no-show will result in a warning and a tagged role. Your second no-show will result " +
            "in another warning, and a disqualification from the following tournament. The third no-show will result in a PERMANENT DISQUALIFICATION from ALL uno tournaments at a minimum.\n" +
            "2. REGULAR UNO RULES APPLY. As in, repeatedly using `uno callout` to gain cards, prolonging the game etc. is strictly prohibited and can get you disqualified. Please do `p*rules uno` for more information, or ask a moderator.\n")
            .WithColor(Color.Red).Build();
        public static Embed tourney2 = new EmbedBuilder().AddField("Specifications (2):",
            "3. REGULAR SERVER RULES APPLY. Bad behavior, pinging, spamming etc. may result in disqualification. Ask a mod if you are not sure, as they will be held responsible for misinformation.\n" +
            "4. This only details the CONDUCT of the Tourney. For rules on gameplay, please type `p*rules tourney gameplay`.").WithColor(Color.Red).Build();
    

        [Command("scoring")]
        public async Task Scoring([Remainder] string args = null) {
            await Rule(args);
        }

        [Command("rules")]
        public async Task Rule([Remainder] string args = null) {
            string format = "Please enter a valid rule category: Uno, Cah, Minesweeper, Knights, Casino, " +
                    "Tetris, Pokeduel, IdleRPG, Chess, Bumps, Trivia, Tourney, Tourney gameplay.";
            if (args == null) {
                await ReplyAsync(format); return;
            }
            try {
                args = args.ToLower().Trim();
                args = args.Replace("pokeduel", "poke");
                args = args.Replace("idlerpg duel", "idle");
                args = args.Replace("idlerpg", "idle");
                args = args.Replace("minesweeper", "ms");
                args = args.Replace("cards against humanity", "cah");
                args = args.Replace("uno tournament", "tourney");
                args = args.Replace("tournament", "tourney");
                args = args.Replace("uno tourney", "tourney");
                args = args.Replace("friday trivia", "trivia");
                args = args.Replace("tourney gameplay", "tourneygameplay");
                if(args == "tourney") {
                    await ReplyAsync("", false, tourney);
                    await ReplyAsync("", false, tourney2);
                    return;
                }
                if(args == "tourneygameplay") {
                    await ReplyAsync("", false, tourneygameplay1);
                    await ReplyAsync("", false, tourneygameplay2);
                    return;
                }
                Embed reply = rules[args];
                await ReplyAsync("", false, reply);
            } catch (Exception e) {
                await ReplyAsync(format);
            }
        }
    }
}
