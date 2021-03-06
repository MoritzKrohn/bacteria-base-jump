﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    /// <summary>
    /// This is the main controller in the game. It creates all actors and holds the basic logic.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        public static void CreateBacteriaAtPoint(float x, float y)
        {
            instance.CreateBacterium(x, y, false);
        }

        public static AudioSource SoundSource { get { return instance.AudioSource;  } }

        public AudioClip GameWonSound;
        public AudioClip GameOverSound;

        public Vector3 ScreenSize { get { return new Vector3(Screen.width, Screen.height, 0); } }

		public float Width { get { return floor.GetComponent<Collider>().bounds.size.x; } }
		public float Height { get { return floor.GetComponent<Collider>().bounds.size.z; } }

        public int MacrophageCount { get { return FindObjectsOfType(typeof(Macrophage)).Length; } }
        public int BacteriaCount { get { return FindObjectsOfType(typeof(Bacteria)).Length; } }
        public Bacteria[] Bacterias { get { return FindObjectsOfType(typeof(Bacteria)) as Bacteria[]; } }

        public GameObject floor;
        public GameObject bacteria;
        public GameObject macrophage;

        public AudioSource AudioSource;

        public ModelParameter Parameter = new ModelParameter() { BacteriaDoublingTime = 20, NumberOfBacteria = 100 };
        public int BacteriaRetries = 20;
        public int NumberOfBacteria;
        public int NumberOfMacrophages;

        private const float mCoughProbability = 0.995F;

        private Text uiBacteriaCounter;
        private Text uiBacteriaDoublingTime;
        private float mStartTime;
        private static GameController instance;
        private bool mMainSceneLoaded = false;

        private bool won = false;
        private bool lost = false;
        private Text _uiFinalText;

        void Awake()
        {
            //Check if instance already exists
            if (instance == null)
            {
                //if not, set instance to this
                instance = this;
            }
            //If instance already exists and it's not this:
            else if (instance != this)
            {
                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);
            }
            
            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += this.OnLoadCallback;
        }

        void Start()
        {
            
        }

        private void OnLoadCallback(Scene scene, LoadSceneMode sceneMode)
        {
            Debug.Log(scene.name);
            if(scene.name == "MainScene")
            {
                InitGame();
                mMainSceneLoaded = true;
            }
            else
            {                
                mMainSceneLoaded = false;
            }
        }

        void InitGame()
        {
            // Overwrite some model parameters from Unity UI
            Parameter.NumberOfBacteria = NumberOfBacteria;
            Parameter.NumberOfMacrophages = NumberOfMacrophages;

            // Find our game UI
            uiBacteriaCounter = GameObject.Find("CountText").GetComponent<Text>();
            uiBacteriaDoublingTime = GameObject.Find("DoublingText").GetComponent<Text>();
            _uiFinalText = GameObject.Find("FinalText").GetComponent<Text>();

            // Initialize our actors
            Vector3 spawnPosition;
            Quaternion spawnRotation = Quaternion.identity;

            for (int i = 0; i < Parameter.NumberOfMacrophages; i++)
            {
                spawnPosition = new Vector3(Random.Range(-Width/4, Width/4), 10, Random.Range(-Height/4, Height/4));
                Instantiate(macrophage, spawnPosition, spawnRotation);
            }

            // disable spawn of bacterias
            /*for (int nb = 0; nb < Parameter.NumberOfBacteria; nb++)
            {
                var x = GaussianRandom(0, Width / 4);
                var z = GaussianRandom(0, Height / 4);
                spawnPosition = new Vector3(x, 0, z);
                Debug.Log("x is "+x+" and z is "+z);
                GameObject bact = Instantiate(bacteria, spawnPosition, spawnRotation);
                bact.transform.parent = GameObject.FindGameObjectWithTag("Bacterias").transform;
            }*/
            

            // This triggers the doubling timer for the bacteria
            mStartTime = Time.realtimeSinceStartup;
        }

        public Bacteria CreateBacterium(float posX, float posZ,bool randomRot)
        {
            GameObject bact = Instantiate(bacteria, new Vector3(posX, 0, posZ), randomRot? Quaternion.Euler(0, Random.Range(0, 360), 0):Quaternion.identity, GameObject.FindGameObjectWithTag("Bacterias").transform);
            Bacteria component = bact.GetComponent<Bacteria>();
            component.CalculateCluster();
            return component;
        }

        void Update()
		{
            if (Input.GetKeyDown(KeyCode.C))
            {
                Cough();
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                KillAll();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopAllCoroutines();
                SceneManager.LoadScene("MainMenu");
            }
		    
            if (mMainSceneLoaded)
            {
                // Show UI information
                uiBacteriaDoublingTime.text = BacteriaRetries.ToString();
                //Debug.LogWarning("retries drawn");
                //Mathf.RoundToInt(Parameter.BacteriaDoublingTime - (Time.realtimeSinceStartup - mStartTime) % Parameter.BacteriaDoublingTime).ToString();
                uiBacteriaCounter.text = BacteriaCount.ToString();

                if (BacteriaCount == 0 && BacteriaRetries < 3 && !lost && !won)
                {
                    lost = true;
                    _uiFinalText.text = "You lost! Press escape to return to the menu.";
                    AudioSource.PlayOneShot(GameOverSound);
                }

                if (BacteriaCount > 100 && !lost && !won)
                {
                    won = true;
                    _uiFinalText.text = "You won! Press escape to return to the menu.";
                    AudioSource.PlayOneShot(GameWonSound);
                    BacteriaRetries = 0;
                }
                    
            }
        }

        /// <summary>
        /// Remove some bacteria from the alveolus through coughing
        /// </summary>
        void Cough()
        {
            // Get all bacteria in flowing state
            var bactList = GameObject.FindObjectsOfType<Bacteria>().Where(b => b.State == Bacteria.MovementStates.FlowingState).ToList();
            Debug.Log(bactList.Count + " bacteria found");
            // Remove some of them according to propability
            for(int b = 0; b < bactList.Count; b++)
            {
                if(Random.Range(0F, 1F) > mCoughProbability)
                {
                    Destroy(bactList[b].gameObject);
                }
            }
        }

        void KillAll()
        {
            var bactList = GameObject.FindObjectsOfType<Bacteria>().ToList();
            bactList.ForEach(b => Destroy(b.gameObject));
        }

        
        /// <summary>
        /// Helper function to generate gaussian distributed random values
        /// </summary>
        /// <param name="mean">Mean value</param>
        /// <param name="stddev">Standard deviation</param>
        /// <returns></returns>
        private static float GaussianRandom(float mean = 0, float stddev = 1)
        {            
            float x1 = 1 - Random.Range(0F, 1F);
            float x2 = 1 - Random.Range(0F, 1F);

            float y1 = Mathf.Sqrt(-2.0F * Mathf.Log(x1)) * Mathf.Cos(2.0F * Mathf.PI * x2);
            return y1 * stddev + mean;
        }

    }

}
