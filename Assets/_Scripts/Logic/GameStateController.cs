using System;
using UnityEngine;
using ProgressiveP.Core;

namespace ProgressiveP.Logic
{
    
    public class GameStateController : MonoBehaviour
    {
    
    }
public enum GameState
{
    Idle,
    Playing,
    LevelTransition,
    Resetting
}
}