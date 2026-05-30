using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleDirector : MonoBehaviour
{
	#region private変数定義
	/// <summary>
	/// SaveDataSelectDirecor（セーブデータ選択）コンポーネント
	/// </summary>
	private SaveDataSelectDirector dataSelect;
	/// <summary>
	/// TransitionDirectorコンポーネント
	/// </summary>
	private TransitionDirector transition;

	/// <summary>
	/// ボタンが押下されたかどうか
	/// </summary>
	private bool isPlessed;

	/// <summary>
	/// 次のシーンに遷移されているか
	/// </summary>
	private bool wentNextScene;
	/// <summary>
	/// コンティニューするかどうか
	/// </summary>
	private bool isContinue;
	#endregion

	#region アタッチ用public変数定義
	[Header("バージョン表示用Text")]
	/// <summary>
	/// バージョン表示用のTextコンポーネントです
	/// </summary>
	public TextMeshProUGUI versionText;
	[Header("表示切替用Button（GameObject）")]
	/// <summary>
	/// タイトル画面で使用しているすべてのButton（GameObject）です
	/// データ選択画面への切り替え時に使用します
	/// </summary>
	public GameObject[] buttons;

	[Header("ニューゲーム開始時に遷移するステージ")]
	/// <summary>
	/// ニューゲームを開始する時の遷移先のステージです
	/// </summary>
	public string nextSceneToStartNewGameName = "Tutorial1";

	[Header("メインメニュー（ステージ選択）シーン")]
	/// <summary>
	/// ステージ選択画面となるメインメニューシーンです
	/// </summary>
	public string mainMenuSceneName = "MainMenu";
	#endregion

	// Start is called before the first frame update
	void Start()
	{
		dataSelect = GameObject.FindGameObjectWithTag("SaveDataSelector").GetComponent<SaveDataSelectDirector>();
		transition = GameObject.FindGameObjectWithTag("TransitionDirector").GetComponent<TransitionDirector>();

		versionText.text = "version: " + Application.version;
		isPlessed = false; wentNextScene = false; isContinue = false;
		SetActiveButtons(true);
	}

	// Update is called once per frame
	void Update()
	{
		if (wentNextScene && transition.IsFadeOutComplete())
		{
			if (isContinue) SceneManager.LoadScene(mainMenuSceneName);
			else SceneManager.LoadScene(nextSceneToStartNewGameName);
		}

	}

	/// <summary>
	/// ニューゲームを開始します
	/// </summary>
	public void StartToNewGame()
	{
		wentNextScene = true;
		isContinue = false;

	}

	/// <summary>
	/// コンティニューしてゲームを開始します
	/// </summary>
	public void StartToContinueGame()
	{
		// ステージを1つもクリアしてない場合はニューゲームとして開始
		if (SaveLoadFile.instance.savedata.stageProgressNum <= 0) StartToNewGame();
		else
		{
			wentNextScene = true;
			isContinue = true;
		}
	}

	/// <summary>
	/// ニューゲームボタンを押した時の処理です
	/// </summary>
	public void PressNewGameButton()
	{
		if (!isPlessed && transition.IsFadeInComplete())
		{
			Debug.Log("ニューゲームが選択されました");
			// ニューゲーム開始処理
			SaveLoadFile.instance.InitializeData2NewGame();

			isPlessed = true;
			SetActiveButtons(false);
			dataSelect.AccessSaveDataSelection(false);
		}
	}

	/// <summary>
	/// コンティニューボタンを押した時の処理です
	/// </summary>
	public void PressContinueButton()
	{
		if (!isPlessed && transition.IsFadeInComplete())
		{
			Debug.Log("コンティニューが選択されました");
			// コンティニュー処理
			dataSelect.AccessSaveDataSelection(true);

			isPlessed = true;
			SetActiveButtons(false);
			dataSelect.AccessSaveDataSelection(true);
		}
	}

	/// <summary>
	/// 終了ボタンを押した時の処理です
	/// </summary>
	public void PressExitButton()
	{
		if (!isPlessed && transition.IsFadeInComplete())
		{
			Debug.Log("ゲーム終了が選択されました");
			//＊できればゲーム終了の確認画面

			// ゲーム終了処理
			Debug.Log("ゲームが終了されます");
			Application.Quit();

			isPlessed = true;
			SetActiveButtons(false);
		}
	}

	/// <summary>
	/// ボタンの有効/無効を切り替えます
	/// </summary>
	/// <param name="isActive">有効かどうか</param>
	private void SetActiveButtons(bool isActive)
	{
		int i, n = buttons.Length;
		if (n > 0) for (i = 0; i < n; ++i)
			buttons[i].SetActive(isActive);
	}
}
