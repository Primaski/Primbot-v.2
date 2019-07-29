using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primbot_v._2.Server {
    public class Channel {

        public static bool AddReactions(IUserMessage msg, List<Emoji> emojis = null) {
            if (msg == null) {
                return false;
            } else if (emojis == null) {
                return false;
            }
            foreach (var emoji in emojis) {
                msg.AddReactionAsync(emoji);
            }
            return true;
        }

        public static bool AddReactions(IUserMessage msg, List<Emote> emotes = null) {
            if (msg == null) {
                return false;
            } else if (emotes == null) {
                return false;
            }
            foreach (var emote in emotes) {
                msg.AddReactionAsync(emote);
            }
            return true;
        }

        public static List<Tuple<IUser, Emoji>> GetReactions(IUserMessage msg, List<Emoji> emojis, double secondInterval) {
            Stopwatch timer = new Stopwatch();
            List<Tuple<IUser, Emoji>> reactionTuples = new List<Tuple<IUser, Emoji>>();
            timer.Start();
            Console.WriteLine("starting");
            while (timer.Elapsed.TotalSeconds < secondInterval) {
                foreach (Emoji emoji in emojis) {
                    IReadOnlyCollection<IUser> users = (IReadOnlyCollection<IUser>)(msg.GetReactionUsersAsync(emoji).Result);
                    for (int i = 0; i < users.Count(); i++) {
                        var u = users.ElementAt(i);
                        bool found = false;
                        foreach (var tuple in reactionTuples) {
                            if (tuple.Item1.Id == u.Id) {
                                found = true;
                                continue;
                            }
                        }
                        if (!found) {
                            reactionTuples.Add(new Tuple<IUser, Emoji>(u, emoji));
                        }
                    }
                }
                //Console.WriteLine(timer.Elapsed.TotalSeconds + " < " + secondInterval + " returns " + (timer.Elapsed.TotalSeconds < secondInterval).ToString());
            }

            return reactionTuples;
        }

        //public static ReceiveEmojiReaction(IUserMessage msg, int secondsToWait = 5, ulong expectedUserID = 0) {
        //var reactions = msg.GetReactionUsersAsync();
        //}

        /// <summary>
        /// Reports message to a channel, and returns the sent message
        /// </summary>
        public static IUserMessage ReportToChannel(SocketTextChannel channel, string text = "", Embed embed = null, List<Emoji> reaction = null) {
            var x = (channel.SendMessageAsync(text));
            var y = (IUserMessage)x.Result;
            if (reaction != null) {
                AddReactions(y, reaction);
                Console.WriteLine("msg reported with emoji ==>");
            } else {
                Console.WriteLine("msg reported ==>");
            }
            return y;
        }

        public static bool ReportToChannel(SocketGuild guild, string channel, string text = "", Embed embed = null) {
            throw new NotImplementedException();
        }

        internal static Tuple<IUser, Emote> GetFirstReaction(RestUserMessage msg, List<Emote> emotes, ulong expectedUser = 0) {
            foreach (Emote emote in emotes) {
                IReadOnlyCollection<IUser> reactions = null;
                if (expectedUser == 0) {
                    reactions = msg.GetReactionUsersAsync(emote).Result;
                } else {
                    reactions = msg.GetReactionUsersAsync(emote, 100, expectedUser).Result;
                }
                for (int i = 0; i < reactions.Count; ++i) {
                    IUser reactor = reactions.ElementAt(i);
                    if (!reactor.IsBot) {
                        return new Tuple<IUser, Emote>(reactor, emote);
                    }
                }
            }
            return null;
        }
    }
}
