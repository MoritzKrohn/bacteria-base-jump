//using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
	public class Bacteria : MonoBehaviour
	{
		private MovementStates mMovementState = MovementStates.SessileState;
        private ModelParameter mParameter;

        // Individual bacteria are between 0.5 and 1.25 micrometers in diameter. From: https://microbewiki.kenyon.edu/index.php/Streptococcus_pneumoniae
        // So we take 1 roughly as guideline

        private float mCurrentAngle = 0;

        public float X { get { return transform.position.x; } }
        public float Y { get { return transform.position.y; } }

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
            mParameter = gc.Parameter;

            StartCoroutine(NewHeadingCoroutine());
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
			InterchangePhase();
			InterchangePhase();			
            
			var x = (float)(Mathf.Cos(mCurrentAngle) * StepSize);
			var y = (float)(Mathf.Sin(mCurrentAngle) * StepSize); // Step into the direction defined

            PlayerMovementClamping();

            // Apply and smooth out movement
            Vector3 movement = new Vector3(x, y, 0);
            movement *= Time.deltaTime;
			transform.Translate(movement);
        }

        void PlayerMovementClamping()
        {
            var viewpointCoord = Camera.main.WorldToViewportPoint(transform.position);
            viewpointCoord.x = Mathf.Clamp01(viewpointCoord.x);
            viewpointCoord.y = Mathf.Clamp01(viewpointCoord.y);
            transform.position = Camera.main.ViewportToWorldPoint(viewpointCoord);
        }

    }
}
