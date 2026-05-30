using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    #region private変数定義
    /// <summary>
    /// GameDirectorコンポーネント
    /// </summary>
    private GameDirector game;
    
    private Animator anim;
    #endregion

    [Header("リスポーン地点の番号")]
    /// <summary>
    /// このリスポーン地点の番号です
    /// 必ず0より大きい数で指定してください
    /// </summary>
    public int pointIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameDirector>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pointIndex != game.respawnNum) anim.SetBool("isAccessed", false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            //＊何らかのアニメーション
            
            Debug.Log("リスポーン地点(" + pointIndex.ToString() + ")に到達しました");
            game.respawnNum = pointIndex;
            anim.SetBool("isAccessed", true);
        }
    }
}
