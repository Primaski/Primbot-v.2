using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using static Primbot_v._2.Uno_Score_Tracking.Defs;
using System.IO;
using System.Globalization;
using System.Threading;
using Discord;
using System.Diagnostics;

namespace Primbot_v._2.Server {
    public static class Trivia {

        private static double TOURNEY_START_DELAY = 280.0;
        private static double HINT_DELAY = 20.0;
        private static double POST_HINT_DELAY = 20.0;
        private static int LONG_PAUSE_DELAY = 5000; //ms
        private static int SHORT_PAUSE_DELAY = 3000; //ms
        private static bool TUTORIAL = true;
        private static bool PIN = true;
        private static int MIN_MSG = 3;
        private static int number = 0;


        private static SocketGuild server;
        private static SocketTextChannel channel;
        private static bool started = false;
        private static Dictionary<ulong, int> players = new Dictionary<ulong, int>();
        private static Random rnd;


        public static List<TriviaQuestion> Gaming;
        public static List<TriviaQuestion> Sports;
        public static List<TriviaQuestion> Science;
        public static List<TriviaQuestion> Geography;
        public static List<TriviaQuestion> History;
        public static List<TriviaQuestion> Language;
        public static List<TriviaQuestion> Entertainment;
        public static List<TriviaQuestion> FINAL;
        public static TriviaQuestion selectedQuestion;

        public static List<string> words = new List<string> { "balloon", "berry", "simple", "diary", "heavenly",
        "bento box", "couldron", "slinky", "squish", "waterfall", "crunch", "zesty", "carefree", "curry",
        "sugar", "makeshift", "poultry", "weary", "neverending" };

        public static bool NewQuestion(string type, string aftermsg = null) {
            if (!started) {
                return false;
            }
            bool gotAQuestion = PickNewQuestion(type);
            if (!gotAQuestion) {
                return false;
            }
            number++;
            AnnounceQuestion();
            if (aftermsg != null) {
                Say(aftermsg);
            }

            var correctAnswerNoHint = AwaitResponses(HINT_DELAY);
            if (correctAnswerNoHint == null) {
                GiveHint();
                var correctAnswerWithHint =
                    AwaitResponses(POST_HINT_DELAY);
                if (correctAnswerWithHint == null) {
                    GiveAnswers();
                } else {
                    Congratulate(correctAnswerWithHint);
                }
            } else {
                Congratulate(correctAnswerNoHint);
            }
            return true;
        }

        private static void Congratulate(IMessage correct) {
            Say("Congratulations, " + correct.Author.Mention + ", that was the correct answer! You will now be awarded points.");
            GivePoints(correct);
            Thread.Sleep(SHORT_PAUSE_DELAY);
            Say("Here's the leaderboard as of now!");
            Say(GetLeaderboard());
            Thread.Sleep(LONG_PAUSE_DELAY);
            Say("The first person to say \"" + RandomWord() + "\" gets to choose the next category!");
        }

        private static string RandomWord() {
            return words[rnd.Next(0, words.Count())];
        }

        private static void GivePoints(IMessage correct) {
            if (players == null) {
                players = new Dictionary<ulong, int>();
            }
            foreach (var player in players) {
                ulong userID = correct.Author.Id;
                if (player.Key == userID) {
                    int score = players[userID];
                    score += TRIVIA_POINT_VALUE;
                    players[userID] = score;
                    return;
                }
            }
            players.Add(correct.Author.Id, TRIVIA_POINT_VALUE);
        }

        private static string GetLeaderboard() {
            StringBuilder w = new StringBuilder();
            var u = players.OrderByDescending(key => key.Value);
            int i = 1; int rank = 0; int prevTop = -1;
            foreach (var player in u) {
                if (player.Value != prevTop) {
                    rank = i;
                }
                w.AppendLine(rank + ". " + (Uno_Score_Tracking.GuildCache.GetUserByID(player.Key).Username ?? "unknown user")
                    + " --> " + player.Value + " points");
                i++;
            }
            return w.ToString();
        }

        public static void EditLeaderboard(ulong ID, int newVal, bool setByDefault = true) {
            if (!players.ContainsKey(ID)) {
                players.Add(ID, newVal);
                return;
            }
            if (setByDefault) {
                players[ID] = newVal;
            } else {
                int orig = players[ID];
                players[ID] = newVal + orig;
            }
            return;
        }

        private static void GiveAnswers() {
            Say("Sorry, no one got it! Here were the acceptable answers:\n");
            Thread.Sleep(SHORT_PAUSE_DELAY);
            StringBuilder w = new StringBuilder("`");
            foreach (string answer in selectedQuestion.acceptableAnswers) {
                w.Append(answer + ", ");
            }
            w.Append("`");
            Say(w.ToString());
            Thread.Sleep(SHORT_PAUSE_DELAY);
            Say("Guess I'm choosing the next category!");
        }

        private static void GiveHint() {
            Say(":exclamation: Need a hint? " + selectedQuestion.hint);
            return;
        }

        //returns message with correct answer
        private static IMessage AwaitResponses(double delayInSeconds) {
            Stopwatch stop = new Stopwatch();
            stop.Start();
            while (stop.Elapsed.TotalSeconds < delayInSeconds) {
                IEnumerable<IMessage> w = channel.GetMessagesAsync(MIN_MSG).FlattenAsync().Result;
                for (int i = 0; i < w.Count(); ++i) {
                    if (selectedQuestion.acceptableAnswers.
                        Contains(w.ElementAt(i).Content.ToLower())) {
                        return w.ElementAt(i);
                    }
                }
            }
            return null;
        }

        private static bool AnnounceQuestion() {
            if (selectedQuestion == null) {
                return false;
            }
            Say("***QUESTION " + number + ":***");
            Say(":question: **" + selectedQuestion.question + "** :question:");
            Say("`(a hint will appear after " + HINT_DELAY.ToString() +
                " seconds if no correct answers)`");
            return true;
        }

        public static void End() {
            HINT_DELAY *= 2;
            POST_HINT_DELAY *= 2;
            //TRIVIA_POINT_VALUE *= 2;
            Say("Well, " + number + " questions later, and we seem to be at the end here.");
            Thread.Sleep(SHORT_PAUSE_DELAY);
            Say("Our final question will be an UNO related one, worth double standard points. You will also have twice as much time to answer");
            Thread.Sleep(SHORT_PAUSE_DELAY);
            Say("Are you guys ready? Here is your final question of the night.");
            Thread.Sleep(LONG_PAUSE_DELAY);
            Say("<:UnoRed:472812408986009600> <:UnoYellow:472812390421889044> <:UnoGreen:472812420952358922> <:UnoBlue:472812434059427850>");
            NewQuestion("FINAL", "<:UnoRed:472812408986009600> <:UnoYellow:472812390421889044> <:UnoGreen:472812420952358922> <:UnoBlue:472812434059427850>");
        }


        public static int Start(SocketTextChannel socketChannel) {
            Console.WriteLine("Trivia started!");
            channel = socketChannel;
            //while (false) {
            Say(":microphone2: **Welcome to Late Night Trivia everyone!** :microphone2:");
            Thread.Sleep(SHORT_PAUSE_DELAY);
            Say("Hosted by me! A Discord bot!");
            Thread.Sleep(SHORT_PAUSE_DELAY);
            Say("I hope everyone is having a nice summer vacation! Except if you don't have a summer vacation, in which case...");
            Thread.Sleep(SHORT_PAUSE_DELAY);
            Say("I hope you're suffering marginally less than usual!");
            Thread.Sleep(LONG_PAUSE_DELAY);
            Say("Well, before I open this channel, let's wait for some more people to arrive.\n\nI'd like to get an idea on how many people I'm expecting?");
            Thread.Sleep(SHORT_PAUSE_DELAY);
            Emoji heartEmoji = new Emoji("\u2665");
            var message = Say("**Don't worry! You can hop in at any time. But for now, just click on this heart reaction to " +
                "let me know if you're in! And I'll start in a few minutes.**", new List<Emoji> { heartEmoji });
            List<Tuple<IUser, Emoji>> reactions = Channel.GetReactions(message, new List<Emoji> { heartEmoji }, TOURNEY_START_DELAY);
            List<IUser> initialParticipants = new List<IUser>();
            foreach (var user in reactions) {
                Console.WriteLine(user.Item1.Username);
                if (!user.Item1.IsBot) {
                    initialParticipants.Add(user.Item1);
                }
            }
            foreach (var user in initialParticipants) {
                Console.WriteLine(user.Username);
            }
            //}
            Preface(channel, initialParticipants);
            return 1;
        }

        public static int Preface(SocketTextChannel channel, List<IUser> initialParticipants = null, bool tutorial = true) {
            InitializeQuestions();
            rnd = new Random();
            started = true;
            if (!tutorial) {
                TUTORIAL = false;
            }
            Say("And we're back!!");
            Thread.Sleep(SHORT_PAUSE_DELAY);
            Say("Looks like we got a nice crowd! Let me ping everyone so they come back.");
            StringBuilder x = new StringBuilder("Who do we got...\n\nSo,");
            if (initialParticipants != null) {
                if (initialParticipants.Count() != 0) {
                    for (int i = 0; i < initialParticipants.Count(); i++) {
                        if ((i + 1) < initialParticipants.Count()) {
                            x.Append(initialParticipants[i].Mention + ", and ");
                        } else {
                            x.Append(initialParticipants[i].Mention + "! Come on back!");
                        }
                    }
                } else {
                    x.Append(" nobody. I see.");
                }
            } else {
                x.Append(" nobody. I see.");
            }
            Say(x.ToString());
            if (TUTORIAL) {
                Thread.Sleep(LONG_PAUSE_DELAY);
                Say("Here is how the game will be played:");
                Thread.Sleep(LONG_PAUSE_DELAY);
                var toPin = Say("**Normally, categories are chosen by the players, over some short competition. Tonight's would have been \"solve the anagram\", but it seems we're a bit short on questions.**");
                Thread.Sleep(SHORT_PAUSE_DELAY);
                Say("There will be 32 questions, plus one final, and I will choose the questions for you guys!");
                Thread.Sleep(SHORT_PAUSE_DELAY);
                Say("Most questions will NOT be multiple choice or T/F. Instead, you will type in the answer directly. As such, the questions have been made a little easier!");
                Thread.Sleep(LONG_PAUSE_DELAY);
                Say("Slowmode will be on with a delay of five seconds, so answers cannot be spammed. Make sure to think out the answer before you say anything!");
                Thread.Sleep(LONG_PAUSE_DELAY);
                Say("However, don't be __too__ slow! ONLY the first person to guess the answer correctly will be awarded points.");
                Thread.Sleep(LONG_PAUSE_DELAY);
                Say("There will be 40 questions, each for " 
                    .ToString() + ", and one final question worth " + (TRIVIA_POINT_VALUE * 2).ToString());
                Thread.Sleep(LONG_PAUSE_DELAY);
                Say(":exclamation: ***VERY IMPORTANT: Please type ONLY the answer: no extra words, no punctuation, JUST the answer.*** :exclamation:");
                Thread.Sleep(SHORT_PAUSE_DELAY);
                Say("Please do NOT cheat by looking up answers or asking others, we want the game to be fair for everyone.");
                Thread.Sleep(SHORT_PAUSE_DELAY);
                Say("After a question is answered, a leaderboard will be shown, and the person with the correct answer will be awarded their points! And if no one gets it correct, then I am granted legal permission to kill one of you at random.");
                Thread.Sleep(LONG_PAUSE_DELAY);
                Say("***Reminder: We are short on questions, so questions will be chosen by Primbot.***");
                Thread.Sleep(LONG_PAUSE_DELAY);
                Say("Anyone can join at any time just by saying the answer. There are no penalties for wrong answers, except for agonizing shame.");
                Thread.Sleep(SHORT_PAUSE_DELAY);
                if (PIN) { toPin.PinAsync(); }
                Say("The instructions can be read again in the pins. If everyone is ready, let's get warmed up. I'll choose the category... how about History?");
            }
            return 1;
        }

        public static IUserMessage Say(string message, List<Emoji> reactions = null, SocketTextChannel x = null) {
            if (x != null) {
                return Channel.ReportToChannel(x, message, null, reactions);
            }
            return Channel.ReportToChannel(channel, message, null, reactions);
        }


        private static bool PickNewQuestion(string type) {
            type = type.ToLower();
            switch (type) {
                case "gaming":
                    if (Gaming.Count() > 0) {
                        int index = rnd.Next(0, Gaming.Count() - 1);
                        selectedQuestion = Gaming[index];
                        Gaming.RemoveAt(index);
                    } else {
                        return false;
                    }
                    break;
                case "sports":
                    if (Sports.Count() > 0) {
                        int index = rnd.Next(0, Sports.Count() - 1);
                        selectedQuestion = Sports[index];
                        Sports.RemoveAt(index);
                    } else {
                        return false;
                    }
                    break;
                case "science":
                    if (Science.Count() > 0) {
                        int index = rnd.Next(0, Science.Count() - 1);
                        selectedQuestion = Science[index];
                        Science.RemoveAt(index);
                    } else {
                        return false;
                    }
                    break;
                case "geography":
                    if (Geography.Count() > 0) {
                        int index = rnd.Next(0, Geography.Count() - 1);
                        selectedQuestion = Geography[index];
                        Geography.RemoveAt(index);
                    } else {
                        return false;
                    }
                    break;
                case "history":
                    if (History.Count() > 0) {
                        int index = rnd.Next(0, History.Count() - 1);
                        selectedQuestion = History[index];
                        History.RemoveAt(index);
                    } else {
                        return false;
                    }
                    break;
                case "language":
                    if (Language.Count() > 0) {
                        int index = rnd.Next(0, Language.Count() - 1);
                        selectedQuestion = Language[index];
                        Language.RemoveAt(index);
                    } else {
                        return false;
                    }
                    break;
                case "entertainment":
                    if (Entertainment.Count() > 0) {
                        int index = rnd.Next(0, Entertainment.Count() - 1);
                        selectedQuestion = Entertainment[index];
                        Entertainment.RemoveAt(index);
                    } else {
                        return false;
                    }
                    break;
                case "final":
                    if (FINAL.Count() > 0) {
                        int index = rnd.Next(0, FINAL.Count() - 1);
                        selectedQuestion = FINAL[index];
                        FINAL.RemoveAt(index);
                    }
                    break;
                default: return false;
            }
            return true;
        }

        public static void InitializeQuestions() {
            //by category

            //2
            Gaming = new List<TriviaQuestion> {
                new TriviaQuestion("Many gamers have recently caused an uproar after Game Freak announced that the latest Pokemon game will not " +
                "contain every Pokemon in the series. What is the name for the device that records information of all Pokemon to have existed, and will not " +
                "make a return in Sword and Shield?",new List<string>{"national dex", "national pokedex", "national pokédex"},"It is a particular kind of Pokédex, " +
                "not one limited to a single region."),
                new TriviaQuestion("In what Japanese city does Nintendo have its headquarters?",new List<string>{ "kyoto" },"This was the former capital of Japan, beginning with a K."),
                //first 3d game
            };

            //7 ->9
            Sports = new List<TriviaQuestion> {
                new TriviaQuestion("In the American NFL, blimps are often seen flying over the stadium, notably during the Super Bowl. Which company owns them?",
                new List<string> { "goodyear", "goodyear auto", "goodyear autos", "goodyear auto service", "good year" },"They are mostly known for their automobile tire division."),
                new TriviaQuestion("In a professional game of Water Polo, how many members are there per team? (including the goalkeeper)", new List<string> { "7", "seven" },""),
                new TriviaQuestion("A viral YouTuber named Jelle has made an increasingly popular series involving an annual sporting competition among marbles. What is this " +
                "series of events called?", new List<string> { "marblelympics", "marblympics", "the marblelympics", "the marblympics"},"The name is a pun that utilizes the word \"Olympics\"."),
                new TriviaQuestion("What Yankee Hall of Famer was called the Sultan of Swat?",new List<string>{ "babe ruth" },"This baseball player was at one time " +
                "baseball's all-time homerun king."),
                new TriviaQuestion("Which New York Met Hall of Fame pitcher was nicknamed \"The Franchise\"?",new List<string>{ "tom seaver", "george thomas seaver",
                "seaver" },"He is also known under the alias \"Tom Terrific\"."),
                new TriviaQuestion("Name one of the boxers who fought in the famous boxing match dubbed \"The Thrilla in Manila\"?",
                new List<string>{ "joe frazier", "muhammad ali", "muhamad ali", "muhammed ali", "the greatest", "smokin joe", "smokin' joe", "smoking joe", "joseph frazier" },
                "One of these famous boxers passed away in 2016 from Parkinson's."),
                new TriviaQuestion("In 1995, a famous wrestling match was held between an American and a Japanese wrestling company that featured a match between famous Antonio Inoki and " +
                "Ric Flair, as a cultural festival for world peace. Which country was this held in?",new List<string>{ "north korea", "dprk", "korea", "people's republic of korea" },"It is one of the " +
                "most secretive and hostile countries on Earth, knowing for hating Japan and the USA immensely."),
            };

            //6 -> 15
            Science = new List<TriviaQuestion> {
                new TriviaQuestion("The small nation of Morocco hosts 75% of one of the world's most vital natural resources, often used in soil. What is it?",new List<string>{
                "phosphorus", "phosphorous", "phosphate", "rock phosphate" },"The rocks that contain this element are found at the bottom of the sea, and is non-renewable."),
                new TriviaQuestion("A server would likely be responsible for connecting devices and applications through the Internet. What would be used for STORING " +
                "large amounts of user data, that are retrieveable over a server?"  ,new List<string>{ "database", "databases", "data base", "data bases" },
                "The word contains \"data\" in it."),
                new TriviaQuestion("If someone studies mycology, what specimen(s) are they studying?",new List<string>{ "fungi", "fungus" },"It is neither animal nor plant."),
                new TriviaQuestion("All insects' bodies contain three main body regions: the head, the abdomen, and what?",new List<string>{ "thorax" },"No hint! Think of ants!"),
                new TriviaQuestion("On a computer monitor, a perfect mixture of the primary colors green and blue will yield cyan. What about red and green?",
                new List<string>{ "yellow" },"The primary colors of a computer monitor are Red, Green and Blue, but printer ink cartridges have the same primary colors as monitors have secondary colors."),
                new TriviaQuestion("In relation to a timeline, B.C. stands for Before Christ and A.D. for Anno Domini. B.C.E. and C.E. have been proposed and are now widely accepted as secular alternatives. What does " +
                "C.E. stand for?",new List<string>{ "current era", "common era" },"The second word in this abbreviation is \"era\"."),
                };

            //2 -> 17
            Geography = new List<TriviaQuestion> {

               new TriviaQuestion("Despite making up half of the world's total area, the Southern Hemisphere only contains 10 - 12% of its population. Two notable countries hold " +
                "almost the entirety of the population south of the Equator. Name one of them.", new List<string> { "indonesia", "brazil", "java" },"One is in South America, the other in Asia."),
                new TriviaQuestion("Wir gehen nach Deutschland! Four cities in this country exceed 1 million in population. Berlin is one. Can you name one of the other three?",
                new List<string> { "hamburg", "munich", "cologne", "münchen", "munchen", "koln", "köln" },"One of the city's has a famous food named directly after them, namely at fast food restaurants."),

            };

            //6 -> 23
            History = new List<TriviaQuestion> {

                new TriviaQuestion("World War 1 was ignited by the assassinated of Archduke Ferdinand. Of what country was he the Archduke?", new List<string>{ "austria", "austria-hungary", "austria hungary",
                "austria hungry", "austria-hungry", "austro-hungarian empire" },"While he was assassinated in Sarajevo, the modern day capital of this country is Vienna."),
                new TriviaQuestion("Who composed the Minute Waltz in 1847?", new List<string>{ "chopin", "choppin", "fredric chopin", "frédéric chopin", "frederick chopin", "fryderyk chopin"  },""),
                new TriviaQuestion("The September 11 attacks were a series of four terrorists attacks by Al-Qaeda: Most notably two of which were the Twin Towers, but also name one of the other buildings targetted.", 
                new List<string>{ "pentagon", "white house", "the pentagon", "the white house" },"They are two of the most notable buildings of power in the US."),
                new TriviaQuestion("Current Event! The largest protest in Hong Kong's history is currently transpiring, thanks to a bill that was attempted to be passed. What type of bill was this?",
                new List<string>{"extradition", "extradition bill", "extradite", "jurisdiction", "extradite bill"},"The term refers to the handing over of a person or entity to the state in which the crime was committed for trial." +
                " (In this case, Hong Kong to China)."),
                new TriviaQuestion("The largest bomb ever created and detonated by man was made in Russia. What is its name?",new List<string>{ "tsar bomba", "tsar bomb", "rds-202",
                "rds 202", "tsar"},""),
                new TriviaQuestion("A formerly common exchange method before medium of exchanges, like currency, could be widely enforced and distributed involved a system where people exchanged goods and services directly " +
                "for other goods and services. What is this system called?",new List<string>{ "barter", "barter system", "bartering", "bartering system", "gift economy", "barter economy" },""),

            };

            //4 -> 27
            Language = new List<TriviaQuestion> {
                new TriviaQuestion("A nation allied with the USSR during the Cold War switched to the Cyrillic script, ending the only major language on Earth that used to " +
                "use a script written from top to bottom. What language is this?", new List<string> { "mongolian", "mongol" },"The country is reknowned to once having the " +
                "largest contiguous empire by land mass on Earth."),
                new TriviaQuestion("Let's switch over to Russian! How do you say hello in Russian?", new List<string>{ "привет", "privet", "privyet" },"No hint!"),
                new TriviaQuestion("What is the most famous conlang (artificially constructed language) in the world? This language gave rise to the origin name of the Pokemon Regigigas.",
                new List<string>{ "esperanto" },"The constructed language rhymes with \"canto\", and boasts over 350 native speakers."),

                new TriviaQuestion("Google defines this word as: \"a word or phrase that is not formal or literary, typically one used in ordinary or familiar conversation.\"",
                new List<string>{ "colloquial", "colloquialism", "coloquial" },"It rhymes with oatmeal, and is often used in describing words in foreign languages to others."),
                
            };

            //5 -> 32
            Entertainment = new List<TriviaQuestion> {
                new TriviaQuestion("Which TV station famously featured cartoons such as Cartoon Sushi, Liquid Television, The Maxx, and Beavis and Butthead?",
                new List<string>{ "mtv", "m.t.v.",  "music television", "m.t.v", "viacom" },"The station primarily focused on music videos, notably pop-up videos."),
                new TriviaQuestion("The most liked non-music video on YouTube was formerly held by Pewdiepie, but has since claimed a new beholder. What is the name of the Youtuber with" +
                " the currently most liked video?",new List<string>{ "mr beast", "mr. beast", "mrbeast", "jimmy donaldson",  },"The YouTuber is known for spending massive amounts of money on his " +
                "friends, doing ridiculous stunts."),
                new TriviaQuestion("In what movie did a man relive a train explosion over and ocver again?",new List<string>{ "source code" },"The director was Duncan Jones, and the movie's name has to do " +
                "with computer coding."),
                new TriviaQuestion("Michael Jackson went by The King of Pop, while which singer beholds the title of the King of Rock and Roll?",new List<string>{ "elvis presley" },"Hound Dog and Jailhouse rock were two famous " +
                "songs by this author."),
                new TriviaQuestion("\"Float like a butterfly, sting like a bee.\" Who muttered this phrase?",new List<string>{ "muhammad ali", "muhamad ali" },"He is known most prominently for his boxing career."),
               };

            //1 -> 33
            FINAL = new List<TriviaQuestion> {
                new TriviaQuestion("The main antagonist of Cuphead (apart from the Devil) is a man in a business suit, with a die for a head. What is his name?",new List<string>{ "king dice", "mister king dice", "mr king dice", "mr. king dice" },
                "The word \"dice\" is in his name."),
                };
        }
    }
}
