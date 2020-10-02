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
            //cmdExec();
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
            TimeSpan timeLeft = TimeUntilMidnight();
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
            Console.WriteLine(rel);
            if (rel.Contains("'s turn")) {
                target = rel.Substring(0, rel.IndexOf("'s turn"));
                target = target.Substring(target.IndexOf("It is now ") + 10);
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

        private Task CheckCount(SocketUserMessage message) {
            var channel = (SocketTextChannel)message.Channel;
            string count = message.ToString().Split(' ')[0] ?? "notanumber";
            if(!Int32.TryParse(count, out int ignore)) {
                channel.SendMessageAsync(message.Author.Mention + ", please start your messages with a number.");
                return Task.CompletedTask;
            }
            using (var tw = new StreamReader(MAGI_COUNT, true)) {
                string buffer = tw.ReadLine();
                if (Int32.Parse(buffer) != Int32.Parse(count) - 1) {
                    channel.SendMessageAsync(message.Author.Mention + ", you miscounted! The last number was " + buffer + "! Please try again.");
                    return Task.CompletedTask;
                }
            }
            File.Delete(MAGI_COUNT);
            using (var tw = new StreamWriter(MAGI_COUNT, true)) {
                tw.WriteLine(count);
            }
            return Task.CompletedTask;
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

            if (message == null || (message.Author.IsBot && message.Author.Id != 494274144159006731)) {
                return;
            }

            int argPos = 0;

            if ((message.HasStringPrefix("p*", ref argPos)) || (message.HasStringPrefix("P*", ref argPos)) ||
                message.HasMentionPrefix(CLIENT.CurrentUser, ref argPos)) {
                var result = await COMMANDS.ExecuteAsync(context, argPos, SERVICES);

                if (!result.IsSuccess) {
                    Console.WriteLine(result.ErrorReason);
                    await context.Channel.SendMessageAsync(":exclamation: **Error:** " + result.ErrorReason);
                }
                return;
            }
            if (context.Guild.Id == MAGI_SERVER_ID && !message.Author.IsBot && !message.HasStringPrefix("p*", ref argPos)) {
                ulong magiid = 515956792459657217;
                if (message.Channel.Id == magiid) {
                    await CheckCount(message);
                }
            }
            return;
        }
    }
}
