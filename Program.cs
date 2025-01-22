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


            for(int j=0;j < 25;j++){
                ICrapsStrategy strategy;
                var crapsGame = new CrapsGame(r);
                strategy = new CrapsGamblingStrategy(crapsGame);
                for (int i = 0; i < 10; i++)
                {
                    var result = strategy.Bet(24);
                }
                Console.WriteLine("Total Shooters: " + (strategy.shooters-1) + Environment.NewLine);
                
                Console.WriteLine("Winning Shooters: " + strategy.winTracker[0]);
                int average_win = strategy.winTracker[0] == 0 ? 0 : strategy.winTracker[1] / strategy.winTracker[0];
                Console.WriteLine("Avg Money won per win: " + average_win + Environment.NewLine);
                
                Console.WriteLine("Losing Shooters: " + strategy.lossTracker[0]);
                int average_loss = strategy.lossTracker[0] == 0 ? 0 : strategy.lossTracker[1] / strategy.lossTracker[0];
                Console.WriteLine("Avg Money won per win: " + average_loss + Environment.NewLine);

                Console.WriteLine("Total winnings: " + strategy.totalWinnings + Environment.NewLine + "---------------------");
            }
        }

        
    }
}