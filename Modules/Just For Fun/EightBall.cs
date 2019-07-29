using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Primbot_v._2.Modules.Copypastas {
    public class EightBall : ModuleBase<SocketCommandContext> {
        string[] responses = { "Not a chance in hell.",
        "I mean, probably?",
        "Absolutely. 100% chance.",
        " *moans sexually* Sorry, I was distracted, can you ask it again?",
        "Ahahahaha. Ahahahaha. No.",
        "To be honest, that's a stupid question.",
        "YES!",
        "Only on Thursdays.",
        "I highly doubt it.",
        "I'm 99% sure the answer is yes.",
        "I'm 99% sure the answer is no.",
        "Why would you even ask such a thing?",
        "Hell yeah, my dude.",
        "Hell no, my dude."};

        [Command("8ball", RunMode = RunMode.Async)]
        public async Task TaskAsync([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            Random rando = new Random();
            int rand = rando.Next(0, responses.Length);
            if (args == null) {
                await ReplyAsync("You should probably ask the 8 ball something. It's not a mind reader. :eyes:");
            } else {
                await ReplyAsync(Context.User.Username + " is wondering: **" + args + "** \n\n" +
                    "Psychic Prim thinks: **" + responses[rand] + "**");
            }
        }

        [Command("choose", RunMode = RunMode.Async)]
        public async Task Choose([Remainder] string args = null) {
            if (args == null) {
                await ReplyAsync("Format: `p*choose [CHOICE]|[CHOICE]|...");
            } else if (args.Contains('|')) {
                await ReplyAsync("The format has changed. Please use `or` instead of `|` to separate your " +
                    "arguments. Example: `p*choose red or blue`"); return;
            }else if(args.Split(new string[] { " or" }, StringSplitOptions.None).Count() <= 1) {
                await ReplyAsync("Please provide at least two choices. Example: `p*choose red or blue`"); return;
            }
            string[] choices = args.Split(new string[] { " or " }, StringSplitOptions.None);
            Random rand = new Random();
            int newno = rand.Next(0, choices.Count());
            await ReplyAsync("I choose... **" + choices[newno] + "**!");
        }


        [Command("perfectyahtzee", RunMode = RunMode.Async)]
        public async Task Yahtzee([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            string help = "Format: `p*perfectyahtzee [number]`\n\n" +
                    "Meaning: I am trying to achieve a PERFECT yahtzee. I will roll 5 dice at the same time. I am trying to get all 5 " +
                    "dice to come up with the same number. The chances of this are roughly 1 in 1,296. Meaning... if you type " +
                    "`p*perfectyahtzee 1`, the AVERAGE number of trials it will take is 1,296.\n" +
                    "Now, for fun, I made it so it will try to get `[number]` perfect yahtzees in a ROW. The average number of rolls this " +
                    "takes goes exponentially up, so it's recommended to use a number 1-3 only. If it takes over 100,000,000 tries, " +
                    "it will give up. Note that it will probably take a very long time to respond, so it will ping you.";
            string yahtzeeImage = "https://cf.ltkcdn.net/boardgames/images/std/143997-425x283-yahtzee.jpg";
            string fiveDie = "<:die:545284312228954123> <:die:545284312228954123> <:die:545284312228954123> <:die:545284312228954123> <:die:545284312228954123>\n";
            if (args == null) {
                await ReplyAsync(help); return;
            }
            if (!Int32.TryParse(args, out int ignore)) {
                await ReplyAsync(help); return;
            }
            ulong ID = Context.User.Id;
            if (Uno_Score_Tracking.GuildCache.yahtzeebois.Where(x => x == ID).Count() > 9) {
                await ReplyAsync(Context.Message.Author.Mention + "\nSlow down! You already have 10 requests!"); return;
            }
            Uno_Score_Tracking.GuildCache.yahtzeebois.Add(ID);
            int noOfTimes = Int32.Parse(args);
            var outcome = RollAPerfectYahtzee(5, noOfTimes);
            int result = outcome.Item1;
            if (result == -1) {
                await ReplyAsync(Context.Message.Author.Mention + "\n" + fiveDie + "It took me too long. I tried 100,000,000 times to get " +
                    noOfTimes + " yahtzees in a row and I have failed :( Try again, or ask less of me.");
            } else if (noOfTimes == 1) {
                await ReplyAsync(Context.Message.Author.Mention + "\n" + fiveDie + "It took me " + result.ToString("#,##0") + " rolls to get a perfect Yahtzee!");
            } else if (noOfTimes == 2) {
                await ReplyAsync(Context.Message.Author.Mention + "\n" + fiveDie + fiveDie + "It took me " + result.ToString("#,##0") + " rolls to get " + noOfTimes + " perfect Yahtzees" +
                    " in a row!");
            } else if (noOfTimes == 3) {
                await ReplyAsync(Context.Message.Author.Mention + "\n" + fiveDie + fiveDie + fiveDie + "It took me " + result.ToString("#,##0") + " rolls to get " + noOfTimes + " perfect Yahtzees" +
                    " in a row!");
            } else {
                await ReplyAsync(Context.Message.Author.Mention + "\n" + fiveDie + "It took me " + result.ToString("#,##0") + " rolls to get a perfect Yahtzee!");
            }
            Uno_Score_Tracking.GuildCache.yahtzeebois.Remove(ID);
            return;
        }

        public static Tuple<int, List<int[]>> RollAPerfectYahtzee(int noOfDice = 5, int numberOfYahtzeesInARow = 1, bool printEachRoll = false) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            int iteration = 0;
            int[] dice = new int[noOfDice];
            List<int[]> res = new List<int[]>();
            Random rand = new Random();
            int numberOfYahtzeesLeft = numberOfYahtzeesInARow;
            while (numberOfYahtzeesLeft > 0) {
                iteration++;
                bool yahtzee = true;
                int prevNo = rand.Next(1, 7);
                int currNo;
                dice[0] = prevNo;
                for (int i = 1; i < noOfDice; i++) {
                    currNo = rand.Next(1, 7);
                    dice[i] = currNo;
                    if (currNo != prevNo) {
                        yahtzee = false;
                    }
                }
                if (yahtzee) {
                    numberOfYahtzeesLeft--;
                    res.Add(dice);
                } else {
                    numberOfYahtzeesLeft = numberOfYahtzeesInARow;
                }
                if (printEachRoll) {
                    Console.Write(iteration + ":");
                    Console.WriteLine("[{0}]", string.Join(", ", dice));
                }
                if (iteration > 100000000) {
                    return Tuple.Create(-1, res);
                }
            }
            return Tuple.Create(iteration, res);
        }
    }
}
