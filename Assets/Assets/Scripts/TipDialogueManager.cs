using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TipDialogueManager : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private float slideDistance, slideSpeed;
    [SerializeField] private float shownTime;

    [Header("Components")]
    [SerializeField] private TextMeshProUGUI defaultTemplateText; // The text template for all the tips
    [SerializeField] private GameObject tipPanel; // This panel should be the gui all tips are shown inside, the tip parent

    private List<TextMeshProUGUI> currentlyShownTips = new List<TextMeshProUGUI>(); // Tracks all currently shown tips
    private Dictionary<TextMeshProUGUI, List<Coroutine>> activeCoroutines = new();

    // Handle new tip
    public void handleTip(TipDialogueBehavior tipInfo)
    {

        // Check if clear, if so delete all prev tips
        if (tipInfo.clearTips == true)
        {
            foreach (TextMeshProUGUI shownTip in new List<TextMeshProUGUI>(currentlyShownTips)) // Can't delete durign iteration, so iterate through a temp copy
            {
                DestroyTip(shownTip);
            }
        }

        // Create new  & Fade In
        TextMeshProUGUI tipTemplate;

        if (tipInfo.overrideTemplate != null)
        {
            tipTemplate = tipInfo.overrideTemplate; // Use tip's override template
        } else
        {
            tipTemplate = defaultTemplateText; // Use default template
        }

        TextMeshProUGUI newText = Instantiate(tipTemplate, tipPanel.transform);
        TrackCoroutine(newText, FadeIn(newText));

        // Edit new Text
        newText.text = tipInfo.message;

        // Slide Down old tips
        foreach (TextMeshProUGUI prevTip in currentlyShownTips)
        {
            TrackCoroutine(prevTip, SlideDown(prevTip));
        }

        // Add to list
        currentlyShownTips.Add(newText);
        TrackCoroutine(newText, TipRoutine(newText, tipInfo));
    }

    // Tip lifetime
    public IEnumerator TipRoutine(TextMeshProUGUI tip, TipDialogueBehavior tipInfo)
    {
        yield return new WaitForSeconds(tipInfo.timeShown);
        yield return StartCoroutine(FadeOut(tip));

        DestroyTip(tip);
    }




    #region Tip Actions

    public void DestroyTip(TextMeshProUGUI tip)
    {
        StopCoroutinesOnTip(tip);
        currentlyShownTips.Remove(tip);
        Destroy(tip);
    }

    // Slide tip down to make room for a new one
    public IEnumerator SlideDown(TextMeshProUGUI text)
    {
        Debug.Log("Sliding");
        Vector2 location = text.transform.position - new Vector3(0, slideDistance, 0);

        while (Vector2.Distance(text.transform.position, location) > .1f)
        {
            text.transform.position = Vector2.MoveTowards(text.transform.position, location, slideSpeed * Time.deltaTime);
            yield return null;
        }

        text.transform.position = location; // Snap to end
    }

    public IEnumerator FadeIn(TextMeshProUGUI text)
    {
        Color c = text.color;
        c.a = 0f;
        text.color = c;

        while (text.color.a < 1f)
        {
            c.a += 0.05f;
            text.color = c;
            yield return null;
        }

        // Snap to end
        c.a = 1f;
        text.color = c;
    }

    public IEnumerator FadeOut(TextMeshProUGUI text)
    {
        Color c = text.color;

        while (text.color.a > 0f)
        {
            c.a -= 0.05f;
            text.color = c;
            yield return null;
        }

        // Snap to end
        c.a = 0f;
        text.color = c;
    }

    #endregion




    #region Helper Functions

    // When starting a routine, track it so it can be stopped later

    public Coroutine TrackCoroutine(TextMeshProUGUI tip, IEnumerator routine)
    {
        Coroutine c = StartCoroutine(routine);

        if (!activeCoroutines.ContainsKey(tip))
        {
            activeCoroutines[tip] = new List<Coroutine>();
        }

        activeCoroutines[tip].Add(c);
        return c;
    }

    public void StopCoroutinesOnTip(TextMeshProUGUI tip)
    {
        if (!activeCoroutines.ContainsKey(tip)) { return; }

        foreach (Coroutine c in activeCoroutines[tip])
        {
            StopCoroutine(c);
        }

        activeCoroutines.Remove(tip);
    }

    #endregion
}