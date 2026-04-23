using System;
using System.Collections.Generic;

namespace ProgressiveP.Core
{ 

 [Serializable]
public struct GameData
{
    public string gameId;
    public int played;
    public string lastPlayedtick; 
    public string startedTick; 
    public string sessionId; 
    public string state;
}

[Serializable]
public struct PlayerData
{
    public string userId;
    public string name;
    public int balance;
    public List<GameData> games;
}



    [Serializable]
    public struct LevelConfig
    {
        public int rows;
        public int numberOfBallsToPass;  
        public float[] multipliers;

    }

    [Serializable]
    public struct GameConfig
    {
        public int initialBalls;
        public int defaultBet;
        public int durationInSeconds;
        public int numberOfLevels;
        public LevelConfig[] levels;

    }


 [Serializable]
    public struct GlobalGameConfig
    {   public string gameId;
        public string version;
        public int initialCoins;   // remove this later 
        public int initialBalls;    
        public int defaultBet;
        public int minBet;
        public int maxBet;
        public int durationInSeconds;
        public int defaultRows;
        public int defaultRisk;
    }
    

    [Serializable]
     public struct BallConfig
     {
        public int[] DestinationRow;
     }
        
    [Serializable]
    public struct PlayerGameSummary
    {
        public string playerId;
        public int    totalSessionsPlayed;
        public double totalBet;
        public double totalWon;
        public double allTimeHighBalance;
        public long   lastPlayedTicks;
        public long   firstPlayedTicks;

    }


      [Serializable]
      public struct RewardRecord
      {
        public int   bucketIndex;
        public float betAmount;
        public float multiplier;          
        public int   payout;              // betAmount * multiplier 
        public long  serverTimestampTicks;

      }

    [Serializable]
    public struct SessionStateData
    {
        public int  currentLevelIndex;
        public int remainingBalls;
        public int  ballsDroppedThisLevel;
        public long resetTargetTicks;  
        public double SecondsUntilReset =>
            TimeSpan.FromTicks(resetTargetTicks - DateTime.UtcNow.Ticks).TotalSeconds;

        public bool IsResetDue => SecondsUntilReset <= 0;
    }

   
    [Serializable]
    public struct NewSessionData
    {
        public string     sessionId;           
        public string     playerId;
        public string     gameId;
        public string     startTime;     
        public long       startTimeTicks;
        public string     state;               // "active" | "expired" add idle 
        public GameConfig gameConfig;          // levels + multipliers from server
        public int        currentLevelIndex;
        public int        totalBallsSpent;
        public int        ballsDroppedThisLevel;
        public RewardRecord[] rewardHistory;
    }

    // Pre-calculated ball targets

    
    [Serializable]
    public struct BallTarget
    {
        public int ballIndex;   
        public int bucketIndex; 
    }

    
    [Serializable]
    public struct LevelBallData
    {
        public string      sessionId;
        public int         levelIndex;
        public int         ballCount;
        public float       betPerBall;
        public BallTarget[] targets;
    }

   
    [Serializable]
    public struct BallBatchResult
    {
        public int           totalPayout;
        public double        newBalance;
        public RewardRecord[] rewards;
    }
 }

        



