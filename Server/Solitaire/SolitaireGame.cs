using Primbot_v._2.Modules.Information;
using Primbot_v._2.Uno_Score_Tracking;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Primbot_v._2.Server.Solitaire {
    public class SolitaireGame {

        /***
         * Cards are defined by bytes, whereby the two MSBs define the suit (club/diamond/heart/spades), 
         * and the four LSBs define the number (NULLa23445678910jqk). The two middle digits are irrelevant and will be ignored.
         * 00 00 0000 - NULL (due to LSBs)
         * 00 00 0001 - Club, Ace
         * 00 11 0001 - Club, Ace (irrelevant middle bits)
         * 01 00 0001 - Diamond, Ace
         * 10 00 1000 - Heart, Eight
         * 11 00 1101 - Spades, King
        ***/

        private static readonly string[] ranks = { "?", "a", "2", "3", "4", "5", "6", "7", "8", "9", "10", "j", "q", "k" };
        private static readonly string[] suits = { "c", "d", "h", "s" };
        private static readonly int NUMBER_OF_SETS = 7; //do not modify
        private enum CardLocation { NOWHERE, SET_ONE, SET_TWO, SET_THREE, SET_FOUR, SET_FIVE, SET_SIX, SET_SEVEN, 
            CLUB_FOUNDATION, DIAMOND_FOUNDATION, HEART_FOUNDATION, SPADE_FOUNDATION, POWER_CARD, DRAWN, }; //do not modify order
        private enum MoveType { UNKNOWN, POWER_CARD_TO_SET, POWER_CARD_TO_FOUNDATION, SET_CARD_TO_SET, SET_CARD_TO_FOUNDATION, FOUNDATION_CARD_TO_SET  }

        private ulong _playerID = 0;
        public ulong playerID { get { return _playerID; } set { if (_playerID == 0) { _playerID = value; } } }
        private List<byte> _drawnCards = new List<byte>();
        public List<byte> drawnCards { get { return _drawnCards; } private set { _drawnCards = value; } }

        private List<byte> deck;
        private List<Tuple<byte, bool>>[] table; //tuple: card, faceUp
        private byte[] foundations = new byte[4] { 0, 0, 0, 0 };
        public byte powerCard { get { return _drawnCards.Last(); } }

        /*use ResetVals() on these values*/
        private byte cardFrom;
        private byte cardTo;
        private CardLocation cardFromLoc;
        private CardLocation cardToLoc;
        private MoveType moveType;

        public bool CreateGame(ulong playerID) {
            this.playerID = playerID;
            table = new List<Tuple<byte, bool>>[NUMBER_OF_SETS];
            ResetVals();
            CreateDeck();
            ShuffleDeck();
            SetTable();
            DrawCards(3);
            return true;
        }
        private void CreateDeck() {
            deck = new List<byte>();
            for (byte i = 1; i <= 0b11001101; i++) {
                if ((i & 0b00001111) % 0b1110 == 0) {
                    i = (byte)((0b11000000 & i) + 0b01000001);
                }
                deck.Add(i);
            }
        }

        private void ShuffleDeck() {
            int len = deck.Count();
            for (int n = len - 1; n > 0; --n) {
                int k = Defs.random.Next(n + 1);
                byte temp = deck[n];
                deck[n] = deck[k];
                deck[k] = temp;
            }
        }

        private void SetTable() {
            //TO-DO: edge case... cards in deck < cards to be dealt on table
            int cardsInSet = 7;
            for (int i = 0; i < NUMBER_OF_SETS; i++) { //focus on set position (no of cards: 7,6,5,4,3,2,1)
                table[i] = table[i] ?? new List<Tuple<byte, bool>>();
                for (int j = cardsInSet; j > 0; j--) { //focus on card position
                    table[i].Add(new Tuple<byte, bool>(DrawFromTopOfDeck(), (j == 1) ? true : false)); //flip up final card
                }
                cardsInSet--;
            }
        }

        private List<byte> DrawCards(int no) {
            if (no > deck.Count()) throw new Exception("Attempted to draw more cards than were left in the deck.");
            for (int i = 0; i < no; i++) {
                drawnCards.Add(DrawFromTopOfDeck());
            }
            return drawnCards;
        }


        /// <summary>
        /// Strictly for moving cards or groupings of cards - and NOT for drawing 
        /// </summary>
        public void MakeMove(string moveCmd) {
            /* All moves are of the format: [cardfrom] [cardto] (example 10h js). Possibilities:
             * 1. Moving power card onto a set (alternating colors, descending)
             * 2. Appending part/all of a set onto another set (same rule as above)
             * 3. Moving king to an empty space (format: [cardfrom] X)
             * 4. Moving a card to a foundation pile
             */

            /*populates global variables cardFrom and cardTo*/
            try { GetCommandCards(moveCmd); } catch (Exception e) { throw e; }
            /*populates global variables cardFromLoc and cardToLoc*/
            CheckTableForCards();
            CheckPowerCardForCards();
            CheckFoundationForCards();
            if(cardFromLoc == CardLocation.NOWHERE || cardToLoc == CardLocation.NOWHERE) {
                string error = (cardFromLoc == CardLocation.NOWHERE ? CardToString(cardFrom) : "") + 
                    (cardToLoc == CardLocation.NOWHERE ? CardToString(cardTo) : "") + " is not a valid playable card!";
                throw new Exception(error);
            }
            /*populates*/
            DetermineMoveType();

            ResetVals(); //necessary at end of function
        }



        public string DisplayTable() {
            //TO-DO: emojify later
            StringBuilder str = new StringBuilder();
            bool resfound = true;
            for (int row = 0; resfound == true; row++) {
                resfound = false;
                for (int column = 0; column < table.Length; column++) {
                    if (row < table[column].Count()) {
                        if (table[column][row].Item2) {
                            resfound = true;
                            str.Append(CardToString(table[column][row].Item1, true) + " ");
                        } else {
                            str.Append("xx "); //hidden
                        }

                    } else {
                        str.Append("-- ");
                    }
                }
                if (resfound) str.AppendLine();
            }
            string final = str.ToString();
            return final.Substring(0, final.LastIndexOf('\n'));
        }

        public string DisplayPowerCard() {
            return CardToString(powerCard, true);
        }





        ///////////////////////* Helper Methods */////////////////////////
        private byte DrawFromDeck(int pos = 0) {
            byte card = deck[pos];
            deck.RemoveAt(pos);
            return card;
        }

        private byte DrawFromTopOfDeck() {
            return DrawFromDeck(0);
        }

        private string CardToString(byte card, bool abbr = false) {
            return RankToString(card, abbr) + (abbr ? "" : " of ") + SuitToString(card, abbr);
        }

        public string DeckToString(bool abbr = false) {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < deck.Count(); i++) {
                str.Append(CardToString(deck[i], abbr) + ", ");
            }
            return str.ToString();
        }

        public string TableToString(bool abbr = false) {
            StringBuilder str = new StringBuilder();
            for (int setNo = 0; setNo < NUMBER_OF_SETS; setNo++) {
                str.Append((setNo + 1).ToString() + ":");
                for (int cardNo = 0; cardNo < table[setNo].Count(); cardNo++) {
                    str.Append(CardToString(table[setNo][cardNo].Item1, abbr) + (table[setNo][cardNo].Item2 ? "" : "(DOWN)") + ", ");
                }
                str.AppendLine();
            }
            return str.ToString();
        }

        private string SuitToString(byte card, bool abbr = false) {
            card = GetSuit(card);
            switch (card) {
                case 0: return (abbr ? "c" : "Clubs");
                case 1: return (abbr ? "d" : "Diamonds");
                case 2: return (abbr ? "h" : "Hearts");
                case 3: return (abbr ? "s" : "Spades");
            }
            throw new Exception("Card with suit " + card + " does not exist.");
        }

        private byte GetSuit(byte card) {
            return (byte)(card >> 6);
        }

        private string RankToString(byte card, bool abbr = false) {
            card = GetRank(card);
            switch (card) {
                case 1: return (abbr ? "A" : "Ace");
                case 11: return (abbr ? "J" : "Jack");
                case 12: return (abbr ? "Q" : "Queen");
                case 13: return (abbr ? "K" : "King");
                default: break;
            }
            if (card > 1 && card < 11) return card.ToString();
            throw new Exception("Card with rank " + card + " does not exist.");
        }

        private byte StringToCard(string card) {
            card = card.Trim().ToLower();
            byte final = 0;
            bool found = false;
            for (byte i = 1; i < ranks.Length; i++) {
                if (card.Contains(ranks[i])) {
                    final += i;
                    card = card.Replace(ranks[i], "");
                    found = true;
                    break;
                }
            }
            if (!found) throw new Exception("Invalid card: " + card);
            found = false;
            for (byte i = 0; i < suits.Length; i++) {
                if (card.Contains(suits[i])) {
                    final += (byte)(i * 64); //bitwise shift to MSD
                    found = true;
                    card = card.Replace(suits[i], "");
                }
            }
            if (!found || card.Length != 0) throw new Exception("Invalid card: " + card);
            return final;
        }

        private byte GetRank(byte card) {
            card = (byte)(card & 0b1111);
            return (card > 0 && card < 14) ? card :
                throw new Exception("Card with rank " + card.ToString() + " does not exist.");
        }

        private bool IsACard(byte card) {
            try {
                GetRank(card);
                return true;
            } catch { return false; }
        }

        public void RevealTableCards() {
            for (int i = 0; i < table.Count(); i++) {
                for (int j = 0; j < table[i].Count(); j++) {
                    table[i][j] = new Tuple<byte, bool>(table[i][j].Item1, true);
                }
            }
            return;
        }
        private void ResetVals() {
            cardFrom = cardTo = 0;
            cardFromLoc = cardToLoc = CardLocation.NOWHERE;
            moveType = MoveType.UNKNOWN;
            return;
        }

        private void GetCommandCards(string moveCmd) {
            cardTo = cardFrom = 0;
            if (String.IsNullOrEmpty(moveCmd)) {
                throw new Exception("Please provide a move of the format `[card from] [card to]`. For example, " +
                "if you were moving a 10 of Hearts onto a Jack of Spades, you would type `p*s 10h js`.");
            }
            try {
                string[] args = moveCmd.Split(' ');
                cardFrom = StringToCard(args[0]);
                cardTo = StringToCard(args[1]);
            } catch {
                throw new Exception("Provide at least two cards of the format [rank][suit], with a space in between.");
            }
            if (!IsACard(cardFrom) || !IsACard(cardTo)) throw new Exception("At least one of your arguments was not a valid card.");
            if (cardTo == cardFrom) throw new Exception("You cannot play a card onto itself.");
            return;
        }
        private bool CheckTableForCards() {
            bool foundBoth = false;
            for (int i = 0; i < table.Length; i++) { //FIRST: Search the table
                for (int j = table[i].Count() - 1; j >= 0; j--) {
                    if (!table[i][j].Item2) break; //face down card, move onto next row
                    if (cardFromLoc == CardLocation.NOWHERE && table[i][j].Item1 == cardFrom) cardFromLoc = (CardLocation)(i + 1);
                    if (cardToLoc == CardLocation.NOWHERE && table[i][j].Item1 == cardTo) cardToLoc = (CardLocation)(i + 1);

                    if (cardFromLoc != CardLocation.NOWHERE && cardToLoc != CardLocation.NOWHERE) {
                        foundBoth = true; 
                        break;
                    }
                }
                if (foundBoth) break;
            }
            return foundBoth;
        }

        private bool CheckPowerCardForCards() {
            if (powerCard == cardFrom) cardFromLoc = CardLocation.POWER_CARD;
            if (powerCard == cardTo) cardToLoc = CardLocation.POWER_CARD;
            return cardFromLoc != CardLocation.NOWHERE && cardToLoc != CardLocation.NOWHERE;
        }
        private bool CheckFoundationForCards() {
            for(int i = 0; i < foundations.Length; i++) {
                //IF ORDER IN CARDLOCATION ENUM CHANGES, THEN THIS METHOD NEEDS TO BE TOO
                if (cardFromLoc != CardLocation.NOWHERE && cardFrom == foundations[i]) cardFromLoc = (CardLocation)(i+8);
                if (cardToLoc != CardLocation.NOWHERE && cardTo == foundations[i]) cardToLoc = (CardLocation)(i+8);
            }
            return cardFromLoc != CardLocation.NOWHERE && cardToLoc != CardLocation.NOWHERE;
        }

        /// <summary>
        /// TO-DO: DOUBLE CHECK
        /// </summary>
        private void DetermineMoveType() {
            if(cardFrom == 0 || cardTo == 0 || cardFromLoc == CardLocation.NOWHERE || cardToLoc == CardLocation.NOWHERE) {
                throw new Exception("Could not determine a move type.");
            }
            if(cardFromLoc == CardLocation.POWER_CARD) {
                if (cardToLoc == CardLocation.SET_ONE || cardToLoc == CardLocation.SET_TWO || cardToLoc == CardLocation.SET_THREE || cardToLoc != CardLocation.SET_FOUR
                    || cardToLoc == CardLocation.SET_FIVE || cardToLoc == CardLocation.SET_SIX || cardToLoc == CardLocation.SET_SEVEN) {
                    moveType = MoveType.POWER_CARD_TO_SET; 
                    return;
                }
                if (cardToLoc == CardLocation.CLUB_FOUNDATION || cardToLoc == CardLocation.DIAMOND_FOUNDATION ||
                    cardToLoc == CardLocation.HEART_FOUNDATION || cardToLoc == CardLocation.SPADE_FOUNDATION) {
                    moveType = MoveType.POWER_CARD_TO_FOUNDATION; 
                    return;
                }
                throw new Exception("Cannot move power card to this location.");
            
            }else if(cardFromLoc == CardLocation.CLUB_FOUNDATION || cardFromLoc == CardLocation.DIAMOND_FOUNDATION ||
                cardFromLoc == CardLocation.HEART_FOUNDATION || cardFromLoc == CardLocation.SPADE_FOUNDATION) {
                if(cardToLoc == CardLocation.SET_ONE || cardToLoc == CardLocation.SET_TWO || cardToLoc == CardLocation.SET_THREE || cardToLoc != CardLocation.SET_FOUR
                    || cardToLoc == CardLocation.SET_FIVE || cardToLoc == CardLocation.SET_SIX || cardToLoc == CardLocation.SET_SEVEN) {
                    moveType = MoveType.FOUNDATION_CARD_TO_SET;
                    return;
                }
                throw new Exception("Cannot move foundation card to this location.");
            
            }else if (cardFromLoc == CardLocation.SET_ONE || cardFromLoc == CardLocation.SET_TWO || cardFromLoc == CardLocation.SET_THREE || cardFromLoc != CardLocation.SET_FOUR
                    || cardFromLoc == CardLocation.SET_FIVE || cardFromLoc == CardLocation.SET_SIX || cardFromLoc == CardLocation.SET_SEVEN) {
                if (cardToLoc == CardLocation.SET_ONE || cardToLoc == CardLocation.SET_TWO || cardToLoc == CardLocation.SET_THREE || cardToLoc != CardLocation.SET_FOUR
                     || cardToLoc == CardLocation.SET_FIVE || cardToLoc == CardLocation.SET_SIX || cardToLoc == CardLocation.SET_SEVEN) {
                    moveType = MoveType.SET_CARD_TO_SET;
                    return;
                }
                if (cardToLoc == CardLocation.CLUB_FOUNDATION || cardToLoc == CardLocation.DIAMOND_FOUNDATION ||
                    cardToLoc == CardLocation.HEART_FOUNDATION || cardToLoc == CardLocation.SPADE_FOUNDATION) {
                    moveType = MoveType.SET_CARD_TO_FOUNDATION;
                    return;
                }
            }
            throw new Exception("Impossible move.");
        }
    }

}
