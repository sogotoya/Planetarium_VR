using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    [Header("自身のオブジェクト"), SerializeField]
    GameObject m_FadeObj;

    [Header("自身のスプライト"),SerializeField]
    Image m_FadeImage;

    [Header("フェードインするまでの指定時間"),SerializeField]
    float m_Time = 225f;

    [Header("加算変数"),SerializeField]
    float m_Timer = 0f;

    private void Start()
    {
        //取得
        m_FadeObj = this.gameObject;
        m_FadeImage = m_FadeObj.GetComponent<Image>();

        //透明度の変更
        m_FadeImage.color = new Color(m_FadeImage.color.r, m_FadeImage.color.g, m_FadeImage.color.b, 0);
    }

    private void Update()
    {
        StartCoroutine(FadeInCoroutine());
    }

    /// <summary>
    /// フェードインの処理
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeInCoroutine()
    {
        //
        if (m_Timer == 0) m_FadeImage.color += new Color(0, 0, 0, 0);

        if (m_Time > m_Timer)
        {
            //透明度の加算
            m_FadeImage.color += new Color(0, 0, 0, 1);

            //時間の加算
            m_Timer++;

            //時間停止
            yield return new WaitForSeconds(0.01f);
        }
    }
}
