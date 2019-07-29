using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Primbot_v._2.Modules.Copypastas {
    public class Event : ModuleBase<SocketCommandContext> {
        string[] common =
            { "Zizfotsys is making a self-degrading joke about himself! Ouch, that one concerned me.",
            "GardenGloves is writing down her devillish plot on how to kill Americans!",
            "Abs is getting piss drunk and telling people she's just tipsy!",
            "Fubar is banning people in the Uno server!",
            "Nothing_Face is drooling over Lolis! Completely non-sexual!",
            "Dinem is drowning ants in baking soda! Someone call PETA!",
            "Arctic Fox is trying to explain her weird fascination with an Icelandic girl to her boyfriend!",
            "Daoloth is making uncomfortable sexual jokes!",
            "Uniea is drawing a picture of her birb in anime style! Weeb.",
            "Jay is on stage singing an album! Both of the members of the audience loved it.",
            "Nanika is posting edgy things on Geeking!",
            "Shay is throwing a flag up into the air! She gave herself a concussion!",
            "Nothing_Face is playing the guitar! He cut his finger on the string! That's pretty metal.",
            "Kero is insulting people intentionally to get himself bullied!",
            "Abs is trying to make planking popular again!",
            "userevent is rapidly typing p\\*event!",
        "Nanika is making friends with a stuffed polar bear!",
            "Abs is sharing tenth-wave memes with her friends!"};
        string[] uncommon =
            {"Primaski is cooking an omelette! The kitchen is on fire!",
            "Darkness is being a loser, like always.",
            "Jay is flirting with abs again! He never understands abs isnt interested.",
            "Zizfotsys is buying pictures of people's knees!",
            "GardenGloves is dabbing on Primaski's corpse!",
        "Zizfotsys is watching a Youtube video on a dead meme that's been parodied a hundred times over!"};
        string[] rare =
            {"Abs is writing an edgy poem!",
            "Ukato is explaining his made-up culture to ignorant Americans!",
            "Morges is uploading a video of herself dabbing on Logan Paul!",
        "Jay is beating the shit out of anyone who insults his doggo!",
        };
        string[] extremelyRare =
            {"Primaski is promoting you to server owner!",
            "Positen is agreeing with an opinion Garden had!"};
           
        [Command("event", RunMode = RunMode.Async)]
        public async Task PingAsync() {
            Random rando = new Random();
            int rand;
            rand = rando.Next(1, 101);
            if(rand <= 70) {
                rand = rando.Next(0, common.Length);
                double likelihood = Math.Round((0.7) / ( 0.7*common.Length + 0.2*uncommon.Length + 0.09*rare.Length + 0.01*extremelyRare.Length),2)*100;
                if (common[rand].Contains("userevent")) {
                    string newstr = common[rand].Replace("userevent", Context.User.Username);
                    await ReplyAsync("***Current Event:\n\n***" + newstr + "\n\n***Likelihood of this event: Uncommon (" + likelihood + "%)***");
                    return;
                }
                await ReplyAsync("***Current Event:\n\n***" + common[rand] + "\n\n***Likelihood of this event: Common (" + likelihood + "%)***");
            }else if(rand <= 90) {
                rand = rando.Next(0, uncommon.Length);
                double likelihood = Math.Round((0.2) / (0.7 * common.Length + 0.2 * uncommon.Length + 0.09 * rare.Length + 0.01 * extremelyRare.Length),2)*100;
                await ReplyAsync("***Current Event:\n\n***" + uncommon[rand] + "\n\n***Likelihood of this event: Uncommon (" + likelihood + "%)***");
            }else if(rand <= 99) {
                rand = rando.Next(0, rare.Length);
                double likelihood = Math.Round((0.19) / (0.7 * common.Length + 0.2 * uncommon.Length + 0.09 * rare.Length + 0.01 * extremelyRare.Length),2)*100;
                await ReplyAsync("***Current Event:\n\n***" + rare[rand] + "\n\n***Likelihood of this event: Rare (" + likelihood + "%)***");
            } else {
                rand = rando.Next(0, extremelyRare.Length);
                double likelihood = Math.Round((0.01) / (0.7 * common.Length + 0.2 * uncommon.Length + 0.19 * rare.Length + 0.01 * extremelyRare.Length), 2)*100;
                await ReplyAsync("***Current Event:\n\n***" + extremelyRare[rand] + "\n\n***Likelihood of this event: Extremely Rare (" + likelihood + "%)***");
            }
            //await ReplyAsync("***Current Event:\n\n***" + responses[rand]);
        }
    }

}
