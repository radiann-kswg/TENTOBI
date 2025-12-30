using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIDirector : MonoBehaviour
{
	#region private変数定義 1
	/// <summary>
	/// このシーンの名前
	/// </summary>
	private string thisSceneName;

	/// <summary>
	/// GameDirectorコンポーネント
	/// </summary>
	private GameDirector game;

	/// <summary>
	/// GravityController(重力操作)コンポーネント
	/// </summary>
	private GravityController gravityController;

	/// <summary>
	/// TransitionDirector(トランジション実行)コンポーネント
	/// </summary>
	private TransitionDirector transition;
	#endregion

	#region Prefab内アタッチ用public変数定義
	[Header("重力表示用のImage")]
	/// <summary>
	/// 重力表示に使用するImageコンポーネントです
	/// </summary>
	public Image gravityIcon;

	[Header("ステージ番号表示用のText")]
	/// <summary>
	/// ステージ番号を表示するためのTextコンポーネントです
	/// </summary>
	public TextMeshProUGUI stageNumText;

	[Header("スコア表示用のText")]
	/// <summary>
	/// スコアを表示するためのTextコンポーネントです
	/// </summary>
	public TextMeshProUGUI scoreText;

	[Header("ライフ表示用のText")]
	/// <summary>
	/// ライフ(残機)を表示するためのTextコンポーネントです
	/// </summary>
	public TextMeshProUGUI lifeText;

	[Header("HPバー(Slider)")]
	/// <summary>
	/// HP(継続ダメージ耐久力)バーとして使用しているSliderコンポーネントです
	/// </summary>
	public Slider hpBar;

	[Header("CPバー(Slider)")]
	/// <summary>
	/// CP(操作力)バーとして使用しているSliderコンポーネントです
	/// </summary>
	public Slider cpBar;

	[Header("インフォテキスト(GameObject)")]
	/// <summary>
	/// インフォテキスト全体のGameObjectです
	/// 表示/非表示の切り替えに使用します
	/// </summary>
	public GameObject infoTextObject;

	[Header("インフォテキスト内容表示用のText")]
	/// <summary>
	/// インフォテキストの子オブジェクトにある内容表示用のTextコンポーネントです
	/// 文章の内容更新に使用します
	/// </summary>
	public TextMeshProUGUI infoText;

	[Header("ゲームオーバー表示用のImage")]
	/// <summary>
	/// ゲームオーバー表示のImageコンポーネントです
	/// 「GameOverImage」というImageコンポーネントを参照してください
	/// </summary>
	public Image gameOverImage;

	[Header("ステージクリア表示用のImage")]
	/// <summary>
	/// ステージクリア表示の親オブジェクトとなっているImageコンポーネントです
	/// 「GameClearImage」というImageコンポーネントを参照してください
	/// </summary>
	public Image gameClearImage;

	[Header("ゲームオーバー表示用の子オブジェクト")]
	/// <summary>
	/// ゲームオーバー表示の子オブジェクトです
	/// 「GameOverLabel」を参照してください
	/// </summary>
	public GameObject gameOverLabel;

	[Header("ステージクリア表示用の子オブジェクト(GameObject)")]
	/// <summary>
	/// ステージクリア表示の子オブジェクトです
	/// 「SatgeClearLabel」を参照してください
	/// </summary>
	public GameObject gameClearLabel;

	[Header("ステータス表示用のText")]
	/// <summary>
	/// ステータス表示用のTextコンポーネントです
	/// ポーズ時などに使用します
	/// </summary>
	public TextMeshProUGUI statusText;

	[Header("ポーズボタン(Button)")]
	/// <summary>
	/// ポーズ機能を使用するButtonコンポーネントです
	/// ポーズ機能などに使用します
	/// </summary>
	public Button pauseButton;

	[Header("オブジェクト操作用オブジェクト(GameObject)")]
	/// <summary>
	/// オブジェクト操作に使用するオブジェクトです
	/// 「ObjectController」を参照してください
	/// </summary>
	public GameObject ObjectController;

	[Header("リトライボタン(Button)")]
	/// <summary>
	/// リトライ機能を使用するButtonコンポーネントです
	/// ステージ終了時に使用します
	/// </summary>
	public Button retryButton;

	[Header("ギブアップボタン(Button)")]
	/// <summary>
	/// ギブアップ機能を使用するButtonコンポーネントです
	/// ゲームオーバー時に使用します
	/// </summary>
	public Button giveUpButton;

	[Header("ステージ選択ボタン(Button)")]
	/// <summary>
	/// ステージ選択画面に移るためのButtonコンポーネントです
	/// ステージ終了時に使用します
	/// </summary>
	public Button stageSellectButton;

	[Header("結果表示用のText")]
	/// <summary>
	/// ステージ結果表示用のTextコンポーネントです
	/// ゲームクリア時に使用します
	/// </summary>
	public TextMeshProUGUI resultText;
	#endregion

	#region private変数定義 2
	/// <summary>
	/// ステージ名
	/// </summary>
	private string stageString = "Satge : ";

	/// <summary>
	/// スコア表示用文字列
	/// </summary>
	private string scoreString = "Score : ";

	/// <summary>
	/// ライフ(残機)表示用文字列
	/// </summary>
	private const string lifeString = "Life :  ";

	/// <summary>
	/// 結果表示用文字列
	/// </summary>
	private const string resultString = "Result : ";

	/// <summary>
	/// アニメーション中か
	/// アニメーション中であればtrueになります
	/// </summary>
	private bool isAnimating;

	/// <summary>
	/// 入力待ち中か
	/// 入力待ち中であればtrueになります
	/// </summary>
	private bool isWaitingInput;

	/// <summary>
	/// ステージクリアしたか
	/// ステージ終了時にゲームオーバーかステージクリアかを分岐させます
	/// </summary>
	private bool isGameClear; // GameOver:false GameClear:true Progressing:don'tCare

	/// <summary>
	/// アニメーション時刻 0
	/// </summary>
	private float t;
	/// <summary>
	/// アニメーション時刻 1
	/// </summary>
	private float t1;

	/// <summary>
	/// アニメーションの待機時間
	/// </summary>
	private const float waitT = 0.35f;

	/// <summary>
	/// トランジション(各種画面へのアニメーション)所要時間
	/// </summary>
	private const float animT = 1.50f;

	/// <summary>
	/// リザルトのアニメーション時間
	/// </summary>
	private const float resultAnimT = 2.50f;

	/// <summary>
	/// 次のステージに推移したかどうか
	/// 推移後、trueになります
	/// </summary>
	private bool wentNextScene;

	/// <summary>
	/// フェードアウトのトランジションが準備できているか
	/// trueでフェードアウトのトランジションを実行します
	/// </summary>
	private bool isReadyFadeOut;

	/// <summary>
	/// ギブアップが選択されたか
	/// </summary>
	private bool isChoosenGiveUp;

	/// <summary>
	/// ステージ選択機能が準備できているか
	/// trueでステージ選択画面に移ります
	/// </summary>
	private bool isReadyStageSelect;
	#endregion

	// Start is called before the first frame update
	void Start()
	{
		thisSceneName = SceneManager.GetActiveScene().name;

		game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameDirector>();
		gravityController = GameObject.FindGameObjectWithTag("Player").GetComponent<GravityController>();
		transition = GameObject.FindGameObjectWithTag("TransitionDirector").GetComponent<TransitionDirector>();
		if (game.isTutorial)
		{
			stageString = "Tutorial ";
			scoreString = "Progress :";
		}

		// ステージごとのパラメータを初期化
		game.InitializeStageParam();

		//UI初期化
		InitializeUIs();

		wentNextScene = false;
		isReadyFadeOut = false;
		isChoosenGiveUp = false; isReadyStageSelect = false;
	}

	// Update is called once per frame
	void Update()
	{
		// トランジションに移行する場合はUIの更新を停止
		if (!isReadyFadeOut)
		{
			if (isAnimating && !isWaitingInput)
			{
				if (t < animT)
				{
					t += Time.deltaTime;
					// ゲームクリア画面へのトランジション
					if (isGameClear)
					{
						if (t < animT - waitT)
							gameClearImage.color = new Color(1.0f, 1.0f, 1.0f, t / (animT - waitT) * 0.4f);
						else gameClearImage.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
					}
					// ゲームオーバー画面へのトランジション
					else
					{
						if (t < animT - waitT)
							gameOverImage.color = new Color(0.4f, 0.4f, 0.4f, t / (animT - waitT) * 0.4f);
						else gameOverImage.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
					}
				}
				// 入力UI表示,ゲームクリアであればリザルト表示
				else
				{
					// ゲームクリア画面の入力UI表示
					if (isGameClear)
					{
						if (game.isTutorial)
						{
							stageSellectButton.gameObject.SetActive(true);

							SwitchIntoWaitingInput();
						}
						else
						{
							if (t1 >= resultAnimT - waitT || Input.GetMouseButtonDown(0))
							{
								resultText.text = resultString + game.scoreInStage.ToString("D4");
								stageSellectButton.gameObject.SetActive(true);

								SwitchIntoWaitingInput();
							}
							else
							{
								t += Time.deltaTime;
								t1 = t - animT;
								resultText.gameObject.SetActive(true);
								int ds = Mathf.FloorToInt((float)game.scoreInStage * (t1 / (resultAnimT - waitT)));
								resultText.text = resultString + ds.ToString("D4");
							}
						}
					}
					// ゲームオーバー画面の入力UI表示
					else
					{
						ActivateGameOverButton();
						SwitchIntoWaitingInput();
					}
				}
			}
			// 各種画面の入力待ちでなければUIを更新
			else if (!isWaitingInput)
			{
				UpdateUIs();
			}
		}
		// トランジション(フェードアウト)に移行した際完了したらシーンを切り替え
		else if (transition.IsFadeOutComplete() && !wentNextScene)
		{
			// ゲームクリア時
			if (isReadyStageSelect)
			{
				// ステージ選択画面へ
				game.GoToMainMenuScene();
			}
			// ゲームオーバー時ギブアップを選択
			else if (isChoosenGiveUp)
			{
				// ステージ選択画面へ
				game.GoToMainMenuScene();
			}
			// ゲームオーバー時リトライを選択
			else
			{
				SceneManager.LoadScene(thisSceneName); // シーンを再読み込み
			}
			wentNextScene = true;
		}
	}

	/// <summary>
	/// ステージ番号を表示します
	/// </summary>
	private void SetStageNumText()
	{
		stageNumText.text = stageString + game.stageNum.ToString("D2");
	}

	/// <summary>
	/// スコア表示を更新します
	/// </summary>
	private void UpdateScoreText()
	{
		if (game.isTutorial) scoreText.text = scoreString + game.scoreInStage.ToString("D3") + "%";
		else scoreText.text = scoreString + game.scoreInStage.ToString("D4");
	}

	/// <summary>
	/// ライフ(残機)表示を初期化します
	/// </summary>
	private void InitializeLifeText()
	{
		lifeText.text = lifeString + game.life_max.ToString();
	}

	/// <summary>
	/// ライフ(残機)表示を更新します
	/// </summary>
	private void UpdateLifeText()
	{
		lifeText.text = lifeString + game.life.ToString();
	}

	/// <summary>
	/// HP(継続ダメージ耐久力)表示を初期化します
	/// </summary>
	private void InitializeHPBar()
	{
		hpBar.maxValue = game.HP_max;
		hpBar.value = game.HP;
	}

	/// <summary>
	/// HP(継続ダメージ耐久力)表示を更新します
	/// </summary>
	private void UpdateHPBar()
	{
		hpBar.value = game.HP;
	}
	/// <summary>
	/// CP(操作力)表示を初期化します
	/// </summary>
	private void InitializeCPBar()
	{
		cpBar.maxValue = game.CP_max;
		cpBar.value = game.CP;
	}

	/// <summary>
	/// CP(操作力)表示を更新します
	/// </summary>
	private void UpdateCPBar()
	{
		cpBar.value = game.CP;
	}

	/// <summary>
	/// GravityController(重力操作)コンポーネントで計算された重力方向から重力方向を示すUIの回転位置を返します
	/// </summary>
	private void UpdateGravityIcon()
	{
		gravityIcon.rectTransform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f + Mathf.Rad2Deg * gravityController.angle);
	}

	/// <summary>
	/// ステージ終了時に表示するUIの表示状況を初期化します
	/// </summary>
	private void InitializeGameEndUIs()
	{
		gameOverImage.color = new Color(0.4f, 0.4f, 0.4f, 0.0f);
		gameClearImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		resultText.gameObject.SetActive(false);
		retryButton.gameObject.SetActive(false);
		giveUpButton.gameObject.SetActive(false);
		stageSellectButton.gameObject.SetActive(false);
	}

	/// <summary>
	/// UIと関連変数を初期化します
	/// </summary>
	private void InitializeUIs()
	{
		SetStageNumText(); // シーン変更がない限り更新することはないので、更新はStart関数のみでOK
		InitializeLifeText();
		InitializeHPBar();
		InitializeCPBar();
		InitializeGameEndUIs();

		isAnimating = false; isWaitingInput = false;
		t = 0.0f;
	}

	/// <summary>
	/// UIを更新します
	/// </summary>
	private void UpdateUIs()
	{
		UpdateScoreText();
		UpdateLifeText();
		UpdateHPBar();
		UpdateCPBar();
		UpdateGravityIcon();
	}

	/// <summary>
	/// インフォメッセージを非表示にします
	/// </summary>
	public void DisableInfoText()
	{
		infoText.text = "";
		infoTextObject.SetActive(false);
		game.SwitchMessagingFlag(false);
	}

	/// <summary>
	/// インフォメッセージを表示します
	/// </summary>
	/// <param name="text">表示するインフォメッセージ</param>
	public void ActivateInfoText(string text)
	{
		game.SwitchMessagingFlag(true);
		infoTextObject.SetActive(true);
		infoText.text = text;
	}

	/// <summary>
	/// ゲームオーバー画面に移行します
	/// </summary>
	public void ActivateGameOverUIs()
	{
		// t = 0.0f;
		gameOverLabel.SetActive(true);
		isAnimating = true;
		isGameClear = false;
	}

	/// <summary>
	/// ゲームクリア画面に移行します
	/// </summary>
	public void ActivateGameClearUIs()
	{
		// t = 0.0f;
		isReadyStageSelect = true;
		gameClearLabel.SetActive(true);
		isAnimating = true;
		isGameClear = true;
	}

	/// <summary>
	/// 入力待ち状態にします
	/// </summary>
	private void SwitchIntoWaitingInput()
	{
		isAnimating = false; t = 0.0f;
		isWaitingInput = true;
	}

	/// <summary>
	/// ゲームオーバー後のボタンを表示します
	/// </summary>
	private void ActivateGameOverButton()
	{
		retryButton.gameObject.SetActive(true);
		giveUpButton.gameObject.SetActive(true);
	}

	/// <summary>
	/// ステータステキストを更新します
	/// </summary>
	/// <param name="status">内容（status="" で非表示にします）</param>
	public void UpdateStatusText(string status)
	{
		if (status == "") statusText.gameObject.SetActive(false);
		else
		{
			statusText.gameObject.SetActive(true);
			statusText.text = status;
		}
	}

	/// <summary>
	/// ポーズボタンを表示します
	/// </summary>
	public void ActivatePauseButton()
	{
		pauseButton.gameObject.SetActive(true);
		if (!game.IsPausing()) pauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pause";
		else pauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
	}

	/// <summary>
	/// ポーズボタンを無効化します
	/// </summary>
	public void DeactivatePauseButton()
	{
		pauseButton.gameObject.SetActive(false);
	}

	/// <summary>
	/// ポーズボタンが押された時の処理です
	/// </summary>
	public void OnPressPauseButton()
	{
		if (!game.ReturnPauseFlagForDamagingOrPausing())
		{
			if (!game.IsPausing())
			{
				game.SwitchPauseMode(true);
				UpdateStatusText("-Pause-");
				pauseButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
			}
			else
			{
				game.SwitchPauseMode(false);
				UpdateStatusText("");
				pauseButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Pause";
			}
		}
	}

	/// <summary>
	/// オブジェクト操作用UIを表示します
	/// </summary>
	public void ActivateController()
	{
		ObjectController.SetActive(true);
	}

	/// <summary>
	/// オブジェクト操作用UIを非表示にします
	/// </summary>
	public void DeactivateController()
	{
		ObjectController.SetActive(false);
	}

	/// <summary>
	/// 実行(Act)ボタンが押された時の処理です
	/// </summary>
	public void OnPressActButton()
	{
		Debug.Log("オブジェクト操作が終了しました");
		DeactivateController();
		UpdateStatusText("");
		game.SwitchControlMode(false);
		ActivatePauseButton();
	}

	/// <summary>
	/// リトライボタンが押された時の処理です
	/// </summary>
	public void OnPressRetryButton()
	{
		Debug.Log("リトライを受け付けました");
		isReadyFadeOut = true;
		transition.StartFadeOut();
	}

	/// <summary>
	/// ギブアップボタンが押された時の処理です
	/// </summary>
	public void OnPressGiveUpButton()
	{
		Debug.Log("ギブアップを受け付けました");
		isReadyFadeOut = true;
		isChoosenGiveUp = true;
		transition.StartFadeOut();
	}

	/// <summary>
	/// ステージ選択ボタンが押された時の処理です
	/// </summary>
	public void OnPressStageSelectButton()
	{
		Debug.Log("ステージ選択を受け付けました");
		isReadyFadeOut = true;
		transition.StartFadeOut();
	}
}