using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform m_Player;


    void Update()
    {
        //ƒvƒŒƒCƒ„[‚Ì•ûŒüæ“¾
        Vector3 vPos=m_Player.position-transform.position;

        //Y²0
        vPos.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(vPos, Vector3.up);


        transform.rotation = targetRotation;
    }
}
