using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIScripts : MonoBehaviour
{

    public void StartGame()
    {
        Debug.Log("Starting");
        var gc = GameObject.Find("GameController").GetComponent<GameController>();
        var bactSlider = GameObject.Find("BacteriaSlider").GetComponent<Slider>();
        var macroSlider = GameObject.Find("MacroSlider").GetComponent<Slider>();
        gc.NumberOfBacteria = (int)bactSlider.value;
        gc.NumberOfMacrophages = (int)macroSlider.value;
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Debug.Log("Quiting");
        Application.Quit();
    }

}
