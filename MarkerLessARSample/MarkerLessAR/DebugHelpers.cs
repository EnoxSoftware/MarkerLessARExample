using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using OpenCVForUnity;

public class DebugHelpers : MonoBehaviour
{

    // Use this for initialization
    void Start ()
    {

    }
    
    // Update is called once per frame
    void Update ()
    {
    
    }

    public void showMat (Mat mat)
    {
//              Debug.Log ("mat " + mat.ToString ());

        Texture2D texture = new Texture2D (mat.cols (), mat.rows (), TextureFormat.RGBA32, false);

        Utils.matToTexture2D (mat, texture);

        gameObject.transform.localScale = new Vector3 (texture.width, texture.height, 1);
    
        gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
    }

    // Draw matches between two images
    public static Mat getMatchesImage (Mat query, Mat pattern, MatOfKeyPoint queryKp, MatOfKeyPoint trainKp, MatOfDMatch matches, int maxMatchesDrawn)
    {
        Mat outImg = new Mat ();

        List<DMatch> matchesList = matches.toList ();

        if (matchesList.Count > maxMatchesDrawn) {
//                      matches.resize (maxMatchesDrawn);
            matchesList.RemoveRange (maxMatchesDrawn, matchesList.Count - maxMatchesDrawn);
        }

        MatOfDMatch tmpMatches = new MatOfDMatch ();
        tmpMatches.fromList (matchesList);
        
        Features2d.drawMatches
            (
                query, 
                queryKp, 
                pattern, 
                trainKp,
                tmpMatches, 
                outImg, 
                new Scalar (0, 200, 0, 255), 
                Scalar.all (-1),
                new MatOfByte (), 
                Features2d.NOT_DRAW_SINGLE_POINTS
        );

        
        return outImg;
    }

}
