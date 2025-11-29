using NUnit.Framework;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class PlayingManager : MonoBehaviour
{
    [SerializeField]
    [Header("最初の説明")]
    AudioSource m_StartAudio;

    [SerializeField]
    [Header("終了の説明")]
    AudioSource m_EndAudio;

    [SerializeField]
    SetActiveManager m_SAM;

    List<GameObject> m_StarObj=new List<GameObject>();

    //説明中のオーディオ
    AudioSource m_AS;

    //現在のリストナンバー
    int m_ID=0;
    private void Start()
    {
        foreach(GameObject obj in m_SAM.m_Parent)
        {
            m_StarObj.Add(obj);
        }
        m_StartAudio.Play();
        StartCoroutine(StartAudio());
    }

    /// <summary>
    /// 説明
    /// </summary>
    /// <returns></returns>
    IEnumerator StartAudio()
    {
        //再生終了したらスタート
        yield return new WaitWhile(() => m_StartAudio.isPlaying);

        StartCoroutine(StartStar());

        //StartCoroutine(StartCapricorn());
    }

    /// <summary>
    /// 星々の説明
    /// </summary>
    /// <returns></returns>
    IEnumerator StartStar()
    {
        //ID番号の表示
        m_StarObj[m_ID].SetActive(true);

        //ID番号のオーディオ取得
        m_AS = m_StarObj[m_ID].GetComponent<AudioSource>();
        //再生
        m_AS.Play();

        //再生終了したらスタート
        yield return new WaitWhile(() => m_AS.isPlaying);
        m_AS.Stop();

        //リストの中身とIDが同じになったら終了サウンドに移行
        if(m_StarObj.Count==m_ID)
        {
            m_EndAudio.Play();
            yield break;
        }

        m_ID++;

        StartCoroutine(StartStar());
    }
}
