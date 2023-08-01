using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum eScene {fe, inGame }

public class GameManager : MonoBehaviour
{
    public static GameManager gm;
    public Settings set;    
    public Player[] players;
    public soRep so_Rep;
    
    public int numPlayers;

    public AudioManager audioManager;
    public CanvasManager canvasManager;    
    public TimerController timerController;
    public Board board;

    public eScene curScene;


    public PlayerFunctions playerFunctions;
    public SpotFunctions spotFunctions;

    private void Awake()
    {
        if (gm != null && gm != this)
        {
            Destroy(gameObject);
        }
        else if (gm != this)
        {
            gm = this;
            DontDestroyOnLoad(gameObject);            
        }

        if (!set) set = gameObject.AddComponent<Settings>();
        
        if (!audioManager) audioManager = Instantiate(Resources.Load("Managers/" + "AudioManager") as GameObject, transform).GetComponent<AudioManager>();
        
        if (!canvasManager) canvasManager = gameObject.AddComponent<CanvasManager>();  //Creates/sets the canvasManager
        if (!playerFunctions) playerFunctions = gameObject.AddComponent<PlayerFunctions>();
        if (!timerController) timerController = gameObject.AddComponent<TimerController>();
        if (!spotFunctions) spotFunctions = gameObject.AddComponent<SpotFunctions>();        
        
    }
   
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(eScene _idx)
    {
        Debug.Log("<color=yellow>Load Scene: " + _idx + "</color>");
        SceneManager.LoadScene((int)_idx);
    }

    private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
    {
        Debug.Log("<color=yellow>New Scene Loaded: " + (eScene)_scene.buildIndex + "</color>");
        curScene = (eScene)_scene.buildIndex;
        switch (curScene)
        {
            case eScene.fe:
                timerController.BeginTimer();
                playerFunctions.CreatePlayers();
                canvasManager.ShowCanvasFE();                
                break;
            case eScene.inGame:                
                LoadEnvironment();
                playerFunctions.SpawnPlayers();
                numPlayers = playerFunctions.players.Count; // Sets this variable to the actual number of players
                canvasManager.ShowCanvasHUD();
                break;
        }
    }

    void LoadEnvironment()
    {
        Instantiate(Resources.Load("GameObjects/Environment/" + "pMonoRoom") as GameObject);
        board = Instantiate(Resources.Load("GameObjects/Board/" + "pMonoBoard") as GameObject).GetComponent<Board>();
        
    }
    public void PlaySound(eSounds _sound)
    {
        audioManager.PlaySound(so_Rep.audioGameSounds[(int)_sound]);
    }

    public void GoToOptions()
    {
        canvasManager.ShowCanvasOptions();
    }

    
}
