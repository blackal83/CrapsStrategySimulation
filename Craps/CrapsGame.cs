using System.ComponentModel;
using System.Dynamic;
using System.Reflection.Metadata.Ecma335;

namespace GamblingAnalysis
{
    public class CrapsGame
    {
        private Random _r;
        private MockRandom _mr = new MockRandom();
        private bool testing = false;
        static int numRolls = 0;

        public CrapsGame(Random r) {
            if (!testing)
            { 
                this._r = r;
            }
            else
            {
                this._r = _mr;
            }
        }

        public int DuckRaguBet(int bet)
        {
            ValidateBet(bet);

            List<PowerPressEntry> powerPressTable = InitPowerPressTable();
            GameState gameState = InitializeGameState(bet);

            while (true)
            {
                var roll = Roll();

                if (IsPushRoll(roll))
                {
                    Console.WriteLine("Push.");
                    continue;
                }

                if (IsSevenRoll(roll))
                {
                    return HandleSevenRoll(gameState, bet);
                }

                if (!gameState.HasMovedToAcross)
                {
                    HandleInitialRolls(gameState, powerPressTable, roll);
                }
                else
                {
                    HandleAcrossRolls(gameState, powerPressTable, roll);
                }

                if (roll == gameState.Point)
                {
                    SetNewPoint(gameState);
                }
            }
        }

        private void ValidateBet(int bet)
        {
            if (bet != 24 && bet != 12)
            {
                throw new ArgumentException("Starting Bet must be 12 or 24 for this strategy!");
            }
        }

        private GameState InitializeGameState(int bet)
        {
            GameState gameState = new GameState(bet)
            {
                ComeOutRoll = Roll()
            };
            while (IsInvalidComeOutRoll(gameState.ComeOutRoll))
            {
                Console.WriteLine("No Point Set.");
                gameState.ComeOutRoll = Roll();
            }
            gameState.Point = gameState.ComeOutRoll;
            Console.WriteLine("Point set to: " + gameState.Point);
            return gameState;
        }

        private bool IsInvalidComeOutRoll(byte roll)
        {
            return roll == 7 || roll == 11 || roll == 2 || roll == 3 || roll == 12;
        }

        private bool IsPushRoll(byte roll)
        {
            return roll == 2 || roll == 3 || roll == 12 || roll == 11;
        }

        private bool IsSevenRoll(byte roll)
        {
            return roll == 7;
        }

        private int HandleSevenRoll(GameState gameState, int bet)
        {
            Console.WriteLine("Shooter Rolled a 7");
            int total = gameState.Winnings - gameState.Bet;
            Console.WriteLine("Total Winnings: " + total);
            return total;
        }

        private void HandleInitialRolls(GameState gameState, List<PowerPressEntry> powerPressTable, byte roll)
        {
            switch (roll)
            {
                case 4:
                case 5:
                case 9:
                case 10:
                    Console.WriteLine("No Win");
                    break;
                case 6:
                case 8:
                    HandleSixOrEightRoll(gameState, powerPressTable, roll);
                    break;
            }
        }

        private void HandleSixOrEightRoll(GameState gameState, List<PowerPressEntry> powerPressTable, byte roll)
        {
            int rollIndex = RollIndex.GetIndex(roll);
            int otherRollIndex = roll == 6 ? RollIndex.Eight : RollIndex.Six;

            if (!gameState.IsPressed[rollIndex] && !gameState.IsPressed[otherRollIndex])  //if neither 6 or 8 has been pressed
            {
                PowerPress(gameState, powerPressTable, roll);
            }
            else if (!gameState.IsPressed[rollIndex] && gameState.IsPressed[otherRollIndex]) //if the just rolled number has not been pressed but the other one has
            {
                if (!gameState.HasReset && !gameState.HasCollected)
                {
                    Reset(gameState, powerPressTable, roll);
                }
                else
                {
                    PowerPress(gameState, powerPressTable, roll);
                }
            }
            else if (gameState.IsPressed[rollIndex] && !gameState.IsPressed[otherRollIndex]) //if the just rolled number has  been pressed but the other one has not
            {
                if (gameState.HasReset || gameState.HasCollected)
                {
                    GoAcross64(gameState, powerPressTable, roll);
                }
                else if (!gameState.HasCollected)
                {
                    Collect(gameState, powerPressTable, roll, false);
                }
            }
            else if (gameState.IsPressed[rollIndex] && gameState.IsPressed[otherRollIndex])  // if both numbers have been pressed
            {
                GoAcross64(gameState, powerPressTable, roll);
            }
        }

        private static void HandleAcrossRolls(GameState gameState, List<PowerPressEntry> powerPressTable, byte roll)
        {
            if (!gameState.IsPressed[RollIndex.GetIndex(roll)])
            {
                PowerPress(gameState, powerPressTable, roll);
            }
            else
            {
                Collect(gameState, powerPressTable, roll, true);
            }
        }

        private void SetNewPoint(GameState gameState)
        {
            Console.WriteLine("Shooter hit the point (" + gameState.Point + ")");
            gameState.ComeOutRoll = Roll();
            while (IsInvalidComeOutRoll(gameState.ComeOutRoll))
            {
                Console.WriteLine("No Point Set.");
                gameState.ComeOutRoll = Roll();
            }
            gameState.Point = gameState.ComeOutRoll;
            Console.WriteLine("Point set to: " + gameState.Point);
        }

        private static List<PowerPressEntry> InitPowerPressTable()
        {
            return new List<PowerPressEntry>
            {
                // Format: InitialBet, Point, Win, Give, NewBet - negative give is a get
                // Point 4 & 10 - starting at 5/6
                new (5,   4,  9,    1,  15 ),
                new (15,  4,  27,   -2,  40 ),
                new (40,  4,  78,   -18, 100 ),
                new (100, 4,  195,  5,   300 ),
                new (300, 4,  585,  15,  900 ),
                new (900, 4,  1755, 0,   900 ),

                new (5,   10, 9,    1,   15 ),
                new (15,  10, 27,   -2,  40 ),
                new (40,  10, 78,   -18, 100 ),
                new (100, 10, 195,  5,   300 ),
                new (300, 10, 585,  15,  900 ),
                new (900, 10, 1755,  0,  900 ),

                 // Point 4 & 10 - starting at 10/12
                new (10,   4,  18,   -3,  25 ),
                new (25,   4,  49,   1,   75 ),
                new (75,   4,  146,  -21, 200 ),
                new (200,  4,  390,  -90, 500 ),
                new (500,  4,  975,  25,  1500 ),
                new (1500, 4,  2925, 0,   1500 ),

                new (10,   10,  18,   -3,  25 ),
                new (25,   10,  49,   1,   75 ),
                new (75,   10,  146,  -21, 200 ),
                new (200,  10,  390,  -90, 500 ),
                new (500,  10,  975,  25,  1500 ),
                new (1500, 10,  2925, 0,   1500 ),

                
                // Point 5 & 9 - starting at 5/6
                new (5,   5,  9,   1,   15 ),
                new (15,  5,  21,  0,   36 ),
                new (36,  5,  50,  0,   86 ),
                new (86,  5,  120, -6,  200 ),
                new (200, 5,  280, 20,  500 ),
                new (500, 5,  700, 0,   500 ),

                new (5,   9,  9,   1,   15 ),
                new (15,  9,  21,  0,   36 ),
                new (36,  9,  50,  0,   86 ),
                new (86,  9,  120, -6,  200 ),
                new (200, 9,  280, 20,  500 ),
                new (500, 9,  700, 0,   500 ),

                // Point 5 & 9 - starting at 10/12
                new (10,  5,  14,   1,   25 ),
                new (25,  5,  35,   0,   60 ),
                new (60,  5,  84,   -19, 125 ),
                new (125, 5,  175,  0,   300 ),
                new (300, 5,  420,  30,  750 ),
                new (750, 5,  1050, 0,   750 ),

                new (10,  9,  14,   1,   25 ),
                new (25,  9,  35,   0,   60 ),
                new (60,  9,  84,   -19, 125 ),
                new (125, 9,  175,  0,   300 ),
                new (300, 9,  420,  30,  750 ),
                new (750, 9,  1050, 0,   750 ),
            
                // Point 6 & 8 - starting at 5/6
                new (6,   6,  7,   5,   18 ),
                new (18,  6,  21,  3,   42 ),
                new (42,  6,  49,  1,   90 ),
                new (90,  6,  105, 15,  210 ),
                new (210, 6,  245, -5,  450 ),
                new (450, 6,  525, 0,   450 ),

                new (6,   8,  7,   5,   18 ),
                new (18,  8,  21,  3,   42 ),
                new (42,  8,  49,  1,   90 ),
                new (90,  8,  105, 15,  210 ),
                new (210, 8,  245, -5,  450 ),
                new (450, 8,  525, 0,   450 ),

                // Point 6 & 8 - starting at 10/12
                new (12,  6,  14,  4, 30 ),
                new (30,  6,  35,  1, 66 ),
                new (66,  6,  77,  7, 150 ),
                new (150, 6,  175, 5, 330 ),
                new (330, 6,  385, 5, 720 ),
                new (720, 6,  840, 0, 720 ),

                new (12,  8,  14,  4, 30 ),
                new (30,  8,  35,  1, 66 ),
                new (66,  8,  77,  7, 150 ),
                new (150, 8,  175, 5, 330 ),
                new (330, 8,  385, 5, 720 ),
                new (720, 8,  840, 0, 720 ),
            };
            
        }

        private static void Reset(GameState gameState, List<PowerPressEntry> powerPressTable, byte roll)
        {
            Collect(gameState, powerPressTable, roll, false);

            gameState.HasReset = true;
            gameState.CurrentBets[RollIndex.Six] = gameState.ResetBet;
            gameState.CurrentBets[RollIndex.Eight] = gameState.ResetBet;
            gameState.IsPressed[RollIndex.Six] = false;
            gameState.IsPressed[RollIndex.Eight] = false;
            
            Console.WriteLine("Reset to " + gameState.ResetBet + " and " + gameState.ResetBet + " on 6/8");
        }

        private static void GoAcross64(GameState gameState, List<PowerPressEntry> powerPressTable, byte roll)
        {

            Collect(gameState, powerPressTable, roll, false);
            gameState.HasMovedToAcross = true;
            gameState.IsPressed[RollIndex.Six] = false;
            gameState.IsPressed[RollIndex.Eight] = false;

            if (gameState.ResetBet == 12) 
            {
                gameState.CurrentBets[RollIndex.Four] = 10;
                gameState.CurrentBets[RollIndex.Five] = 10;
                gameState.CurrentBets[RollIndex.Six] = 12;
                gameState.CurrentBets[RollIndex.Eight] = 12;
                gameState.CurrentBets[RollIndex.Nine] = 10;
                gameState.CurrentBets[RollIndex.Ten] = 10;
                gameState.Bet = 64;
                Console.WriteLine("Moved to 64 across.");
            }
            else if (gameState.ResetBet == 6) 
            {
                gameState.CurrentBets[RollIndex.Four] = 5;
                gameState.CurrentBets[RollIndex.Five] = 5;
                gameState.CurrentBets[RollIndex.Six] = 6;
                gameState.CurrentBets[RollIndex.Eight] = 6;
                gameState.CurrentBets[RollIndex.Nine] = 5;
                gameState.CurrentBets[RollIndex.Ten] = 5;
                gameState.Bet = 32;
                Console.WriteLine("Moved to 32 across.");
            }

        }

        private static void Collect(GameState gameState, List<PowerPressEntry> powerPressTable, byte roll, bool resetPressed )
        {
            int cbIndex = RollIndex.GetIndex(roll);
            var powerPress = powerPressTable.Find(x => x.InitialBet == gameState.CurrentBets[cbIndex] && x.Point == roll);
            if (powerPress == null)
            {
                Console.WriteLine("Could not find initialbet and point match in the power press table, exiting...");
                System.Environment.Exit(1);
            }
            gameState.Winnings += powerPress.Win;
            if (powerPress.InitialBet != powerPress.NewBet && resetPressed) 
            {
                gameState.IsPressed[cbIndex] = false;
            }
            gameState.HasCollected = true;
            Console.WriteLine("Won " + powerPress.Win + " on an initial bet of " + powerPress.InitialBet + " and collected it.");
        }

        private static void PowerPress(GameState gameState, List<PowerPressEntry> powerPressTable, byte roll)
        {
            int cbIndex = RollIndex.GetIndex(roll);
            var powerPress = powerPressTable.Find(x => x.InitialBet == gameState.CurrentBets[cbIndex] && x.Point == roll);

            if (powerPress == null)
            {
                Console.WriteLine("Could not find initialbet and point match in the power press table, exiting...");
                System.Environment.Exit(1);
            }
            gameState.Winnings += powerPress.Win;

            gameState.CurrentBets[cbIndex] = powerPress.NewBet;
            gameState.IsPressed[cbIndex] = true;
            gameState.Bet = gameState.CurrentBets.Sum();
            Console.WriteLine("Won " + powerPress.Win + " on an initial bet of " + powerPress.InitialBet + " and pressed to " + powerPress.NewBet + ".");
        }

        private class PowerPressEntry
        {
            public int InitialBet { get; }
            public int Point { get; }
            public int Win { get; }
            public int Give { get; }
            public int NewBet { get; }

            public PowerPressEntry(int initialBet, int point, int win, int give, int newBet)
            {
                InitialBet = initialBet;
                Point = point;
                Win = win;
                Give = give;
                NewBet = newBet;
            }
        }
        private class GameState
        {
            public int[] CurrentBets { get; set; }
            public bool[] IsPressed { get; set; }
            public bool HasMovedToAcross { get; set; }
            public bool HasReset { get; set; }
            public bool HasCollected { get; set; }
            public int Winnings { get; set; }
            public int Bet {get; set; }
            public int ResetBet { get; set; }
            public byte ComeOutRoll {get; set; }
            public byte Point { get; set;}

            public GameState(int bet) 
            {
                Bet = bet;
                ResetBet = bet / 2;
                CurrentBets = new int[] { 0, 0, ResetBet, ResetBet, 0, 0 }; //{4,5,6,8,9,10}
                IsPressed = new bool[] { false, false, false, false, false, false };
                HasMovedToAcross = false;
                HasReset = false;
                HasCollected = false;
                Winnings = 0;
                ComeOutRoll = 0;
                Point = 0;
            }
        }

        private class RollIndex
        {
            public const int Four = 0;
            public const int Five = 1;
            public const int Six = 2;
            public const int Eight = 3;
            public const int Nine = 4;
            public const int Ten = 5;

            public static int GetIndex(byte roll) 
            {
                if (roll == 4) return Four;
                if (roll == 5) return Five;
                if (roll == 6) return Six;
                if (roll == 8) return Eight;
                if (roll == 9) return Nine;
                if (roll == 10) return Ten;
                return -1;
            }
           
        }

        private class MockRandom : Random
        {
            private Queue<int> _rolls;

            public MockRandom()
            {
                var rolls = new List<int> {8, 9, 4, 10, 8, 6, 4, 7}; // Simulate rolls: 1+2+2, 3+4+2, 5+0+2
                //var rolls = new List<int> { 3, 3, 4, 8, 4, 4, 3, 3, 2, 2, 1, 1, 1, 4}; // Simulate rolls: 1+2+2, 3+4+2, 5+0+2
                _rolls = new Queue<int>(rolls);
            }

            public override int Next(int maxValue)
            {
                try
                {
                    return _rolls.Dequeue();
                }
                catch (InvalidOperationException)
                {
                    return -1;
                }
            }
        }

        private byte Roll()
        {
            numRolls++;
            byte roll;
            if (!testing) 
            {
                roll = (byte)(this._r.Next(6) + this._r.Next(6) + 2);
            }
            else
            {
                roll = (byte)(_r.Next(6));
            }
            Console.Write("Roll "+ numRolls + ": " + roll + " --- ");
            return roll;

        }
    }
}
