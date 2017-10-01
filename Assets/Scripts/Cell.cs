using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class Cell : MonoBehaviour
    {
        private static float[] values;
        private static int valueAmount;
        private static int ptr = 0;
        private ModelParameter mParameter;
        private GameController mGameController;
        private ParticleSystem mChemokineEmitter;

        public float ChemokineLevels = 0F; // This is only a display in the unity UI
        public bool DebugInformation = false;
        private float mChemokine = 0F;

        public int BacteriaOnCell;
        public List<Macrophage> MacrophageOnCell = new List<Macrophage>();
        // Cell width is mParameter.EpithelialCellWidth (30)

        public void Start()
        {
            mGameController = GameObject.Find("GameController").GetComponent<GameController>();
            mChemokineEmitter = GetComponent<ParticleSystem>();

            mParameter = mGameController.Parameter;
            
            valueAmount = GameObject.FindObjectsOfType<Cell>().Length;
            if (valueAmount > 5)
            {
                values = new float[valueAmount];
            }
            
        }

        private void OnTriggerEnter(Collider e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                BacteriaOnCell++;
                e.gameObject.GetComponent<Bacteria>().CloseToCells.Add(this);
            }
            else if (e.gameObject.name.Contains("Macrophage"))
            {
                var mac = e.gameObject.GetComponent<Macrophage>();
                if (!MacrophageOnCell.Contains(mac))
                {
                    MacrophageOnCell.Add(mac);
                }
            }
        }
        private void OnTriggerExit(Collider e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                BacteriaOnCell--;
                e.gameObject.GetComponent<Bacteria>().CloseToCells.Remove(this);
            }
            else if (e.gameObject.name.Contains("Macrophage"))
            {
                var mac = e.gameObject.GetComponent<Macrophage>();
                if (MacrophageOnCell.Contains(mac))
                {
                    MacrophageOnCell.Remove(mac);
                }
            }
        }

        public void RemoveBacteria()
        {
            if (BacteriaOnCell - 1 >= 0)
                BacteriaOnCell--;
        }

        public void Update()
        {
        }

        private float max, min;

        public float Chemokine
        {
            set
            {
                mChemokine = value;

                if (values != null)
                {
                    values[ptr++] = value;

                   

                    if (ptr >= values.Length)
                    {
                        
                        ptr = 0;
                        
                    }
                    max = values[0];
                    min = values[0];
                    for (int i = 1; i < values.Length; i++)
                    {
                        var val = values[i];
                        if (val > max)
                        {
                            max = val;
                        }
                        if (val < min)
                        {
                            min = val;
                        }

                    }
                    
                    var input = Mathf.InverseLerp(min, max, value);
                    //Debug.LogWarning("min:" + min + ", max:  " + max + " value:" + value+ ", input:" + input);
                    
                    ChemokineLevels = input * 100;

                    if (mChemokineEmitter)
                    {
                        var m = mChemokineEmitter.main;
                        var e = mChemokineEmitter.emission;
                        var rateOverTime = (int)(input * 50);
                        e.rateOverTime = rateOverTime < 3 ? 0 : rateOverTime;
                        m.maxParticles = (int)(input * 20);
                    }
                }

                /*//var input = Mathf.InverseLerp(min, max, value);
                var input = Mathf.InverseLerp(1e-6F, 1e-4F, value);

                ChemokineLevels = input * 100;

                if (mChemokineEmitter)
                {
                    var m = mChemokineEmitter.main;
                    var e = mChemokineEmitter.emission;
                    var rateOverTime = (int)(input * 50);
                    e.rateOverTime = rateOverTime < 3 ? 0 : rateOverTime;
                    m.maxParticles = (int)(input * 20);
                }*/
            }
            get
            {
                if (mParameter != null)
                    return Mathf.Round(mChemokine / mParameter.SensitivityToFeelCytokineGradient) * mParameter.SensitivityToFeelCytokineGradient;
                return 0;
            }
        }
    }
}
