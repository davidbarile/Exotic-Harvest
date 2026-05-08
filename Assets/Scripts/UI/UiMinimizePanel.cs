using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UiMinimizePanel : UIPanelBase
{
    [SerializeField] private GameObject[] minimizeButtons;
    [SerializeField] private Transform selectedButtonParent;
    [SerializeField] private Transform buttonMenuParent;
    [SerializeField] private GameObject menuHideTrigger;

    [SerializeField] private UnityEvent[] buttonActions;

    private int selectedButtonIndex = 0;

    private void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void HandleMenuButtonPress(int inIndex)
    {
        if (inIndex == this.selectedButtonIndex)
        {
            HandleSelectedButtonPress();
            return;
        }

        // Move the currently selected button back to the menu
        this.minimizeButtons[this.selectedButtonIndex].transform.SetParent(this.buttonMenuParent, false);

        // Update the selected button index
        this.selectedButtonIndex = inIndex;

        for (int i = 0; i < this.minimizeButtons.Length; i++)
        {
            var button = this.minimizeButtons[i];
            if (i == this.selectedButtonIndex)
            {
                // Highlight the selected button
                button.transform.SetParent(this.selectedButtonParent, false);
                button.transform.SetAsFirstSibling();
                var rectTransform = button.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
            else
            {
                // Move non-selected buttons back to the menu
                button.transform.SetParent(this.buttonMenuParent, false);
                button.transform.SetAsLastSibling();
            }
        }
        
        HandleSelectedButtonPress();
    }

    public void HandleSelectedButtonPress()
    {
        if(this.selectedButtonIndex < this.buttonActions.Length)
        {
            var buttonEvent = this.buttonActions[this.selectedButtonIndex];
            buttonEvent?.Invoke();
        }

        switch (this.selectedButtonIndex)
        {
            case 0:
                // Handle first button action
                Debug.Log("First button pressed!");
                ScreenManager.IN.SetMinOrMaximized(false);
                break;
            case 1:
                // Handle second button action
                Debug.Log("Second button pressed!");
                ScreenManager.IN.SetBgVisibility(false);
                break;
            case 2:
                // Handle third button action
                Debug.Log("Third button pressed!");
                break;

            case 3:
                // Handle fourth button action
                Debug.Log("Fourth button pressed!");
                break;
            case 4:
                UIConfirmPanel.IN.Show("Quit Game", "Are you sure you want to quit the game?", () =>
                {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif  
                });
                break;
        }

        this.menuHideTrigger.SetActive(true);
        Hide();
    }
}