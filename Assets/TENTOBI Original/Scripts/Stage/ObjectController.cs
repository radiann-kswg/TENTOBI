// 引用URL: https://qiita.com/JunShimura/items/4547563fbb2691f40626 (2020.10.17)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectController : MonoBehaviour, IPointerClickHandler
{
    #region private変数定義
    /// <summary>
    /// GameDirectorコンポーネント
    /// </summary>
    private GameDirector game;

    /// <summary>
    /// UIDirectorコンポーネント
    /// </summary>
    private UIDirector ui;

    private Rigidbody2D rigidbody;

    /// <summary>
    /// 操作速度
    /// </summary>
    private const float controlSpeed = 4.0f;
    /// <summary>
    /// 操作量ごとの消費CP(操作力)
    /// </summary>
    private const float costCPPerInput = 0.025f;
    /// <summary>
    /// 当たり判定の閾値
    /// </summary>
    private const float collisionThreshold = 0.15f;

    /// <summary>
    /// このオブジェクトが操作時に選択されているか
    /// </summary>
    private bool isSelected;
    /// <summary>
    /// このオブジェクトが操作中に接触したかどうか
    /// </summary>
    private bool isInCollision;
    /// <summary>
    /// 当たり判定の更新用フラグ
    /// Enter:触れ始めたとき, Stay:接し続けているとき, Exit:離れたとき
    /// </summary>
    private bool isTriggerEnter, isTriggerStay, isTriggerExit;

    /// <summary>
    /// 操作コスト
    /// </summary>
    private float ctrlCost;
    /// <summary>
    /// 位置誤差
    /// </summary>
    private Vector2 diff;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameDirector>();
        rigidbody = this.gameObject.GetComponent<Rigidbody2D>();
        ui = GameObject.FindGameObjectWithTag("UI").GetComponent<UIDirector>();

        isSelected = false;
        isInCollision = false;

        ctrlCost = 0.0f;
        diff = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
        {
            if (!game.IsControlling())
            {
                isSelected = false;
                ui.DeactivateController();
            }else if (game.CP > 0.0f)
            {
                game.CP -= ctrlCost;
            }
            else
            {
                //＊何らかの効果音

                FinishControl();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isSelected && game.IsControlling())
        {
            if (game.CP > 0.0f)
            {
                Vector2 input = new Vector2(SimpleInput.GetAxis("Horizontal"), SimpleInput.GetAxis("Vertical"));
                input *= controlSpeed;
                if (!Mathf.Approximately(input.x, 0.0f) && !Mathf.Approximately(input.y, 0.0f))
                {
                    if (IsInCollision())
                    {
                        if (diff.x > collisionThreshold && input.x > 0.0f) input.x = 0.0f;
                        else if (diff.x < -collisionThreshold && input.x < 0.0f) input.x = 0.0f;
                        if (diff.y > collisionThreshold && input.y > 0.0f) input.y = 0.0f;
                        else if (diff.y < -collisionThreshold && input.y < 0.0f) input.y = 0.0f;
                    }
                    rigidbody.linearVelocity = input;
                    ctrlCost = input.magnitude / controlSpeed * costCPPerInput;
                }
                else
                {
                    rigidbody.linearVelocity = Vector2.zero;
                    ctrlCost = 0.0f;
                }
                rigidbody.angularVelocity = 0.0f;
            }
        }
    }

    /// <summary>
    /// 当たり判定の結果
    /// </summary>
    /// <returns>触れているときにtrueを返します</returns>
    private bool IsInCollision()
    {
        if (isTriggerEnter || isTriggerStay) isInCollision = true;
        else if (isTriggerExit) isInCollision = false;

        isTriggerEnter = false;
        isTriggerStay = false;
        isTriggerExit = false;
        return isInCollision;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isSelected && game.IsControlling())
        {
            isTriggerEnter = true;
            Vector2 contact = collision.ClosestPoint(this.transform.position);
            Vector2 thisPositon = new Vector2(this.transform.position.x, this.transform.position.y);
            diff = contact - thisPositon;
            Debug.Log("操作中のオブジェクトが障害物に当たりました(向き：x=" + diff.x.ToString() + ", y=" + diff.y.ToString() + ")");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isSelected && game.IsControlling())
        {
            isTriggerStay = true;
            Vector2 contact = collision.ClosestPoint(this.transform.position);
            Vector2 thisPositon = new Vector2(this.transform.position.x, this.transform.position.y);
            diff = contact - thisPositon;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isSelected && game.IsControlling())
        {
            isTriggerExit = true;
            diff = Vector2.zero;
            Debug.Log("操作中のオブジェクトが障害物から離れました");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (game.IsPausing())
        {
            game.SwitchPauseMode(false);
            game.SwitchControlMode(true);
            ui.DeactivatePauseButton();
            ui.UpdateStatusText("-Controling-");
            isSelected = true;
            ui.ActivateController();
        }
    }

    /// <summary>
    /// オブジェクト操作を終了します
    /// </summary>
    private void FinishControl()
    {
        ui.OnPressActButton();
    }
}
