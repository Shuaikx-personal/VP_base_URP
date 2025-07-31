using System.Collections;
using UnityEngine;

//using UnityEngine.Rendering.Universal;

public class PassthroughControl : MonoBehaviour
{
    public Camera m_Camera;
    public GameObject bloom;
    //private UniversalAdditionalCameraData m_UniversalAdditionalCameraData;

    [Tooltip("Main Camera")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private Color backgroundColor = Color.black;

    [SerializeField] private bool isTransparent = true; 
    private Coroutine activeFadeCoroutine;

    public static PassthroughControl Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        if (m_Camera == null)
        {
            Debug.LogError("ControlURPCameraPostProcessing: No Camera component found on this GameObject. This script requires a Camera component.", this);
            enabled = false; // Disable the script if no Camera is found
            return;
        }

        
    }

    private void Start()
    {
        if (bloom != null)
            bloom.SetActive(false);
        m_Camera.clearFlags = CameraClearFlags.Color;
        m_Camera.backgroundColor = Color.clear;
        isTransparent = true;
    }
    

    public void ToggleHDR()
    {
        m_Camera.allowHDR = !m_Camera.allowHDR;
    }

    public void TogglePostProcessing()
    {
        if (bloom != null)
        {
            bloom.SetActive(!bloom.activeSelf);
        }
    }

    public void TogglePassthrough()
    {
        // ToggleHDR();
        // TogglePostProcessing();
        isTransparent = !isTransparent;

        Color targetColor = isTransparent ? Color.clear : backgroundColor;

        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }

        Debug.Log($"Toggle the passthrough to be {isTransparent}");
        activeFadeCoroutine = StartCoroutine(FadeColorCoroutine(targetColor, fadeDuration));
    }
    
    public void SetHDR(bool State)
    {
        m_Camera.allowHDR = !m_Camera.allowHDR;
    }

    public void SetPostProcessing(bool state)
    {
        if (bloom != null)
        {
            if (bloom.activeSelf == state)
                return;
            
            bloom.SetActive(!bloom.activeSelf);
        }
    }

    public void SetPassthrough(bool state)
    {
        if (isTransparent == state)
            return;

        isTransparent = !isTransparent;
        Color targetColor = isTransparent ? Color.clear : backgroundColor;

        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }

        Debug.Log($"Toggle the passthrough to be {isTransparent}");
        activeFadeCoroutine = StartCoroutine(FadeColorCoroutine(targetColor, fadeDuration));
    }

    private IEnumerator FadeColorCoroutine(Color targetColor, float duration)
    {
        Color startingColor = m_Camera.backgroundColor;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            m_Camera.backgroundColor = Color.Lerp(startingColor, targetColor, t);
            yield return null;
        }

        m_Camera.backgroundColor = targetColor;
        activeFadeCoroutine = null;
    }

    public void DoQuit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
