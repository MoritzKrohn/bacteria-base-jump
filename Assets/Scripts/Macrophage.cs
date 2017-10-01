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
        private Rigidbody mRigidBody;
        // Eaten bacteria
        private int mBacteriaEaten = 0;

	    private int _damage { get { return mParameter.MacrophageDamage; } }

        public float X { get { return transform.position.x; } }
        public float Z { get { return transform.position.z; } }
		// Biological parameters for macrophages		
        private Vector3 mDirection = Vector3.one;
        private GameObject target;
        public Vector2 velocity;
	    public Transform TopLight;

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
            set
            {
                if (value != movementState)
                {
                    lastMovementState = movementState; movementState = value;
                }
            }
        }

        // Internal state. Don't touch
        public int mBacteriaNear = 0;

	    private Vector3 floorSize;

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
            floorSize = gc.floor.GetComponent<Collider>().bounds.size;
            mRigidBody = GetComponent<Rigidbody>();
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
                    mDirection = new Vector3(Random.Range(-1F, 1F),0, Random.Range(-1F, 1F)); // Random direction
                    target = this.gameObject;
                    break;
                case MovementStates.ChemokineFound:

                    var cellList = GetObjectsAround<Cell>("Cell", 30F);


                    /*List<Cell> chemokineCells = cellList.OrderByDescending(c => c.Chemokine).ToList();
                    int n = chemokineCells.Count;
                    int topGroupCount = Mathf.RoundToInt(n / 4f);
                    int randomIndex = Mathf.RoundToInt(Random.Range(-0.49f, topGroupCount - 0.51f));
                    Cell cellWithMaxChemokine = chemokineCells[randomIndex];*/
                    Cell cellWithMaxChemokine = cellList.OrderByDescending(c => c.Chemokine).First();

                    target = cellWithMaxChemokine.gameObject;
                    mDirection = (target.transform.position - transform.position).normalized;
                    break;
                case MovementStates.BaceriaInRange:
                    var bactList = GameObject.FindGameObjectsWithTag("Bacteria").ToList();

                    var nearestBactObj = bactList
                         .OrderBy(b => Vector3.Distance(transform.position, b.transform.position))
                        .FirstOrDefault();
                   

                    if (nearestBactObj != null)
                    {
                        if (Vector3.Distance(nearestBactObj.transform.position, transform.position) > 40)
                        {
                            Debug.LogWarning("hack applied ;)");
                            MovementState = MovementStates.Idle;
                            break;
                        }

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

            if(target != null)
                Debug.DrawLine(transform.position, target.transform.position, Color.blue);

            if (MovementState == MovementStates.BaceriaInRange)
            {
                speed *= 4;
            }

            Vector3 myPosition = transform.position; // trick to convert a Vector3 to Vector2
            transform.position = Vector3.MoveTowards(transform.position, myPosition + mDirection, speed * Time.deltaTime);
            //mRigidBody.MovePosition(myPosition + mDirection * speed * Time.deltaTime);
            // If our spider senses are tingeling and we smell chemokine we switch to search mode.
            // But only if we don't have a bacteria inside. Need to exterminate them first
            if (MovementState != MovementStates.BaceriaInRange)
            {
                var cellList = GetObjectsAround<Cell>("Cell", 40F);
                if (cellList.Count > 0 && cellList.Max(c => c.Chemokine) > 0)
                {
                    MovementState = MovementStates.ChemokineFound;
                }
                else
                {
                    if(lastMovementState != MovementStates.BaceriaInRange)
                        MovementState = lastMovementState;
                }
            }
            if (TopLight)
                TopLight.transform.position = new Vector3(transform.position.x, transform.position.y + 6, transform.position.z);
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
            /*var viewpointCoord = Camera.main.WorldToViewportPoint(transform.position);
            viewpointCoord.x = Mathf.Clamp01(viewpointCoord.x);
            viewpointCoord.y = Mathf.Clamp01(viewpointCoord.y);
            transform.position = Camera.main.ViewportToWorldPoint(viewpointCoord);*/

            float x = Mathf.Clamp(transform.position.x, -floorSize.x / 2, floorSize.x/2);
            float z = Mathf.Clamp(transform.position.z, -floorSize.z / 2, floorSize.z/2);
            transform.position = new Vector3(x, 0, z);
        }


#region Collider
        /// <summary>
        /// Handles the entry collision with this object
        /// </summary>
        /// <param name="e">other object</param>
        private void OnTriggerEnter(Collider e)
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
        private void OnTriggerStay(Collider e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                var distToBact = Vector3.Distance(transform.position, e.transform.position);
                var macBounds = 1.4F;
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
            foreach (Cell cell in _bacteriaBeingEaten.GetComponent<Bacteria>().CloseToCells)
            {
                cell.RemoveBacteria();
            }

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

        private void OnTriggerExit(Collider e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                BacteriaNear--;
            }
        }
#endregion
    }

}
