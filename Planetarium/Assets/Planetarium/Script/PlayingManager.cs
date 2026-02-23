using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

/// <summary>
/// 今のサウンド終了後、次のオブジェクトのサウンドを再生する
/// </summary>
public class PlayingManager : MonoBehaviour
{
    [Header("最初の説明"), SerializeField]
    AudioSource m_StartAudio;

    [Header("終了の説明"), SerializeField]
    AudioSource m_EndAudio;

    [Header("SetActiveManagerをオブジェクトアタッチ"), SerializeField]
    SetActiveManager m_SAM;
    [Header("フェードの処理時間"), SerializeField]
    float m_FadeDuration = 1.5f;

    [Header("最小音量（背中を向けている時）"), SerializeField]
    float m_MinVolume = 0.2f;

    [Header("最大音量（正面を向いている時）"), SerializeField]
    float m_MaxVolume = 1.0f;

    Transform m_CameraTransform;

    List<GameObject> m_StarObj = new List<GameObject>();
    List<GameObject> m_StarImage = new List<GameObject>();
    AudioSource m_AS;
    int m_ID = 0;
    CancellationTokenSource m_Cts;
    bool m_IsSkipped = false;

    private void Start()
    {
        m_Cts = new CancellationTokenSource();
        // メインカメラのTransformを取得（VR環境の頭の位置・向きとして利用）
        if (Camera.main != null)
        {
            m_CameraTransform = Camera.main.transform;
        }

        //発動しなかったとき対策
        foreach (Transform obj in m_SAM.m_Parent)
        {
            m_StarObj.Add(obj.gameObject);
            obj.gameObject.SetActive(false); // 初期は非表示
        }
        foreach (Transform obj in m_SAM.m_ParentImage)
        {
            m_StarImage.Add(obj.gameObject);
            obj.gameObject.SetActive(false); // 初期は非表示
        }

        m_StartAudio.Play();
        StartAudio().Forget();
    }

    private void Update()
    {
        // Pキーでデバッグスキップ
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame && !m_IsSkipped)
        {
            SkipAll();
        }

        // --- Y軸（水平面）の向きによる音量調整 ---
        // 再生中の音声があり、かつ目標の星座オブジェクトが存在する場合
        if (m_AS != null && m_AS.isPlaying && m_CameraTransform != null && m_ID < m_StarObj.Count)
        {
            GameObject currentTarget = m_StarObj[m_ID];
            
            // カメラとターゲットの水平面(XZ平面)上の方向ベクトルを計算 (Y座標を0にして高さを完全に無視)
            Vector3 targetPosXZ = new Vector3(currentTarget.transform.position.x, 0f, currentTarget.transform.position.z);
            Vector3 cameraPosXZ = new Vector3(m_CameraTransform.position.x, 0f, m_CameraTransform.position.z);
            
            // カメラからターゲットへの水平方向ベクトル（長さを1に正規化）
            Vector3 dirToTargetXZ = (targetPosXZ - cameraPosXZ).normalized;
            
            // カメラの正面方向の水平ベクトル（長さを1に正規化）
            Vector3 cameraForwardXZ = new Vector3(m_CameraTransform.forward.x, 0f, m_CameraTransform.forward.z).normalized;
            
            // Y軸回転(水平面)上での内積を計算
            // 1.0: 完全に同じ方向(正面) / 0.0: 真横 / -1.0: 完全に後ろ
            float dot = Vector3.Dot(cameraForwardXZ, dirToTargetXZ);
            
            // 内積 (-1.0 ～ 1.0) を (0.0 ～ 1.0) の割合に変換
            float targetPercentage = (dot + 1f) / 2f;
            
            // 割合に応じて音量をリアルタイムに設定 (m_MinVolume ～ m_MaxVolume)
            m_AS.volume = Mathf.Lerp(m_MinVolume, m_MaxVolume, targetPercentage);
        }
    }

    /// <summary>
    /// 全音声停止・全星座即時表示・終了あいさつ再生
    /// </summary>
    private void SkipAll()
    {
        m_IsSkipped = true;

        // 1. 非同期処理をキャンセル
        m_Cts?.Cancel();

        // 2. 再生中の全音声を停止
        m_StartAudio.Stop();
        if (m_AS != null) m_AS.Stop();

        // 3. 全星座オブジェクトを即座にアルファ1.0で表示
        for (int i = 0; i < m_StarObj.Count; i++)
        {
            m_StarObj[i].SetActive(true);
            SetAlpha(m_StarObj[i], 1f);
        }
        for (int i = 0; i < m_StarImage.Count; i++)
        {
            m_StarImage[i].SetActive(true);
            SetAlpha(m_StarImage[i], 1f);
        }

        // 4. 終了あいさつを再生
        m_EndAudio.Play();

        Debug.Log("デバッグスキップ: 全星座表示完了");
    }

    async UniTask StartAudio()
    {
        await UniTask.WaitWhile(() => m_StartAudio.isPlaying, cancellationToken: m_Cts.Token);
        Debug.Log("始まりのあいさつ終了");
        await StartStarAsync();
    }

    /// <summary>
    /// 星座の説明処理
    /// </summary>
    async UniTask StartStarAsync()
    {
        if (m_StarObj.Count == m_ID)
        {
            Debug.Log("〆のあいさつ");
            m_EndAudio.Play();
            return;
        }

        GameObject currentStarObj = m_StarObj[m_ID];
        GameObject currentStarImage = m_StarImage[m_ID];

        //オブジェクトを表示し、透明度を0に設定
        //(ここで画像は画面に表示されるが、透明で目に見えない状態になる)
        currentStarObj.SetActive(true);
        currentStarImage.SetActive(true);
        SetAlpha(currentStarObj, 0f);
        SetAlpha(currentStarImage, 0f);

        //オーディオ再生と待機
        m_AS = currentStarObj.GetComponent<AudioSource>();
        
        // 3Dサウンド設定（左右のパンニングによる立体感のみ残す）
        m_AS.spatialBlend = 1.0f; 
        
        // 距離による減衰を完全に無効化する
        m_AS.rolloffMode = AudioRolloffMode.Linear; 
        m_AS.minDistance = 100000f;  
        m_AS.maxDistance = 100000f; 
        
        m_AS.Play();

        //音声再生が終了するまで待機
        await UniTask.WaitWhile(() => m_AS.isPlaying, cancellationToken: m_Cts.Token);

        Debug.Log($"{currentStarObj.name} の説明終了。画像フェードイン開始。");

        //フェードイン処理
        await UniTask.WhenAll(
            Fade(currentStarObj, m_FadeDuration, targetAlpha: 1f, m_Cts.Token),
            Fade(currentStarImage, m_FadeDuration, targetAlpha: 1f, m_Cts.Token)
        );

        m_ID++;
        await StartStarAsync();
    }

    /// <summary>
    /// SpriteRenderer/Imageの透明度を瞬時に設定する補助メソッド
    /// </summary>
    private void SetAlpha(GameObject targetObj, float alpha)
    {
        if (targetObj.TryGetComponent(out SpriteRenderer sr))
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
        }
        if (targetObj.TryGetComponent(out Image img))
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
        }
    }

    /// <summary>
    /// 透明度の変更 (汎用メソッド)
    /// targetAlpha が 1f ならフェードイン、0f ならフェードアウトとして機能します。
    /// </summary>
    async UniTask Fade(GameObject targetObj, float duration, float targetAlpha, CancellationToken cancellationToken = default)
    {
        // 処理対象のコンポーネントと現在の色を取得
        Color currentColor;
        float startAlpha;
        SpriteRenderer sr = null;
        Image img = null;

        if (targetObj.TryGetComponent(out sr))
        {
            currentColor = sr.color; 
            startAlpha = currentColor.a; 
        }
        else if (targetObj.TryGetComponent(out img)) 
        {
            currentColor = img.color; 
            startAlpha = currentColor.a; 
        }
        else 
        { 
            return; 
        }

        //フェードアウト完了時以外は SetActive(true) のまま
        if (targetAlpha > 0f && !targetObj.activeSelf)
        {
            targetObj.SetActive(true);
        }

        float startTime = Time.time;
        float timer = 0f;

        while (timer < duration)
        {
            timer = Time.time - startTime;
            float t = Mathf.Clamp01(timer / duration);

            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            // 透明度を更新
            Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            if (sr != null) sr.color = newColor;
            if (img != null) img.color = newColor;

            // 1フレーム待機
            await UniTask.Yield(cancellationToken);
        }

        //処理終了後に完全に目標透明度に設定
        Color finalColor = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
        if (sr != null) sr.color = finalColor;
        if (img != null) img.color = finalColor;

        // targetAlphaが0f (フェードアウト完了) の場合のみ、SetActive(false)
        if (targetAlpha < 0.01f)
        {
            targetObj.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        m_Cts?.Cancel();
        m_Cts?.Dispose();
    }
}

