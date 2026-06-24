using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager IN;
    public static bool IsTutorial = false;

    [SerializeField] private GameObject tutorialOverlay;

    public void Awake()
    {
        this.tutorialOverlay.SetActive(false);
    }

    public void SetTutorialMode(bool inIsTutorial)
    {
        IsTutorial = inIsTutorial;
        this.tutorialOverlay.SetActive(inIsTutorial);
    }
}