using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicJoystickHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private UltimateJoystick joystick;
    // 拖入你的 Ultimate Joystick 游戏对象
    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private RectTransform joystickRect;

    void Awake()
    {
        // 初始状态隐藏
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        joystickRect.position = eventData.position;
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        joystick.OnPointerDown(eventData);

        if (!SWGameManager.Instance.IsInit)
        {
            SWGameManager.Instance.IsInit = true;
            SWGameManager.Instance.IsWin = false;
            SWGameManager.Instance.OnGameStartEvent.Send();
            AudioManager.Instance.PlayBGM();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 必须实现这个接口，OnPointerDown 之后的拖拽才能持续传递
        joystick.OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 手动通知插件抬起
        joystick.OnPointerUp(eventData);

        // 隐藏摇杆
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }
}
