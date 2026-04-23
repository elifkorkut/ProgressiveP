using UnityEngine;

public static class Helpers
{
    
    public static float GetScreenHeight(){
        
        return Camera.main.orthographicSize * 1.8f;
    }

    public static float GetScreenWidth(){
        return GetScreenHeight() * Screen.width / Screen.height;
    }
}
