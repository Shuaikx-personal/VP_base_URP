using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events; 


public class SliderInteraction : MonoBehaviour
{
    public bool IsMoveable = false;
    public Renderer sliderRenderer;
    public Material originalMaterial;
    public Material grayMaterial;
    public Material greenMaterial;
    public UnityEvent OnToggleActivated;
    public UnityEvent OnToggleInactivated;
    public UnityEvent OnFingerEnter;
    public UnityEvent OnFingerExit;

    public UnityEvent ResetToggle;
    public List<GameObject> contactingDragCubes = new List<GameObject>();
    private Rigidbody sliderRb; // 获取 SliderCube 的 Rigidbody
    private Vector3 originPosition;

    private Vector3 initialSliderToDragMidpointOffset; // SliderCube 中心到双 DragCube 中心点的初始向量偏移
    void Start()
    {
        sliderRb = GetComponent<Rigidbody>();
        originPosition = this.transform.position;

        originalMaterial = sliderRenderer.material;
        if (grayMaterial == null || greenMaterial == null)
        {
            Debug.LogError("请为 SliderInteraction 脚本指定 Gray Material 和 Green Material！", this);
            enabled = false;
        }
        ResetSlider();
    }

    void FixedUpdate()
    {
        // 移除列表中已经被销毁或不再激活的物体 (安全检查)
        contactingDragCubes.RemoveAll(item => item == null || !item.activeInHierarchy);
        if (!IsMoveable)
            return;
        //UpdateSliderState(); // 根据接触的 DragCube 数量更新材质

            if (contactingDragCubes.Count == 2)
            {
                Vector3 dragCube1Pos = contactingDragCubes[0].transform.position;
                Vector3 dragCube2Pos = contactingDragCubes[1].transform.position;
                Vector3 currentDragMidpoint = (dragCube1Pos + dragCube2Pos) / 2f;

                // 如果是刚刚进入拖拽状态 (从非拖拽到拖拽)
                if (initialSliderToDragMidpointOffset == Vector3.zero) // 初始偏移量为零表示刚刚开始拖拽
                {
                    initialSliderToDragMidpointOffset = transform.position - currentDragMidpoint;
                }

                // 计算目标位置：基于当前拖拽中心点加上初始偏移
                Vector3 targetPosition = currentDragMidpoint + initialSliderToDragMidpointOffset;

                // 使用 Rigidbody.MovePosition 进行平滑物理移动
                // 为了防止穿透和保持物理交互，我们不再直接设置位置，而是施加力或速度
                // 但对于拖拽，直接设置目标位置更直观，所以这里用MovePosition
                sliderRb.MovePosition(targetPosition);
            }
            else
            {
                // 如果不足两个 DragCube 接触，重置拖拽偏移
                initialSliderToDragMidpointOffset = Vector3.zero;
            }
    }

    void OnTriggerEnter(Collider collider)
    {
        OnFingerEnter?.Invoke();
        if (collider.gameObject.CompareTag("DragCube") && !contactingDragCubes.Contains(collider.gameObject))
        {
            Debug.Log($"Collide with {collider.gameObject.name}");
            contactingDragCubes.Add(collider.gameObject);
            UpdateSliderState();
        }
        if (contactingDragCubes.Count == 2)
        {
            OnToggleActivated?.Invoke();
        }
    }

    void OnTriggerExit(Collider collider)
    {
        OnFingerExit?.Invoke();
        if (collider.gameObject.CompareTag("DragCube") && contactingDragCubes.Contains(collider.gameObject))
        {
            contactingDragCubes.Remove(collider.gameObject);

            UpdateSliderState();
        }
        if (contactingDragCubes.Count == 0)
        {
            OnToggleInactivated?.Invoke();
        }
    }

    public void ResetSlider()
    {
        sliderRb.MovePosition(originPosition);

        ResetToggle?.Invoke();
        
    }

    private void UpdateSliderState()
    {
        int count = contactingDragCubes.Count;
        switch (count)
        {
            case 2:
                break;
            case 1:
                break;
            default:
                break;
        }
        SetSliderMaterial(count);

    }

    private void SetSliderMaterial(int index)
    {
        Material targetMaterial;
        switch (index)
        {
            case 2:
                targetMaterial = greenMaterial;
                break;
            case 1:
                targetMaterial = grayMaterial;
                break;
            default:
                targetMaterial = originalMaterial;
                break;
        }
        if (sliderRenderer.material != targetMaterial)
        {
            sliderRenderer.material = targetMaterial;
        }
    }
}