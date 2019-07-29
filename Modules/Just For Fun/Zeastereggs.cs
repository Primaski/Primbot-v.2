using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Globalization;
using Primbot_v._2.Uno_Score_Tracking;

namespace Primbot_v._2.Modules {
    public class Zeastereggs : ModuleBase<SocketCommandContext> {

        [Command("pickrandom", RunMode = RunMode.Async)]
        public async Task PickRandom([Remainder] string args = null) {
            var users = Context.Guild.Users;
            if(users == null) {
                await ReplyAsync("Cache is empty."); return;
            }
            Random rand = new Random();
            var randomUser = users.ElementAt(rand.Next(0, users.Count()));
            while (randomUser.IsBot) {
                randomUser = users.ElementAt(rand.Next(0, users.Count()));
            }
            Byte[] b = new Byte[3];
            rand.NextBytes(b);
            await ReplyAsync("", false, new EmbedBuilder().WithTitle("**" + randomUser.Username + "**")
                .WithImageUrl(randomUser.GetAvatarUrl())
                .WithColor(b[0],b[1],b[2]).Build());
        }

        [Command("rand", RunMode = RunMode.Async)]
        public async Task RandomNumber([Remainder] string args = null) {
            Random rand = new Random();
            if(args == null) {
                await ReplyAsync("Please enter a range"); return;
            }
            if(args.Split(' ').Count() != 2) {
                await ReplyAsync("`p*rand [lower bound] [upper bound]"); return;
            }
            try {
                int lower = Int32.Parse(args.Split(' ')[0]);
                int upper = Int32.Parse(args.Split(' ')[1]);
                if(lower >= upper) {
                    await ReplyAsync("`p*rand [lower bound] [upper bound]"); return;
                }
                await ReplyAsync(rand.Next(lower, upper).ToString());
            } catch {
                await ReplyAsync("`p*rand [lower bound #] [upper bound #]"); return;
            }
        }

        [Command("easteregg", RunMode = RunMode.Async)]
        public async Task EasterEggSecond() {
            await ReplyAsync("Yes, you have found an Easter Egg. I bet you feel special.");
        }

        [Command("stayinside", RunMode = RunMode.Async)]
        public async Task StayInside() {
            await ReplyAsync("You stay inside. You have a good day as a result.");
            return;
        }

        [Command("ban", RunMode = RunMode.Async)]
        public async Task BanUser([Remainder] string args = null) {
            if (args != null) {
                if (args.Contains("@everyone") || args.Contains("@here")) {
                    await ReplyAsync("Please say it again, but without the ping.");
                    return;
                }
                await ReplyAsync("Banned user: **" + args + "** :ok_hand:");
            } else {
                await ReplyAsync("Who should I ban?");
            }
            return;
        }

        [Command("nickname")]
        public async Task Q([Remainder] string args = null) {
            if(Context.User.Id != SaveFiles_GlobalVariables.MY_ID) {
                return;
            }
            args = args ?? "";
            if (args.Contains("local")) {
                args = args.Replace("local", "");
                args = args.Trim();
                await Context.Guild.GetUser(487718576892018689).ModifyAsync(x => {
                    x.Nickname = args;
                });
                await ReplyAsync("Modified to " + args);
                return;
            }
            var splits = args.Split(' ');
            try{
                //159985870458322944
                var id = UInt64.Parse(splits[0]);
                args = args.Replace(splits[0], "");
                args = args.Trim();
                await Context.Guild.GetUser(id).ModifyAsync(x => {
                    x.Nickname = args;
                });
                await ReplyAsync("Modified to " + args);
            } catch (Exception e) {
                await ReplyAsync(e.Message);
            }
            return;
        }

        [Command("niii", RunMode = RunMode.Async)]
        public async Task NikoNikoNiii([Remainder] string args = null) {
            await ReplyAsync("https://cdn.discordapp.com/attachments/474325790578704384/505489927010517022/niko.png");
        }

        /*[Command("freenanikaandabsandcherry", RunMode = RunMode.Async)]
        public async Task FreeThem([Remainder] string args = null) {
            await Context.Guild.DownloadUsersAsync();
            IGuildUser Cherry = Context.Guild.GetUser(382975353473466368);
            IGuildUser Nanika = Context.Guild.GetUser(442601155022028800);
            IGuildUser Abs = Context.Guild.GetUser(432371769723060233);
            await Cherry.RemoveRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Skipped"));
            await Nanika.RemoveRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Skipped"));
            await Abs.RemoveRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Skipped"));
            await ReplyAsync("Skipped roles removed");
            return;
        }

        [Command("spatgif", RunMode = RunMode.Async)]
        public async Task SPATGIF([Remainder] string args = null) {
            await Context.Channel.SendFileAsync("C:\\Users\\mike\\source\\repos\\Primaski Bot\\Primaski Bot\\spat.gif");
            return;
        }*/

        [Command("d2h")]
        public async Task ConvertToHex([Remainder] string args = null) {
            if (!Int32.TryParse(args, NumberStyles.HexNumber, null, out int ignore)) {
                await ReplyAsync(args + " is not an integer (try something like `p*d2h 69`)");
                return;
            }
            await ReplyAsync(args + " in hexadecimal is " + Int32.Parse(args).ToString("x16").TrimStart('0').ToUpper() + " (signed integer)");
        }

        [Command("h2d")]
        public async Task ConvertToDec([Remainder] string args = null) {
            if (!Int32.TryParse(args, NumberStyles.HexNumber,null, out int ignore)) {
                await ReplyAsync(args + " is not an integer (try something like `p*h2d deadbeef`)");
                return;
            }
            await ReplyAsync(args + " in decimal is " + Int32.Parse(args,NumberStyles.HexNumber).ToString().TrimStart('0') + " (signed integer)");
        }

        [Command("binary", RunMode = RunMode.Async)]
        public async Task ConvertToBinary([Remainder] string args = null) {
            Console.WriteLine("got here");
            if (args.ToLower().Contains("ascii")) {
                byte[] bytes = Encoding.ASCII.GetBytes(args.Replace("ascii ", ""));
                StringBuilder result = new StringBuilder();
                foreach (byte b in bytes) {
                    result.Append(Convert.ToString(b, 2) + " ");
                }
                string final = result.ToString();
                await ReplyAsync(final);
                return;
            }
            if (!Int32.TryParse(args, out int ignore)) {
                await ReplyAsync(args + " is not a number");
                return;
            }
            string binary = Convert.ToString(Int32.Parse(args), 2);
            await ReplyAsync(binary);
        }


        [Command("catch", RunMode = RunMode.Async)]
        public async Task Mistypepoke([Remainder] string args = null) {
            if (args == null) {
                args = "pokemon";
            }
            await ReplyAsync("I'm not the Pokecord bot, haha! Maybe you'll have more luck finding your " + args + " if you do `p!catch`!");
            return;
        }

        [Command("dab", RunMode = RunMode.Async)]
        public async Task Dab([Remainder] string args = null) {
            await ReplyAsync("https://steamuserimages-a.akamaihd.net/ugc/872998007404242358/0B5020C9375C74493C763E7F489694091311EDD1/");
            return;
        }

        [Command("rim")]
        public async Task Primaski([Remainder] string args = null) {
            await ReplyAsync("Oh, you think you're so clever, do you? Yes, yes, that's my name ♥");
            return;
        }

        [Command("rima")]
        public async Task Prim([Remainder] string args = null) {
            await Primaski();
            return;
        }

        [Command("rimaski")]
        public async Task Prima([Remainder] string args = null) {
            await Primaski();
            return;
        }

        [Command("mail")]
        public async Task Pmail([Remainder] string args = null) {
            await ReplyAsync("Yes, that's Gwen. uwu");
            return;
        }

        [Command("bananaboy", RunMode = RunMode.Async)]
        public async Task Bananaboy([Remainder] string args = null) {
            if(args == "disable" && Context.User.Id == SaveFiles_GlobalVariables.MY_ID) {
                await ReplyAsync("Disabled.");
                SaveFiles_GlobalVariables.disableBananaBoy = true;
                return;
            }
            if (args == "enable" && Context.User.Id == SaveFiles_GlobalVariables.MY_ID) {
                await ReplyAsync("Enabled.");
                SaveFiles_GlobalVariables.disableBananaBoy = false;
                return;
            }
            if (SaveFiles_GlobalVariables.disableBananaBoy) {
                await ReplyAsync(Context.User.Username + ", NO");
                return;
            }
            await ReplyAsync("i work ever day and peel banana. it is the only thing i know how to do.");
            await Task.Delay(2000);
            await ReplyAsync("i start at young age, picking up banana from market and taking it home to mom.");
            await Task.Delay(2000);
            await ReplyAsync("\"look mom\" i say in excitement.");
            await Task.Delay(2000);
            await ReplyAsync("\"banana\" my mom agrees, tears of excitement running down her face.");
            await Task.Delay(2000);
            await ReplyAsync("it was mom who taught me to peel banana. she cry every time.");
            await Task.Delay(2000);
            await ReplyAsync("every day mom cry.");
            await Task.Delay(2000);
            await ReplyAsync("one day i bring home banana, mom is not there.");
            await Task.Delay(2000);
            await ReplyAsync("my nine brothers are not there.");
            await Task.Delay(2000);
            await ReplyAsync("there is a note on the kitchen table. i stare at the words.");
            await Task.Delay(2000);
            await ReplyAsync("it is too bad that i can not read.");
            await Task.Delay(2000);
            await ReplyAsync("the only thing i know is banana.");
            await Task.Delay(2000);
            await ReplyAsync("i wait all night. i peel banana.");
            await Task.Delay(2000);
            await ReplyAsync("mom does not come back.");
            await Task.Delay(2000);
            await ReplyAsync(" next morning i find more banana. i come home and there is new family in my home.");
            await Task.Delay(2000);
            await ReplyAsync("they say they don't want banana.");
            await Task.Delay(2000);
            await ReplyAsync("i peel banana. new family throws me out.");
            await Task.Delay(2000);
            await ReplyAsync("the must not like banana. i am alone.");
            await Task.Delay(2000);
            await ReplyAsync("i only have banana in this world. it is all i need.");
            await Task.Delay(2000);
            return;
        }

    }
}
