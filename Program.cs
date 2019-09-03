using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using static Primbot_v._2.Uno_Score_Tracking.Defs;
using System.IO;
using System.Net;
using Discord.Rest;
using Primbot_v._2.Uno_Score_Tracking;
using Primbot_v._2.Server;
using System.Threading;

namespace Primbot_v._2 {
    public class Program {
        private static string AUTH = "";
        private static DiscordSocketClient CLIENT;
        private static CommandService COMMANDS;

        private static System.Threading.Timer midnightTimer;

        private static IServiceProvider SERVICES;
        //private static readonly ulong UNO_SERVER_ID = 506267529036169257; //test server
        public static Program self;
        private static readonly string JSON_OUTPUT_PATH =
            UNO_SAVE_FILES_DIRECTORY + "\\score.json";
        static bool updateGuildCache = true;
        Random rand = new Random();

        static void Main(string[] args) {
            cmdExec();
            int iterations = 1;
            int sleepTime = 5000;
            while (true) {
                try {
                    self = new Program();
                    self.RunBotAsync().GetAwaiter().GetResult();
                } catch (Exception e) {
                    int retryTime = sleepTime * iterations;
                    if (iterations == 1) {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> " + e.Message);
                    }
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> " + iterations + " of 6 attempts were made to connect. Retrying in " + retryTime / 1000 + " seconds...");
                    if (iterations > 6) {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> " + iterations + " of 6 attempts were made to connect. Discontinuing...");
                        return;
                    }
                    Thread.Sleep(retryTime);
                    ++iterations;
                }
            }
        }

        public async Task RunBotAsync() {
            CLIENT = new DiscordSocketClient(new DiscordSocketConfig {
                AlwaysDownloadUsers = true
            });

            COMMANDS = new CommandService();
            SERVICES = new ServiceCollection()
                .AddSingleton(CLIENT)
                .AddSingleton(COMMANDS)
                .BuildServiceProvider();

            CLIENT.Log += Log;
            CLIENT.GuildMemberUpdated += MemberUpdated;
            CLIENT.GuildAvailable += UpdateServerCache;
            CLIENT.UserJoined += UpdateServerCache;


            await RegisterCommandsAsync();
            AUTH = GetAUTH();
            await CLIENT.LoginAsync(TokenType.Bot, AUTH);
            await CLIENT.StartAsync();
            await CLIENT.SetGameAsync("p*help");

            await Task.Delay(-1);

        }

        private static void cmdExec() {
            if (File.Exists(COMMANDS_TODAY_BACKUP_DIRECTORY)) {
                using (StreamReader sr = new StreamReader(COMMANDS_TODAY_BACKUP_DIRECTORY)) {
                    string buffer = sr.ReadLine();
                    if (Int32.TryParse(buffer, out int ignore)) {
                        cmdEx = Int32.Parse(buffer);
                    }
                    sr.Close();
                }
            } else {
                GuildCache.IncrementCMD(0);
            }
        }

        private static bool MidnightTimer() {
            bool retVal = false;
            DateTime now = DateTime.Now;
            DateTime midnight = DateTime.Today.AddDays(1).AddHours(EST_OFFSET).AddTicks(-1);
            TimeSpan timeLeft = midnight.Subtract(now);
            Console.WriteLine("Minutes until midnight... " + timeLeft.TotalMinutes );
            midnightTimer = new System.Threading.Timer(x => {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " --> New day called");
                NewDay();
                Thread.Sleep(1000);
                MidnightTimer();
                retVal = true;
            }, null, (int)timeLeft.TotalMilliseconds, Timeout.Infinite);
            return retVal;
        }

        private static void NewDay() {
            SocketTextChannel channel = GuildCache.Uno_Cache?.GetTextChannel(483379609723863051) ?? null;
            if (channel == null) {
                Console.WriteLine("Failed to retrieve channel at Midnight.");
                return;
            }
            GuildCache.NewDay(channel);
            midnightTimer.Dispose();
            return;
        }

        private string GetAUTH() {
            string path = DIR + "\\Server_Txt\\Key.txt";
            if (!File.Exists(path)) {
                Console.WriteLine("Error in retrieving key. Terminating.");
                Environment.Exit(0);
            }
            using (StreamReader sr = new StreamReader(path)) {
                string res = sr.ReadLine() ?? "";
                if (res != "") {
                    return res;
                }
            }
            return "";
        }

        private Task UpdateServerCache(SocketGuild arg) {

            GuildCache.connectedGuilds.Add(arg);
            //Console.WriteLine(arg.Name);
            if (updateGuildCache) {
                if (arg.Id == UNO_SERVER_ID) {
                    var v = arg.DownloaderPromise;
                    GuildCache.InitializeUnoServer(arg);
                    Console.WriteLine("Loaded UNO server.");
                    var users = arg.Users;
                    foreach (var user in users) {
                        if (!user.IsBot) {
                            SaveFiles_Mapped.CreateUserSaveFolder(user);
                        }
                    }
                    updateGuildCache = false;
                    MidnightTimer();
                } else if (arg.Id == MAGI_SERVER_ID) {
                    var u = arg.DownloaderPromise;
                    GuildCache.InitializeMyServer(arg);
                    Console.WriteLine("Loaded Magilouvre.");
                } else if (arg.Id == 597469488778182656) {
                    var u = arg.DownloaderPromise;
                    GuildCache.InitializePokeCollectors(arg);
                    Console.WriteLine("Loaded PokeCollectors.");
                }
            }
            return Task.CompletedTask;
        }

        private void KillSwitchProtocol(SocketGuildUser user) {

            if (Killswitch.GetState() == Killswitch.Status.Off) {
                return;
            } else if (Killswitch.GetState() == Killswitch.Status.Mute) {
                user.AddRoleAsync(user.Guild.GetRole(472806594292482088)); //skipped
            } else if (Killswitch.GetState() == Killswitch.Status.Kick) {
                user.KickAsync();
            } else if (Killswitch.GetState() == Killswitch.Status.Ban) {
                user.Guild.AddBanAsync(user);
            }
            return;
        }

        private Task UpdateServerCache(SocketGuildUser user) {
            if (user.Guild.Id == 597469488778182656) {
                GuildCache.Uno_Cache.GetUser(332788739560701955).SendMessageAsync(user.Username + " has joined PokéCollectors~ Help them out! <#598458534405079041>");
                GuildCache.Uno_Cache.GetUser(MY_ID).SendMessageAsync(user.Username + " has joined PokéCollectors~ Help them out! <#598458534405079041>");
                GuildCache.Uno_Cache.GetUser(456335175538835467).SendMessageAsync(user.Username + " has joined PokéCollectors~ Help them out! <#598458534405079041>");
            }
            if (Killswitch.GetState() != Killswitch.Status.Off && user.Guild.Id == UNO_SERVER_ID) {
                KillSwitchProtocol(user);
            }
            if (user.Guild.Id == UNO_SERVER_ID) {
                GuildCache.InitializeUnoServer(user.Guild);
            }
            SaveFiles_Mapped.CreateUserSaveFolder(user);
            return Task.CompletedTask;
        }

        //public Task SetGameAsync(string name, string streamUrl = null, StreamType streamType = StreamType.NotStreaming);

        private Task Log(LogMessage arg) {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private Task MemberUpdated(SocketGuildUser x, SocketGuildUser y) {
            if (y.IsBot) { return Task.CompletedTask; }

            string newTeam = "-";
            if (y.Guild.Id != UNO_SERVER_ID) {
                return Task.CompletedTask;
            } else {
                newTeam = UnoTeamChange(x, y);
            }
            bool newAccount = SaveFiles_Mapped.CreateUserSaveFolder(y);
            if (!newAccount && newTeam != "") {
                string saveFileDirectory = Defs.USER_SAVE_DIRECTORY + "\\" +
                    y.Id.ToString() + "\\" + Defs.DEFAULT_SAVE_FILE_NAME;
                SaveFiles_Mapped.ModifyFieldValue("UNOTeam", saveFileDirectory, newTeam);
            }
            return Task.CompletedTask;
        }

        //come back to
        private string UnoTeamChange(SocketGuildUser x, SocketGuildUser y) {
            /*string teamBefore = GuildCache.GetTeam(x);
            string teamAfter = GuildCache.GetTeam(y);

            if(teamBefore == teamAfter) {
                return "";
            }

            //exception unhandled string to int
            if (!Defs.UNO_SERVER_TEAMS.Contains(UInt64.Parse(teamAfter))) {
                return "";
            }
            return teamAfter;*/
            return "";
        }

        public async Task RegisterCommandsAsync() {
            CLIENT.MessageReceived += HandleCommandAsync;
            
            await COMMANDS.AddModulesAsync(Assembly.GetEntryAssembly());

        }

        private async Task UnoPings(SocketUserMessage message) {
            string target = "";
            string rel = message.Embeds.ElementAt(0).Description;
            if (rel.Contains("'s turn")) {
                target = rel.Substring(0, rel.IndexOf("'s turn"));
                target = target.Substring(target.IndexOf("now ") + 4);
                target.Trim();
            }
            if (target == "") return;
            SocketGuildUser user = GuildCache.GetUserByUsername(target);
            if (user == null) return;
            string userID = user.Id.ToString();
            try {
                bool requested = SaveFiles_Entries.EntryExists(UNO_PING_LOG, userID);
                if (requested) {
                    await message.Channel.SendMessageAsync(user.Mention + ", it's your turn! " +
                        "If you'd like to opt out of Uno pings, just type `p*unodontping`. If you'd like to opt in, " +
                        "type `p*unoping`!");
                    return;
                }
            }catch(Exception e) {
                Console.WriteLine(e);
                return;
            }
        }


        private async Task CahPings(SocketUserMessage message) {
            Console.WriteLine("cah bot");
            var channel = (SocketTextChannel)message.Channel;
            string v = message.Embeds.FirstOrDefault().Description.ToString();
            if (v.Contains(">!")) {
                await channel.SendMessageAsync(v);
                Console.WriteLine("called");
            }
        }


        private async Task HandleCommandAsync(SocketMessage arg) {
            var message = (SocketUserMessage)arg;
            var context = new SocketCommandContext(CLIENT, message);

            if (context.Guild.Id == UNO_SERVER_ID) {
                if (message.Author.Id == UNO_BOT_ID) {
                    await UnoPings(message);
                    //LogUnoGame(arg.Attachments);
                    //await message.Channel.SendMessageAsync("Retrieved the JSON file! Please check " + JSON_OUTPUT_PATH);
                }

                if (message.Author.Id == CAH_BOT_ID) {
                    await CahPings(message);
                }
            }

            if (message == null || message.Author.IsBot) {
                return;
            }

            int argPos = 0;

            if (context.Guild.Id == UNO_SERVER_ID) {
                //UNCOMMENT TOMORROW
                //LuckySpawn(context);
            }

            if ((context.Guild.Id == MAGI_SERVER_ID && message.HasStringPrefix(".catch", ref argPos))) {
                Thread.Sleep(1000);
                await message.DeleteAsync();
            }



            //if (GuildCache.SearchAwaitedMessage(context.Guild.Id, context.Channel.Id, context.User.Id, context.Message.Content) != -1) {}

            if ((message.HasStringPrefix("P*", ref argPos)) || (message.HasStringPrefix("p*", ref argPos)) ||
                message.HasMentionPrefix(CLIENT.CurrentUser, ref argPos)) {
                var result = await COMMANDS.ExecuteAsync(context, argPos, SERVICES);

                if (!result.IsSuccess) {
                    Console.WriteLine(result.ErrorReason);
                    await context.Channel.SendMessageAsync(":exclamation: **Error:** " + result.ErrorReason);
                }
            }
            return;
        }


        /*
        private async void LuckySpawn(SocketCommandContext cxt) {
            if (cxt.User.IsBot) {
                return;
            }
            int LOW_BALL = 10;
            int MID_BALL = 25;
            int HIGH_BALL = 75;
            int LEG_BALL = 150;


            //spam suppression
            char[] v = { 'a', 'e', 'i', 'o', 'u' };
            if (cxt.Channel.Id == 496058002630246430) {
                int temprand = new Random().Next(1, 4);
                if (temprand != 3) {
                    return;
                }
            }
            if (cxt.Message.Content.Split(' ').Count() < 2
                || cxt.Message.Content.Length < 7
                || !v.Any(s => (cxt.Message.Content).Contains(s))) {
                return;
            }
            //range 0, 1999
            int messageSpawn = rand.Next(0, 2900);
            //common - 1% - range of 20 in 2,000
            //uncommon - 0.4% - range of 8 in 2,000
            //rare - 0.1% - range of 2 in 2,000
            //legendary - 0.05 - range of 1 in 2,000
            EmbedBuilder emb = new EmbedBuilder().WithColor(Color.DarkPurple)
                .WithCurrentTimestamp();

            //await cxt.Channel.SendMessageAsync(messageSpawn.ToString());

            //COMMON
            Console.WriteLine(cxt.User.Username + " -> " + messageSpawn);
            if (messageSpawn >= 700 && messageSpawn < 740) {
                emb.WithTitle("Congratulations " + cxt.User.Username + ", you have found a " +
                    "**Common Spawn**!")
                    .WithDescription("This is a gift for a special one-day event to celebrate " +
                    "Uno's birthday. The server points have been automatically added to your account" +
                    " <#537104363127046145>." +
                    " Keep chatting for more gifts, and rarer spawns!")
                    .WithFooter("Rarity: 1 in 100 messages, Reward: 10 points")
                    .WithImageUrl("https://i.ytimg.com/vi/F4ZjgDQz2SI/maxresdefault.jpg");
                await cxt.Channel.SendMessageAsync(cxt.User.Mention, false, emb.Build());
                Modules.Scoring.Log x = new Modules.Scoring.Log();
                try {
                    await x.LogEvent(LOW_BALL.ToString() + " " + cxt.User.Id);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    return;
                }
                await cxt.Channel.SendMessageAsync("Successfully logged " + LOW_BALL + " points for " + cxt.User.Username);

            }

            //UNCOMMON
            if (messageSpawn >= 1198 && messageSpawn < 1212) {
                emb.WithTitle("Congratulations " + cxt.User.Username + ", you have found a " +
                 "**Uncommon Spawn**!")
                 .WithDescription("This is a gift for a special one-day event to celebrate " +
                 "Uno's birthday. The server points have been automatically added to your account" +
                 " <#537104363127046145>." +
                " Keep chatting for more gifts, and rarer spawns!")
                 .WithFooter("Rarity: 2 in 500 messages, Reward: 25 points")
                 .WithImageUrl("https://i.ytimg.com/vi/qNF5DPHaHMg/maxresdefault.jpg");
                await cxt.Channel.SendMessageAsync(cxt.User.Mention, false, emb.Build());
                Modules.Scoring.Log x = new Modules.Scoring.Log();
                try {
                    await x.LogEvent(MID_BALL.ToString() + " " + cxt.User.Id);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    return;
                }
                await cxt.Channel.SendMessageAsync("Successfully logged " + MID_BALL + " points for " + cxt.User.Username);


            }

            //RARE
            if (messageSpawn >= 1850 && messageSpawn < 1852) {
                emb.WithTitle("Congratulations " + cxt.User.Username + ", you have found a " +
    "**Rare Spawn**!")
    .WithDescription("This is a gift for a special one-day event to celebrate " +
    "Uno's birthday. The server points have been automatically added to your account" +
    " <#537104363127046145>." +
    " Keep chatting for more gifts, and rarer spawns!")
    .WithFooter("Rarity: 1 in 1,000 messages, Reward: 75 points")
    .WithImageUrl("https://i.ytimg.com/vi/YS26SubnQNk/maxresdefault.jpg");
                await cxt.Channel.SendMessageAsync(cxt.User.Mention, false, emb.Build());
                Modules.Scoring.Log x = new Modules.Scoring.Log();
                try {
                    await x.LogEvent(HIGH_BALL.ToString() + " " + cxt.User.Id);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    return;
                }
                await cxt.Channel.SendMessageAsync("Successfully logged " + HIGH_BALL + " points for " + cxt.User.Username);


            }

            //LEGENDARY
            if (messageSpawn == 36) {
                emb.WithTitle("Congratulations " + cxt.User.Username + ", you have found a " +
    "**LEGENDARY Spawn**! This is the absolute rarest gift to receive!")
    .WithDescription("This is a gift for a special one-day event to celebrate " +
    "Uno's birthday. The server points have been automatically added to your account" +
    " <#537104363127046145>." +
    " Keep chatting for more gifts, and rarer spawns!")
    .WithFooter("Rarity: 1 in 2,000 messages, Reward: 150 points")
    .WithImageUrl("https://thumbs.gfycat.com/DismalGiftedEel-size_restricted.gif");
                await cxt.Channel.SendMessageAsync(cxt.User.Mention, false, emb.Build());
                Modules.Scoring.Log x = new Modules.Scoring.Log();
                try {
                    await x.LogEvent(LEG_BALL.ToString() + " " + cxt.User.Id);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    return;
                }
                await cxt.Channel.SendMessageAsync("Successfully logged " + LEG_BALL + " points for " + cxt.User.Username);
            }
            
        }

        private void SpawnTrack(SocketUserMessage message) {
            var channel = (SocketTextChannel)message.Channel;
            bool wild = message.Embeds?.ElementAt(0).Title.Contains("A wild") ?? false;
            if (wild && !(
                message.Channel.Name.Contains("solo")
                || message.Channel.Name.Contains("secret")
                || message.Channel.Name.Contains("gym")
                || message.Channel.Name.Contains("farm")
                || message.Channel.Name.Contains("spam")
                || message.Channel.Name.Contains("private")
                )
                ) {
                var guild = channel.Guild;
                if (spawntrack && guild.GetUser(MY_ID) != null) {
                    GuildCache.Uno_Cache.GetUser(MY_ID).SendMessageAsync("Pokespawn in " + channel.Guild.Name + "! " + channel.Mention);
                }
                if (spawntrackalicia && guild.GetUser(332788739560701955) != null) {
                    GuildCache.Uno_Cache.GetUser(332788739560701955).SendMessageAsync("Pokespawn in " + channel.Guild.Name + "! " + channel.Mention);
                }
                if (spawntrackdom && guild.GetUser(534194469302566922) != null) {
                    GuildCache.Uno_Cache.GetUser(534194469302566922).SendMessageAsync("Pokespawn in " + channel.Guild.Name + "! " + channel.Mention);
                }
            }
        }
    }
    */

        /*private void LogUnoGame(IReadOnlyCollection<Attachment> jsonOutput) {
           IEnumerator<Attachment> outputs = jsonOutput.GetEnumerator();
           string desiredFileUrl = "";
            try {
                while(outputs.MoveNext()) {
                    if (outputs.Current.Url.EndsWith("json")) {
                        desiredFileUrl = outputs.Current.Url;
                    }
                }
            }catch(Exception e) {
                throw e;
            }
            if(desiredFileUrl != "") {
                using(WebClient wc = new WebClient()) {
                    wc.DownloadFile(desiredFileUrl, JSON_OUTPUT_PATH);
                }
            }
            Games_Multiplayer.UnoLog(JSON_OUTPUT_PATH);
            return;
        }*/
    }
}
