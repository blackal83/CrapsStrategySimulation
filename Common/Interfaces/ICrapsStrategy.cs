namespace CrapsStrategySimulator
{
    public interface ICrapsStrategy
    {
        public int shooters { get; }
        public int totalWinnings { get; }
        public int[] winTracker { get;  }
        public int[] lossTracker { get;  }
        /// <summary>
        /// Runs a bet using the gambling strategy.
        /// </summary>
        /// <param name="betAmount">Amount that was bet</param>
        /// <returns>0 if a loss or bet plus winnings if a win</returns>
        int Bet(int betAmount);
    }
}
