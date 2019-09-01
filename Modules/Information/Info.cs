using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.IO;
using Discord.WebSocket;
using System.Threading;

namespace Primbot_v._2.Modules.Just_For_Fun {
    public class Info : ModuleBase<SocketCommandContext> {

        Embed unogen = new EmbedBuilder().WithColor(Color.Blue).WithAuthor("General Uno Commands")
           .WithDescription("p*command [mandatory] (optional) - lowercase type literally.")
            .AddField("p*lb (CATEGORY) (page #) (fn #)/(overall)",
                "Shows leaderboard in current fortnight, unless otherwise specified. Categories will narrow it down by " +
            "game (Categories: uno, cah, minesweeper, tetris, knights, pokeduel, idlerpg, bumps)")
            .AddField("p*score (CATEGORY) (USER) (fn #) (overall)", "By default, shows the current user's score " +
            "this fortnight. Can limit by category, see above for categories.")
            .AddField("p*profile (USER)", "Shows current user's profile, listing out comprehensive overall stats for " +
            "games in a neat fashion.")
            .AddField("p*rules [GAME]", "List out rules and scoring for an Uno Server game (under construction)")
            .AddField("p*spc [HEX COLOR VALUE]", "Changes the color of your profile")
            .AddField("p*badges (USER)", "Lists out badges that you have on your profile by default (under construction)")
            .AddField("p*cah", "Pings users during their turn. Should be called before the start of a CAH game. p\\*cahend to terminate.")
            .AddField("p*fnend", "How long until the end of the FN?")
            .Build();

        Embed unoadmin = new EmbedBuilder().WithColor(Color.Blue).WithAuthor("Administrative Uno Commands - Point Managers and Above")
           .WithDescription("p*command [mandatory] (optional)")
            .AddField("p*loguno [MESSAGE]", "Manual log of an Uno game. Paste the win message literally.")
            .AddField("p*logcah [USER],[USER]...", "Manual log of a CAH game. List users from highest to lowest ranking, separated by commas. Put multiple " +
            "users in parentheses if they tied.")
            .AddField("p*log[TYPE] [USER],(USER)...",
                "The way to log all user minigames. List up to ten, separated by a comma. Type username, ping, or ID. (Types: " +
            "ms, knights, poke, idle, tourney, bump").
            AddField("p*logtetris [AMOUNT] [USER]",
                "Log Tetris by listing the score and then the user. Cannot do several at a time.")
            .AddField("p*logns [AMOUNT] [USER]", "Log a general constant score for a user, without a category.")
            .AddField("p*revert [GAME ID]", "Revert a previous log's score. Check in #game-logs for Game ID number.")
            .AddField("p*killswitch [mute/kick/ban]", "Automatically perform this action against all new users in the case of a raid.")
            .AddField("p\\*newday / p\\*newfn", "The first command should be run at midnight (GMT -4), the other at the end of a fortnight.")
            .Build();

        Embed reaccs = new EmbedBuilder().WithColor(Color.Blue).WithAuthor("Reactions")
           .WithDescription("p*command [mandatory] (optional)")
            .AddField("p*marry [USER]", "Marry someone! Will be saved. p\\*mylove to see who you married, p\\*divorce to leave them.", true)
            .AddField("p*hug [USER]", "Hugs!", true)
            .AddField("p*kiss [USER]", "Kisses!", true)
            .AddField("p*punch [USER]", "Punches!", true)
            .AddField("p*headpat [USER]", "Headpats!", true)
            .AddField("p*handshake [USER]", "Handshakes...", true)
            .AddField("p*slap [USER]", "Slaps!", true)
            .AddField("p*breed [USER]", "U-Uhm...", true)
            .AddField("p*gooutside [USER]", "You'll see!", true)
            .AddField("p*choose [CHOICE] or [CHOICE] or ...", "Make Primbot choose for you!", true)
            .AddField("p*8ball [QUESTION]", "What does Primbot think?", true)
            .AddField("p*uwu", "Uwu", true)
            .AddField("p*navyseal (uwu)", "Recite the (uwu) Navy Seal", true)
            .AddField("p*lenny", "yes", true)
            .AddField("p*bananaboy", "Abs...", true)
            .Build();

        Embed bot = new EmbedBuilder().WithColor(Color.Blue).WithAuthor("General Bot Commands")
           .WithDescription("p*command [mandatory] (optional)")
            .AddField("p\\*info/p\\*invite", "Invite link and info about the bot.", true)
            .AddField("p\\*suggest/p\\*report [MESSAGE]", "Report an error or suggestion, to be posted in a channel in my local server.", true)
            .AddField("p*ing", "Ping!", true)
            .AddField("p*embed", "Make your own custom embeds! Type `p*embed` to learn how.", true)
            .Build();


        [Command("info", RunMode = RunMode.Async)]
        public async Task GetInfo() {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await ReplyAsync(
                "***Bot Name:*** Primaski Bot#4325\n" +
                "***Bot Creator:*** Primaski#0826\n" +
                "***Bot Creation Date:*** 2018/09/07\n" +
                "***Bot Prefix:*** p*\n" +
                "***Version:*** 2.0, 2019/1/21\n" +
                "***Written in:*** C#\n" +
                "***Bot Token:*** u wish\n" +
                "***Source code:*** https://github.com/Primaski/Primbot-v.2 \n" +
                "***Bot Invite Link:*** No longer publicly invitable \n");
        }

        [Command("invite", RunMode = RunMode.Async)]
        public async Task Invite() {
            await GetInfo();
        }

        [Command("categories", RunMode = RunMode.Async)]
        public async Task Categories([Remainder] string args = null) {
            await ReplyAsync("**Gaming,\nSports,\nScience,\nGeography,\nHistory,\nLanguage,\nEntertainment**");
        }

	[Command("code", RunMode = RunMode.Async)]
	public async Task Code([Remainder] string args = null){ await GetInfo(); }

	[Command("github", RunMode = RunMode.Async)]
	public async Task Coder([Remainder] string args = null){ await GetInfo(); }


        [Command("say", RunMode = RunMode.Async)]
        public async Task Say([Remainder] string args = null) {
            //await ReplyAsync("Disabled for Trivia Night.");
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args == null) {
                await ReplyAsync("I mean, like, what do you want me to say?");
            } else {
                if (args.Contains("p*")) {
                    await ReplyAsync("Why are you trying to break me?");
                    return;
                }
                if (args.Contains("@everyone") || args.Contains("@here")) {
                    await ReplyAsync("Say that again, but without the ping.");
                    return;
                }
                if (args.ToLower() == "uno") {
                    await ReplyAsync("<:UnoRed:472812408986009600> <:UnoYellow:472812390421889044> <:UnoGreen:472812420952358922> <:UnoBlue:472812434059427850>"); return;
                }
                await Context.Message.DeleteAsync();
                await ReplyAsync(args);

            }
        }

        [Command("ping", RunMode = RunMode.Async)]
        public async Task Ping() {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await ReplyAsync("Pong! My latency is `" + Context.Client.Latency + " ms`!");
        }

        [Command("ing", RunMode = RunMode.Async)]
        public async Task Pingv2() {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await ReplyAsync("P*ong! My latency is `" + Context.Client.Latency + " ms`!");
        }

        [Command("ong", RunMode = RunMode.Async)]
        public async Task Pongv2() {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await ReplyAsync("P*ing! My latency is `" + Context.Client.Latency + " ms`!");
        }

        [Command("pong", RunMode = RunMode.Async)]
        public async Task Pong() {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await ReplyAsync("Ping! My latency is `" + Context.Client.Latency + " ms`!");
        }

        [Command("report", RunMode = RunMode.Async)]
        public async Task Report([Remainder] string args = null) {
            var x = ReportToMagi(args, false).Result;
            if (x == null) {
                return;
            }
            await ReplyAsync("", false, x);
            await ReplyAsync("The above message has been reported in the Magilouvre to Prim for review. Please note "
                + "that the bot is undergoing a lot of changes currently, and suggestions may not be attended to immediately.");
        }

        [Command("suggest")]
        public async Task Suggest([Remainder] string args = null) {
            var x = ReportToMagi(args, true).Result;
            if (x == null) {
                return;
            }
            await ReplyAsync("", false, x);
            await ReplyAsync("The above message has been reported in the Magilouvre to Prim for review. Please note "
                + "that the bot is undergoing a lot of changes currently, and suggestions may not be attended to immediately.");
        }

        public async Task<Embed> ReportToMagi(string args, bool suggest = true) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args == null) {
                await ReplyAsync("What do you want to suggest? Please include only text"); return null;
            }
            if (args.Contains("@")) {
                args = (args.Replace("@", ""));
            }
            SocketTextChannel channel = null;
            try {
                channel = (SocketTextChannel)Uno_Score_Tracking.GuildCache.Magi_Cache.GetChannel(580565141565997056);
            } catch {
                await ReplyAsync("Magilouvre's Cache was not properly loaded. Try again later."); return null;
            }

            EmbedBuilder emb = new EmbedBuilder();
            emb.WithColor(00, 0xff, 0xff);
            emb.WithTitle("Suggestion");
            if (!suggest) {
                emb.WithColor(0xff, 00, 00);
                emb.WithTitle("Error Report");
            }
            emb.WithThumbnailUrl(Context.User.GetAvatarUrl());
            emb.WithAuthor(Context.User.Username + " (" + Context.User.Id + ")");
            emb.WithDescription(args);
            emb.WithFooter("From: " + (Context.Guild.Name).ToUpper() + " (" + Context.Guild.Id + ")");
            emb.WithCurrentTimestamp();
            Embed final = emb.Build();
            try {
                await channel.SendMessageAsync("<@!" + Uno_Score_Tracking.Defs.MY_ID + ">", false, final);
            } catch (Exception e) {
                await ReplyAsync("Error posting: " + e.Message);
            }
            return final;
        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task Help([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args != null) {
                switch (args) {
                    case "uno": await ReplyAsync("", false, unogen); break;
                    case "admin": break;
                    case "bot":
                    case "general": await ReplyAsync("", false, bot); break;
                    case "reactions": await ReplyAsync("", false, reaccs); break;
                }
            }
            var msg = await Context.Channel.SendMessageAsync("**Please choose a Help category:**\n" +
                "\n<:UnoRed:472812408986009600>  *Uno Server* <:UnoRed:472812408986009600> " +
                "\n<:UnoADMIN:540770331426816001>  *Uno Admin* <:UnoADMIN:540770331426816001> " +
                "\n<:uuwuu:580852963371646996> *Reactions* <:uuwuu:580852963371646996>" +
                "\n:gear: *General Bot* :gear:");
            Emote.TryParse("<:UnoRed:472812408986009600>", out var blueUno);
            Emote.TryParse("<:UnoADMIN:540770331426816001>", out var blackUno);
            Emote.TryParse("<:uuwuu:580852963371646996>", out var uwu);
            Emote.TryParse("<:legear:580865450850779154>", out var gear);
            Server.Channel.AddReactions(msg, new List<Emote> { blueUno, blackUno, uwu, gear });
            Discord.Emote reaccType = null;
            for (int i = 0; i < 150; i++) {
                var reaction = Server.Channel.GetFirstReaction(msg, new List<Emote> { blueUno,
                    blackUno, uwu, gear }, Context.User.Id);
                if (reaction != null) {
                    reaccType = reaction.Item2;
                    break;
                }
                Thread.Sleep(100);
                i++;
            }
            if (reaccType == null) {
                await ReplyAsync("The help request has timed out."); return;
            }
            switch (reaccType.Name.ToLower()) {
                case "unored":
                    await ReplyAsync("", false, unogen);
                    break;
                case "unoadmin":
                    await ReplyAsync("", false, unoadmin);
                    break;
                case "uuwuu":
                    await ReplyAsync("", false, reaccs);
                    break;
                case "legear":
                    await ReplyAsync("", false, bot);
                    break;
            }

            return;
        }

        [Command("teamcount")]
        public async Task TeamCount() {
            try {
                int blue, red, green, yellow, none;
                blue = red = green = yellow = none = 0;
                var guild = Uno_Score_Tracking.GuildCache.Uno_Cache.Users;
                await ReplyAsync("`Counting...`");
                for (int i = 0; i < guild.Count(); i++) {
                    if (guild.ElementAt(i).IsBot) {
                        continue;
                    }
                    var team = Uno_Score_Tracking.GuildCache.GetTeam(guild.ElementAt(i)) ?? "none";
                    switch (team) {
                        case "472801252330176524": green++; break;
                        case "472614570096328714": yellow++; break;
                        case "472614539087577099": red++; break;
                        case "472614593110474752": blue++; break;
                        default: none++; break;
                    }
                }
                await ReplyAsync(":green_heart: " + green + "\n:blue_heart: " + blue +
                    "\n:yellow_heart: " + yellow + "\n:hearts: " + red + "\nNone: " + none);
            } catch (Exception e) {
                await ReplyAsync(e.Message);
            }

        }

        [Command("fnend")]
        public async Task FNEnd() {
            var fnend = Uno_Score_Tracking.Defs.fnStartDate.AddDays(14);
            TimeSpan timesp = fnend.Subtract(DateTime.Now);
            if (timesp.Ticks < 0) {
                await ReplyAsync("Error retrieving. Fortnight end date has already passed."); return;
            } else if (timesp.TotalDays < 1) {
                await ReplyAsync("The fortnight ends tonight. (GMT -4)"); return;
            } else if (timesp.TotalDays < 2) {
                await ReplyAsync("The fortnight ends tomorrow. (GMT -4)"); return;
            }
            await ReplyAsync("Fortnight " + Uno_Score_Tracking.Defs.FORTNIGHT_NUMBER
                + " ends in " + ((int)timesp.TotalDays / 7) + " week and " +
                (int)(timesp.TotalDays % 7) + " days.");
            return;
        }
    }
}
