using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField]
    private GameOverUI m_BgUI_L, m_BgUI_P;

    private GameOverUI m_CurrentBGUI;

    public void Show(bool isWin)
    {
        this.gameObject.SetActive(true);
        this.m_CurrentBGUI.Show(isWin);
    }


    public void ScreenDirChangedEvent(bool landspace)
    {
        this.m_BgUI_L.gameObject.SetActive(landspace);
        this.m_BgUI_P.gameObject.SetActive(!landspace);
        this.m_CurrentBGUI = landspace ? this.m_BgUI_L : this.m_BgUI_P;
    }

    public void DoUpdate()
    {
        this.m_CurrentBGUI.DoUpdate();
    }
}