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
        public int initialCoins;
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
        public float payout;              // betAmount * multiplier, validated server-side
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
        public string     sessionId;       // e.g. "20260422_153045_123"
        public string     playerId;
        public string     gameId;
        public string     startTime;       // ISO-8601 string
        public long       startTimeTicks;
        public string     state;           // "active"
        public GameConfig gameConfig;      // levels + multipliers from server
    }
      
  
        
 }

        



