using UnityEngine;
namespace ProgressiveP.Logic
{   
public class PlinkoBall : MonoBehaviour
{


    [SerializeField] private ForceMode2D forcemode = ForceMode2D.Impulse;

    [SerializeField] private float thrust = 1f;

    private new Rigidbody2D rigidbody2D;

    private string lastHit = "";
    private float betAmount = 0f;

    
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Setup(float increment, float betAmount = 5f){
        transform.localScale = new Vector2(Helpers.GetScreenWidth() * increment, Helpers.GetScreenWidth() * increment);
        this.betAmount = betAmount;
    }

    public float GetBetValue(){
        return betAmount;
    }


    public void ResetForPool()
    {
        if (rigidbody2D != null)
        {
            rigidbody2D.linearVelocity        = Vector2.zero;
            rigidbody2D.angularVelocity = 0f;
        }
        lastHit   = string.Empty;
        betAmount = 0f;
    }


    private void OnCollisionEnter2D(Collision2D collision2D){


        if(collision2D.gameObject.CompareTag("StaticBall") && lastHit != collision2D.gameObject.name)
        {
            lastHit = collision2D.gameObject.name;
            int randomValue = UnityEngine.Random.Range(0,100);

            collision2D.gameObject.GetComponent<StaticBall>().StartBop();

            if(randomValue>50){
                rigidbody2D.AddForce(new Vector2(thrust,0), forcemode);
                Debug.Log("I want to go right " + collision2D.gameObject.name);
            }else{
                rigidbody2D.AddForce(new Vector2(-thrust,0), forcemode);
                Debug.Log("I want to go Left " + collision2D.gameObject.name);

            }
        }
    }
}
}