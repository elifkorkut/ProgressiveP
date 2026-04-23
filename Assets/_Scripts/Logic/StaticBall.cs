using UnityEngine;
using ProgressiveP.Logic.Effects;

namespace ProgressiveP.Logic
{

public class StaticBall : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float redTimerLength = 0.5f;
    
    private float timer;
    private bool isColored;
    private bool timerOn;

    private void Start() { }

   public void StartBop(){
        
        animator.ResetTrigger("bop");
        animator.SetTrigger("bop");

        if(!isColored){

            SoundManager.Instance?.PlayBallHit();

            spriteRenderer.color = Color.red;
            timer = 0f;
            timerOn = true;
            isColored = true;     
        }
    
    }

    void Update(){
        if(timerOn){
            timer+=Time.deltaTime;
            if(timer > redTimerLength){
                spriteRenderer.color = Color.white;
                timerOn = false;
                isColored = false; 
            }
        }
    }

}
}