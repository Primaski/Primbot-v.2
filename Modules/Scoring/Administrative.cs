using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using static Primbot_v._2.Uno_Score_Tracking.Bridge;
using static Primbot_v._2.Uno_Score_Tracking.SaveFiles_GlobalVariables;
using System.Text.RegularExpressions;
using Primbot_v._2.Uno_Score_Tracking;
using System.Globalization;
using System.IO;
using Primbot_v._2.Server;
using System.Collections.Immutable;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using System.Net;

namespace Primbot_v._2.Modules.Scoring {
    //commands of this class should not be ASYNC
    public class Administrative : ModuleBase<SocketCommandContext> {

        /*[Command("neb")]
        public async Task Neb([Remainder] string args = null) {
            if (Directory.Exists(USER_SAVE_DIRECTORY)) {
                string[] foldersindirectory = Directory.GetDirectories(USER_SAVE_DIRECTORY);
                foreach (string subdir in foldersindirectory) {
                     if(File.Exists(subdir + "\\Unoprofile.txt") && 
                        File.Exists(subdir + "\\FN0_Unoprofile.txt")) {
                        try {
                            int pointsEarned = Int32.Parse(SaveFiles_Mapped.SearchMappedSaveFile(
                                subdir + "\\FN0_Unoprofile.txt",
                                "POINTS-SERVER"
                                ));
                            if (pointsEarned != 0) {
                                /*Console.WriteLine((GuildCache.GetUserByID(
                                    UInt64.Parse(new DirectoryInfo(subdir).Name))?.Username ?? "idk") +
                                    "===> " + pointsEarned.ToString());
                                int pointssincev2 = Int32.Parse(SaveFiles_Mapped.SearchMappedSaveFile(
                                    subdir + "\\Unoprofile.txt",
                                    "POINTS-SERVER"
                                    ));
                                int newValue;
                                newValue = pointsEarned + pointssincev2;
                                SaveFiles_Mapped.SetFieldValue("POINTS-SERVER", subdir + "\\Unoprofile.txt", newValue.ToString());
                                string newVal = SaveFiles_Mapped.SearchMappedSaveFile(
                                    subdir + "\\Unoprofile.txt",
                                    "POINTS-SERVER"
                                    );
                                Console.WriteLine((GuildCache.GetUserByID(
                                    UInt64.Parse(new DirectoryInfo(subdir).Name))?.Username ?? "idk") +
                                    "===> \nOld:" + pointsEarned + "\nNew:" + pointssincev2 + "\nTogether:" + newVal);
                            }
                        }catch(Exception e) {
                            Console.WriteLine("Failed: " + subdir + ", " + e.Message);
                        }
                     }
                }
            }
        }


        [Command("qaf")]
        public async Task Qaf([Remainder] string args = null) {
            string[] foldersindirectory = Directory.GetDirectories(USER_SAVE_DIRECTORY);
            foreach(string subdir in foldersindirectory) {
                try {
                    string filename = new DirectoryInfo(subdir).Name;
                    string username = GuildCache.GetUserByID(UInt64.Parse(filename))?.Username
                        ?? "...";
                    Console.WriteLine(filename + " => " + username);
                } catch (Exception e) {
                    Console.WriteLine("!!! " + e.Message);
                }
            }
        }*/

        //cache
        [Command("c")]
        public async Task Cache([Remainder] string args = null) {
            if(Context.User.Id != MY_ID) {
                return;
            }
            try {
                var channel = GuildCache.Uno_Cache.GetChannel(472821708319883265);
                var channel2 = GuildCache.Magi_Cache.GetChannel(486364420847697932);
                //var channel3 = GuildCache.Poke_Cache.GetChannel(598458534405079041);
                await ReplyAsync("Successfully loaded server caches (`res` for manual)");
                return;
            } catch {
                await ReplyAsync("Server caches improperly loaded, reloading...");
                await Restart();
            }
        }

        [Command("azurill")]
        public async Task Az([Remainder] string args = "") {
            await Context.Message.DeleteAsync();
            string[] azurillimages = {
                "https://cdn.discordapp.com/attachments/494692343182655489/597458951155613719/A1AhL3Q.gif",
                "https://cdn.discordapp.com/attachments/494692343182655489/597460260789157925/Azurill-Anime.jpg",
                "https://cdn.discordapp.com/attachments/494692343182655489/597460452808851470/images_2.jpeg",
                "https://cdn.discordapp.com/attachments/494692343182655489/597460452808851470/images_2.jpeg",
                "https://cdn.discordapp.com/attachments/494692343182655489/597460565396422666/tumblr_n3hew7NW4D1rpn9eno1_500.gif",
                "https://cdn.discordapp.com/attachments/494692343182655489/597460767922716710/0b79d8acd5965f81b92d8dab3e529154cf62bcc0r1-1536-2048v2_hq.jpg",
                "https://cdn.discordapp.com/attachments/494692343182655489/597461061725061130/azurill_by_sakuraichuu_dar4qqm-pre.jpg",
                "https://cdn.discordapp.com/attachments/494692343182655489/597461129366732821/68e1d20096e3d1631e9d34c87e0f6604.jpg",
                "https://cdn.discordapp.com/attachments/494692343182655489/597461333306245130/tumblr_mh3oawnWWP1r72ht7o1_250.gif",
                "https://cdn.discordapp.com/attachments/494692343182655489/597461636579721226/4451c22d7ad9b5b8bfb9560bcb1178d4.gif",
            };
            Random rand = new Random();
            int randNo = rand.Next(0, azurillimages.Length);
            string url = azurillimages[randNo];
            if (Context.User.Id != 332788739560701955 && Context.User.Id != MY_ID) {
                return;
            } else if (args.Split(' ').Count() != 2) {
                await ReplyAsync("Format: `[prev count] [new count]`."); return;
            }
            var argsSplits = args.Split(' ');

            if (!Int32.TryParse(argsSplits[0], out int ignore) || !Int32.TryParse(argsSplits[1], out int ignore2)) {
                await ReplyAsync("Please enter a valid set of integers."); return;
            }
            int orig = Int32.Parse(argsSplits[0]);
            int update = Int32.Parse(argsSplits[1]);
            int diff = update - orig;
            if (diff < 0) {
                await ReplyAsync("New is smaller than old."); return;
            }
            Byte[] b = new Byte[3];
            rand.NextBytes(b);

            Embed emb = new EmbedBuilder().WithTitle("Alicia has collected **" + diff + " more Azurills**!")
                .WithDescription("Alicia now has **" + update + " Azurills**!")
                .WithCurrentTimestamp().WithColor(b[0], b[1], b[2]).WithImageUrl(url).Build();
            await ReplyAsync("", false, emb);
        }

        [Command("shinyazurill")]
        public async Task Shaz([Remainder] string args = "") {
            await Context.Message.DeleteAsync();
            string[] azurillimages = {
                "https://cdn.discordapp.com/attachments/494692343182655489/597459052368232490/dblq9sl-8fdc6f87-5443-47a1-8c4f-432eeda1ce60.gif",
                "https://cdn.discordapp.com/attachments/494692343182655489/597459052368232490/dblq9sl-8fdc6f87-5443-47a1-8c4f-432eeda1ce60.gif",
            };
            Random rand = new Random();
            int randNo = rand.Next(0, azurillimages.Length);
            string url = azurillimages[randNo];
            if (Context.User.Id != 332788739560701955 && Context.User.Id != MY_ID) {
                return;
            } else if (args.Split(' ').Count() != 2) {
                await ReplyAsync("Format: `[prev count] [new count]`."); return;
            }
            var argsSplits = args.Split(' ');

            if (!Int32.TryParse(argsSplits[0], out int ignore) || !Int32.TryParse(argsSplits[1], out int ignore2)) {
                await ReplyAsync("Please enter a valid set of integers."); return;
            }
            int orig = Int32.Parse(argsSplits[0]);
            int update = Int32.Parse(argsSplits[1]);
            int diff = update - orig;
            if (diff < 0) {
                await ReplyAsync("New is smaller than old."); return;
            }
            Byte[] b = new Byte[3];
            rand.NextBytes(b);

            Embed emb = new EmbedBuilder().WithTitle("Alicia has collected a SHINY Azurill**!")
                .WithDescription("Alicia now has **" + update + " SHINY Azurills**!")
                .WithCurrentTimestamp().WithColor(b[0], b[1], b[2]).WithImageUrl(url).Build();
            await ReplyAsync("", false, emb);
        }

        [Command("spoink")]
        public async Task R([Remainder] string args = "") {
            await Context.Message.DeleteAsync();
            string[] spoinkimages = { "https://i.pinimg.com/originals/a9/df/3e/a9df3e32416606d26de4ca6fdcd81dd0.gif",
                "https://cdn.bulbagarden.net/upload/thumb/f/fa/Spoink_anime.png/250px-Spoink_anime.png",
            "https://www.goombastomp.com/wp-content/uploads/2018/06/spoink.gif",
            "http://33.media.tumblr.com/tumblr_m4uywuqW3p1qd87hlo1_500.gif",
            "https://i.kinja-img.com/gawker-media/image/upload/s--8AXr-tJr--/c_fill,f_auto,fl_progressive,g_center,h_675,pg_1,q_80,w_1200/cu4wazcuuoyxz4mu4mxz.png",
            "http://i.imgur.com/keXhta0.gif",
            "https://66.media.tumblr.com/33cf83c074f7499ea2881b5873b0445f/tumblr_o7alch8b7b1rpn9eno1_400.gif",
            "https://i.imgur.com/WDTjmBQ.jpg",
            "https://i.redd.it/xxq0lexkmlk21.png",
            "https://i.ytimg.com/vi/CLSwwALaS6U/maxresdefault.jpg",
            "https://i.imgur.com/16xOWm9.gif",
            "https://art.ngfiles.com/images/567000/567504_gatekid3_spoink.gif?f1511999787"};
            Random rand = new Random();
            int randNo = rand.Next(0, spoinkimages.Length);
            string url = spoinkimages[randNo];
            if (Context.User.Id != MY_ID) {
                return;
            } else if(args.Split(' ').Count() != 2) {
                await ReplyAsync("Format: `[prev count] [new count]`."); return;
            }
            var argsSplits = args.Split(' ');

            if (!Int32.TryParse(argsSplits[0], out int ignore) || !Int32.TryParse(argsSplits[1], out int ignore2)) {
                await ReplyAsync("Please enter a valid set of integers."); return;
            }
            int orig = Int32.Parse(argsSplits[0]);
            int update = Int32.Parse(argsSplits[1]);
            int diff = update - orig;
            if(diff < 0) {
                await ReplyAsync("New is smaller than old."); return;
            }
            Byte[] b = new Byte[3];
            rand.NextBytes(b);

            Embed emb = new EmbedBuilder().WithTitle("Prim has collected **" + diff + " more Spoinks**!")
                .WithDescription("Prim now has **" + update + " Spoinks**!")
                .WithCurrentTimestamp().WithColor(b[0],b[1],b[2]).WithImageUrl(url).Build();
            await ReplyAsync("", false, emb);

        }

        [Command("shinyspoink")]
        public async Task RR([Remainder] string args = "") {
            await Context.Message.DeleteAsync();
            if (Context.User.Id != MY_ID) {
                return;
            } else if (args.Split(' ').Count() != 2) {
                await ReplyAsync("Format: `[prev count] [new count]`."); return;
            }
            var argsSplits = args.Split(' ');

            if (!Int32.TryParse(argsSplits[0], out int ignore) || !Int32.TryParse(argsSplits[1], out int ignore2)) {
                await ReplyAsync("Please enter a valid set of integers."); return;
            }
            int orig = Int32.Parse(argsSplits[0]);
            int update = Int32.Parse(argsSplits[1]);
            int diff = update - orig;
            if (diff < 0) {
                await ReplyAsync("New is smaller than old."); return;
            }
            Random rand = new Random();
            Byte[] b = new Byte[3];
            rand.NextBytes(b);

            Embed emb = new EmbedBuilder().WithTitle("Prim has collected a **SHINY Spoink**!")
                .WithDescription("Prim now has **" + update + " Shiny Spoinks**!")
                .WithCurrentTimestamp().WithColor(b[0], b[1], b[2]).WithImageUrl("https://pm1.narvii.com/6001/cb263f6e86fe18a90914f27bb7ac2130b6fd0f88_hq.jpg").Build();
            await ReplyAsync("", false, emb);

        }

        [Command("massping", RunMode = RunMode.Async)]
        public async Task Pinger([Remainder] string args = null) {
            if (Context.User.Id != MY_ID) {
                return;
            }
            if (args == null) { await ReplyAsync("args is null"); return; }
            string[] argssplits = args.Split(' ');
            if (argssplits.Count() != 2) { await ReplyAsync("need 2 args"); return; }
            var user = GuildCache.InterpretUserInput(argssplits[0])[0];
            if(user == null) { await ReplyAsync("user doesn't exist"); return; }
            if(!Int32.TryParse(argssplits[1],out int ignore)) { await ReplyAsync("not an int"); return; }
            int pings = Int32.Parse(argssplits[1]);

            for(int i = 1; i <= pings; ++i) {
                await ReplyAsync(user.Mention + "(ping " + i + ")");
            }
            return;
        }

        [Command("p")]
        public async Task Q([Remainder] string args = null) {
            
            
            string[] links = {
                "https://i.imgur.com/TGKJgBm.png",
                "https://i.imgur.com/wSbED79.png",
                "https://i.imgur.com/5N93Ibg.png",
                "https://i.imgur.com/cRsub5F.png",
                "https://i.imgur.com/8rTtuoh.png",
                "https://i.imgur.com/QoibGCU.png",
                "https://i.imgur.com/fMUSwqh.png",
                "https://i.imgur.com/A4Ea6Yl.png",
                "https://i.imgur.com/A4Ea6Yl.png",
                "https://i.imgur.com/R830RYO.png",
                "https://i.imgur.com/3SmXS1c.png",
                "https://i.imgur.com/LpvY4sk.png",
                "https://i.imgur.com/Cb4MZrG.png",
                "https://i.imgur.com/hSjoxiU.png",
                //"https://cdn.discordapp.com/attachments/429838928707846165/440781998433370123/448e4a3b9fa8727e34f02b33fb4fd9aa.png",
                "https://i.imgur.com/UsNPChJ.png",
                "https://i.imgur.com/JkXrfo7.png",
                "https://i.imgur.com/JkXrfo7.png",
                "https://i.imgur.com/FqedIDK.png",
                "https://i.imgur.com/iqR9VHb.png",
                "https://i.imgur.com/lUvdOib.png",
                "https://i.imgur.com/IBh3uqi.png",
                "https://i.imgur.com/c6IfYuk.png",
                "https://i.imgur.com/3PWNEDG.png",

                "https://i.imgur.com/AlF80vr.png",
                "https://i.imgur.com/q1yrOUj.png",
                "https://i.imgur.com/I4YCbGJ.png",
                "https://i.imgur.com/Uw7RCfj.png",
                "https://i.imgur.com/Y9pHfoc.png",
                "https://i.imgur.com/dDH9U3s.png",
            };

            /*string[] links = new string[] {
                "https://i.imgur.com/yalozuD.png",
                "https://i.imgur.com/f9JOteA.png",
                "https://i.imgur.com/hnHIrD1.png",
                "https://i.imgur.com/nOYryC3.png",
                "https://i.imgur.com/gamez3k.png",
                "https://i.imgur.com/3R8N1FX.png",
                "https://i.imgur.com/YHpUkKG.png",
                "https://i.imgur.com/edqOSGV.png",
                "https://i.imgur.com/aueS1I5.png",
                "https://i.imgur.com/V0EMAHY.png",
                "https://i.imgur.com/OkiW8Oo.png"
            };*/
            
            Random rand = new Random();
            string imageurl = links[rand.Next(0, links.Count())];
            if (args == "uno") {
                await GuildCache.Uno_Cache.GetTextChannel(487825348310859777).SendMessageAsync("", false, new EmbedBuilder().WithTitle("A wild pokémon has appeared!"
                ).WithColor(0,174,134).WithDescription("Guess the pokémon and type .catch <pokémon> to catch it!")
                .WithImageUrl(imageurl).Build());
            }
            await ReplyAsync("", false, new EmbedBuilder().WithTitle("A wild pokémon has appeared!"
                ).WithColor(0,174,134).WithDescription("Guess the pokémon and type .catch <pokémon> to catch it!")
                .WithImageUrl(imageurl).Build());
               
            await Context.Message.DeleteAsync();
            //await ReplyAsync(Context.User.Mention + " is a monster who tried spawning a fake Pokemon in this channel. Ridicule them.");
            return;
        }



        [Command("res")]
        public async Task Restart() {
            if (Context.User.Id != MY_ID) {
                return;
            }
            // Starts a new instance of the program itself
            var fileName = Assembly.GetExecutingAssembly().Location;
            System.Diagnostics.Process.Start(fileName);

            // Closes the current process
            Environment.Exit(0);
            return;
        }

        [Command("off")]
        public async Task Off() {
            if (Context.User.Id != MY_ID) {
                return;
            }
            Environment.Exit(0);
        }

        [Command("translatedatehex")]
        public async Task TransDateHex([Remainder] string args = null) {
            if (args == null) {
                await ReplyAsync("Improper format."); return;
            }
            try {
                await ReplyAsync(SaveFiles_Sequences.TranslateDateHex(Int32.Parse(args, NumberStyles.HexNumber)).ToString());
            } catch (Exception e) {
                await ReplyAsync(e.Message);
            }
        }

        [Command("cc")]
        public async Task CommandCounter([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args == null) {
                await ReplyAsync("`" + cmdEx.ToString() + "` Primbot commands executed today.");
                return;
            }

            if (args == "help") {
                await ReplyAsync("Action? `count`,`set`,`log`"); return;
            }

            if (Context.User.Id != MY_ID) {
                await ReplyAsync("You do not have permission to execute this command."); return;
            }

            if (args == "log") {
                GuildCache.LogCMD();
                await ReplyAsync("Reset to `0`!");
                return;
            }

            if (args.Split().Count() > 1) {
                if (args.Split()[0] == "set" && Int32.TryParse(args.Split()[1], out int ignore)) {
                    File.Delete(COMMANDS_TODAY_BACKUP_DIRECTORY);
                    using (StreamWriter sw = new StreamWriter(COMMANDS_TODAY_BACKUP_DIRECTORY)) {
                        sw.WriteLine(args.Split()[1]);
                    }
                    await ReplyAsync("New value: `" + args.Split()[1] + "`");
                    return;
                }
            }
        }

        /// <summary>
        /// Deletes all accounts that neither exist in the server, nor have accumulated any points.
        /// </summary>
        [Command("purgeuno")]
        public async Task PurgeAccounts([Remainder] string args = null) {
            if (Context.User.Id != MY_ID) {
                return;
            }
            int counter = 0;
            List<string> records = new List<string>();
            await ReplyAsync("`Purging inactive accounts...`");
            if (Directory.Exists(USER_SAVE_DIRECTORY)) {
                string[] foldersindirectory = Directory.GetDirectories(USER_SAVE_DIRECTORY);
                foreach (string subdir in foldersindirectory) {
                    try {
                        ulong userID = UInt64.Parse(new DirectoryInfo(subdir).Name);
                        //if user is in server -> keep
                        if(GuildCache.GetUserByID(userID,GuildCache.Uno_Cache) == null) {
                            //if file doesn't exist, then they're not in uno server -> keep
                            string fullDirName = subdir + "\\" + UNO_SAVE_FILE_NAME;
                            if (File.Exists(fullDirName)) {
                                int val = Int32.Parse(SaveFiles_Mapped.SearchMappedSaveFile
                                    (fullDirName, "POINTS-UNO"));
                                //if they have ever earned points -> keep
                                if(val == 0) {
                                    Directory.Delete(subdir,true);
                                    Console.WriteLine(userID + "=====> DELETING ");
                                    counter++;
                                    records.Add(counter.ToString() + ": " + userID.ToString());
                                } else {
                                    Console.WriteLine(userID + " has points.");
                                }
                            } else {
                                Console.WriteLine(userID + " has no Unoprofile.");
                            }
                        } else {
                            Console.WriteLine(userID + " is still in server.");
                        }
                    }catch(Exception e) {
                        Console.WriteLine(subdir + " ==>\n" + e.Message);
                    }
                }
            }
            if(records.Count() != 0) {
                await ReplyAsync("`Creating record of purged accounts...`");
                File.WriteAllLines(PURGED_SAVE_FILES_DIRECTORY + "\\Purges_" + DateTime.Now.ToString("yyyy-MM-dd") + "-" +
                    DateTime.Now.ToString("HH-mm") + ".txt", records.ToArray());
                await ReplyAsync("`Resetting leaderboard cache...`");
                await ResetLBCache();
                await ReplyAsync("`" + counter + "` account files purged. Purge log saved at: `" + 
                    PURGED_SAVE_FILES_DIRECTORY + "\\Purges_" + DateTime.Now.ToString("yyyy-MM-dd") +
                    DateTime.Now.ToString("HH-mm") + "`");
            } else {
                await ReplyAsync("No accounts were found to be purged... all users currently in the directory either:\n" +
                    "1. Are in the server\n2. Have no Uno profile\n3. Have Uno points");
            }
        }

        [Command("addr", RunMode = RunMode.Async)]
        public async Task addrole([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args == null || Context.User.Id != MY_ID) {
                await ReplyAsync("failed"); return;
            }
            string[] argos = args.Split();
            if (argos.Count() <= 1) {
                await ReplyAsync("failed"); return;
            }
            ulong ID; string rolename;
            try {
                if (argos[0] != "local") {
                    ID = UInt64.Parse(argos[0]);
                } else {
                    ID = Uno_Score_Tracking.SaveFiles_GlobalVariables.MY_ID;
                }
                rolename = args.Substring(args.IndexOf(" ") + 1);
            } catch (Exception e) {
                await ReplyAsync(e.Message); return;
            }
            await Context.Guild.DownloadUsersAsync();
            IGuildUser user = Context.Guild.GetUser(ID);
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            if (user == null || role == null) {
                await ReplyAsync("returned null"); return;
            }
            await user.AddRoleAsync(role);
            await ReplyAsync("success");
            return;
        }

        [Command("slave")]
        public async Task slave([Remainder] string args = null) {
            if(Context.User.Id != MY_ID) {
                return;
            }
            if(args != null) {
                var argssplits = args.Split(' ');
                switch (argssplits[0]) {
                    case "set":
                        
                        if(argssplits.Count() > 2) {
                            switch (argssplits[1]) {
                                case "channel":
                                    try {
                                        slavechannel = UInt64.Parse(argssplits[2]);
                                    }catch(Exception e) {
                                        await ReplyAsync(e.Message);
                                    }
                                    await ReplyAsync("Success.");
                                    return;
                                case "count":
                                    try {
                                        SaveFiles_GlobalVariables.slave = Int32.Parse(argssplits[2]);
                                    } catch (Exception e) {
                                        await ReplyAsync(e.Message);
                                    }
                                    await ReplyAsync("Success.");
                                    return;
                                case "value":
                                    try {
                                        SaveFiles_GlobalVariables.slavevalue = Int32.Parse(argssplits[2]);
                                    } catch (Exception e) {
                                        await ReplyAsync(e.Message);
                                    }
                                    await ReplyAsync("Success.");
                                    return;
                                case "threshold":
                                    try {
                                        SaveFiles_GlobalVariables.slavethreshold = Int32.Parse(argssplits[2]);
                                    } catch (Exception e) {
                                        await ReplyAsync(e.Message);
                                    }
                                    await ReplyAsync("Success.");
                                    return;
                            }
                            break;
                        }
                        await ReplyAsync("Format: `p*slave set [channel,count,value,threshold] [value]`");
                        break;
                    default: return;
                }
                return;
            }
            await ReplyAsync(":dollar: **" + 
                String.Format("{0:n0}",(SaveFiles_GlobalVariables.slave * slavevalue)) + "** earned so far.\n**" +
                (slavethreshold-(SaveFiles_GlobalVariables.slave%slavethreshold)) + "** until claim is available.");
        }

        [Command("remover", RunMode = RunMode.Async)]
        public async Task removerole([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args == null || Context.User.Id != MY_ID) {
                await ReplyAsync("failed"); return;
            }
            string[] argos = args.Split();
            if (argos.Count() <= 1) {
                await ReplyAsync("failed"); return;
            }
            ulong ID; string rolename;
            try {
                if (argos[0] != "local") {
                    ID = UInt64.Parse(argos[0]);
                } else {
                    ID = MY_ID;
                }
                rolename = args.Substring(args.IndexOf(" ") + 1);
            } catch (Exception e) {
                await ReplyAsync(e.Message); return;
            }
            await Context.Guild.DownloadUsersAsync();
            IGuildUser user = Context.Guild.GetUser(ID);
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == rolename);
            if (user == null || role == null) {
                await ReplyAsync("returned null"); return;
            }
            await user.RemoveRoleAsync(role);
            await ReplyAsync("success");
            return;
        }

        [Command("kick", RunMode = RunMode.Async)]
        public async Task kick([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (args == null || Context.User.Id != MY_ID) {
                await ReplyAsync("failed"); return;
            }
            string[] argos = args.Split();
            if (argos.Count() < 1) {
                await ReplyAsync("failed"); return;
            }
            ulong ID;
            try {
                ID = UInt64.Parse(argos[0]);
            } catch (Exception e) {
                await ReplyAsync(e.Message); return;
            }
            await Context.Guild.DownloadUsersAsync();
            IGuildUser user = Context.Guild.GetUser(ID);
            if (user == null) {
                await ReplyAsync("failed");
            }
            try {
                await user.KickAsync();
            }catch(Exception e) {
                await ReplyAsync(e.Message); return;
            }
            await ReplyAsync("Kicked " + user.Username + ".");
            return;
        }

        [Command("killswitch")]
        public async Task Killswitchactivate([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (GuildCache.ExtractRoleSubsetFromUser((SocketGuildUser)Context.User, new List<string> { "490398065761583104" }, true).Count() == 0) {
                await ReplyAsync("This role was intended for Point Managers for the Uno Server, and you lack the appropriate permissions."); return;
            }
            if (args == null) {
                await ReplyAsync(Killswitch.GetHelpMenu());
                await ReplyAsync("The current state of the Killswitch is: `" + Killswitch.GetState() + "`.");
                return;
            }
            args = args.ToLower();
            if (args.Contains("off")) {
                Killswitch.SetStatus(Killswitch.Status.Off);
            } else if (args.Contains("mute")) {
                Killswitch.SetStatus(Killswitch.Status.Mute);
            } else if (args.Contains("kick")) {
                Killswitch.SetStatus(Killswitch.Status.Kick);
            } else if (args.Contains("ban")) {
                Killswitch.SetStatus(Killswitch.Status.Ban);
            }
            await ReplyAsync("The current state of the Killswitch is: `" + Killswitch.GetState() + "`.");
            return;
        }

        [Command("get")]
        public async Task get([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await ReplyAsync("`destroyHumanity` is set to `false`.");
        }

        [Command("poke")]
        public async Task poke([Remainder] string args = null) {
            int curr = -1;
            char state = 'f';
            switch (Context.User.Id) {
                case 263733973711192064:
                spawntrack = !spawntrack;
                    if (spawntrack) { state = 't'; } 
                    curr = 0;
                    await ReplyAsync("Track `" + spawntrack.ToString() + "`");
                    break;
                case 332788739560701955:
                    curr = 1;
                    spawntrackalicia = !spawntrackalicia;
                    if (spawntrackalicia) { state = 't'; }
                    await ReplyAsync("Track `" + spawntrackalicia.ToString() + "`");
                    break;
                case 534194469302566922:
                    curr = 2;
                    spawntrackdom = !spawntrackdom;
                    if (spawntrackdom) { state = 't'; }
                    await ReplyAsync("Track `" + spawntrackdom.ToString() + "`");
                    break;
                default:
                    return;
            }
            if(curr != -1) {
                if (File.Exists(POKE)) {
                    string[] sw = File.ReadAllLines(POKE);
                    string line = sw[0];
                    if((line ?? "").Length < 3) {
                        Console.WriteLine("error updating pokecord");
                        return;
                    }
                    line = line.Substring(0,curr) + state + line.Substring(curr+1);
                    File.WriteAllLines(POKE, new string[] { line });
                }
            }
        }

        [Command("foom")]
        public async Task foom() {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (Context.User.Id != MY_ID) {
                return;
            }
            DirectoryInfo info = new DirectoryInfo(USER_SAVE_DIRECTORY);
            var dir = info.EnumerateDirectories();
            foreach (var path in dir) {
                string fullpath = USER_SAVE_DIRECTORY + "\\" + path + "\\FN13_Unoprofile.txt";
                string shortpath = USER_SAVE_DIRECTORY + "\\" + path;
                if (File.Exists(fullpath)) {
                    string val1 = SaveFiles_Mapped.SearchMappedSaveFile(fullpath, "ITER-MS");
                    if (val1 != "0") {
                        Console.WriteLine("ok this has something");
                        Console.WriteLine(SaveFiles_Mapped.SearchMappedSaveFile(shortpath + "\\UnoProfile.txt", "ITER-MS"));
                    }
                    string val2 = SaveFiles_Mapped.SearchMappedSaveFile(fullpath, "POINTS-MS");
                    SaveFiles_Mapped.AddFieldValue("ITER-MS", shortpath + "\\UnoProfile.txt", val1);
                    SaveFiles_Mapped.AddFieldValue("POINTS-MS", shortpath + "\\UnoProfile.txt", val1);
                }
            }
            await ReplyAsync("Complete.");
        }

        [Command("ang")]
        public async Task ang() {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (Context.User.Id != MY_ID) {
                return;
            }
            //await Context.Guild.GetTextChannel(486364420847697932).SendMessageAsync("Question! Which insect do you think has the best vision (according to scientists)? Shout out your answers - and I'll tell you if you're correct.");
            await Context.Guild.GetTextChannel(472821708319883265).SendMessageAsync("Question! Which insect do you think has the best vision (according to scientists)? Shout out your answers - and I'll tell you if you're correct.");
        }


        [Command("usernameandid")]
        public async Task UsernameAndID() {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (GuildCache.ExtractRoleSubsetFromUser((SocketGuildUser)Context.User, new List<string> { "490398065761583104" }, true).Count() == 0) {
                await ReplyAsync("This role was intended for Point Managers for the Uno Server, and you lack the appropriate permissions."); return;
            }
            var users = GuildCache.Uno_Cache.Users;
            var orderedUsers = users.OrderBy(x => x.Username);
            foreach (var user in orderedUsers) {
                Console.WriteLine(user.Username + "==>" + user.Id);
            }
        }

        [Command("nd")]
        public async Task NewDay2([Remainder] string args = null) {
            if (GuildCache.ExtractRoleSubsetFromUser((SocketGuildUser)Context.User, new List<string> { "490398065761583104" }, true).Count() == 0) {
                await ReplyAsync("This role was intended for Point Managers for the Uno Server, and you lack the appropriate permissions."); return;
            }
            await NewDay(false, args);
        }

        [Command("day")]
        public async Task NewDay3([Remainder] string args = null) {
            if (GuildCache.ExtractRoleSubsetFromUser((SocketGuildUser)Context.User, new List<string> { "490398065761583104" }, true).Count() == 0) {
                await ReplyAsync("This role was intended for Point Managers for the Uno Server, and you lack the appropriate permissions."); return;
            }
            await NewDay(false, args);
        }


        [Command("newDay")]
        public async Task NewDay(bool first = false, [Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (GuildCache.ExtractRoleSubsetFromUser((SocketGuildUser)Context.User, new List<string> { "490398065761583104" }, true).Count() == 0) {
                await ReplyAsync("This role was intended for Point Managers for the Uno Server, and you lack the appropriate permissions."); return;
            }
            if (!first) {
                await ReplyAsync("`In progress...`");
            }
            await ReplyAsync("`Resetting user daily limits...`");
            List<string> PLAYSTODAY_TYPES = new List<string>();
            foreach (var keyValue in UnoSaveFields) {
                if (keyValue.Key.StartsWith("PLAYSTODAY")) {
                    PLAYSTODAY_TYPES.Add(keyValue.Key);
                }
            }
            var directories = Directory.EnumerateDirectories(USER_SAVE_DIRECTORY);
            foreach (var user in directories) {
                //Console.WriteLine("Performing on " + user);
                foreach (var playsToday in PLAYSTODAY_TYPES) {
                    try {
                        SaveFiles_Mapped.ModifyFieldValue(playsToday, user + "\\" + UNO_SAVE_FILE_NAME, "0");
                    } catch { }
                }
            }
            await ReplyAsync("`Storing metadata statistics...`");
            GuildCache.LogCMD();
            await ReplyAsync("`Complete.`");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Started a new Uno day for " + Context.User.Username);
            return;
        }

        [Command("clearlbcache")]
        public async Task ResetLBCache([Remainder] string args = null) {

            if (GuildCache.ExtractRoleSubsetFromUser((SocketGuildUser)Context.User, new List<string> { "490398065761583104" }, true).Count() == 0) {
                await ReplyAsync("This role was intended for Point Managers for the Uno Server, and you lack the appropriate permissions."); return;
            }
            await ReplyAsync("`In progress...`");
            if (Directory.Exists(LEADERBOARD_DIRECTORY)) {
                var dir = new DirectoryInfo(LEADERBOARD_DIRECTORY);
                var files = dir.GetFiles();
                foreach (var file in files) {
                    file.Delete();
                }
            }
            await ReplyAsync("`Complete.`");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Cleared LB cache for " + Context.User.Username);
        }

        [Command("newFN")]
        public async Task NewFN([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (GuildCache.ExtractRoleSubsetFromUser((SocketGuildUser)Context.User, new List<string> { "490398065761583104" }, true).Count() == 0) {
                await ReplyAsync("This role was intended for Point Managers for the Uno Server, and you lack the appropriate permissions."); return;
            }
            await ReplyAsync("`In progress...`");
            await ReplyAsync("`Generating new fortnight save files...`");
            var directories = Directory.EnumerateDirectories(USER_SAVE_DIRECTORY);
            foreach (var user in directories) {
                if (File.Exists(user + "\\" + UNO_SAVE_FILE_NAME)) {
                    SaveFiles_Mapped.CreateMappedSaveFileFields(user + "\\FN" + FORTNIGHT_NUMBER + "_" + UNO_SAVE_FILE_NAME,
                        UnoSaveFields);
                }
            }
            await NewDay(true);
            await ReplyAsync("`Complete.`");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> Started new FN for " + Context.User.Username);
        }
        [Command("qqqq")]
        public async Task qqqq() {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await ReplyAsync(FORTNIGHT_NUMBER.ToString());
        }

        [Command("adminHelp")]
        public async Task AdminHelp([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            await ReplyAsync("`printDirectory`\n`findKey`\n`setKey`\n`translateSeed`\n`seekID`\n`newDay`");
        }

        [Command("printDirectory")]
        public async Task PrintDirectory([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (Context.User.Id != MY_ID) {
                return;
            }
            string dir = DIR;
            if (args != null) {
                dir += "\\" + args;
            }
            try {
                StringBuilder res = new StringBuilder();
                string pathLine = "Path: `" + dir + "`";
                res.AppendLine(pathLine);
                var localFiles = Directory.EnumerateFiles(dir);
                var localDirectories = Directory.EnumerateDirectories(dir);
                foreach (var directory in localDirectories) {
                    string line = ":file_folder: **" + Path.GetFileName(directory.ToString()) + "**";
                    res.AppendLine(line);
                }
                foreach (var file in localFiles) {
                    string line = ":newspaper: " + Path.GetFileName(file.ToString());
                    res.AppendLine(line);
                }
                string final = res.ToString();
                if (final.Length > 2000) {
                    final = final.Substring(0, 2000);
                }

                await ReplyAsync(final);
            } catch (Exception e) {
                await ReplyAsync(e.Message);
            }
            return;
        }

        [Command("findKey")]
        public async Task FindKey([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (Context.User.Id != MY_ID) {
                return;
            }
            string[] argsSplits = args.Split();
            if (argsSplits.Count() != 2) {
                await ReplyAsync("Format: `p*findKey [filepath] [key]`");
            }
            string dir = DIR + "\\" + argsSplits[0];
            string key = argsSplits[1];
            try {
                string res = SaveFiles_Mapped.SearchMappedSaveFile(dir, key) ?? "";
                await ReplyAsync("Path: `" + dir + "`\n\nKey: `" + key + "`\nValue: `" + res + "`");
            } catch (Exception e) {
                await ReplyAsync(e.Message + "\n" + e.StackTrace); return;
            }
        }

        [Command("setKey")]
        public async Task MappedSearch([Remainder] string args = null) {
            if (Context.User.Id != MY_ID) {
                return;
            }
            string[] argsSplits = args.Split();
            if (argsSplits.Count() != 3) {
                await ReplyAsync("Format: `p*setKey [filepath] [key] [value]`");
            }
            string dir = DIR + "\\" + argsSplits[0];
            string key = argsSplits[1];
            string value = argsSplits[2];
            string oldVal = SaveFiles_Mapped.SearchMappedSaveFile(dir, key) ?? "";
            await ReplyAsync("Path: `" + dir + "`\n\nKey: `" + key + "`\nOld Value: `" + oldVal + "`\nNew Value: `" +
                SaveFiles_Mapped.SetFieldValue(key, dir, value) + "`");
        }

        [Command("translateSeed")]
        public async Task Translate([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (Context.User.Id != MY_ID) {
                return;
            }
            Dictionary<string, string> x = new Dictionary<string, string>();
            if (args != null) {
                SavedGame w = new SavedGame(new List<string> { args });
                x = w.MakeHumanReadable();
            }

            var result = new EmbedBuilder();

            byte inlineFields = 1;
            foreach (var keyvalue in x) {
                if ((keyvalue.Key).StartsWith("Player") && inlineFields % 3 != 0) {
                    result.AddField(keyvalue.Key, keyvalue.Value, true);
                    inlineFields++;
                } else {
                    result.AddField(keyvalue.Key, keyvalue.Value);
                    inlineFields = 1;
                }
            }

            result.WithColor(Color.Red);

            Embed embed = result.Build();

            await Context.Channel.SendMessageAsync("", false, embed);
            return;
        }

        [Command("seekID")]
        public async Task seekID([Remainder] string args = null) {
            Uno_Score_Tracking.GuildCache.IncrementCMD();
            if (Context.User.Id != MY_ID) {
                return;
            }
            Dictionary<string, string> x = new Dictionary<string, string>();
            if (args != null) {
                List<string> w = SaveFiles_Sequences.SearchSaveFileForID(Int32.Parse(args, NumberStyles.HexNumber));
                x = SaveFiles_Sequences.ConvertSequencesToObject(w).MakeHumanReadable();

            }
            var result = new EmbedBuilder();

            byte inlineFields = 1;
            foreach (var keyvalue in x) {
                if ((keyvalue.Key).StartsWith("Player") && inlineFields % 3 != 0) {
                    result.AddField(keyvalue.Key, keyvalue.Value, true);
                    inlineFields++;
                } else {
                    result.AddField(keyvalue.Key, keyvalue.Value);
                    inlineFields = 1;
                }
            }
            result.WithColor(Color.Red);

            Embed embed = result.Build();

            await Context.Channel.SendMessageAsync("", false, embed);
            return;
        }

        [Command("cah", RunMode = RunMode.Async)]
        public async Task CahPings([Remainder] string args = null) {
            ISocketMessageChannel channel = Context.Channel;
            if (channel.Id != 474410621144793088 && channel.Id != 567459967691522059 && args == null) {
                await ReplyAsync("Not a CAH channel."); return;
            }

            if(args != null) {
                if(args == "www") {
                    channel = (ISocketMessageChannel)GuildCache.Uno_Cache.GetChannel(474410621144793088);
                }
            }

            await ReplyAsync("Started tracking pings!");
            cahtrack = true;
            ulong prevMsg = 0;
            while (cahtrack) {
                IEnumerable<IMessage> w = channel.GetMessagesAsync(2).FlattenAsync().Result;
                for (int i = 0; i < w.Count(); ++i) {
                    if (w.ElementAt(i).Author.Id == 204255221017214977 && w.ElementAt(i).Id != prevMsg) {
                        try {
                            var u = w.ElementAt(i).Embeds.ElementAt(0);
                            string v = u.Description.ToString();
                            if (v.Contains(">!")) {
                                prevMsg = w.ElementAt(i).Id;
                                await channel.SendMessageAsync(v + "");
                            }
                        } catch {
                        }
                    }
                }
            }
        }

            [Command("cahend", RunMode = RunMode.Async)]
        public async Task CahPingers([Remainder] string args = null) {
            cahtrack = false;
            await ReplyAsync("Stopped tracking pings!");
            return;
        }

        [Command("pingme")]
        public async Task Pingme([Remainder] string args = null) {
            if(Context.User.Id == MY_ID) {
                pingme = !pingme;
                await ReplyAsync("Uno ping: `" + pingme + "`");
                return;
            }
        }


        [Command("embed")]
        public async Task Embed([Remainder] string args = null) {
            Context.Message.DeleteAsync();
            string err = "Confused on how it works? Here's an example:\n`p*embed\n" +
                    "title: hello there\n" +
                    "description: i bet you're wondering who i am\n" +
                    "field: i certainly am | i can't even answer that\n" +
                    "field: isn't that scary | i think it is\n" +
                    "thumb: [some url]\n" +
                    "image: [some url]\n" +
                    "timestamp: true\n" +
                    "author: true\n" +
                    "color: cyan`\n\n" +
                    "(You can use as many or as few of these fields as you want.)";
            EmbedBuilder builder = new EmbedBuilder();
            if (args == null) {
                await ReplyAsync(err); return;
            }
            string[] argslines = args.Split('\n');
            if(argslines.Count() < 1) {
                await ReplyAsync(err); return;
            }
            builder = WithColor(builder, "random");
            foreach (string line in argslines) {
                try {
                    string key = line.Split(':')[0].ToLower();
                    string val = line.Substring(line.IndexOf(":")+1);
                    val = val.Trim();
                    switch (key) {
                        case "header": case "title":
                            builder = AddTitle(builder,val);
                            break;
                        case "description": case "main body": case "body":
                            builder = AddDescription(builder, val);
                            break;
                        case "field":
                            if (!val.Contains("|")) {
                                await ReplyAsync("Field must contain `|`, format: `Field: [key] | [value]`"); return;
                            }
                            string[] splits = val.Split('|');
                            builder = AddField(builder, splits[0],splits[1]);
                            break;
                        case "author":
                            if(val.ToLower() == "true") {
                                SocketUser author = Context.User;
                                builder = AddAuthor(builder, author);
                            }
                            break;
                        case "timestamp":
                            if(val.ToLower() == "true") {
                                builder.WithCurrentTimestamp();
                            }
                            break;
                        case "image": case "imageurl": case "image url":
                            builder = AddImageUrl(builder, val);
                            break;
                        case "thumbnail": case "thumb": case "thumbnailurl": case "thumburl":
                            builder = AddThumbnailUrl(builder, val);
                            break;
                        case "color":
                            builder = WithColor(builder, val);
                            break;
                        default:
                            break;
                    }
                } catch {
                    await ReplyAsync("Line: `" + line + "` contains no delimiter `:`. Unable to parse.");
                    return;
                }
            }

            await ReplyAsync("", false, builder.Build());

        }

        private EmbedBuilder WithColor(EmbedBuilder builder, string v) {
            v = v.ToLower();
            switch (v) {
                case "random":
                    Byte[] b = new Byte[3];
                    new Random().NextBytes(b);
                    builder.WithColor(b[0], b[1], b[2]);
                    return builder;
                case "red":
                    builder.WithColor(0xff, 0x00, 0x00); break;
                case "orange":
                    builder.WithColor(0xff, 0xa5, 0x00); break;
                case "yellow":
                    builder.WithColor(0xff, 0xff, 0x00); break;
                case "green":
                    builder.WithColor(0x00, 0xff, 0x00); break;
                case "blue":
                    builder.WithColor(0x00, 0x00, 0xff); break;
                case "cyan":
                    builder.WithColor(0x00, 0xff, 0xff); break;
                case "magenta":
                    builder.WithColor(0xff, 0x00, 0xff); break;
                case "purple":
                    builder.WithColor(0x80, 0x00, 0x80); break;
                case "black":
                    builder.WithColor(0x01, 0x01, 0x01); break;
                case "white":
                    builder.WithColor(0xfe, 0xfe, 0xfe); break;
                case "gray":
                case "grey":
                    builder.WithColor(0x80, 0x80, 0x80); break;
                case "pokecord":
                    builder.WithColor(0x00, 0xae, 0x86); break;
                default: break;
            }
            return builder;
        }

        public static EmbedBuilder AddField(EmbedBuilder before, string key, string value) {
            if(key == "" || key == " ") {
                key = "null";
            }
            if(value == "" || value == " ") {
                value = "null";
            }
            before.AddField(key, value);
            return before;
        }

        public static EmbedBuilder AddAuthor(EmbedBuilder before, SocketUser user) {
            if(user == null) { return before; }
            before.WithAuthor(new EmbedAuthorBuilder().WithIconUrl(user.GetAvatarUrl()).WithName(user.Username + " (" 
                + user.Id + ")" ));
            return before;
        }

        public static EmbedBuilder AddTitle(EmbedBuilder before, string header) {
            if(header == "" || header == " ") { header = "null"; }
            before.WithTitle(header);
            return before;
        }

        public static EmbedBuilder AddDescription(EmbedBuilder before, string desc) {
            if (desc == "" || desc == " ") { desc = "null"; }
            before.WithDescription(desc);
            return before;
        }

        public static bool IsImage(string url) {
            string[] acceptableEndings = { ".png", ".jpg", ".jpeg", ".gif" };
            foreach(string str in acceptableEndings) {
                if (url.EndsWith(str)) {
                    return true;
                }
            }
            if (url.Contains("?")) {
                url = url.Substring(0, url.IndexOf("?"));
            }
            foreach (string str in acceptableEndings) {
                if (url.EndsWith(str)) {
                    return true;
                }
            }
            return false;
        }

        public static EmbedBuilder AddImageUrl(EmbedBuilder before, string url) {
            try {
                if (IsImage(url)) {
                    before.WithImageUrl(url);
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            return before;
        }

        public static EmbedBuilder AddThumbnailUrl(EmbedBuilder before, string url) {
            if (IsImage(url)) {
                before.WithThumbnailUrl(url);
            }
            return before;
        }


    }
}
