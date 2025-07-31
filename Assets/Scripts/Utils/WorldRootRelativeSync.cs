using Unity.Netcode;
using UnityEngine;

public class WorldRootRelativeSync : NetworkBehaviour
{
    // 定义一个 NetworkVariable，用于同步相对位置和旋转。
    // 服务器拥有写入权限，所有客户端都有读取权限。
    [SerializeField]
    private NetworkVariable<Vector3> m_RelativePosition = new(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField]
    private NetworkVariable<Quaternion> m_RelativeRotation = new(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // World Root 是所有客户端同步的公共参考系。
    [Tooltip("场景中作为同步参考系的World Root物体")]
    [SerializeField]
    private Transform m_WorldRoot;

    // 平滑同步所需的插值参数。
    [Tooltip("插值速度，用于平滑地同步客户端上的位置。")]
    [SerializeField]
    private float m_PositionLerpSpeed = 10f;

    [Tooltip("插值速度，用于平滑地同步客户端上的旋转。")]
    [SerializeField]
    private float m_RotationLerpSpeed = 10f;

    // 在 Start 中查找 World Root
    private void Start()
    {
        if (m_WorldRoot == null)
        {
            GameObject worldRootObject = GameObject.Find("World Root");
            if (worldRootObject != null)
            {
                m_WorldRoot = worldRootObject.transform;
            }
            else
            {
                Debug.LogError("World Root not found! Please create a GameObject named 'World Root' in the scene.");
                enabled = false; // 禁用脚本
                return;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        // 客户端在生成时，订阅 NetworkVariable 的值变更事件。
        // 服务器端不需要订阅，因为它会自己更新值。
        if (!IsServer)
        {
            m_RelativePosition.OnValueChanged += OnRelativePositionChanged;
            m_RelativeRotation.OnValueChanged += OnRelativeRotationChanged;
        }

        // 服务器端初始化 NetworkVariable 的值。
        // 如果服务器端有该物体的NetworkObject权限，它可以在OnNetworkSpawn中设置初始值
        // 但为了确保所有客户端在同一帧获取，我们让FixedUpdate来处理。
    }

    private void FixedUpdate()
    {
        // 只有服务器端才需要更新 NetworkVariable 的值。
        if (!IsServer || m_WorldRoot == null)
        {
            return;
        }

        // 计算本物体相对于 World Root 的相对位置和旋转。
        Vector3 relativePosition = m_WorldRoot.InverseTransformPoint(transform.position);
        Quaternion relativeRotation = Quaternion.Inverse(m_WorldRoot.rotation) * transform.rotation;

        // 如果相对姿态发生变化，则更新 NetworkVariable。
        // Netcode 会自动将这些变化同步到所有客户端。
        if (m_RelativePosition.Value != relativePosition)
        {
            m_RelativePosition.Value = relativePosition;
        }
        if (m_RelativeRotation.Value != relativeRotation)
        {
            m_RelativeRotation.Value = relativeRotation;
        }
    }

    // 当相对位置发生变化时，更新本地客户端物体的世界位置。
    private void OnRelativePositionChanged(Vector3 previousValue, Vector3 newValue)
    {
        // 仅非服务器端（客户端）执行此逻辑。
        if (IsServer || m_WorldRoot == null)
        {
            return;
        }
        // 使用插值来平滑地更新位置。
        Vector3 targetWorldPosition = m_WorldRoot.TransformPoint(newValue);
        transform.position = Vector3.Lerp(transform.position, targetWorldPosition, Time.fixedDeltaTime * m_PositionLerpSpeed);
    }

    // 当相对旋转发生变化时，更新本地客户端物体的世界旋转。
    private void OnRelativeRotationChanged(Quaternion previousValue, Quaternion newValue)
    {
        // 仅非服务器端（客户端）执行此逻辑。
        if (IsServer || m_WorldRoot == null)
        {
            return;
        }
        // 使用插值来平滑地更新旋转。
        Quaternion targetWorldRotation = m_WorldRoot.rotation * newValue;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetWorldRotation, Time.fixedDeltaTime * m_RotationLerpSpeed);
    }

    // 当 NetworkObject 销毁时，取消订阅事件以防止内存泄漏。
    public override void OnDestroy()
    {
        if (m_RelativePosition != null)
        {
            m_RelativePosition.OnValueChanged -= OnRelativePositionChanged;
        }
        if (m_RelativeRotation != null)
        {
            m_RelativeRotation.OnValueChanged -= OnRelativeRotationChanged;
        }
        base.OnDestroy();
    }
}