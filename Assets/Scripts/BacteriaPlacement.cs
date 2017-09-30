using UnityEngine;

namespace Assets.Scripts
{
    public class BacteriaPlacement : MonoBehaviour
    {
        void Update()
        {
            /*if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                Debug.Log("Touch registered");
                Vector2 touchPosition = Input.GetTouch(0).position;
                GameController.CreateBacteriaAtPoint(touchPosition.x, touchPosition.y);
            }*/

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                GameController.CreateBacteriaAtPoint(position.x, position.y);
            }
        }

    }
}
