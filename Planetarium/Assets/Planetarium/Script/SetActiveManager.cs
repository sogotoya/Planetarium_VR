using UnityEngine;

public class SetActiveManager : MonoBehaviour
{

    [Tooltip("星座をまとめている親オブジェクト")]
    public Transform m_Parent;

    [Tooltip("いて座")]
    public GameObject m_Sagittarius;

    [Tooltip("やぎ座")]
    public GameObject m_Capricorn;

    [Tooltip("みずがめ座")]
    public GameObject m_Aquarius;

    [Tooltip("うお座")]
    public GameObject m_Pisces;

    [Tooltip("しし座")]
    public GameObject m_Leo;

    [Tooltip("おとめ座")]
    public GameObject m_Virgo;

    [Tooltip("てんびん座")]
    public GameObject m_Libra;

    [Tooltip("さそり座")]
    public GameObject m_Scorpius;


    private void Start()
    {
        //オブジェクト一括非表示
        foreach (Transform child in m_Parent)
        {
            child.gameObject.SetActive(false);
        }
    }
}
