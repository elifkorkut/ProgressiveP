using UnityEngine;

namespace ProgressiveP.Logic
{   
    public class PlinkoBall : MonoBehaviour
    {
        
        [SerializeField] private float thrust = 1.5f; 
        

        [SerializeField] private float snapThreshold = 0.05f;

        private new Rigidbody2D rigidbody2D;
        private string lastHit = "";
        private float betAmount = 0f;
        private float targetMultiplier = 0f;

        private int _ballIndex = -1;
        private int _targetBucketIndex = -1;
        private Transform _targetBasket;

        public int BallIndex => _ballIndex;
        public int TargetBucketIndex => _targetBucketIndex;

        void Awake()
        {
            rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void Setup(float increment, float betAmount = 5f)
        {
            transform.localScale = new Vector2(
                Helpers.GetScreenWidth() * increment,
                Helpers.GetScreenWidth() * increment);
            this.betAmount = betAmount;
        }

        public float GetBetValue() => betAmount;
        public float GetTargetMultiplier() => targetMultiplier;
        
        public void AssignIndex(int index) => _ballIndex = index;

        public void AssignTarget(int bucketIndex, Transform basketTransform)
        {
            _targetBucketIndex = bucketIndex;
            _targetBasket = basketTransform;

            if (basketTransform != null)
                Debug.Log($"[PlinkoBall #{_ballIndex}] Target assigned → bucket {bucketIndex} ({basketTransform.name})");
            else
                Debug.LogWarning($"[PlinkoBall #{_ballIndex}] Target bucket {bucketIndex} assigned but basket Transform is null.");
        }

        public void ResetForPool()
        {
            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
                rigidbody2D.angularVelocity = 0f;
            }
            lastHit = string.Empty;
            betAmount = 0f;
            _ballIndex = -1;
            _targetBucketIndex = -1;
            _targetBasket = null;
        }

        private void OnCollisionEnter2D(Collision2D collision2D)
        {
            // ── Basket hit 
            var basket = collision2D.gameObject.GetComponent<CollectionBasket>();
            if (basket != null)
            {
                int hitBucket = basket.BucketIndex;
                if (_targetBucketIndex >= 0)
                {
                    if (hitBucket == _targetBucketIndex)
                        Debug.Log($"[PlinkoBall #{_ballIndex}] HIT correct bucket {hitBucket} ✓");
                    else
                        Debug.LogWarning($"[PlinkoBall #{_ballIndex}] MISSED target — expected {_targetBucketIndex}, landed in {hitBucket}");
                }
                return;
            }

            // ── Peg hit 
            if (!collision2D.gameObject.CompareTag("StaticBall") || lastHit == collision2D.gameObject.name) return;

            lastHit = collision2D.gameObject.name;
            collision2D.gameObject.GetComponent<StaticBall>()?.StartBop();

            bool goRight;
            if (_targetBasket != null)
            {
                //  steer toward target 
                float dx = _targetBasket.position.x - transform.position.x;
                if (Mathf.Abs(dx) > snapThreshold)
                    goRight = dx > 0f;
                else
                    goRight = UnityEngine.Random.value > 0.5f; // either side is fine
            }
            else
            {
                goRight = UnityEngine.Random.value > 0.5f;
            }

        
            Vector2 currentVel = rigidbody2D.linearVelocity;
            
            currentVel.x = goRight ? thrust : -thrust;
           //Dampen
            currentVel.y *= 0.85f; 
            
            rigidbody2D.linearVelocity = currentVel;
        }
    }
}