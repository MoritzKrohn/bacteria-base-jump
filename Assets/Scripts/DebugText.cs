using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugText : MonoBehaviour {

    // Use this for initialization
    TextMesh text;
    Cell cell;
    void Start () {
        var parent = transform.parent;
        cell = transform.parent.GetComponent<Cell>();

        var parentRenderer = parent.GetComponent<Renderer>();
        var renderer = GetComponent<Renderer>();
        renderer.sortingLayerID = parentRenderer.sortingLayerID;
        renderer.sortingOrder = parentRenderer.sortingOrder;

        var spriteTransform = parent.transform;
        text = GetComponent<TextMesh>();
        var pos = spriteTransform.position;
        text.text = "";
        if (cell.DebugInformation)
        {
            text.text = string.Format("Bact: {0}\nMacs: {1}", 0, 0);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {

        if (cell.DebugInformation)
        {
            text.text = string.Format("Bact: {0}\nMacs: {1}", cell.BacteriaOnCell, cell.MacrophageOnCell.Count);
        }
        
    }
}
