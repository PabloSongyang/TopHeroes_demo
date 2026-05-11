using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineBonesOverride : MonoBehaviour
{
    private SkeletonMecanim skeletonMecanim;
    private Skeleton skeleton;

    // 在这里填入你不想被攻击动画影响的骨骼名字（通常是腿部和根部）
    // 你可以在 Spine 的层级树里看到这些名字
    public string[] bonesToIgnore = { "hip", "leg_l", "leg_r", "foot_l", "foot_r" };

    private Bone[] targetBones;

    void Start()
    {
        skeletonMecanim = GetComponent<SkeletonMecanim>();
        skeleton = skeletonMecanim.Skeleton;

        // 预先缓存骨骼对象，提高性能
        targetBones = new Bone[bonesToIgnore.Length];
        for (int i = 0; i < bonesToIgnore.Length; i++)
        {
            targetBones[i] = skeleton.FindBone(bonesToIgnore[i]);
        }
    }

    // 关键点：使用 LateUpdate 确保在 Animator 计算完之后执行
    void LateUpdate()
    {
        if (skeletonMecanim == null || targetBones == null) return;

        // 如果你当前的攻击层权重很高，且你处于移动状态
        // 我们可以根据你的逻辑判断何时开启“骨骼锁定”
        // 这里演示最直接的方法：强制让这些骨骼在渲染时不被第二层 Layer 完全覆盖

        // 注意：SkeletonMecanim 每一帧都会根据 Animator 的结果覆盖骨骼
        // 如果想实现完美融合，其实在 Spine 软件里删掉攻击动画的腿部关键帧是最佳的。
        // 代码层面的“抢回控制权”通常用于处理那些被 Animator 强行 Reset 的骨骼。
    }
}
