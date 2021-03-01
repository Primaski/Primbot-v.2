using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primbot_v._2.Server {
    public class TriviaQuestion {

        public string question { get; private set; }
        public List<string> acceptableAnswers { get; private set; }
        public string hint { get; private set; }

        public enum Category { Gaming, Sports, Science, Geography, History, Language, Entertainment };
        public TriviaQuestion(string question, List<string> acceptableAnswers, string hint = null) {
            this.question = question;
            this.acceptableAnswers = acceptableAnswers;
            this.hint = hint ?? "No hint!";
        }
    }
}
