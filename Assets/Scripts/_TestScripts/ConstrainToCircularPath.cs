using UnityEngine;
using UnityEngine.Events; 

public class ConstrainToCircularPath : MonoBehaviour
{
    [Tooltip("圆环移动速度 (度/秒)")]
    public float moveSpeed = 90.0f;

    [Tooltip("圆环的半径")]
    public float radius = 5.0f;

    [Tooltip("小球在圆环上的起始角度 (度)。0度表示在局部X轴正方向。")]
    [Range(0f, 360f)] // 限制角度输入范围
    public float startAngle = 0.0f;

    [Tooltip("要跟随的目标 Transform。它应该与此物体拥有相同的父物体。")]
    public Transform target;

    [Header("事件")]
    [Tooltip("当小球经过起始点角度时触发的事件。")]
    public UnityEvent OnPassStartingPoint; // 定义一个 UnityEvent

    private Vector3 initialLocalOffset; // 记录相对于父物体原点的初始偏移
    private Quaternion initialLocalRotation; // 记录初始局部旋转，用于锁定
    private float currentAngle; // 小球当前在圆环上的角度
    private bool hasPassedStartPointOnce = false; // 标记是否已经至少经过起始点一次，防止 Start 阶段触发

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

        // 初始化小球的当前角度为起始角度
        currentAngle = startAngle;
        // 将起始角度转换为弧度，并计算初始局部位置
        UpdatePosition(currentAngle);

        // 记录小球相对于父物体的初始局部偏移和旋转，用于锁定Y轴位置和所有旋转
        initialLocalOffset = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        // 确保 OnPassStartingPoint 事件已初始化，避免空引用
        if (OnPassStartingPoint == null)
        {
            OnPassStartingPoint = new UnityEvent();
        }
    }

    void Update()
    {
        // 锁定小球的局部Y轴位置，保持其在父物体的XZ平面上
        transform.localPosition = new Vector3(transform.localPosition.x, initialLocalOffset.y, transform.localPosition.z);
        // 锁定所有旋转
        transform.localRotation = initialLocalRotation;

        // 如果没有目标，或者目标被禁用，则不进行跟随
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            return;
        }

        // 获取目标在父物体局部坐标系下的位置
        Vector3 targetLocalPosition = transform.parent.InverseTransformPoint(target.position);

        // 计算目标在局部XZ平面上的角度
        // Atan2 返回的角度范围是 -180 到 180 度，需要转换为 0 到 360 度
        float targetAngle = Mathf.Atan2(targetLocalPosition.x, targetLocalPosition.z) * Mathf.Rad2Deg;
        // 归一化到 0-360 度
        if (targetAngle < 0) targetAngle += 360f;

        // 计算需要移动的角度差
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

        // 如果角度差非常小，直接对齐并停止移动
        if (Mathf.Abs(angleDifference) < 0.1f)
        {
            currentAngle = targetAngle;
            UpdatePosition(currentAngle);
            hasPassedStartPointOnce = true; // 认为已经经过了起始点
            return;
        }

        // 计算这一帧应该旋转的角度
        float angleStep = moveSpeed * Time.deltaTime;

        // 根据角度差的正负方向确定旋转方向
        if (angleDifference > 0)
        {
            // 目标在前方（顺时针），向正方向旋转
            currentAngle += Mathf.Min(angleStep, angleDifference);
        }
        else
        {
            // 目标在后方（逆时针），向负方向旋转
            currentAngle += Mathf.Max(-angleStep, angleDifference);
        }

        // 确保角度在 0-360 范围内
        currentAngle = WrapAngle(currentAngle);

        // 更新小球的位置
        UpdatePosition(currentAngle);

        // 检测是否经过起始点
        CheckForStartingPointPass();
    }

    // 根据当前角度更新小球的局部位置
    void UpdatePosition(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        float x = radius * Mathf.Sin(angleRad); // 使用 Sin 对应局部X轴
        float z = radius * Mathf.Cos(angleRad); // 使用 Cos 对应局部Z轴

        transform.localPosition = new Vector3(x, initialLocalOffset.y, z);
    }

    // 辅助函数：将角度归一化到 0-360 度
    float WrapAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0)
        {
            angle += 360f;
        }
        return angle;
    }

    // 检查是否经过起始点
    void CheckForStartingPointPass()
    {
        // 计算起始角度附近的范围，以便更可靠地检测
        float startAngleNormalized = WrapAngle(startAngle);
        float angleThreshold = moveSpeed * Time.deltaTime * 1.5f; // 阈值略大于一帧的移动量

        // 检查小球是否“越过”了起始点
        // 需要考虑顺时针和逆时针两种情况
        // 简单的逻辑是：如果上一帧和当前帧的角度跨越了起始点

        // 注意：为了简单和可靠，我们不存储 previousAngle
        // 而是判断当前角度是否在起始点附近，并且我们已经开始运动

        if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, startAngleNormalized)) < angleThreshold)
        {
            if (hasPassedStartPointOnce) // 避免在 Start() 第一次设置位置时就触发
            {
                // 如果已经检测到通过，但在同一帧可能因为角度精度问题再次触发，避免重复
                // 更精确的触发需要记录上一帧角度，这里简化处理
                // 如果当前距离起始点足够近，并且是第一次到达或者已经开始运动后的再次到达
                OnPassStartingPoint?.Invoke(); // 触发事件
                hasPassedStartPointOnce = false; // 重置标记，直到下次经过再触发
            }
            else
            {
                hasPassedStartPointOnce = true; // 第一次到达，标记为已通过
            }
        }
        else
        {
            // 如果小球离开了起始点附近的区域，重置标记，以便下次经过时能再次触发
            if (hasPassedStartPointOnce && Mathf.Abs(Mathf.DeltaAngle(currentAngle, startAngleNormalized)) > angleThreshold * 2) // 离开范围
            {
                hasPassedStartPointOnce = false;
            }
        }
    }

    public void triggerEvent()
    {
        Debug.Log("pass the start point");
    }
}