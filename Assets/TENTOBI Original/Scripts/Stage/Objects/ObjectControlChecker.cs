// 引用URL: https://qiita.com/JunShimura/items/4547563fbb2691f40626 (2020.10.17)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectControlChecker : MonoBehaviour
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
	#endregion

	public Rigidbody2D rb;


	// Start is called before the first frame update
	void Start()
	{
		game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameDirector>();
		ui = GameObject.FindGameObjectWithTag("UI").GetComponent<UIDirector>();

		isSelected = false;
	}

	// Update is called once per frame
	void Update()
	{
		if (!isSelected) return;

		if (game.IsControlling())
		{
			// ※何らかの操作エフェクト

			return;
		}
		isSelected = false;
		ui.DeactivateController();
	}

	/// <summary>
	/// オブジェクト操作を開始します
	/// </summary>
	public void OnUserAction()
	{
		game.SwitchControlMode(true);
		isSelected = true;
		ui.ActivateController();
	}

	#region 当たり判定
	/// <summary>
	/// 当たり判定の結果を返します
	/// </summary>
	/// <returns>何かに接触しているかどうか</returns>
	public bool IsInCollision()
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
		isTriggerEnter = true;
		Debug.Log("操作中のオブジェクトが障害物に当たりました");
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		isTriggerStay = true;
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		isTriggerExit = true;
		Debug.Log("操作中のオブジェクトが障害物から離れました");
	}
	#endregion
}
