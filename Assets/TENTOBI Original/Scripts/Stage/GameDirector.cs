// 引用URL: https://dkrevel.com/makegame-beginner/make-2d-action-game-manager/ (2020.09.12)

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
	#region private変数定義 1
	/// <summary>
	/// PlayerDirectorコンポーネント
	/// </summary>
	private PlayerDirector playerDir;

	/// <summary>
	/// プレイヤーのSpriteRendererコンポーネント
	/// アニメーションなどに使用します
	/// </summary>
	private MeshRenderer playerRend;

	/// <summary>
	/// StagePointDirecotr(ステージ中継ポイント処理)コンポーネント
	/// </summary>
	private StagePointDirector stage;

	/// <summary>
	/// UIDirecotrコンポーネント
	/// </summary>
	private UIDirector ui;

	/// <summary>
	/// TransitionDirectorコンポーネント
	/// </summary>
	private TransitionDirector transition;
	#endregion

	#region ステージ情報
	[Header("現在攻略中のシーン")]
	/// <summary>
	/// 現在攻略中のシーン
	/// </summary>
	public int stageNum;
	[Header("現在攻略中のシーンで獲得したスコア")]
	/// <summary>
	/// 現在攻略中のシーンで獲得したスコア
	/// </summary>
	public int scoreInStage;
	[Header("現在のリスポーン地点番号")]
	/// <summary>
	/// リスポーン地点番号
	/// </summary>
	public int respownNum;
	[Header("ライフ(残機)")]
	/// <summary>
	/// ライフ(残機)
	/// </summary>
	public int life;
	[Header("HP(継続ダメージ耐久力)")]
	/// <summary>
	/// HP(継続ダメージ耐久力)
	/// </summary>
	public float HP;
	[Header("CP(操作力)")]
	/// <summary>
	/// CP(操作力)
	/// </summary>
	public float CP;

	[Header("チュートリアルモード")]
	/// <summary>
	/// このステージがチュートリアル用であればtrueを指定してください
	/// trueでチュートリアル専用の処理を行います
	/// </summary>
	public bool isTutorial = false;
	#endregion

	[Header("メインメニュー（ステージ選択）シーン")]
	/// <summary>
	/// ステージ選択画面となるメインメニューシーンです
	/// </summary>
	public string mainMenuSceneName = "MainMenu";

	#region パラメータの上限
	[Header("ライフ(残機)上限")]
	/// <summary>
	/// ライフ(残機)上限
	/// </summary>
	public int life_max = 10;
	[Header("HP(継続ダメージ耐久力)上限")]
	/// <summary>
	/// HP(継続ダメージ耐久力)上限
	/// </summary>
	public float HP_max = 1.0f;
	[Header("CP(操作力)上限")]
	/// <summary>
	/// CP(操作力)上限
	/// </summary>
	public float CP_max = 100.0f;
	[Header("継続ダメージの強度[単位:%毎秒]")]
	/// <summary>
	/// 継続ダメージの強度[単位:%毎秒]
	/// </summary>
	public float DamageLv = 8.0f;
	[Header("ボーナススコア(残機)[1機ごと]")]
	/// <summary>
	/// ボーナススコア(残機)[1機ごと]
	/// </summary>
	public int BonusScorePerLife = 10;
	[Header("クリア目標タイム[単位:秒]")]
	/// <summary>
	/// クリア目標タイム[単位:秒]
	/// </summary>
	public float CleartimeNorma = 240.0f;
	#endregion

	#region private変数定義 2
	/// <summary>
	/// ゲームオーバーかどうか
	/// </summary>
	private bool isGameOver;
	/// <summary>
	/// ステージクリアしたかどうか
	/// </summary>
	private bool isGameClear;
	/// <summary>
	/// アニメーション再生中かどうか
	/// </summary>
	private bool isAnimating;
	/// <summary>
	/// インフォメッセージを表示中か
	/// </summary>
	private bool isMessaging;
	/// <summary>
	/// インフォメッセージが連続しているかどうか
	/// </summary>
	private bool isMessageContinued;
	/// <summary>
	/// ポーズ中かどうか
	/// </summary>
	private bool isPausing;
	/// <summary>
	/// オブジェクト操作中かどうか
	/// </summary>
	private bool isControlling;

	/// <summary>
	/// 次のシーンに遷移させているか
	/// </summary>
	private bool wentNextScene;

	/// <summary>
	/// 処理用時刻
	/// </summary>
	private float t;
	/// <summary>
	/// クリアタイム（ステージ開始〜クリアまでの経過時間）
	/// ポーズ/メッセージ/操作中など、ゲーム進行が停止している間は加算しません
	/// </summary>
	private float clearTime;
	/// <summary>
	/// 点滅周期
	/// </summary>
	private const float flashT = 0.6f;
	#endregion


	// Start is called before the first frame update
	void Start()
	{
		if (isTutorial) life_max = 99;
		playerDir = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerDirector>();
		playerRend = GameObject.FindGameObjectWithTag("PlayerRending").GetComponent<MeshRenderer>();
		stage = GameObject.FindGameObjectWithTag("StagePointDirector").GetComponent<StagePointDirector>();
		ui = GameObject.FindGameObjectWithTag("UI").GetComponent<UIDirector>();
		transition = GameObject.FindGameObjectWithTag("TransitionDirector").GetComponent<TransitionDirector>();

		isGameOver = false;
		isAnimating = false;
		isMessaging = false;
		isMessageContinued = false;
		isControlling = false;
		wentNextScene = false;
		t = 0.0f;
		clearTime = 0.0f;
	}

	// Update is called once per frame
	void Update()
	{
		if (!wentNextScene)
		{
			// クリアタイム計測（プレイ進行が止まる状況では加算しない）
			if (isRunningCleartimer())
			{
				clearTime += Time.deltaTime;
			}

			// チュートリアルモードで進捗(スコア)が100[%]に到達したらゲームクリア処理へ
			if (isTutorial && scoreInStage >= 100)
			{
				SwitchToGameClear();
			}

			// ゲームクリアしたorトランジション(フェードイン)が完了していない場合はすべての更新処理を停止
			if (!isGameClear && transition.IsFadeInComplete())
			{
				if (HP <= 0.0f)
				{
					if (!isAnimating)
					{
						t = 0;
						// 即死処理
						DeathPlayerAtKilled();
					}
					else
					{
						// 死亡アニメーション
						t += Time.deltaTime;
						if (t < 1.0f) playerRend.sharedMaterial.SetColor("_Color", new Color(1.0f, 1.0f - t, 1.0f - t, 1.0f));
						else if (t < 2.0f) playerRend.sharedMaterial.SetColor("_Color", new Color(1.0f, 0.0f, 0.0f, 2.0f - t));
						else if (t < 2.5f) playerRend.sharedMaterial.SetColor("_Color", Color.clear);
						// リスポーン
						else
						{
							t = 0.0f;
							if (!isGameOver)
							{
								RespownPlayer();
								isAnimating = false;
							}
							else playerDir.gameObject.SetActive(false);
						}
					}
				}
				else if (!isGameOver)
				{
					// 継続ダメージを受けている場合はプレイヤーを橙色に点滅
					if (playerDir.isDamaged)
					{
						t += Time.deltaTime;
						float b = Mathf.Abs(Mathf.Repeat(t + flashT / 2.0f, flashT) - flashT / 2.0f) / flashT * 0.6f;
						float g = b * 0.75f;
						playerRend.sharedMaterial.SetColor("_Color", new Color(1.0f, 1.0f - g, 1.0f - b, 1.0f));
					}
					else
					{
						t = 0.0f;
						playerRend.sharedMaterial.SetColor("_Color", Color.white);
					}
				}
				// ゲームオーバー処理
				if (isGameOver)
				{
					// UIをゲームオーバー画面へ
					ui.ActivateGameOverUIs();
				}

				if (!ReturnPauseFlagForDamagingOrPausing())
				{
					// 継続ダメージ処理
					if (playerDir.isDamaged)
					{
						if (HP > 0.0f) HP -= DamageLv * 0.01f * Time.deltaTime;
					}
					// 継続ダメージ回復処理
					else if (HP < 1.0f && !isAnimating)
					{
						HP += 0.05f * Time.deltaTime;
					}
					else if (HP > 1.0f)
					{
						HP = 1.0f;
					}
				}
				// インフォメッセージ表示(タップ入力で非表示)
				else
				{
					if (isMessaging && Input.GetMouseButtonDown(0))
					{
						ui.DisableInfoText();
					}
				}

				// 操作力上限/下限処理
				if (CP > CP_max) CP = CP_max;
				else if (CP < 0.0f) CP = 0.0f;
			}
			// ゲームクリア処理
			else if (isGameClear)
			{
				// UIをゲームクリア画面へ
				ui.ActivateGameClearUIs();
			}
		}
		else if (transition.IsFadeOutComplete()) SceneManager.LoadScene(mainMenuSceneName);
	}

	/// <summary>
	/// ステージごとのパラメータを初期化します
	/// </summary>
	public void InitializeStageParam()
	{
		respownNum = 0;
		scoreInStage = 0;
		life = life_max;
		HP = HP_max;
		CP = CP_max * 0.5f;
		clearTime = 0.0f;
	}

	/// <summary>
	/// ボーナススコアを加算します
	/// </summary>
	private void AddBonusScore()
	{
		// クリアタイム
		if (clearTime < CleartimeNorma)
		{
			int bonus = Mathf.FloorToInt(CleartimeNorma - clearTime);
			if (bonus > 0) scoreInStage += bonus;
		}
		// 残機
		scoreInStage += life > 1 ? (life - 1) * BonusScorePerLife : 0;
	}

	/// <summary>
	/// 即死処理をします
	/// </summary>
	public void DeathPlayerAtKilled()
	{
		HP = 0.0f;

		// 死亡アニメーション
		isAnimating = true;

		if (!isTutorial) --life;
		if (life <= 0)
		{
			// ゲームオーバー処理へ
			isGameOver = true;
		}
	}

	/// <summary>
	/// ゲームクリアの処理へ移行します
	/// </summary>
	public void SwitchToGameClear()
	{
		isGameClear = true;
		AddBonusScore();
		UpdateStageProgress(); // ステージ進捗に進展があれば更新
							   //if (!isTutorial && stageNum > 0) UpdateScoreParStage();
	}

	/// <summary>
	/// メインメニュー（ステージ選択）画面へ移動します
	/// </summary>
	public void GoToMainMenuScene()
	{
		wentNextScene = true;
	}

	///<summary>
	///残機を1つ減らし、特定のリスポーン地点へ復活
	///</summary>
	private void RespownPlayer()
	{
		HP = HP_max;
		stage.RespownAndResetObjectsPositions();
		playerRend.sharedMaterial.SetColor("_Color", Color.white);
	}
	/*
    /// <summary>
    /// ステージごとの入手スコアを更新します
    /// </summary>
    public void UpdateScoreParStage()
    {
        // そのステージで以前獲得したスコアより多くのスコアを得た場合は更新
        // ※テスト用ステージ番号は0なので、ステージ番号が0の場合は更新しません
        int gotten = scoreInStage - SaveLoadFile.instance.savedata.scorePerSatge[stageNum];
        if (gotten > 0 && stageNum > 0)
        {
            SaveLoadFile.instance.savedata.scorePerSatge[stageNum] = scoreInStage;
            SaveLoadFile.instance.savedata.gottenScore += gotten;
        }
    }
    */

	/// <summary>
	/// ステージ進捗を更新します
	/// </summary>
	private void UpdateStageProgress()
	{
		int progress = SaveLoadFile.instance.savedata.stageProgressNum;
		var stageNumProgress = MainMenuDirector.main.stageViewList[progress];
		int oldScorePerStage = SaveLoadFile.instance.savedata.scorePerSatge[stageNum];
		// ステージ進捗を更新した場合
		if (stageNumProgress.stageNum <= stageNum)
		{
			// ステージ進捗を更新
			SaveLoadFile.instance.savedata.stageProgressNum += 1;
			// ハイスコアを更新（チュートリアルでない場合）
			if (!isTutorial)
			{
				SaveLoadFile.instance.savedata.scorePerSatge[stageNum] = scoreInStage;
				SaveLoadFile.instance.UpdateGottenScore();
			}
			// セーブデータを保存
			SaveLoadFile.instance.SaveDataToFile();
		}
		// ステージ進捗は更新していないがチュートリアルではなくかつステージのハイスコアを更新した場合
		else if (!isTutorial && scoreInStage > oldScorePerStage)
		{
			// ハイスコアのみ更新
			SaveLoadFile.instance.savedata.scorePerSatge[stageNum] = scoreInStage;
			SaveLoadFile.instance.UpdateGottenScore();
			SaveLoadFile.instance.SaveDataToFile();
		}
	}

	/// <summary>
	/// メッセージを表示するためのフラグを変更します
	/// </summary>
	/// <param name="sw">メッセージを表示するかどうか</param>
	public void SwitchMessagingFlag(bool sw)
	{
		isMessaging = sw;
	}

	/// <summary>
	/// メッセージを連続して表示するためのフラグを変更します
	/// </summary>
	/// <param name="sw">メッセージが連続しているか</param>
	public void SwitchMessageContinuedFlag(bool sw)
	{
		isMessageContinued = sw;
	}

	/// <summary>
	/// 次のメッセージに切り替え中かを返します
	/// </summary>
	/// <returns>次のメッセージに切り替え中かどうか</returns>
	public bool IsSwitchingNextMessage()
	{
		if (!isMessaging && isMessageContinued) return true;
		return false;
	}

	/// <summary>
	/// ポーズモードかどうかを返します
	/// </summary>
	/// <returns>ポーズモードかどうか</returns>
	public bool IsPausing()
	{
		return isPausing;
	}


	/// <summary>
	/// クリアタイムを計測すべきかどうかを返します
	/// </summary>
	/// <returns>クリアタイム計測すべきかどうか</returns>
	private bool isRunningCleartimer()
	{
		return !isGameClear && transition.IsFadeInComplete()
				&& !isGameOver && !isAnimating
				&& !isMessaging && !isMessageContinued
				&& !isPausing && !isControlling;
	}

	/// <summary>
	/// ポーズモードかどうかを切り替えます
	/// </summary>
	/// <param name="isPause">ポーズモードかどうか</param>
	public void SwitchPauseMode(bool isPause)
	{
		isPausing = isPause;
		if (isPause) Debug.Log("ポーズモードに移行しました");
		else Debug.Log("ポーズモードが終了しました");
	}

	/// <summary>
	/// オブジェクト操作モードかどうかを返します
	/// </summary>
	/// <returns>オブジェクト操作モードかどうか</returns>
	public bool IsControlling()
	{
		return isControlling;
	}

	/// <summary>
	/// オブジェクト操作モードかどうかを切り替えます
	/// </summary>
	/// <param name="isCtrl">オブジェクト操作モードかどうか</param>
	public void SwitchControlMode(bool isCtrl)
	{
		isControlling = isCtrl;
		if (isCtrl) Debug.Log("オブジェクト操作モードに移行しました");
		else Debug.Log("オブジェクト操作モードが終了しました");
	}

	/// <summary>
	/// 何らかのフラグによってポーズすべき状況かを返します
	/// </summary>
	/// <returns>ポーズすべき状況であるかどうか</returns>
	public bool ReturnPauseFlag()
	{
		return isGameOver || isGameClear || isAnimating
			|| isMessaging || isMessageContinued || isPausing || isControlling
			|| !transition.IsFadeInComplete() || !transition.IsFadeOutComplete();
	}

	/// <summary>
	/// 何らかのフラグによってダメージ処理やポーズメニューなどを停止すべき状況かを返します
	/// </summary>
	/// <returns>ダメージ処理やポーズメニューを停止すべき状況かどうか</returns>
	public bool ReturnPauseFlagForDamagingOrPausing()
	{
		return isAnimating || isMessaging || isMessageContinued;
	}
}