using UnityEngine;

public class PhysicalButtonToggle : MonoBehaviour
{
    public GameObject ObjectToggled;

    public void ToggleObject()
    {
        ObjectToggled.SetActive(!ObjectToggled.activeSelf);
    }
}
