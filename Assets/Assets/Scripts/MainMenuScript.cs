using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(AudioSource))]
public class MainMenuScript : MonoBehaviour
{
    [Header("Game Canvas Settings")]
    [Tooltip("Main Menu Canvas reference.")]
    public Canvas menuCanvas;
    
    [Header("Music Settings")]
    [Tooltip("Audio source for menu music need the AudioListener component on the same GameObject for hearing and change sound file.")]
    public AudioSource menuMusicSource;
    [Tooltip("Audio clip for menu music need to be the same of the AudioListener.")]
    public AudioClip menuMusicClip;
    
    [Header("Input Action Settings")]
    [Tooltip("Input Action Reference for opening and closing the menu.")]
    public InputActionReference openMenuAction;

    [Header("Light Settings")]
    public GameObject[] lights;
    
    [Header("Menu Events")]
    [Tooltip("Events to invoke when the menu awake.")]
    public UnityEvent onEnable;
    [Tooltip("Events to invoke when the menu disable.")]
    public UnityEvent onDisable;
    
    private void Awake()
    {
        menuCanvas.enabled = false;
        SetTimeScale(1f);
        
    }

    public void Update()
    {
        
        
    // ====================== Menu Open/Close =====================    
        if (!openMenuAction.action.WasPressedThisFrame())
            return;

        bool isOpen = menuCanvas.enabled;

        if (!isOpen)
        {
            //Open menu
            menuCanvas.enabled = true;
            SetTimeScale(0f);
            PlayMenuMusic();
            //Debug.Log("Menu opened");
        }
        else
        {
            // Close menu
            menuCanvas.enabled = false;
            SetTimeScale(1f);
            //Debug.Log("Menu closed");
        }
    }

    public void OnEnable()
    {
        if (menuCanvas == null)
            menuCanvas = GetComponent<Canvas>();
        
        // Enable input so the menu can open
        openMenuAction.action.Enable();
        
        // onEnable.Invoke();
    }
    
    public void OnDisable()
    {
        onDisable.Invoke();
    }
    
    // ====================== Menu Methods =====================
    
    public void SetTimeScale(float timeScaleValue)
    {
        Time.timeScale = timeScaleValue;
    }
    
    public void ChangeScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    
    public void TurnOffOtherMusic(AudioSource otherMusicSource)
    {
        otherMusicSource.Stop();
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
    
    
    // ====================== Music Voids =====================
    
    public void PlayMenuMusic()
    {
        if(menuMusicSource != null && menuMusicClip != null)
        {
            menuMusicSource.clip = menuMusicClip;
            menuMusicSource.loop = true;
            menuMusicSource.Play();
        }
    }
    
    // ==================== Button Methods ====================
    
    
    
    
    // ==================== Light Methods ====================

    public void SetLightValue(float lightValue)
    {
        foreach (var VARIABLE in lights)
        {
            Light light = VARIABLE.GetComponent<Light>();
            if (light != null)
            {
                light.intensity = lightValue;
            }
        }
    }
    
}