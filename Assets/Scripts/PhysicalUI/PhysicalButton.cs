using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class PhysicalButton : MonoBehaviour
{
    // public MeshRenderer ButtonMesh;
    // public Material InactiveMat;
    // public Material HoverMat;
    // public Material ActiveMat;
    public UnityEvent OnActivateEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnInactivate();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnHover()
    {
        //ButtonMesh.material = HoverMat;
    }

    public void OnActivate()
    {
        //ButtonMesh.material = ActiveMat;
        Debug.Log("button activate");
        OnActivateEvent?.Invoke();
    }
    
    public void OnInactivate()
    {
        //ButtonMesh.material = InactiveMat;
    }
}
