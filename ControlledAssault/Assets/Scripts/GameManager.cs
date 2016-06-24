using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Text userHealth;

    public Text redInfo;

    public Text blueInfo;

    public Canvas uiCanvas;

    public GameObject userPanel;

    public SoundManager soundManager;

    public Canvas mainMenu;

    public Canvas endMenu;

    public Text winMsg;

    public Text usedSeedText;

    public InputField seedInput;

    public Level levelPrefab;

    public Canvas twoDMap;

    public Unit unitPrefab;

    public Material teamOneUnit;

    public Material teamTwoUnit;

    public Material teamOneSightMaterial;

    public Material teamTwoSightMaterial;

    private Level levelInstance;

    public int unitsPerTeam;

    private TeamManager teamOne;

    private TeamManager teamTwo;

    public GameObject playerCamera;

    public GameObject twoDCamera;

    private Unit userUnit;

    public Image twoDRedUnit;

    public Image twoDBlueUnit;

    private Vector3 defaultCamPos;

    private float defaultCamSize;

    private int seed;

    private bool paused = false;

    private bool inControl = false;

    private bool postGeneratedOne = false;

    private bool postGeneratedTwo = false;

    private bool respawning = false;

    private bool gameRunning = false;

    private bool previousUserHit = false;

    private bool inTransition = false;
    void Start()
    {
        Random.seed = Random.Range(0, int.MaxValue);
        defaultCamPos = twoDCamera.transform.position;
        defaultCamSize = twoDCamera.GetComponent<Camera>().orthographicSize;
        //playerCamera.GetComponent<Camera>().farClipPlane = 8.0f;
    }

    void Update()
    {
        if (gameRunning)
        {
            /*if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetUserUnit(teamOne.units[0]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                TwoDMapView();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SceneManager.LoadScene("Game");
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Application.LoadLevel("Demo");
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                Application.LoadLevel("QA");
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                Pause();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                foreach (Unit unit in teamOne.units)
                {
                    unit.health = 0;
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                foreach (Unit unit in teamTwo.units)
                {
                    unit.health = 0;
                }
            }*/
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                RestartLevel();
            }
            if (Input.GetKeyDown(KeyCode.LeftShift) && inControl)
            {
                TwoDMapView();
            }
            if (!postGeneratedOne && teamOne.units.Count == 0)
            {
                levelInstance.PostGenerateOne(twoDMap, seed, Random.seed);
                StartCoroutine(RespawnTeamOne());
                postGeneratedOne = true;
                respawning = true;
            }
            if (!postGeneratedTwo && teamTwo.units.Count == 0)
            {
                levelInstance.PostGenerateTwo(twoDMap, seed, Random.seed);
                StartCoroutine(RespawnTeamTwo());
                postGeneratedTwo = true;
                respawning = true;
            }
            List<Unit> killedUnits = new List<Unit>();
            foreach (Unit unit in teamOne.units)
            {
                if (unit.health <= 0)
                {
                    killedUnits.Add(unit);
                }
            }
            foreach (Unit unit in killedUnits)
            {
                unit.twoDImage.enabled = false;
                teamOne.units.Remove(unit);
                Destroy(unit.gameObject);
                redInfo.text = "Red Left: " + teamOne.units.Count;
            }
            killedUnits = new List<Unit>();
            foreach (Unit unit in teamTwo.units)
            {
                if (unit.health <= 0)
                {
                    killedUnits.Add(unit);
                }
            }
            foreach (Unit unit in killedUnits)
            {
                unit.twoDImage.enabled = false;
                teamTwo.units.Remove(unit);
                Destroy(unit.gameObject);
                blueInfo.text = "Blue Left: " + teamTwo.units.Count;
            }
            teamOne.Update();
            teamTwo.Update();
            foreach (Unit unit in teamTwo.units)
            {
                if (unit.visible && !teamOne.visibleEnemies.Contains(unit))
                {
                    unit.visible = false;
                    unit.twoDImage.CrossFadeAlpha(0.0f, 0.5f, false);
                    HideUnit(unit);
                }
            }
            foreach (Unit unit in teamOne.visibleEnemies)
            {
                unit.visible = true;
                unit.twoDImage.CrossFadeAlpha(1.0f, 0.0f, false);
                unit.gameObject.transform.GetChild(0).gameObject.layer = 8;
                unit.gameObject.transform.GetChild(1).GetChild(0).gameObject.layer = 11;
                unit.gameObject.transform.GetChild(1).GetChild(1).gameObject.layer = 11;
                unit.gameObject.transform.GetChild(1).GetChild(2).gameObject.layer = 11;
            }
            if (!inControl)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    twoDCamera.transform.Translate(new Vector3(0.0f, 7.0f * Time.deltaTime, 0.0f));
                }
                if (Input.GetKey(KeyCode.S))
                {
                    twoDCamera.transform.Translate(new Vector3(0.0f, -7.0f * Time.deltaTime, 0.0f));
                }
                if (Input.GetKey(KeyCode.A))
                {
                    twoDCamera.transform.Translate(new Vector3(-7.0f * Time.deltaTime, 0.0f, 0.0f));
                }
                if (Input.GetKey(KeyCode.D))
                {
                    twoDCamera.transform.Translate(new Vector3(7.0f * Time.deltaTime, 0.0f, 0.0f));
                }
                float delta = Input.GetAxis("Mouse ScrollWheel");
                if (delta > 0.0f)
                {
                    twoDCamera.GetComponent<Camera>().orthographicSize -= 28.0f * Time.deltaTime;
                    //twoDCamera.transform.Translate(new Vector3(0.0f, 0.0f, 28.0f * Time.deltaTime));
                }
                else if (delta < 0.0f)
                {
                    twoDCamera.GetComponent<Camera>().orthographicSize += 28.0f * Time.deltaTime;
                    //twoDCamera.transform.Translate(new Vector3(0.0f, 0.0f, -28.0f * Time.deltaTime));
                }
                Vector3 clampedPos = twoDCamera.transform.position;
                clampedPos.x = Mathf.Clamp(clampedPos.x, -21.0f, 21.0f);
                clampedPos.y = Mathf.Clamp(clampedPos.y, 13.0f, defaultCamPos.y);
                clampedPos.z = Mathf.Clamp(clampedPos.z, -21.0f, 21.0f);
                twoDCamera.transform.position = clampedPos;

                twoDCamera.GetComponent<Camera>().orthographicSize = Mathf.Clamp(twoDCamera.GetComponent<Camera>().orthographicSize, 2.0f, defaultCamSize);

                
            }
            else
            {
                /*if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    userUnit.health = 0;
                }*/
                if (userUnit != null)
                {
                    if (userUnit.hit && !previousUserHit)
                    {
                        soundManager.PlayHit();
                    }
                    previousUserHit = userUnit.hit;
                    userHealth.text = "HP: " + userUnit.health + "/10";
                }
            }
            if (inControl && (userUnit == null || userUnit.health <= 0))
            {
                TwoDMapView();
            }
            if (postGeneratedOne && teamOne.units.Count == 0 && !respawning)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                endMenu.enabled = true;
                winMsg.text = "Blue Team Wins";
                gameRunning = false;
                usedSeedText.text = seed.ToString();
                soundManager.StopSounds();
            }
            if (postGeneratedTwo && teamTwo.units.Count == 0 && !respawning)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                endMenu.enabled = true;
                winMsg.text = "Red Team Wins";
                gameRunning = false;
                usedSeedText.text = seed.ToString();
                soundManager.StopSounds();
            }
        }
    }

    public void TwoDMapView()
    {
        userPanel.SetActive(false);
        previousUserHit = true;
        inControl = false;
        inTransition = true;
        if (userUnit != null)
        {
            userUnit.gameObject.transform.GetChild(0).gameObject.layer = 8;
            userUnit.GetComponent<Rigidbody>().velocity = Vector3.zero;
            userUnit.controller = new AIController(levelInstance.nodeSystem);
            userUnit = null;
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        StartCoroutine(WorldToMap());
    }

    private IEnumerator MapToWorld(Unit unit)
    {
        WaitForSeconds delay = new WaitForSeconds(0.0f);
        Vector3 lastUnitPosition = unit.transform.position;
        Quaternion lastUnitRotation = unit.transform.rotation;
        playerCamera.transform.position = twoDCamera.transform.position;
        playerCamera.transform.rotation = twoDCamera.transform.rotation;
        CanvasGroup map = twoDMap.gameObject.GetComponent<CanvasGroup>();
        while (map.alpha > 0.0f)
        {           
            map.alpha -= 2.0f * Time.deltaTime;          
            yield return delay;
        }
        playerCamera.GetComponent<Camera>().enabled = true;
        twoDCamera.GetComponent<Camera>().enabled = false;
        Vector3 camPos = twoDCamera.transform.position;
        map.alpha = 0.0f;
        float beta = 1.0f;
        while (beta > 0.0f)
        {
            if (unit != null)
            {
                lastUnitPosition = unit.transform.position;
                lastUnitRotation = unit.transform.rotation;
            }
            Vector3 target = lastUnitPosition;
            Vector3 newPosition = new Vector3();
            beta -= 1.5f * Time.deltaTime;
            newPosition.x = ((1 - beta) * target.x) + (beta * camPos.x);
            newPosition.y = ((1 - beta) * target.y) + (beta * camPos.y);
            newPosition.z = ((1 - beta) * target.z) + (beta * camPos.z);

            playerCamera.transform.position = newPosition;
            playerCamera.transform.rotation = Quaternion.RotateTowards(playerCamera.transform.rotation, lastUnitRotation, 4.5f);
            yield return delay;
        }
        if (unit != null)
        {
            unit.controller = new UserController(playerCamera, 1.5f);
            playerCamera.transform.position = unit.transform.position;
            playerCamera.transform.rotation = unit.transform.rotation;
            unit.gameObject.transform.GetChild(0).gameObject.layer = 9;
            twoDCamera.transform.position = defaultCamPos;
            //playerCamera.GetComponent<Camera>().farClipPlane = 8.0f;
            userUnit = unit;
            userPanel.SetActive(true);
            RenderSettings.fog = true;
            inControl = true;
        }
        else
        {
            TwoDMapView();
        }
        inTransition = false;
    }

    private IEnumerator WorldToMap()
    {
        RenderSettings.fog = false;
        WaitForSeconds delay = new WaitForSeconds(0.0f);
        CanvasGroup map = twoDMap.gameObject.GetComponent<CanvasGroup>();
        Vector3 camPos = twoDCamera.transform.position;
        Vector3 target = playerCamera.transform.position;
        playerCamera.GetComponent<Camera>().farClipPlane = 30.0f;
        float beta = 0.0f;
        while (beta < 1.0f)
        {
            beta += 1.5f* Time.deltaTime;
            Vector3 newPosition = new Vector3();
            newPosition.x = ((1 - beta) * target.x) + (beta * camPos.x);
            newPosition.y = ((1 - beta) * target.y) + (beta * camPos.y);
            newPosition.z = ((1 - beta) * target.z) + (beta * camPos.z);

            playerCamera.transform.position = newPosition;
            playerCamera.transform.rotation = Quaternion.RotateTowards(playerCamera.transform.rotation, twoDCamera.transform.rotation, 4.5f);
            yield return delay;
        }
        playerCamera.GetComponent<Camera>().enabled = false;
        twoDCamera.GetComponent<Camera>().enabled = true;
        while (map.alpha < 1.0f)
        {           
            map.alpha += 2.0f * Time.deltaTime;            
            yield return delay;
        }
        map.alpha = 1.0f;
        inTransition = false;
    }

    public void SetUserUnit(Unit unit)
    {
        if (unit.team.Equals(teamOne) && !inTransition)
        {
            inTransition = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            StartCoroutine(MapToWorld(unit));
        }
    }

    public void StartUserUnit(Unit unit)
    {
        playerCamera.GetComponent<Camera>().enabled = true;
        twoDCamera.GetComponent<Camera>().enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        unit.controller = new UserController(playerCamera, 1.5f);
        playerCamera.transform.position = unit.transform.position;
        playerCamera.transform.rotation = unit.transform.rotation;
        unit.gameObject.transform.GetChild(0).gameObject.layer = 9;
        userUnit = unit;
        inControl = true;
    }

    public void BeginGame()
    {
        string inputSeed = seedInput.text;
        int testSeed = 0;
        if (int.TryParse(inputSeed, out testSeed))
        {
            Random.seed = testSeed;
        }
        seed = Random.seed;
        teamOne = new TeamManager();
        teamTwo = new TeamManager();
        levelInstance = Instantiate(levelPrefab) as Level;
        //StartCoroutine(levelInstance.GenerateCinematically(twoDMap));
        levelInstance.Generate(twoDMap);
        SpawnUnits();
        teamOne.enemyTeam = teamTwo;
        teamTwo.enemyTeam = teamOne;
        teamOne.userTeam = true;
        StartUserUnit(teamOne.units[0]);
        mainMenu.enabled = false;
        gameRunning = true;
        Random.seed = System.DateTime.Now.Millisecond;
        //soundManager.PlayBackground();
        uiCanvas.enabled = true;
    }

    private void SpawnUnits()
    {
        int mid = ((levelInstance.size.z - 1) / 2);
        int start = -(unitsPerTeam / 2);
        List<Unit> units = new List<Unit>();
        for(int i = 0; i < unitsPerTeam; i++)
        {
            units.Add(CreateUnit(teamOneUnit, twoDRedUnit, teamOne, -mid, start + i));
        }
        teamOne.units = units;
        units = new List<Unit>();
        for (int i = 0; i < unitsPerTeam; i++)
        {
            units.Add(CreateUnit(teamTwoUnit, twoDBlueUnit, teamTwo, mid, start + i));
        }
        teamTwo.units = units;
        foreach (Unit unit in teamOne.units)
        {
            unit.visible = true;
        }
        foreach (Unit unit in teamTwo.units)
        {
            unit.twoDImage.CrossFadeAlpha(0.0f, 0.0f, false);
            HideUnit(unit);
        }
    }

    private void HideUnit(Unit unit)
    {
        unit.gameObject.transform.GetChild(0).gameObject.layer = 9;
        unit.gameObject.transform.GetChild(1).GetChild(0).gameObject.layer = 12;
        unit.gameObject.transform.GetChild(1).GetChild(1).gameObject.layer = 12;
        unit.gameObject.transform.GetChild(1).GetChild(2).gameObject.layer = 12;
    }

    private IEnumerator RespawnTeamOne()
    {
        while (!levelInstance.finishGeneration)
        {
            yield return new WaitForSeconds(0.0f);
        }
        int edge = levelInstance.size.z;
        int start = -(unitsPerTeam / 2);
        List<Unit> units = new List<Unit>();
        for (int i = 0; i < unitsPerTeam / 2 + 1; i++)
        {
            units.Add(CreateUnit(teamOneUnit, twoDRedUnit, teamOne, -edge, start + i));
        }
        teamOne.units = units;
        levelInstance.finishGeneration = false;
        foreach (Unit unit in teamOne.units)
        {
            unit.visible = true;
        }
        redInfo.text = "Red Left: " + teamOne.units.Count;
        respawning = false;
    }

    private IEnumerator RespawnTeamTwo()
    {
        while (!levelInstance.finishGeneration)
        {
            yield return new WaitForSeconds(0.0f);
        }
        int edge = levelInstance.size.z;
        int start = -(unitsPerTeam / 2);
        List<Unit> units = new List<Unit>();
        for (int i = 0; i < unitsPerTeam / 2 + 1; i++)
        {
            units.Add(CreateUnit(teamTwoUnit, twoDBlueUnit, teamTwo, edge, start + i));
        }
        teamTwo.units = units;
        levelInstance.finishGeneration = false;
        foreach (Unit unit in teamTwo.units)
        {
            unit.twoDImage.CrossFadeAlpha(0.0f, 0.0f, false);
            HideUnit(unit);
        }
        blueInfo.text = "Blue Left: " + teamTwo.units.Count;
        respawning = false;
    }

    private Unit CreateUnit(Material material, Image twoDImage, TeamManager team, int x, int z)
    {
        Unit unit = Instantiate(unitPrefab) as Unit;
        List<Node> nodeSystem = levelInstance.nodeSystem;
        unit.transform.position = new Vector3(x, 0.4f, z);
        unit.SetController(new AIController(nodeSystem));
        unit.gameObject.transform.GetChild(0).GetComponent<Renderer>().material = material;
        Renderer[] wepRenderers = unit.transform.GetChild(1).GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in wepRenderers)
        {
            rend.material = material;
        }
        Image twoDUnit = Instantiate(twoDImage) as Image;
        twoDUnit.transform.SetParent(twoDMap.transform, false);
        twoDUnit.gameObject.GetComponent<TwoDUnit>().unit = unit;
        twoDUnit.gameObject.GetComponent<UnitTakeover>().SetManager(this);
        twoDUnit.gameObject.GetComponent<UnitTakeover>().SetUnit(unit);
        unit.twoDImage = twoDUnit;
        unit.team = team;
        return unit;
    }

    private IEnumerator RestartGame()
    {
        WaitForSeconds delay = new WaitForSeconds(0.1f);
        foreach (Unit unit in teamOne.units)
        {
            Destroy(unit.gameObject);
        }
        foreach (Unit unit in teamTwo.units)
        {
            Destroy(unit.gameObject);
        }
        Destroy(levelInstance.gameObject);
        levelInstance = null;
        yield return delay;
        BeginGame();
    }

    public void RestartLevel()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Game");
    }

    private void Pause()
    {
        if (!paused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        paused = !paused;
    }
    public void Quit()
    {
        Application.Quit();
    }
}