using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class PressTrap : MonoBehaviour
{
    [Header("動きの設定")]
    [Tooltip("移動速度")]
    [SerializeField] private float moveSpeed = 2.0f;

    [Tooltip("下に移動する距離")]
    [SerializeField] private float moveDistance = 3.0f;

    [Header("待機時間の設定")]
    [Tooltip("上で待機する時間（秒）")]
    [SerializeField] private float waitTimeTop = 1.0f;

    [Tooltip("下で待機する時間（秒）")]
    [SerializeField] private float waitTimeBottom = 0.5f;

    [Header("開始時の設定")]
    [Tooltip("ゲーム開始時に最初に待機する時間（ズレを作りたい時に使用）")]
    [SerializeField] private float startDelay = 0f;

    // 内部変数
    private Vector2 initialPosition; // 初期位置
    private Vector2 targetPosition;  // 現在の目指す位置
    private Vector2 bottomPosition;  // 下の到達点
    private Rigidbody2D rb;
    private float timer;
    private bool isMovingDown = true; // 今下がっているか？
    private bool isWaiting = false;   // 今待機中か？

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Rigidbodyの設定漏れ防止
        if (rb != null)
        {
            rb.isKinematic = true; // 物理演算の影響を受けない（重力で落ちない）ようにする
            rb.linearVelocity = Vector2.zero;
        }

        // 位置の計算
        initialPosition = transform.position;
        bottomPosition = initialPosition + (Vector2.down * moveDistance);
        
        // 最初は下を目指す
        targetPosition = bottomPosition;

        // スタート時の遅延設定
        if (startDelay > 0)
        {
            isWaiting = true;
            timer = startDelay;
        }
    }

    void FixedUpdate()
    {
        if (isWaiting)
        {
            HandleWait();
        }
        else
        {
            MoveTrap();
        }
    }

    // 移動処理
    void MoveTrap()
    {
        // 現在位置からターゲットへ向かって移動
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // 目的地に非常に近づいたら（到達したら）
        if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
        {
            StartWait();
        }
    }

    // 待機開始処理
    void StartWait()
    {
        isWaiting = true;
        
        // 到達した場所に応じて次の目標と待機時間を設定
        if (isMovingDown)
        {
            // 下に到着した -> 次は上へ
            timer = waitTimeBottom;
            targetPosition = initialPosition;
            isMovingDown = false;
        }
        else
        {
            // 上に到着した -> 次は下へ
            timer = waitTimeTop;
            targetPosition = bottomPosition;
            isMovingDown = true;
        }
    }

    // 待機中のタイマー処理
    void HandleWait()
    {
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
        {
            isWaiting = false;
        }
    }

    // エディタ上で移動範囲を線で表示する（可視化機能）
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) // ゲーム実行中でない時だけ表示
        {
            Gizmos.color = Color.red;
            Vector3 endPos = transform.position + (Vector3.down * moveDistance);
            Gizmos.DrawLine(transform.position, endPos); // 移動ルートを線で描画
            Gizmos.DrawWireCube(endPos, transform.localScale); // 到達地点の箱を描画
        }
    }
}