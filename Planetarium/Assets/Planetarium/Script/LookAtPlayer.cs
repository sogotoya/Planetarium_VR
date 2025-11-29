using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform m_Player;


    void Update()
    {
        transform.LookAt(m_Player);
    }
}
