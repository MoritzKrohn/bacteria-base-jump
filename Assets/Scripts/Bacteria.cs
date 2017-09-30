//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
	public class Bacteria : MonoBehaviour
	{
	    public HashSet<Cell> CloseToCells = new HashSet<Cell>();
        private MovementStates mMovementState = MovementStates.SessileState;
	    public event DeathEvent OnDead; 
	    public delegate void DeathEvent();
	    public static event LandedEvent OnLanded;
        public delegate void LandedEvent();

	    public static List<Bacteria> AllBacteria = new List<Bacteria>();

        private ModelParameter mParameter;

	    private Vector3 floorSize;

        // Individual bacteria are between 0.5 and 1.25 micrometers in diameter. From: https://microbewiki.kenyon.edu/index.php/Streptococcus_pneumoniae
        // So we take 1 roughly as guideline

        private float mCurrentAngle = 0;

        public float X { get { return transform.position.x; } }
        public float Y { get { return transform.position.y; } }

	    private float _healthMultiplier = 1f;
	    private int _damageReceived = 0;

	    private int _healthPoints
	    {
	        get { return Mathf.RoundToInt(mParameter.BacteriaDefaultHealth * _healthMultiplier) - _damageReceived; }
	    }
        public int HealthPoints { get { return _healthPoints;  } }

        public float StepSize
		{
			get
			{
                float step = 0;
				if (mMovementState == MovementStates.FlowingState)
				{
					step = mParameter.MovementInFlowingPhase;
				}
				else
				{
					step = mParameter.MovementInSessilePhase;
				}
				return step;
			}
		}
		public MovementStates State { get { return this.mMovementState; } }

		public enum MovementStates
		{
			SessileState,
			FlowingState
		}

        public void Start()
        {
            GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
            floorSize = gc.floor.GetComponent<Collider>().bounds.size;
            mParameter = gc.Parameter;
            mMovementState = MovementStates.SessileState;
            Bacteria.AllBacteria.Add(this);
            Bacteria.OnLanded += RecalculateHealthMultiplier;
            if (Bacteria.OnLanded != null)
                Bacteria.OnLanded.Invoke();

            StartCoroutine(NewHeadingCoroutine());
        }

	    private void RecalculateHealthMultiplier()
	    {
	        int numberOfBacteriaInProximity = 0;
	        int n = Bacteria.AllBacteria.Count;
	        for (var i = 0; i < n; i++)
	        {
	            Bacteria otherBacteria = Bacteria.AllBacteria[i];
	            if (otherBacteria != this && Vector3.Distance(transform.position, otherBacteria.transform.position) < 1.5f)
	                numberOfBacteriaInProximity++;
	        }
	        _healthMultiplier = 1f + numberOfBacteriaInProximity * 0.2f;
	    }

	    public int ReduceHealth(int damage)
	    {
	        _damageReceived += damage;
	        if (_healthPoints <= 0)
	            Die();
	        return _healthPoints;
	    }

	    private void Die()
	    {
	        Bacteria.AllBacteria.Remove(this);
	        if (OnDead != null)
	            OnDead();
	        Destroy(gameObject);
	    }

        private void SetNewHeading()
        {
            mCurrentAngle = Random.Range(0F, 1F) * 2 * Mathf.PI; // Random direction
        }

        private IEnumerator NewHeadingCoroutine()
        {
            while (true)
            {
                SetNewHeading();
                yield return new WaitForSeconds(0.1F);
            }
        }

        private void InterchangePhase()
		{
            var randNum = Random.Range(0F, 1F);
			// Change state if probability is met. This will change the step size as well
			if (randNum > mParameter.ProbabilityInterchanged)
			{
				if (mMovementState == MovementStates.FlowingState)
				{
					mMovementState = MovementStates.SessileState;
				}
				else
				{
					mMovementState = MovementStates.FlowingState;
				}
			}
		}
		/// <summary>
		/// Moves the bacteria. It will jump to mobile phase with a certain propability
		/// </summary>
		public void Update()
		{
			/*InterchangePhase();
			InterchangePhase();			
            
			var x = (float)(Mathf.Cos(mCurrentAngle) * StepSize);
			var z = (float)(Mathf.Sin(mCurrentAngle) * StepSize); // Step into the direction defined

            PlayerMovementClamping();

            // Apply and smooth out movement
            Vector3 movement = new Vector3(x, 0, z);
            movement *= Time.deltaTime;
			transform.Translate(movement);*/
        }

        void PlayerMovementClamping()
        {
            float x = Mathf.Clamp(transform.position.x,-floorSize.x/2,floorSize.x/2);
            float z = Mathf.Clamp(transform.position.z,-floorSize.z/2,floorSize.z/2);
            transform.position = new Vector3(x,0,z);
        }

    }
}

	    
