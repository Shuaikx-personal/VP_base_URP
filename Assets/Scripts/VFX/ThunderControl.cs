using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.VFX;

public class ThunderControl : BaseAffordanceControl
{
    public VisualEffect localThunder;
    public VisualEffect netCenterThunder;
    public VisualEffect netLeftThunder;
    public VisualEffect netRightThunder;
    public ParentConstraint localLeftIndexParent;
    public ParentConstraint localRightIndexParent;
    public ParentConstraint netLeftIndexParent;
    public ParentConstraint netRightIndexParent;

    //public ParentConstraint localLeftIndexParent;

    public NetworkHandsRelationManager[] playerManagers;

    protected override void OnEnable()
    {
        playerManagers = FindObjectsOfType<NetworkHandsRelationManager>();

        if (playerManagers.Length == 1)
        {
            // localLeftIndex = playerManagers[0].leftIndexTip;
            // localRightIndex = playerManagers[0].rightIndexTip;
            ConstraintSource source = new ConstraintSource();
            source.weight = 1;

            if (localLeftIndexParent.sourceCount == 0)
            {
                source.sourceTransform = playerManagers[0].leftIndexTip;
                localLeftIndexParent.AddSource(source);
            }
            if (localRightIndexParent.sourceCount == 0)
            {
                source.sourceTransform = playerManagers[0].rightIndexTip;
                localRightIndexParent.AddSource(source);
            }

            localThunder.enabled = true;
        }
        else if (playerManagers.Length >= 2)
        {
            NetworkHandsRelationManager hostManager = playerManagers[0].IsHost ? playerManagers[0] : playerManagers[1];
            NetworkHandsRelationManager clientManager = playerManagers[0].IsHost ? playerManagers[1] : playerManagers[0];
            // localLeftIndex = hostManager.leftIndexTip;
            // localRightIndex = hostManager.rightIndexTip;
            // netLeftIndex = clientManager.leftIndexTip;
            // netRightIndex = clientManager.rightIndexTip;
            ConstraintSource source = new ConstraintSource();
            source.weight = 1;
            if (localLeftIndexParent.sourceCount == 0)
            {
                source.sourceTransform = hostManager.leftIndexTip;
                localLeftIndexParent.AddSource(source);
            }
            if (localRightIndexParent.sourceCount == 0)
            {
                source.sourceTransform = hostManager.rightIndexTip;
                localRightIndexParent.AddSource(source);
            }
            if (netLeftIndexParent.sourceCount == 0)
            {
                source.sourceTransform = clientManager.leftIndexTip;
                netLeftIndexParent.AddSource(source);
            }
            if (netRightIndexParent.sourceCount == 0)
            {
                source.sourceTransform = clientManager.rightIndexTip;
                netRightIndexParent.AddSource(source);
            }
            localThunder.enabled = true;
            netCenterThunder.enabled = true;
            netLeftThunder.enabled = true;
            netRightThunder.enabled = true;
        }
        else
        {
            return;
        }
        
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        localThunder.enabled = false;
        netCenterThunder.enabled = false;
        netLeftThunder.enabled = false;
        netRightThunder.enabled = false;
        base.OnDisable();
    }


    public override void OnTipRelationDataUpdated(float distance, Vector3 centerPosition)
    {
        
    }

    public override void OnCameraRelationDataUpdated(float distance, Vector3 centerPosition)
    {
        return;
    }
    
}