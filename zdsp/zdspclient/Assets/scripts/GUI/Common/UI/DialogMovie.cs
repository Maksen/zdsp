using UnityEngine;
using UnityEngine.UI;
using System;
public class DialogMovie : MonoBehaviour
{
    public MoviePlayback mv;

    //used to be in not in ui hierachy. If the scene required, then add this in.

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void StartPlay(string movie, Action callback)
    {        
        mv.movieFileName = movie;
        gameObject.SetActive(true);
        mv.StartPlay(callback);
    }
}

