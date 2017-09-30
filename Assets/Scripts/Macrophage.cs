using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
	public class Macrophage : MonoBehaviour
	{
        // Model parameter
        ModelParameter mParameter;
        private Rigidbody2D mRigidBody;
        // Eaten bacteria
        private int mBacteriaEaten = 0;

	    private int _damage { get { return mParameter.MacrophageDamage; } }

        public float X { get { return transform.position.x; } }
        public float Y { get { return transform.position.y; } }
		// Biological parameters for macrophages		
        private Vector2 mDirection = Vector2.one;
        private GameObject target;
        public Vector2 velocity;

	    private Bacteria _bacteriaBeingEaten = null;

        /// <summary>
        /// Movement states the macrophage can be in
        /// </summary>
        enum MovementStates
        {
            /// <summary>If not sensing chemokine or bacterias are close, we can idle around doing macrophagy things</summary>
            Idle,
            ///<summary>Somethings wrong in the neighbourhood, we should check it out</summary> 
            ChemokineFound,
            ///<summary>EXTERMINATE!!! EXTERMINAAAAATTTE!!!</summary> 
            BaceriaInRange
        }

        /// <summary>
        /// Current movement state
        /// </summary>
        private MovementStates movementState = MovementStates.Idle;
        /// <summary>
        /// Last movement state
        /// </summary>
        private MovementStates lastMovementState = MovementStates.Idle;

        /// <summary>
        /// The last state will always be remembered in lastMovementState
        /// </summary>
        /// <seealso cref="lastMovementState"/>
        private MovementStates MovementState
        {
            get { return movementState; }
            set { if (value != movementState) { lastMovementState = movementState; movementState = value; } }
        }

        // Internal state. Don't touch
        private int mBacteriaNear = 0;

        /// <summary>
        /// Number of bacteria in range
        /// </summary>
        private int BacteriaNear
        {
            set
            {
                mBacteriaNear = value;
                if (value > 0)
                {
                    MovementState = MovementStates.BaceriaInRange;
                }
                else
                {
                    MovementState = lastMovementState;
                }
            }
            get
            {
                return mBacteriaNear;
            }
        } // Counter for close bacterias. Also sets the state to exterminate!!!

        public void Start()
        {
            GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
            mParameter = gc.Parameter;
            mRigidBody = GetComponent<Rigidbody2D>();
            if (!mRigidBody)
            {
                Debug.LogError("No rigidBody attached!");
            }
            StartCoroutine(NewHeadingCoroutine());
        }

        /// <summary>
        /// Set new heading depending on state
        /// </summary>
        private void SetNewHeading()
        {
            switch (movementState)
            {
                case MovementStates.Idle:
                    mDirection = new Vector2(Random.Range(-1F, 1F), Random.Range(-1F, 1F)); // Random direction
                    target = this.gameObject;
                    break;
                case MovementStates.ChemokineFound:
                    var cellList = GetObjectsAround<Cell>("Cell", 1.5F);

                    List<Cell> chemokineCells = cellList.OrderByDescending(c => c.Chemokine).ToList();
                    int n = chemokineCells.Count;
                    int topGroupCount = Mathf.RoundToInt(n / 4f);
                    int randomIndex = Mathf.RoundToInt(Random.Range(-0.49f, n - 0.51f));
                    Cell cellWithMaxChemokine = chemokineCells[randomIndex];

                    target = cellWithMaxChemokine.gameObject;
                    mDirection = (target.transform.position - transform.position).normalized;
                    break;
                case MovementStates.BaceriaInRange:
                    var bactList = GameObject.FindGameObjectsWithTag("Bacteria").ToList();
                    var nearestBactObj = bactList
                        .OrderBy(b => Vector2.Distance(transform.position, b.transform.position))
                        .FirstOrDefault();
                    if (nearestBactObj != null)
                    {
                        Bacteria nearestBact = nearestBactObj.GetComponent<Bacteria>();
                        target = nearestBact.gameObject;
                        mDirection = (target.transform.position - transform.position).normalized;
                    }
                    break;
                default:
                    Debug.LogError("Macrophage state not implemented!");
                    break;
            }
        }

        /// <summary>
        /// New heading coroutine. Will generate a new heading every 3 seconds if in idle state and every 100ms in agitated state
        /// </summary>
        /// <returns></returns>
        private IEnumerator NewHeadingCoroutine()
        {
            while (true)
            {                
                SetNewHeading();                
                yield return new WaitForSeconds(movementState == MovementStates.Idle ? 3F : 0.1F);
            }
        }

        public void FixedUpdate()
        {
            PlayerMovementClamping();
            var speed = mParameter.MacrophageMovement * 1;
            if (target != null)
                Debug.DrawLine(transform.position, target.transform.position, Color.black);
            Vector2 myPosition = transform.position; // trick to convert a Vector3 to Vector2
            mRigidBody.MovePosition(myPosition + mDirection * speed * Time.deltaTime);
            // If our spider senses are tingeling and we smell chemokine we switch to search mode.
            // But only if we don't have a bacteria inside. Need to exterminate them first
            if (MovementState != MovementStates.BaceriaInRange)
            {
                var cellList = GetObjectsAround<Cell>("Cell", 1.5F);
                if (cellList.Count > 0 && cellList.Max(c => c.Chemokine) > 0)
                {
                    MovementState = MovementStates.ChemokineFound;
                }
                else
                {
                    MovementState = lastMovementState;
                }
            }
        }

        /// <summary>
        /// Get all objects with a tag of type T within the radius
        /// </summary>
        /// <typeparam name="T">Object type to be returned</typeparam>
        /// <param name="tag">Tag of the object</param>
        /// <param name="radius">Radius to search for</param>
        /// <returns>List of T</returns>
        List<T> GetObjectsAround<T>(string tag, float radius)
        {
            return GameObject.FindGameObjectsWithTag(tag)
                .Where(go => Vector3.Distance(go.transform.position, transform.position) <= radius)
                .Select(go => go.GetComponent<T>())
                .ToList();
        }

        void PlayerMovementClamping()
        {
            var viewpointCoord = Camera.main.WorldToViewportPoint(transform.position);
            viewpointCoord.x = Mathf.Clamp01(viewpointCoord.x);
            viewpointCoord.y = Mathf.Clamp01(viewpointCoord.y);
            transform.position = Camera.main.ViewportToWorldPoint(viewpointCoord);
        }


#region Collider
        /// <summary>
        /// Handles the entry collision with this object
        /// </summary>
        /// <param name="e">other object</param>
        private void OnTriggerEnter2D(Collider2D e)
        {
            if(e.gameObject.name.Contains("Bacteria"))
            {
                BacteriaNear++;
            }
        }

        /// <summary>
        /// Checks permanently for any objects inside the hit area
        /// </summary>
        /// <param name="e">other object</param>
        private void OnTriggerStay2D(Collider2D e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                var distToBact = Vector2.Distance(transform.position, e.transform.position);
                var macBounds = 0.7F;
                if (distToBact < macBounds)
                {
                    if (_bacteriaBeingEaten != null) return;

                    BeginEatingBacterium(e.GetComponent<Bacteria>());
                }
            }
        }

	    private void BeginEatingBacterium(Bacteria bacterium)
	    {
	        _bacteriaBeingEaten = bacterium;
	        _bacteriaBeingEaten.OnDead += HandleBacteriumEaten;
	        StartCoroutine(EatBacterium());
	    }

	    private void HandleBacteriumEaten()
	    {
	        _bacteriaBeingEaten.OnDead -= HandleBacteriumEaten;
	        _bacteriaBeingEaten = null;
	        BacteriaNear--;
	        mBacteriaEaten++;
        }

	    private IEnumerator EatBacterium()
	    {
	        while (_bacteriaBeingEaten != null && _bacteriaBeingEaten.HealthPoints > 0)
	        {
	            _bacteriaBeingEaten.ReduceHealth(_damage);
	            yield return new WaitForSeconds(0.25f);
	        }
	    }

        private void OnTriggerExit2D(Collider2D e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                BacteriaNear--;
            }
        }
#endregion
    }

}
