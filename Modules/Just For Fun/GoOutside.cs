using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Primbot_v._2.Modules.Copypastas {
    public class GoOutside : ModuleBase<SocketCommandContext> {
        string[] responses = {
        "You found a corpse floating in a pond. You poke it with a stick. The corpse giggled. Awkward. You went back home before it gets any more uncomfortable.",
        "You saw a cat stuck up on a tree branch. You try to rescue it. You hold your arms out. The cat jumps down, and you fail at catching it. The owner noticed. You flee the country.",
        "You see this guy in a leather jacket snapping his fingers by the water fountain. You run back inside before anything happened.",
        "You see a fire burning in the distance. You run over to see that it's your local church. You try to put the fire out with your pee. The fireman pushes you out of the way. You continue to pee as you lie in the fire. Your corpse is buried, but the pee continues to flow out, making your grave become a fountain. A Brita® filter is put over it so people may enjoy the stream.",
        "You breathe fresh air. It causes you to faint.",
        "You see a sleeping cow at the local farm. You try to push it over, to see if it will really fall. You use the entirety of your strength, and the cow doesn't budge. You try to go to the gym, but you're rejected, as you don't have a gym membership.",
        "You see a local protest going on. You grab a picket sign, and join in the fun. Only once you're in handcuffs, do you realize you were actually supporting the Discord Light Theme.",
        "You lay down in the grass, soaking in the sun. You fall asleep. You wake up in a hospital bed. You got skin poisoning.",
        "You decide to go to a coffee shop. A girl invites you over to have coffee with her. The meeting goes well, and you're both interested in each other. She offers sex. You bashfully lower her dress. She has a dick.",
        "You decide to partake of some spiritual Falun Gong. Unfortunately, your neighbor works for the Chinese government, and the next thing you know, your house is Falun GONE.",
        "You see a little girl playing on the playground. You smile at her. She smiles back. The police officer smiles as he throws you into the back of the police car.",
        "You stretch your legs out on a park bench. A pervert takes pictures of your feet from behind a bush. You agree to allow him to keep them for $10 a pop. He says he was just taking pictures of the beautiful sky, and shows you his phone as proof. Turns out you're the conceited pervert.",
        "You see a woman at a stand, promoting some political movement. Instead of looking away, you smile at her, which gives her reason to promote her political agenda for 45 minutes. You decide to never be a nice person again.",
        "You hear someone playing some sick jams over by the soccer field. You rush over and start breaking out your dance moves. All of a sudden, it becomes cloudy and lightning strikes you. God was saving humanity from that atrocity you called \"dancing\".",
        "You see a rave in the distance, and decide it'd be fun for you to join. The bouncer says you were not dressed appropriately for this event. You take your clothes off. The bouncer lets you in.",
        "You see this garage sale. You try to buy a used microwave. What a bargain, only $3! The only downside was that it didn't have a door.",
        "You see a snowplow sponsered by PornHub. You question whether you actually left the Internet.",
        "You are attacked by a persistent wasp. You scream as it bangs itself against you over and over. It falls into your screaming mouth. You set yourself on fire.",
        "You see an alt-right supporter criticizing you for your music taste. You call his mother and inform her of this misconduct. He apologizes, and buys you a drink.",
        "You open your door. The sun is shining, birds are chirping, abs is singing owl city at the top of her lungs. You close your door. Maybe tomorrow.",
        "You see an edgy teenager spinning a fidget spinner. You are so impressed, you take a selfie with him. Half of your friends unfriend you on Snapchat.",
        "You jump into your neighborhood pool. You have a great time. You try to shower upon exitting the pool, and the shower only sprays water with chlorine. You invest in revitalizing your neighborhood. Except you don't have money. You try to get a job. You get accepted for one, and then you realize you're way too lazy to continue. You go home.",
        "You see your neighborhood milk man delivering milk to your neighbor, but not you. You'd like to confront him, especially since you're craving milk, but you have crippling social anxiety.",
        "You see your ex out there. He wants to get back together with you. You're cripplingly lonely, so you accept. You and him make out violently. Just like the good old days.",
        "You decide to go to Six Flags, as you seek thrill. You bring an extra flag with you, because more flags = more fun. You get caught with it as you are going through the metal detector. You are forced to leave the park with no refund.",
        "You see a Pokemon. You reach into your bag to pull out a Pokeball. The Pokemon pulls out its own Pokeball to catch you. Things get awkward.",
        "You realize it's actually Halloween. Trick or Treaters are frolicking around merrily. You decide to tag along with a group, as they were impressed by how horrifying your costume was. Except you weren't wearing anything more than regular clothing.",
        "You see this Lemonade Stand. They're selling Lemonade for only 25 cents. You slip the little girl a $50 and tell her to spike yours with alcohol. She gladly accepts.",
        "You find someone's lost child. The child tries to direct you home to his parents, but you realize he is simply leading you back to your own house. You tear up from the wholesomeness.",
        "You feel the rain pattering on your head. Still, you keep a smile on your face, letting the rain patter on your tonuge. Did you expect this to take a wrong turn? Because, it's not going to. It was just a good day, okay?",
        "You step into a pile of fire ants, by accident. Luckily, the ants are nice enough to forgive you. You buy their Queen a coffee as a thank you. No, not tea. Coffee.",
        "You see your old friend. You catch up with him, and he informs you of all the ill omens that has transpired for him. Turns out, every misfortune of his was a direct result of you leaving him. You are unable to go to sleep that night.",
        "You hop in your car and drive down to the gas station. You tell the guy to fill it up. He informs you that they only have diesel. You don't know what that is, so you accept it with a smile. Your dad makes you pay for a new car.",
        "You see a local shop closing down. This brings tears to your eyes, as you can remember the days where you used to go outside and enjoy breakfast there. You run over to the shop owner and ask if there's anything you can do to save the shop. There are tears in your eyes. She says she's just closing for the day, it's 5 PM.",
        "You see a bunch of cute anime girls and a trap playing at your local pool. You understand this scene is only for fanservice, but you can't help your primal urges, and you run over. The 2D girls inform you that they are only attracted to two-dimensional people, and that they'd rank you a 0/10. You call them one-dimensional characters and storm out."
        };

        [Command("gooutside", RunMode = RunMode.Async)]
        public async Task TaskAsync([Remainder] string args = null) {
            if (args == null) {
                Random rando = new Random();
                int rand = rando.Next(0, responses.Length);
                await ReplyAsync("You dared to venture outside.\n\n" + responses[rand] + "\n\n ***Response Number: " + (rand + 1) + "***.");
            } else {
                int m = -1;
                try {
                    m = Int32.Parse(args);
                } catch (FormatException e) {
                    await ReplyAsync(args + " is not a number!");
                    return;
                }
                m -= 1;
                if(m >= responses.Length || m < 0) {
                    await ReplyAsync("That number is not in the proper range. Consider using a number between 1 and " + responses.Length);
                } else {
                    await ReplyAsync(responses[m] + "\n\n***Response Number: " + (m+1) + "***.");
                }
            }
        }
    }
}
