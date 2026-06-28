using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager IN;

    [SerializeField] private SingletonManager singletonManager;

    public bool HideSplashScreenInEditor;

    private void Awake()
    {
        this.singletonManager.Init();
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        SaveManager.IN.Init();

        var isNewGame = !SaveManager.IN.HasSaveFile;
        var showSplashScreen = SaveManager.Data.ShowSplashScreen;

        if(this.HideSplashScreenInEditor)    
            showSplashScreen = false;

        UiManager.IN.Init();
        ScreenManager.IN.Init();
        AudioManager.IN.Init();

        if (isNewGame)
        {
            InventoryManager.IN.AddDefaultItemsToInventory();
        }
        else
        {
            InventoryManager.IN.AddSavedItemsToInventory();
        }

        if (showSplashScreen)
        {
            UiManager.IN.SplashScreenPanel.PlaySplashAnim();
            AudioManager.IN.StartSplashScreenAudio();
        }
        else
        {
            UiManager.IN.SplashScreenPanel.SetVisible(false);
            AudioManager.IN.StartGameAudio();
            StartGame();
        }

        UiManager.IN.ResourcesPanel.Init();
    }
    

    public void StartGame()
    {
        WeatherManager.IN.Init();
        TimeManager.IN.Init();
        ForagingManager.IN.Init();
        DecorationManager.IN.InitDecorationParents();
        NotificationManager.IN.Init();
        ShopManager.IN.Init();
    }
}