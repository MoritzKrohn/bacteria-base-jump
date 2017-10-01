using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class Parachute : MonoBehaviour
    {
        public GameObject BacteriaPrefab;
        public GameObject PlayerModel;

        private Rigidbody _rigidBody;
        private bool _falling = true;
        private bool _returning = false;

        private Vector3 _origin;

        private float _fallingSpeed = 50f;
        private GameController gameController;
        private bool _watching;

        void Awake()
        {
            gameController = FindObjectOfType<GameController>();
            _rigidBody = GetComponent<Rigidbody>();
            _origin = transform.position;
            Time.timeScale = 1.5f;
        }

        void FixedUpdate()
        {
            if (_falling)
            {
                _fallingSpeed++;
                var mousePosition = Input.mousePosition;
                var direction = new Vector3(mousePosition.x - Screen.width / 2, -_fallingSpeed, mousePosition.y - Screen.height / 2);
                direction /= 30;
                _rigidBody.AddForce(direction);

                //transform.Rotate(Vector3.up, 5f);
            }
            else if (_returning)
            {
                float step = 4f;
                transform.position = Vector3.MoveTowards(transform.position, _origin, step);
                if (transform.position == _origin && !_watching)
                {
                    _returning = false;
                    _falling = true;
                    PlayerModel.SetActive(true);
                    _fallingSpeed = 50;
                }
            }
        }

        void OnCollisionEnter(Collision c)
        {
            if (c.transform.tag == "Floor")
            {
                
                gameController.BacteriaRetries--; // reduce retries
                
                if (gameController.BacteriaRetries <= 0)
                {
                    _watching = true;
                    PlayerModel.GetComponent<MeshRenderer>().enabled = false;
                }
                InstantiateBacterium();
                _returning = true;
                _falling = false;
                
                PlayerModel.SetActive(false);
            }
        }

        private void InstantiateBacterium()
        {
            GameObject bact = Instantiate(BacteriaPrefab, PlayerModel.transform.position,
                PlayerModel.transform.rotation, GameObject.FindGameObjectWithTag("Bacterias").transform);
            Bacteria component = bact.GetComponent<Bacteria>();
            component.CalculateCluster();
            Debug.LogWarning("Added to a cluster with " + component.Cluster.Count + " elements.");
        }
    }
}