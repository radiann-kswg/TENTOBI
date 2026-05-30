using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingBar : MonoBehaviour
{
    #region private変数定義
    /// <summary>
    /// GameDirectorコンポーネント
    /// </summary>
    private GameDirector game;
    #endregion

    [Header("回転速度")]
    /// <summary>
    /// この回転バーの回転速度です
    /// </summary>
    public float angleVelocity = 60.0f;
    [Header("時計回りが正方向どうか")]
    /// <summary>
    /// この回転バーの回転速度を時計回り正で指定している場合、trueを指定してください
    /// falseを指定すると、回転速度が反時計回りを正として計算されます
    /// </summary>
    public bool clockwize = false;

    // Start is called before the first frame update
    void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameDirector>();
    }

    // Update is called once per frame
    void Update()
    {
        // ポーズ中でない場合
        if (!game.ReturnPauseFlag())
        {
            // 回転処理（1指令ごとの回転量(float)はCalculateRotateAngle関数で計算される）
            this.gameObject.transform.Rotate(0.0f, 0.0f, CalculateRotateAngle());
        }
    }

    #region 回転量計算
    /// <summary>
    /// フレーム単位にし方向設定を反映した回転角を返します
    /// </summary>
    /// <returns>Update関数内でRotate関数からZ軸回転入力をするための引数z</returns>
    private float CalculateRotateAngle()
    {
        float th = angleVelocity * Time.deltaTime;
        if (clockwize) th = -th;
        return th;
    }
    #endregion
}
