using System.Reflection.PortableExecutable;

namespace CrapsStrategySimulator
{
    public enum Game
    {
        Baccarat = 0,
        Blackjack = 1,
        Craps = 2,
        Lottery = 3,
        Roulette = 4,
        Slots = 5
    }

    internal class Program
    {
        public const Game GameToPlay = Game.Craps;

        static void Main(string[] args)
        {
            Random r = new Random();


            ICrapsStrategy strategy;
            var crapsGame = new CrapsGame(r);
            strategy = new CrapsGamblingStrategy(crapsGame);
            strategy.Bet(24);
        }

        /// <summary>
        /// Represents a strategy where you make flat bets for your whole bankroll and pocket winnings.
        /// </summary>
        /// <param name="bankroll">Bankroll entering with.</param>
        /// <param name="bet">Flat bet per game.</param>
        /// <param name="gamblingStrategy">Strategy/game played</param>
        /// <returns>Money at the end of the session</returns>
        static int GambleAllBankroll(int bankroll, int bet, ICrapsStrategy gamblingStrategy)
        {
            int winnings = 0;
            while (bankroll >= bet)
            {
                var result = gamblingStrategy.Bet(bet);
                if (result >= 0)
                {
                    winnings += result;
                }
                else
                {
                    bankroll += result;
                }
            }

            return winnings + bankroll;
        } 

        static int MakeNBets(int money, int bet, int numBets, ICrapsStrategy gamblingStrategy)
        {
            for (int i = 0; i < numBets; i++)
            {
                var result = gamblingStrategy.Bet(bet);
                // Don't count break-evens as a bet.
                while (result == 0)
                {
                    result = gamblingStrategy.Bet(bet);
                }

                money += result;
            }

            return money;
        }
    }
}