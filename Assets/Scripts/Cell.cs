using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class Cell : MonoBehaviour
    {
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
        }

        private void OnTriggerEnter2D(Collider2D e)
        { 
            if (e.gameObject.name.Contains("Bacteria"))
            {
                BacteriaOnCell++;
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
        private void OnTriggerExit2D(Collider2D e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                BacteriaOnCell--;
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

        public void Update()
        {
        }

        public float Chemokine
        {
            set
            {
                mChemokine = value;
                //var input = Mathf.Clamp01(value * 1e4F);
                var input = Mathf.InverseLerp(1e-6F, 1e-3F, value);
                ChemokineLevels = input*100;
                if (mChemokineEmitter)
                {
                    var m = mChemokineEmitter.main;
                    var e = mChemokineEmitter.emission;
                    m.maxParticles = (int)(input*100);
                    e.rateOverTime = (int)(input*50);
                }
            }
            get
            {
                return Mathf.Round(mChemokine / mParameter.SensitivityToFeelCytokineGradient) * mParameter.SensitivityToFeelCytokineGradient;
            }
        }
    }
}
