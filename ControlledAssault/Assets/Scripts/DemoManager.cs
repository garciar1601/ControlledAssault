using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoManager : MonoBehaviour
{
    public Canvas twoDMap;

    public Level levelPrefab;

    private Level levelInstance;

    private bool finished = false;

    private bool cleaned = false;

    private bool runned = false;

    private bool runned1 = false;

    private bool runned2 = false;

    private bool runned3 = false;

    public Canvas title;

    public Canvas tech;

    public Canvas key;

    public Canvas created;

    private int seed;
    void Start()
    {
        Random.seed = 55;
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Application.LoadLevel("Game");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!runned)
            {
                title.enabled = false;
                tech.enabled = true;
                runned = true;
            }
            else if (!runned1)
            {
                tech.enabled = false;
                key.enabled = true;
                runned1 = true;
            }
            else if (!runned2)
            {
                key.enabled = false;
                created.enabled = true;
                runned2 = true;
            }
            else if (!runned3)
            {
                created.enabled = false;
                BeginGame();
                runned3 = true;
            }
            else if (!cleaned)
            {
                StartCoroutine(levelInstance.CleanLevelCinematically());
                cleaned = true;
            }
            else if (!finished)
            {
                StartCoroutine(levelInstance.CreateConnectionsCinematically());
                finished = true;
            }
            else
            {
                Application.LoadLevel("Game");
            }
        }
    }
    private void BeginGame()
    {
        levelInstance = Instantiate(levelPrefab) as Level;
        StartCoroutine(levelInstance.GenerateCinematically(twoDMap));
    }
}
