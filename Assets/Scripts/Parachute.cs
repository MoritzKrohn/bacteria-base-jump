﻿using UnityEngine;

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

        void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _origin = transform.position;
            Time.timeScale = 1.5f;
        }

        void FixedUpdate()
        {
            if (_falling)
            {
                Debug.LogWarning("updated");
                var mousePosition = Input.mousePosition;
                var direction = new Vector3(mousePosition.x - Screen.width / 2, -50, mousePosition.y - Screen.height / 2);
                direction /= 30;
                _rigidBody.AddForce(direction);

                //transform.Rotate(Vector3.up, 5f);
            }
            else if (_returning)
            {
                float step = 4f;
                transform.position = Vector3.MoveTowards(transform.position, _origin, step);
                if (transform.position == _origin)
                {
                    _returning = false;
                    _falling = true;
                    PlayerModel.SetActive(true);
                }
                    
            }
        }

        void OnCollisionEnter(Collision c)
        {
            Debug.LogWarning("test");
            if (c.transform.tag == "Floor")
            {
                _falling = false;
                InstantiateBacterium();
                _returning = true;
                PlayerModel.SetActive(false);
            }
            
        }

        private void InstantiateBacterium()
        {
            GameObject bact = Instantiate(BacteriaPrefab, PlayerModel.transform.position, PlayerModel.transform.rotation, GameObject.FindGameObjectWithTag("Bacterias").transform);
        }

    }
}