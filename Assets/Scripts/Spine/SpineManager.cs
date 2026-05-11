using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpineComponent
{
    public uint SpineAssetID;

    public SkeletonGraphic spineGraphic;
    public SkeletonAnimation spineAnimation;
    public SkeletonRenderer spineRenderer;

    public SkeletonDataAsset skeletonDataAsset;

    public SpineComponent()
    {

    }

    public SpineComponent(SkeletonGraphic spineGraphic, SkeletonAnimation spineAnimation)
    {
        this.spineGraphic = spineGraphic;
        this.spineAnimation = spineAnimation;
    }

    public SpineComponent(uint spineAssetID, SkeletonGraphic spineGraphic, SkeletonAnimation spineAnimation, SkeletonDataAsset skeletonDataAsset)
    {
        this.SpineAssetID = spineAssetID;
        this.spineGraphic = spineGraphic;
        this.spineAnimation = spineAnimation;
        this.skeletonDataAsset = skeletonDataAsset;

        if (this.spineGraphic != null)
        {
            this.spineGraphic.skeletonDataAsset = skeletonDataAsset;
        }
        else
        {
            Debug.LogError("没找到 SkeletonGraphic 组件。。。");
        }

        if (this.spineAnimation == null)
        {
            Debug.LogError("没找到 SkeletonAnimation 组件。。。");
        }
    }

    public void Show()
    {
        this.spineGraphic?.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.spineGraphic?.gameObject.SetActive(false);
    }
}

[System.Serializable]
public class SpineSlotAttachmentInfo
{
    [SerializeField]
    private string m_Label;
    [SerializeField]
    private List<SpineSlotAttachment> m_SpineSlotAttachmentList;

    public string Label => this.m_Label;
    public List<SpineSlotAttachment> SpineSlotAttachmentList => this.m_SpineSlotAttachmentList;
}

[System.Serializable]
public class SpineSlotAttachment
{
    public enum ForceUpdateType
    {
        [InspectorName("(无) None")] None,
        [InspectorName("(隐藏) Hide")] Hide,
        [InspectorName("(显示) Show")] Show,
    }

    public string SpineSlotName => this.m_SpineSlotName;
    public List<string> SpineAttachmentName => this.m_SpineAttachmentName;
    public bool IsVisible => this.m_IsVisible;
    public ForceUpdateType ForceUpdate => this.m_ForceUpdate;

    [SerializeField]
    private string m_SpineSlotName;

    [SerializeField]
    private List<string> m_SpineAttachmentName;

    [SerializeField]
    private bool m_IsVisible = true;

    [SerializeField]
    private ForceUpdateType m_ForceUpdate;
}

public class SpineManager : MonoBehaviour
{
    public static SpineManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 控制特定插槽下的指定附件的显隐。
    /// </summary>
    /// <param name="slotName">插槽名称 (例如: "QUNZI")</param>
    /// <param name="attachmentName">附件名称 (例如: "QUNZI_1")</param>
    /// <param name="isVisible">是否显示该附件</param>
    public void SetSpecificAttachmentVisibility(SpineComponent spineComponent, List<SpineSlotAttachment> spineSlotAttachments)
    {
        if (spineComponent == null)
            return;

        if (spineComponent.spineAnimation == null)
            return;

        if (spineSlotAttachments == null)
            return;

        if (spineSlotAttachments.Count == 0)
            return;

        Skeleton skeleton = null;
        if (spineComponent.spineAnimation == null)
        {
            if (spineComponent.spineRenderer != null)
            {
                skeleton = spineComponent.spineRenderer.Skeleton;
            }
        }
        else
        {
            skeleton = spineComponent.spineAnimation.Skeleton;
        }

        if (skeleton == null)
            return;

        foreach (var ssaItem in spineSlotAttachments)
        {
            if (ssaItem.IsVisible)
            {
                try
                {
                    foreach (var an in ssaItem.SpineAttachmentName)
                    {
                        // 如果要显示 QUNZI_1，就将插槽的当前附件设置为它。
                        // Spine 运行时会自动从当前皮肤或默认皮肤中查找该名称的附件。
                        skeleton.SetAttachment(ssaItem.SpineSlotName, an);
                    }

                }
                catch (ArgumentException e)
                {
                    Debug.LogError($"{ssaItem.SpineSlotName} 在当前Spine动画资源中并不存在");
                }

            }
            else
            {
                // 如果要隐藏 QUNZI_1（或任何当前附件），就将该插槽的附件设置为空 (null)。skeleton.FindSlot(slotName).AppliedPose.Attachment
                // 注意：这个方法会隐藏当前插槽中的所有东西，无论当前显示的是 QUNZI_1 还是 QUNZI_2。
                // 它不会单独隐藏一个“节点”，而是控制“QUNZI”插槽整体显示什么。
                Slot slot = skeleton.FindSlot(ssaItem.SpineSlotName);
                if (slot != null)
                {
                    Attachment attachment = slot.AppliedPose.Attachment;
                    if (attachment != null)
                    {
                        skeleton.SetAttachment(ssaItem.SpineSlotName, null);
                    }
                }
            }
        }
    }
}
