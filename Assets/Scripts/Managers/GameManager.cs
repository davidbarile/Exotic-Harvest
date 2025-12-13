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
        var isNewGame = !SaveManager.IN.HasSaveFile;
        SaveManager.IN.Init();

        InventoryManager.IN.InitInventoryDict();

        if (isNewGame)
            InventoryManager.IN.AddDefaultItemsToInventory();
        else
            InventoryManager.IN.AddSavedItemsToInventory(SaveManager.IN.CurrentSaveData.InventoryDataDict, SaveManager.IN.CurrentSaveData.AllInventoryItems);
    
        ResourceDisplayManager.IN.Init();
    }
}