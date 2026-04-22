using UnityEngine;

namespace BackendServices
{
[CreateAssetMenu(fileName = "GameRulesSO", menuName = "ScriptableObjects/GameRulesSO", order = 1)]  
public class GameRulesSO : ScriptableObject
{
    public int durationInSeconds = 900;
    public int initialBalls = 200;
    public int defaultPriceOfBall = 100;
    public int minPriceOfBall = 100;
    public int maxPriceOfBall = 1000;
   
   
}

}