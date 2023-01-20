using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Test : MonoBehaviour
{
    public TextMeshProUGUI directionText;
    private Touch theTouch;
    private Vector2 touchStartPos, touchEndPos;
    private string direction;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            theTouch = Input.GetTouch(0);

            if (theTouch.phase == TouchPhase.Began)
            {
                touchStartPos = theTouch.position;
            }
            else if (theTouch.phase == TouchPhase.Moved || theTouch.phase == TouchPhase.Ended)
            {
                touchEndPos = theTouch.position;

                float x = touchEndPos.x - touchStartPos.x;
                float y = touchEndPos.y - touchStartPos.y;

                if (Mathf.Abs(x) == 0 && Mathf.Abs(y) == 0)
                {
                    direction = "Tapped";
                }
                else if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    direction = x > 0 ? "Right" : "Left";
                }
                else
                {
                    direction = y > 0 ? "Up" : "Down";
                }
            }
        }


        directionText.text = direction;
    }
}
