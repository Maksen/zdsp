using UnityEngine;
using System.Collections;

public class UI_ParticleScaleAspectRatio : MonoBehaviour
{
    //Assume 16:9 (1.777778) is the Aspect Ratio used when developing. 
    //Assume developing Scale at 1,1,1
    //4:3 will become Scale 0.75,0.75,0.75

    private float newScale = 1f;

    void Start()

    {
        //print("WidthHeight = " +Screen.width + "x" + Screen.height);

        float currentAspectRatio = (float)Screen.width / (float)Screen.height;
        //print("currentAspectRatio = " + currentAspectRatio);

        if (currentAspectRatio < 1.85)//Not accurate, could be 1.99...
        {
            // Common Ratios (16:9, 4:3....)
            //print("COMMON SCREEN FORMAT");
            newScale = currentAspectRatio / 1.777778f;
            
            gameObject.transform.localScale = new Vector3(newScale, newScale, newScale);
        }
        /* 
        // If Long Format, no need to scale, use Scale 1,1,1
        else
        {
            //18:9 Ratio
            //print("LONG SCREEN FORMAT, NO NEED TO SCALE");
            newScale = 1f;
        }
        

        //Round to 3 decimals
        //newScale = Mathf.Round(newScale * 1000f) / 1000f;
        
        gameObject.transform.localScale = new Vector3(newScale, newScale, newScale);
        */
    }
}
