using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Threading;

namespace Primbot_v._2.Modules.Copypastas {
    public class React : ModuleBase<SocketCommandContext> {

        [Command("marry", RunMode = RunMode.Async)]
        public async Task Marry([Remainder] string args = null) {
            if (args == null) {
                await ReplyAsync("Tell me who you want to marry, then!");
                return;
            }
            string val = Uno_Score_Tracking.SaveFiles_Mapped.SearchMappedSaveFile(
                Uno_Score_Tracking.SaveFiles_GlobalVariables.MARRIAGE_FILE, Context.User.Id.ToString());
            if (val != null) {
                await ReplyAsync("Aren't you already married to <@" + val + ">? Don't make me tell them you're " +
                    "trying to have an affair! Divorce if you're not happy!");
            } else {
                List<SocketGuildUser> x = Uno_Score_Tracking.GuildCache.InterpretUserInput(args, Context.Guild);
                if (x[0] == null) {
                    await ReplyAsync("That's not a proper user! Who do you *really* want to marry?");
                    return;
                }
                var theMarried = x[0];
                if (theMarried.Id == Context.User.Id) {
                    await ReplyAsync("Please don't marry yourself :/");
                    return;
                } else if (theMarried.Id == 487718576892018689) {
                    await ReplyAsync("S-Sorry, I'm flattered, but I think you can find someone better than me.");
                    return;
                }
                string w = Uno_Score_Tracking.SaveFiles_Mapped.SearchMappedSaveFile(
                Uno_Score_Tracking.SaveFiles_GlobalVariables.MARRIAGE_FILE, theMarried.Id.ToString());
                if (w != null) {
                    await ReplyAsync(theMarried.Username + " is already married!!");
                    return;
                }
                var msg = await Context.Channel.SendMessageAsync("Love is blooming! " + theMarried.Mention + " , do you accept?");
                Emoji emoji = new Emoji("\u2665");
                await msg.AddReactionAsync(emoji);
                bool reacc = false;
                for (int i = 0; i < 100 && !reacc; i++) {
                    var reactions = await msg.GetReactionUsersAsync(emoji, 5);
                    foreach (var reactionUser in reactions) {
                        if (reactionUser.Id == theMarried.Id) {
                            reacc = true;
                            break;
                        }
                    }
                    Thread.Sleep(100);
                    i++;
                }
                if (!(reacc)) {
                    await ReplyAsync("They did not accept in time! I'm sorry!"); return;
                }
                await ReplyAsync("https://media.giphy.com/media/3QAUnTsafSMQE/giphy.gif");
                await ReplyAsync("They've accepted! Let it be known that " + Context.User.Username + " and " + theMarried.Username + " are married!\nType `p*mylove` to show off your love to everyone else!");
                Uno_Score_Tracking.SaveFiles_Mapped.NewLineMappedSaveFile(Uno_Score_Tracking.SaveFiles_GlobalVariables.MARRIAGE_FILE,
                    Context.User.Id.ToString(), theMarried.Id.ToString());
                Uno_Score_Tracking.SaveFiles_Mapped.NewLineMappedSaveFile(Uno_Score_Tracking.SaveFiles_GlobalVariables.MARRIAGE_FILE, theMarried.Id.ToString(),
                    Context.User.Id.ToString());
            }
            return;
        }

        [Command("mylove")]
        public async Task Spouse([Remainder] string args = null) {
            string w = Uno_Score_Tracking.SaveFiles_Mapped.SearchMappedSaveFile(
                Uno_Score_Tracking.SaveFiles_GlobalVariables.MARRIAGE_FILE, Context.User.Id.ToString());
            if (w == null) {
                await ReplyAsync("Sorry! You're forever alone!"); return;
            }
            await ReplyAsync("You're married to <@" + w + ">!");
        }

        [Command("divorce")]
        public async Task Divorce([Remainder] string args = null) {
            string w = Uno_Score_Tracking.SaveFiles_Mapped.SearchMappedSaveFile(
                Uno_Score_Tracking.SaveFiles_GlobalVariables.MARRIAGE_FILE, Context.User.Id.ToString());
            if (w == null) {
                await ReplyAsync("You can't divorce if you're not married! Or... can you?");
            } else {
                var msg = await Context.Channel.SendMessageAsync("Are you SURE you wish to divorce <@" + w + ">?");
                Emoji emoji = new Emoji("\u2665");
                await msg.AddReactionAsync(emoji);
                bool reacc = false;
                for (int i = 0; i < 100 && !reacc; i++) {
                    var reactions = await msg.GetReactionUsersAsync(emoji, 5);
                    foreach (var reactionUser in reactions) {
                        if (reactionUser.Id == Context.User.Id) {
                            reacc = true;
                            break;
                        }
                    }
                    Thread.Sleep(100);
                    i++;
                }
                if (!(reacc)) {
                    await ReplyAsync("You didn't answer in time! Guess you changed your mind."); return;
                }
                await ReplyAsync("You are now loveless. I'm sorry for your loss. Or theirs. Hm.");
                await ReplyAsync("Remind Prim to make a `RemoveFieldValue` function, cause uhm, UR NOT ACTUALLY DIVORCED LOL");
            }
            return;
        }

        [Command("reactions", RunMode = RunMode.Async)]
        public async Task Reactions([Remainder] string args = null) {
            await Reaction();
        }

        [Command("react", RunMode = RunMode.Async)]
        public async Task Reaction([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await ReplyAsync("Reactions:\n`p*hug [user]`\n`p*headpat [user]`\n`p*kiss [user]`\n`p*slap [user]`\n`p*punch [user]`\n`p*handshake [user]`\n`p*breed [user]`\n`p*marry [user]` (special)");
        }

        [Command("hug", RunMode = RunMode.Async)]
        public async Task Hug([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args.Contains("@everyone") || args.Contains("@here")) {
                await ReplyAsync("Please say it again, but without the ping.");
                return;
            }
            string[] hugEmotes = {
            "https://pre00.deviantart.net/9d78/th/pre/f/2018/191/d/e/magilou_about_to_hug_ya_by_buuwad-dcgt3rf.png",
            "https://i.imgur.com/V706MEl.gif",
            "https://tenor.com/view/hug-anime-gif-11074788",
            "https://i.imgur.com/r9aU2xv.gif",
            "https://66.media.tumblr.com/21f89b12419bda49ce8ee33d50f01f85/tumblr_o5u9l1rBqg1ttmhcxo1_500.gif",
            "https://i.imgur.com/wOmoeF8.gif",
            "https://thumbs.gfycat.com/BlindOblongAmurratsnake-small.gif"};

            Random rand = new Random();
            string randomHugEmote = hugEmotes[rand.Next(0, hugEmotes.Length)];
            if (args == null) {
                await ReplyAsync("Aw, need a hug? Here's a hug, " + Context.User.Username + "!");
            } else if ((args.ToLower()).Contains("primaski bot")) {
                await ReplyAsync("You... want me to hug myself...?");
                return;
            } else if ((args.ToLower()).Contains("prim") || args.ToLower().Contains(Uno_Score_Tracking.SaveFiles_GlobalVariables.MY_ID.ToString())) {
                await ReplyAsync("Prim likes hugs >w< uwu");
            } else {
                await ReplyAsync("Aw! Here's a hug, " + args + "!");
            }
            await ReplyAsync("\n" + randomHugEmote);
            return;
        }


        [Command("handshake", RunMode = RunMode.Async)]
        public async Task Handshake([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args.Contains("@everyone") || args.Contains("@here")) {
                await ReplyAsync("Please say it again, but without the ping.");
                return;
            }
            string[] handshakeEmotes = {
                "https://i.kym-cdn.com/photos/images/original/001/191/228/9ff.gif",
                "https://media.giphy.com/media/rA54nlVe8VkRy/giphy.gif",
                "http://i.imgur.com/ZLAvohG.gif"
            };
            Random rand = new Random();
            string randomHandshakeEmotes = handshakeEmotes[rand.Next(0, handshakeEmotes.Length)];
            if (args == null) {
                await ReplyAsync("You want me to shake your hand? You got it, chief, " + Context.User.Username + ".");
            } else if ((args.ToLower()).Contains("prim") || args.ToLower().Contains(Uno_Score_Tracking.SaveFiles_GlobalVariables.MY_ID.ToString())) {
                await ReplyAsync("Prim gives you a FIRM handshake.");
            } else {
                await ReplyAsync(args + " has received a firm handshake from " + Context.User.Username);
            }
            await ReplyAsync("\n" + randomHandshakeEmotes);
        }

        [Command("slap", RunMode = RunMode.Async)]
        public async Task Slap([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args.Contains("@everyone") || args.Contains("@here")) {
                await ReplyAsync("Please say it again, but without the ping.");
                return;
            }
            string[] slapEmotes = {
                "https://media1.tenor.com/images/b6d8a83eb652a30b95e87cf96a21e007/tenor.gif?itemid=10426943",
                "https://i.imgur.com/4MQkDKm.gif",
                "https://i.pinimg.com/originals/fc/e1/2d/fce12d3716f05d56549cc5e05eed5a50.gif",
                "https://media.giphy.com/media/iUgoB9zOO0QkU/giphy.gif",
                "https://i.pinimg.com/originals/46/b0/a2/46b0a213e3ea1a9c6fcc060af6843a0e.gif",
                "https://media.giphy.com/media/Zau0yrl17uzdK/giphy.gif"
            };
            Random rand = new Random();
            string randomSlapEmote = slapEmotes[rand.Next(0, slapEmotes.Length)];
            if (args == null) {
                await ReplyAsync("I mean, you didn't specify anyone, so I'm just going to slap you, " + Context.User.Username);
            } else if ((args.ToLower()).Contains((Context.User.Username).ToLower()) || args.Contains(Context.User.Mention) || args.Contains(Context.User.Id.ToString())) {
                await ReplyAsync("Why would you want me to slap you...? \n https://orig00.deviantart.net/cf61/f/2017/121/a/7/__litten_frightened___by_screinja_x-db7t8ml.gif");
                return;
            } else if ((args.ToLower()).Contains("prim") || args.ToLower().Contains(Uno_Score_Tracking.SaveFiles_GlobalVariables.MY_ID.ToString())) {
                await ReplyAsync("I refuse to slap Primaski. Nerd.");
                return;
            } else {
                await ReplyAsync(args + " has received a slap from " + Context.User.Username);
            }
            await ReplyAsync("\n" + randomSlapEmote);
            return;
        }

        [Command("punch", RunMode = RunMode.Async)]
        public async Task Punch([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args.Contains("@everyone") || args.Contains("@here")) {
                await ReplyAsync("Please say it again, but without the ping.");
                return;
            }
            string[] punchEmotes = {
                "https://media1.tenor.com/images/7a582f32ef2ed527c0f113f81a696ae3/tenor.gif?itemid=5012021",
                "https://i.kym-cdn.com/photos/images/original/001/047/566/0b4.gif",
                "https://data.whicdn.com/images/286613675/original.gif",
                "https://i.imgur.com/Edya2Qn.gif"
            };
            Random rand = new Random();
            string randomPunchEmote = punchEmotes[rand.Next(0, punchEmotes.Length)];
            if (args == null) {
                await ReplyAsync("I mean, you didn't specify anyone, so I'm just going to punch you, " + Context.User.Username);
            } else if ((args.ToLower()).Contains((Context.User.Username).ToLower()) || args.Contains(Context.User.Mention) || args.Contains(Context.User.Id.ToString())) {
                await ReplyAsync("Why would you want me to punch you...? \n https://orig00.deviantart.net/cf61/f/2017/121/a/7/__litten_frightened___by_screinja_x-db7t8ml.gif");
                return;
            } else if ((args.ToLower()).Contains("prim") || args.ToLower().Contains(Uno_Score_Tracking.SaveFiles_GlobalVariables.MY_ID.ToString())) {
                await ReplyAsync("I refuse to punch Primaski. Nerd.");
                return;
            } else {
                await ReplyAsync(args + " has received a punch from " + Context.User.Username);
            }
            await ReplyAsync("\n" + randomPunchEmote);
        }

        [Command("headpat", RunMode = RunMode.Async)]
        public async Task HeadPat([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args.Contains("@everyone") || args.Contains("@here")) {
                await ReplyAsync("Please say it again, but without the ping.");
                return;
            }
            string[] headpatEmotes = {
                "https://media1.tenor.com/images/c0bcaeaa785a6bdf1fae82ecac65d0cc/tenor.gif?itemid=7453915",
                "https://i.imgur.com/WyMHuyL.gif",
                "https://thumbs.gfycat.com/FlimsyDeafeningGrassspider-small.gif",
                "https://media1.tenor.com/images/1e92c03121c0bd6688d17eef8d275ea7/tenor.gif?itemid=9920853",
                "https://thumbs.gfycat.com/ImpurePleasantArthropods-small.gif",
                "https://i.imgur.com/laEy6LU.gif",
                "https://i.imgur.com/fp9XJZO.gif"
            };
            Random rand = new Random();
            string randomHeadpatEmote = headpatEmotes[rand.Next(0, headpatEmotes.Length)];
            if (args == null) {
                await ReplyAsync("Aw, need a headpat? Here's a headpat, " + Context.User.Username + " ^w^");
            } else if ((args.ToLower()).Contains("prim") || args.ToLower().Contains(Uno_Score_Tracking.SaveFiles_GlobalVariables.MY_ID.ToString())) {
                await ReplyAsync("Prim likes headpats >w< uwu");
            } else {
                await ReplyAsync(args + " has received a headpat from " + Context.User.Username);
            }
            await ReplyAsync("\n" + randomHeadpatEmote);
        }

        [Command("kiss", RunMode = RunMode.Async)]
        public async Task Kiss([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args.Contains("@everyone") || args.Contains("@here")) {
                await ReplyAsync("Please say it again, but without the ping.");
                return;
            }
            string[] kissEmotes = {
                "https://media.tenor.com/images/8cf98d92c54ee938e1c6617ad8c0e167/tenor.gif",
                "https://media.giphy.com/media/FqBTvSNjNzeZG/giphy.gif",
                "https://i.imgur.com/4Ad9iwh.gif",
                "https://media.giphy.com/media/KkhhOqtJA2eRy/giphy.gif",
                "https://i.pinimg.com/originals/79/02/b4/7902b4282389c29e93aae4ead68b29e7.gif",
                "https://i.imgur.com/nrqkMny.gif"
            };
            Random rand = new Random();
            string randomKissEmote = kissEmotes[rand.Next(0, kissEmotes.Length)];
            if (args == null) {
                await ReplyAsync("I think I'd prefer to keep this at a friendship level..."); return;
            } else if (args.Contains("487718576892018689") || args.Contains("bot")) {
                await ReplyAsync("I think I'd prefer to keep this at a friendship level..."); return;
            } else if ((args.ToLower()).Contains("prim") || args.ToLower().Contains(Uno_Score_Tracking.SaveFiles_GlobalVariables.MY_ID.ToString())) {
                await ReplyAsync("Y-you want to kiss Prim? >///<");
            } else if (args.Contains(Context.User.Id.ToString()) || Context.User.Username.ToLower().Contains(args)) {
                await ReplyAsync("Are you feeling lonely..?"); return;
            } else {
                await ReplyAsync(args + " has received a kiss from " + Context.User.Username);
            }
            await ReplyAsync("\n" + randomKissEmote);
        }

        [Command("breed", RunMode = RunMode.Async)]
        public async Task Breed([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            Random rando = new Random();
            int rand = -1;
            if (args != null) {
                if (args.Contains("@everyone") || args.Contains("@here")) {
                    await ReplyAsync("Please say it again, but without the ping.");
                    return;
                }
                await ReplyAsync(Context.User.Username + " asks " + args + " for a night.");
                await Task.Delay(1000);
                rand = rando.Next(1, 101);
                if (rand < 1) {
                    await ReplyAsync(args + " is tired! Try again later.");
                } else {
                    await ReplyAsync(args + " licks their lips. " + args + " is ready.");
                    await Task.Delay(1000);
                    await ReplyAsync(Context.User.Username + " thrusts!");
                    await Task.Delay(1000);
                    await ReplyAsync(args + " thrusts!");
                    await Task.Delay(2000);
                    rand = rando.Next(1, 101);
                    if (rand < 90) {
                        await ReplyAsync("That was wonderful! However, you were unsuccessful at having a child.");
                    } else {
                        await ReplyAsync("That was wonderful! What's this? " + args + " is pregnant!");
                    }
                }
                return;
            } else {
                await ReplyAsync("Excuse me, but who would breed with you? Put a name if you'd like to breed with someone!");
            }
        }
    }
}
