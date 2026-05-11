using Cysharp.Threading.Tasks.Triggers;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineSkinSwitcher : MonoBehaviour
{
    [SerializeField]
    private SkeletonMecanim skeletonMecanim;

    [SerializeField]
    private SpineAnimationEvent m_Skin_A, m_Skin_B, m_Skin_C;

    public void ChangeSkin(string skinName)
    {
        if (!string.IsNullOrEmpty(skinName))
        {
            var skeleton = skeletonMecanim.Skeleton;
            // 1. 设置皮肤
            skeleton.SetSkin(skinName);
            // 2. 关键：Mecanim模式下也需要重置槽位以刷新显示
            skeleton.SetupPose();
        }
    }

    public SpineAnimationEvent ChangeSkinObject(string skinName)
    {
        SpineAnimationEvent sae = null;
        if (!string.IsNullOrEmpty(skinName))
        {
            this.m_Skin_A.gameObject.SetActive(false);
            this.m_Skin_B.gameObject.SetActive(false);
            this.m_Skin_C.gameObject.SetActive(false);

            switch (skinName)
            {
                case "skin_A":
                    this.m_Skin_A.gameObject.SetActive(true);
                    sae = this.m_Skin_A;
                    break;
                case "skin_B":
                    this.m_Skin_B.gameObject.SetActive(true);
                    sae = this.m_Skin_B;
                    break;
                case "skin_C":
                    this.m_Skin_C.gameObject.SetActive(true);
                    sae = this.m_Skin_C;
                    break;
            }
        }

        return sae;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (skeletonMecanim == null) this.skeletonMecanim = this.GetComponent<SkeletonMecanim>();
    }
#endif
}