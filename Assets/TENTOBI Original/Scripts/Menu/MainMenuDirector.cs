// 引用URL : https://ftvoid.com/blog/post/732 (2021.08.23)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MainMenuDirector : MonoBehaviour
{
	#region シングルトン化
	public static MainMenuDirector main = null;
	private void Awake()
	{
		if (main == null)
		{
			main = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);
	}
	#endregion

	#region ステージ選択画面表示用クラス
	/// <summary>
	/// ステージ選択画面に表示するためのクラスです
	/// </summary>
	[Serializable]
	public class StageView
	{
		[Header("このステージがチュートリアルか")]
		/// <summary>
		/// このステージがチュートリアルである場合はtrueを指定してください
		/// </summary>
		public bool isTutorial;

		[Header("ステージ番号")]
		/// <summary>
		/// このステージのステージ番号です
		/// </summary>
		public int stageNum;

		[Header("ステージ表記用サムネイル（Image）")]
		/// <summary>
		/// ステージ表記に使用するサムネイル画像です
		/// </summary>
		public Image stageImage;
	}

	[Header("ステージ選択画面表示用クラス（配列）")]
	/// <summary>
	/// StageView（ステージ選択画面表示用）クラスを配列にしたものです
	/// </summary>
	[SerializeField] public StageView[] stageViewList;
	#endregion ここまで

	[Header("現在表示しているページ番号")]
	/// <summary>
	/// ステージ選択画面で現在表示しているページの番号です
	/// UIなどで表示する際は+1してください
	/// </summary>
	public int page = 0;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	/// <summary>
	/// ステージ選択ボタンに対応した表示用クラス配列の添え字を計算します
	/// </summary>
	/// <param name="buttonID">ステージ選択ボタンのID</param>
	/// <returns>ステージ選択ボタンに対応する表示用クラス配列の添え字</returns>
	public int returnThisStageViewID(int buttonID)
	{
		return buttonID + main.page * 6;
	}
}
