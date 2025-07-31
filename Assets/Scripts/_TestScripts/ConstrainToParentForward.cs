using UnityEngine;

public class ConstrainToParentForward : MonoBehaviour
{
    [Tooltip("移动速度")]
    public float moveSpeed = 1.0f; 

    [Tooltip("要跟随的目标 Transform。它应该与此物体拥有相同的父物体。")]
    public Transform target; 

    [Header("移动范围限制 (局部Z轴)")]
    [Tooltip("Cube 在父物体局部Z轴上能移动的最小Z值。")]
    public float zMinLimit = -5.0f; // 默认开始阈值

    [Tooltip("Cube 在父物体局部Z轴上能移动的最大Z值。")]
    public float zMaxLimit = 5.0f; // 默认结束阈值

    [Tooltip("当 Cube 达到或超过 ZMaxLimit 时，此属性将变为 true。")]
    public bool IsFinished;// 新增的属性，表示是否完成

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    void Start()
    {
        // 确保物体有一个父物体
        if (transform.parent == null)
        {
            Debug.LogWarning("此物体没有父物体，脚本可能无法按预期工作。", this);
            enabled = false; 
            return;
        }

        // 检查 target 是否被设置
        if (target == null)
        {
            Debug.LogWarning("未设置目标 Transform (target)。请在 Inspector 中指定一个目标。", this);
            enabled = false; 
            return;
        }

        // 检查 target 是否与此物体共享同一个父物体
        if (target.parent != transform.parent)
        {
            Debug.LogWarning("目标物体 (target) 与此物体没有相同的父物体。请确保它们共享同一个父物体。", this);
            enabled = false; 
            return;
        }

        // 检查 Z 轴限制的有效性
        if (zMinLimit >= zMaxLimit)
        {
            Debug.LogWarning("Z轴最小限制 (zMinLimit) 必须小于最大限制 (zMaxLimit)。请调整 Inspector 中的值。", this);
            enabled = false;
            return;
        }

        // 记录物体相对于父物体的初始局部位置和旋转
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        // 初始化 IsFinished 状态
        IsFinished = false; 
    }

    void Update()
    {
        // 锁定 X 和 Y 轴的局部位置，保持初始值
        transform.localPosition = new Vector3(initialLocalPosition.x, initialLocalPosition.y, transform.localPosition.z);

        // 锁定所有旋转，保持初始局部旋转
        transform.localRotation = initialLocalRotation;

        // 如果没有目标，或者目标被禁用，则不进行跟随
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            return;
        }

        // 获取目标在父物体局部坐标系下的 Z 轴位置
        Vector3 targetLocalPosition = transform.parent.InverseTransformPoint(target.position);
        
        // 获取当前物体在父物体局部坐标系下的 Z 轴位置
        float currentLocalZ = transform.localPosition.z;
        float targetLocalZ = targetLocalPosition.z;

        // 计算 Z 轴上的目标位置（需要被限制的）
        float clampedTargetZ = Mathf.Clamp(targetLocalZ, zMinLimit, zMaxLimit);

        // 如果 Cube 已经到达了 ZMaxLimit 并且目标Z仍然大于ZMaxLimit，
        // 或者 Cube 已经到达了 ZMinLimit 并且目标Z仍然小于ZMinLimit，
        // 则停止移动。
        if ((currentLocalZ >= zMaxLimit && targetLocalZ >= currentLocalZ) ||
            (currentLocalZ <= zMinLimit && targetLocalZ <= currentLocalZ) )
        {
            // 如果目标超出了 ZMaxLimit 并且当前 Cube 已经到达或超过 ZMaxLimit，
            // 那么将 Cube 锁定在 ZMaxLimit
            if (targetLocalZ >= zMaxLimit)
            {
                transform.localPosition = new Vector3(initialLocalPosition.x, initialLocalPosition.y, zMaxLimit);
                IsFinished = true; // 达到或超出结束阈值，设置为 true
            }
            // 如果目标超出了 ZMinLimit 并且当前 Cube 已经到达或低于 ZMinLimit，
            // 那么将 Cube 锁定在 ZMinLimit
            else if (targetLocalZ <= zMinLimit)
            {
                transform.localPosition = new Vector3(initialLocalPosition.x, initialLocalPosition.y, zMinLimit);
                IsFinished = false; // 在开始阈值，未完成
            }
            else
            {
                // 目标在限制范围内，但因为当前位置已经达到某个边界，且目标还在同方向移动，
                // 则保持 IsFinished 状态不变（如果是在 ZMaxLimit，则保持 true）
                // 如果目标回到了范围内，IsFinished会在下方更新
            }
            return; // 停止移动
        }


        // 计算 Z 轴上的距离（使用被限制后的目标位置）
        float distanceZ = clampedTargetZ - currentLocalZ;

        // 如果已经非常接近目标 Z 轴位置，则停止移动
        if (Mathf.Abs(distanceZ) < 0.01f) 
        {
            transform.localPosition = new Vector3(initialLocalPosition.x, initialLocalPosition.y, clampedTargetZ);
        }
        else
        {
            // 计算这一帧应该移动的距离
            float moveAmount = moveSpeed * Time.deltaTime;

            // 根据距离的正负方向确定移动方向
            if (distanceZ > 0)
            {
                // 目标在前方，向正 Z 方向移动
                transform.localPosition += new Vector3(0, 0, Mathf.Min(moveAmount, distanceZ));
            }
            else
            {
                // 目标在后方，向负 Z 方向移动
                Mathf.Min(moveAmount, distanceZ); // 修正：应使用 Math.Max 负数
                transform.localPosition += new Vector3(0, 0, Mathf.Max(-moveAmount, distanceZ));
            }
        }

        // 更新 IsFinished 状态
        // 只有当 Cube 的局部Z位置达到或超过 ZMaxLimit 时才设置为 true
        IsFinished = transform.localPosition.z >= zMaxLimit;
    }
}