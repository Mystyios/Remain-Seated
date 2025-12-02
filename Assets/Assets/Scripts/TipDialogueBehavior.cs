using TMPro;
using UnityEngine;

public class TipDialogueBehavior : MonoBehaviour
{
    [Header("Message")]
    [TextArea]
    [SerializeField] public string message;

    [Header("Settings")]
    [SerializeField] public float timeShown = 5;
    [SerializeField] public bool clearTips = false; // If true, clears tip screen before adding more tips

    [Header("Components")]
    [SerializeField] TipDialogueManager tipManager;
    [SerializeField] public TextMeshProUGUI overrideTemplate;

    // Send message to Tip Dialogue Manager
    public void sendTip()
    {
        tipManager.handleTip(this);
    }

}

