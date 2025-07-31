using UnityEngine;

public class UIInteractionControl : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject slider1;
    public GameObject slider2;
    void Start()
    {
        slider1.SetActive(false);
        slider2.SetActive(false);
    }

    // Update is called once per frame
    public void toggleSlidersActivate()
    {
        slider1.SetActive(!slider1.activeSelf);
        slider2.SetActive(!slider2.activeSelf);
    }
}
