using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager IN;

    [SerializeField] private SingletonManager singletonManager;

    private void Awake()
    {
        singletonManager.Init();
        DontDestroyOnLoad(gameObject);
    }
}