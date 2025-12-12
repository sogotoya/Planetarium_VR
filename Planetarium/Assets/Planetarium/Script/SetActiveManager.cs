using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

/// <summary>
/// プロジェクト起動時星座画像とオーディオ、画像仮想をまとめているそれぞれを非表示に設定
/// </summary>
public class SetActiveManager : MonoBehaviour
{

    [Header("星座の画像とオーディオまとめている親オブジェクト")]
    public Transform m_Parent;

    [Header("星座の画像仮想をまとめている親オブジェクト")]
    public Transform m_ParentImage;

    private void Start()
    {
        //オブジェクト一括非表示
        foreach (Transform child in m_Parent)
        {
            child.gameObject.SetActive(false);
        }
        //オブジェクト一括非表示
        foreach (Transform childImage in m_ParentImage)
        {
            SpriteRenderer image = childImage.GetComponent<SpriteRenderer>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
        }
    }
}
