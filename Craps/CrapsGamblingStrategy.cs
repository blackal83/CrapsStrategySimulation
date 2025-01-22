namespace GamblingAnalysis
{
    public class CrapsGamblingStrategy : IGamblingStrategy
    {
        private CrapsGame _crapsGame;
        public int shooters { get; private set; } = 0;
        public int totalWinnings { get; private set; } = 0;
        public int[] winTracker { get; private set; } = {0,0};
        public int[] lossTracker { get; private set; } = {0,0};

        public CrapsGamblingStrategy(CrapsGame crapsGame) {
            this._crapsGame = crapsGame;
        }

        public int Bet(int betAmount)
        {
            //return this._crapsGame.DontPassBetWithOdds(betAmount);
            int result = this._crapsGame.DuckRaguBet(betAmount);
            shooters++;
            totalWinnings += result;
            if (result >= 0) { //a push is considered a win in my book, since you didn't lose money
                winTracker[0]++;
                winTracker[1] += result;
            }
            else {
                lossTracker[0]++;
                lossTracker[1] += result;
            }
            return result;
        }
    }
}
