using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager IN;

    [SerializeField] private SingletonManager singletonManager;

    private void Awake()
    {
        this.singletonManager.Init();
        DontDestroyOnLoad(gameObject);
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        SaveManager.IN.Init();
        ShopManager.IN.Init();
        AudioManager.IN.Init();

        var isNewGame = !SaveManager.IN.HasSaveFile;

        if (isNewGame)
            InventoryManager.IN.AddDefaultItemsToInventory();
        else
            InventoryManager.IN.AddSavedItemsToInventory();
    
        UiManager.IN.ResourcesPanel.Init();
    }
}