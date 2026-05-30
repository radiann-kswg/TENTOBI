using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTile : MonoBehaviour
{
	private Rigidbody2D rb;

	[Header("風力")]
	/// <summary>
	/// このタイルマップがプレイヤーに印加する風力です
	/// </summary>
	public float addPower = 20.0f;

	[Header("風向き")]
	/// <summary>
	/// このタイルマップがプレイヤーに印加する風力の画面反時計回りを正にしたときの向きです
	/// （画面上方向:0.0, 画面左方向:90.0, 画面下方向:180.0, 画面右方向:-90.0）
	/// </summary>
	public float zAngle = 0.0f; // down:180.0f left:90.0f up:0.0f right:-90.0f

	#region private変数定義
	/// <summary>
	/// 当たり判定の更新用フラグ
	/// Enter:触れ始めたとき, Stay:接し続けているとき, Exit:離れたとき
	/// </summary>
	private bool isInWindEnter, isInWindStay, isInWindExit;

	/// <summary>
	/// GameDirectorコンポーネント
	/// </summary>
	private GameDirector game;
	#endregion

	// Start is called before the first frame update
	void Start()
	{
		game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameDirector>();
		rb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update()
	{

	}

	private void FixedUpdate()
	{
		// ポーズ中は加速しないようにする
		if (game.ReturnPauseFlag()) return;
		// このタイルマップに接している場合
		if (IsInWind())
		{
			// 風力を印可（風力(Vector2)はCalculateForceAndAngle関数で計算される）
			rb.AddForce(CalculateForceAndAngle(), ForceMode2D.Force);

			Debug.Log("風の影響で加速(Z回転角:" + zAngle.ToString() + "度, 加速度:+" + addPower.ToString() + ")しています");
		}
	}

	#region 風力計算
	private Vector2 CalculateForceAndAngle()
	{
		return addPower * new Vector2(-Mathf.Sin(Mathf.Deg2Rad * zAngle), Mathf.Cos(Mathf.Deg2Rad * zAngle));
	}

	#endregion

	#region 当たり判定
	/// <summary>
	/// 風(Wind)と接触しているかを返します
	/// </summary>
	private bool IsInWind()
	{
		bool isInWind = false;
		if (isInWindEnter || isInWindStay) isInWind = true;
		else if (isInWindExit) isInWind = false;

		isInWindEnter = false;
		isInWindStay = false;
		isInWindExit = false;
		return isInWind;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			isInWindEnter = true;
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			isInWindStay = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			isInWindExit = true;
		}
	}
	#endregion
}
