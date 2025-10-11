using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{

    #region Variables
    public static GameManager instance = null;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    #region Unity Event Functions
    void Awake()
    {
        if (!instance)
        {
            instance = this;
        }

    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion

    private void StartGame()
    {

    }
}