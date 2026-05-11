using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public UltimateJoystick joystick; // 拖入场景中的摇杆

    [SerializeField]
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [SerializeField]
    private Player m_Player;

    [SerializeField]
    private SkeletonMecanim m_SkeletonMecanim;

    [SerializeField]
    private LayerMask m_MoveLayerMask;

    [SerializeField] private float checkDistance = 0.2f; // 稍微调大一点，防止嵌入墙体

    private float m_originalMoveSpeed;

    private void Awake()
    {
        this.m_originalMoveSpeed = this.moveSpeed;
    }

    public void Init()
    {


        if (this.m_SkeletonMecanim != null)
        {
            this.m_SkeletonMecanim.Skeleton.SetupPose();
            this.m_SkeletonMecanim.Skeleton.SetupPoseSlots();
        }

        foreach (var item in this.m_Player.SpineAnimationEvents)
        {
            item.Animator.SetFloat("Speed", 0);
            item.Animator.SetFloat("X", 0);
            item.Animator.SetFloat("Y", 0);
            item.Animator.SetBool("IsDead", true);
            item.Animator.Play("BlendMotion", 0, 0f);
            item.Animator.ResetTrigger("Dead");
            item.Animator.Update(0);
            item.Animator.enabled = false;
            item.Animator.enabled = true;
        }



    }


    public void OnMove()
    {
        // 获取摇杆输入
        moveInput.x = joystick.GetHorizontalAxis();
        moveInput.y = joystick.GetVerticalAxis();

        int x = 0;
        if (moveInput.x > 0.01f)
        {
            x = 1;
        }
        else if (moveInput.x < -0.01f)
        {
            x = -1;
        }

        int y = 0;
        if (moveInput.y > 0.01f)
        {
            y = 1;
        }
        else if (moveInput.y < -0.01f)
        {
            y = -1;
        }

        float speedModifier = 1.0f;
        this.moveSpeed = this.m_originalMoveSpeed;
        AnimatorStateInfo stateInfo = this.m_Player.CurrentSpineAnimationEvent.Animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("attack_01") || stateInfo.IsName("attack_02"))
        {
            float progress = stateInfo.normalizedTime % 1f;

            // 映射速度系数：动画刚开始(0)速度为 1.0，接近结束(1)速度变为 0.2
            // 你可以根据手感调整 0.2f 这个数值
            if (progress > 0.2f && progress < 0.6f)
            {
                speedModifier = Mathf.SmoothStep(1.0f, 0.3f, progress + .2f);
            }

        }

        Vector2 finalMovement = moveInput * speedModifier;
        moveSpeed *= speedModifier;

        this.m_Player.CurrentSpineAnimationEvent.Animator.SetFloat("X", x);
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetFloat("Y", y);
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetFloat("Speed", finalMovement.magnitude);

        //float checkDistance = 0.1f;
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, moveInput, checkDistance, this.m_MoveLayerMask);

        //if (hit.collider == null)
        //{
        //    if (moveInput.magnitude > .1f)
        //        transform.Translate(moveSpeed * Time.deltaTime * moveInput, Space.World);
        //    //rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * moveInput);
        //}
        this.MoveReycast(finalMovement);


        if (this.m_Player.PlayerAutoAttack.CurrentTarget != null)
        {
            transform.localScale = new Vector3(this.m_Player.PlayerAutoAttack.CurrentTarget.position.x > this.transform.position.x ? -1 : 1, 1, 1);
        }
        else
        {
            if (moveInput.x != 0)
            {
                transform.localScale = new Vector3(moveInput.x > 0 ? -1 : 1, 1, 1);
            }
        }
    }

    void MoveReycast(Vector2 moveInput)
    {
        if (moveInput.magnitude < 0.01f) return;

        Vector2 movement = moveInput.normalized * moveSpeed * Time.deltaTime;

        // 1. 尝试直接移动
        RaycastHit2D hit = Physics2D.Raycast(transform.position, movement.normalized, movement.magnitude + checkDistance, m_MoveLayerMask);

        if (hit.collider == null)
        {
            // 前方没障碍，直接走
            transform.Translate(movement, Space.World);
        }
        else
        {
            // 2. 撞墙了，计算滑动方向
            // 获取墙面的法线
            Vector2 normal = hit.normal;

            // 计算投影：从原始移动向量中减去垂直于墙的部分，得到平行的部分
            // 公式：slideMovement = original - (original dot normal) * normal
            Vector2 slideMovement = movement - Vector2.Dot(movement, normal) * normal;

            // 3. 对滑动方向再做一次射线检测，防止滑进死角
            if (slideMovement.magnitude > 0.001f)
            {
                RaycastHit2D slideHit = Physics2D.Raycast(transform.position, slideMovement.normalized, slideMovement.magnitude + checkDistance, m_MoveLayerMask);
                if (slideHit.collider == null)
                {
                    transform.Translate(slideMovement, Space.World);
                }
            }
        }
    }
}
