using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagePointDirector : MonoBehaviour
{
	#region private変数定義
	/// <summary>
	/// GameDirectorコンポーネント
	/// </summary>
	private GameDirector game;

	/// <summary>
	/// プレイヤー(GameObject)
	/// </summary>
	private GameObject player;
	#endregion

	[Header("スタート地点とリスポーン地点(GameObject)")]
	/// <summary>
	/// elements 0にスタート地点、elements 1以降にリスポーン地点のオブジェクトを参照してください
	/// </summary>
	public GameObject[] point;

	[Header("重力操作などで動くオブジェクト(GameObject)")]
	/// <summary>
	/// 重力操作で動くオブジェクトを参照してください
	/// </summary>
	public GameObject[] movingObject;

	[Header("重力操作などで動くオブジェクトの初期位置を示すGameObject")]
	/// <summary>
	/// 重力操作などで動くオブジェクトがリスポーン時などに再配置される配置場所を示すオブジェクトです
	/// 必ずmovingObject(重力操作で動くオブジェクト)の配列添え字に対応させてください
	/// </summary>
	public GameObject[] movingObject_defaultPoint;

	// Start is called before the first frame update
	void Start()
	{
		game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameDirector>();
		player = GameObject.FindGameObjectWithTag("Player");
	}

	// Update is called once per frame
	void Update()
	{

	}

	/// <summary>
	/// プレイヤーをリスポーンさせ、指定されたオブジェクトを元の位置に戻します
	/// </summary>
	public void RespawnAndResetObjectsPositions()
	{
		int p = game.respawnNum;
		if (p < point.Length && point.Length > 0)
		{
			player.transform.position = point[p].transform.position;
			player.transform.rotation = point[p].transform.rotation;
			player.GetComponent<Rigidbody2D>().linearVelocity
				= player.GetComponent<GravityController>().velocity = Vector2.zero;
			player.GetComponent<Rigidbody2D>().angularVelocity
				= player.GetComponent<GravityController>().angularVelocity = 0.0f;
		}
		else
		{
			Debug.Log("スタート地点またはリスポーン地点の引数エラーです");
		}
		int n = 0;
		if (movingObject_defaultPoint.Length < movingObject.Length)
			n = movingObject_defaultPoint.Length;
		else n = movingObject.Length;
		if (n > 0)
		{
			int i;
			for (i = 0; i < n; ++i)
			{
				movingObject[i].transform.position = movingObject_defaultPoint[i].transform.position;
				movingObject[i].transform.rotation = movingObject_defaultPoint[i].transform.rotation;
				movingObject[i].GetComponent<Rigidbody2D>().linearVelocity
					= movingObject[i].GetComponent<GravityController>().velocity = Vector2.zero;
				movingObject[i].GetComponent<Rigidbody2D>().angularVelocity
					= movingObject[i].GetComponent<GravityController>().angularVelocity = 0.0f;
			}
		}
	}
}
