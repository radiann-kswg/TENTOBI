using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SaveDataSelectDirector : MonoBehaviour
{
	#region private変数定義 1
	/// <summary>
	/// このシーンの名前
	/// </summary>
	private string thisSceneName;

	/// <summary>
	/// TransitionDirector(トランジション実行)コンポーネント
	/// </summary>
	private TransitionDirector transition;

	private TitleDirector title;
	#endregion

	#region アタッチ用public変数定義
	[Header("VirtualKeyBoard(バーチャルキーボード)")]
	/// <summary>
	/// セーブデータの名前入力に使用するVirtualKeyBoard(バーチャルキーボード)コンポーネントです
	/// </summary>
	public VirtualKeyboard virtualKeyboard;

	[Header("メッセージ表示用のText")]
	/// <summary>
	/// メッセージを表示するためのTextコンポーネントです
	/// </summary>
	public TextMeshProUGUI messageText;

	[Header("セーブデータ選択用UIの親オブジェクト")]
	/// <summary>
	/// このコンポーネントを処理するためのUIがまとめられた親オブジェクトを参照してください
	/// </summary>
	public GameObject usingUis;

	[Header("セーブデータ選択機能のスキップフラグ（推奨:true）")]
	/// <summary>
	/// プレイヤー名の入力によるセーブデータ選択機能が不要な場合はtrueを指定してください
	/// </summary>
	public bool isSkippedSelection = true;

	[Header("セーブデータ選択機能スキップ時に使用するセーブデータ名")]
	/// <summary>
	/// セーブデータ選択を行わない場合に使用するセーブデータ名です
	/// 機能の実装やバージョンアップに応じて変更しても構いません
	/// </summary>
	public string defaultPlayerName = "ABURI_C";

	[Header("セーブデータ選択機能スキップ時には表示しないUIの子オブジェクト")]
	/// <summary>
	/// セーブデータ選択用UIのうちセーブデータ選択を行わない場合は表示しない子オブジェクトを指定してください
	/// </summary>
	public GameObject[] deactivateObjectsToSkippingSelection;
	#endregion

	#region private変数定義 2
	/// <summary>
	/// セーブデータの名前が表示されるTextコンポーネントです
	/// </summary>
	private TextMeshProUGUI dataNameField;

	/// <summary>
	/// trueの時にコンポーネントが処理されます
	/// </summary>
	private bool isActive;

	/// <summary>
	/// コンティニューを選択したときにtrueになります
	/// </summary>
	private bool isContinue;

	/// <summary>
	/// 表示するメッセージの文字列です
	/// </summary>
	private string message;

	/// <summary>
	/// 次のシーンに推移したときにtrueになります
	/// </summary>
	private bool wentNextScene;
	#endregion

	// Start is called before the first frame update
	void Start()
	{
		thisSceneName = SceneManager.GetActiveScene().name;

		title = GameObject.FindGameObjectWithTag("GameController").GetComponent<TitleDirector>();
		transition = GameObject.FindGameObjectWithTag("TransitionDirector").GetComponent<TransitionDirector>();
		dataNameField = virtualKeyboard.input;

		isActive = false;
		isContinue = false;
		message = "";
		wentNextScene = false;
		usingUis.SetActive(false);
		if (isSkippedSelection) SetDeactiveWhenSelectionSkkipped();
	}
	// Update is called once per frame
	void Update()
	{
		if (!wentNextScene)
		{
			if (isActive)
			{
				if (message == "") messageText.text = "プレイヤー名を入力してください";
				else
				{
					if (isSkippedSelection) usingUis.SetActive(true);
					messageText.text = message;
				}
				// データ選択機能をスキップする場合
				if (isSkippedSelection)
				{
					// デフォルトのプレイヤー名を指定して決定ボタンを押す処理を実行
					SaveLoadFile.instance.savedataName = defaultPlayerName;
					if (message == "") PressOKButton();
				}
			}
			else usingUis.SetActive(false);
		}
	}

	/// <summary>
	/// セーブデータの作成/選択画面に移行します
	/// </summary>
	/// <param name="isContinue">コンティニューするかどうか</param>
	public void AccessSaveDataSelection(bool isChoosenContinue)
	{
		// コンティニューかどうかのフラグを設定
		if (isChoosenContinue) isContinue = true;
		else isContinue = false;

		if (!isSkippedSelection) usingUis.SetActive(true);
		isActive = true;
	}

	/// <summary>
	/// 決定ボタンを押した時の処理です
	/// </summary>
	public void PressOKButton()
	{
		if (!wentNextScene)
		{
			// メッセージが何もない（＝プレイヤー名/セーブデータ名を待ち受け中）場合はファイルにアクセス
			if (message == "")
			{
				if (dataNameField.text != "" || isSkippedSelection)
				{
					if (!isSkippedSelection) SaveLoadFile.instance.savedataName = dataNameField.text;
					Debug.Log("セーブデータにプレイヤー名「" + SaveLoadFile.instance.savedataName + "」でアクセスします");

					// ニューゲームの場合
					if (!isContinue)
					{
						message = SaveLoadFile.instance.ReturnMessageToSaveData();
						// 上書きの恐れなくセーブできる場合はファイルを作成してゲーム冒頭画面へ
						if (message == "")
						{
							SaveLoadFile.instance.SaveDataToFile();

							transition.StartFadeOut();
							wentNextScene = true;

							// ゲーム冒頭画面へ
							title.StartToNewGame();

						}
						// 上書きの恐れがある場合は入力フォームの色を変えて無効化
						else SetActiveDataNameInput(false);
					}
					// コンティニューの場合
					else
					{
						message = SaveLoadFile.instance.ReturnMessageToLoadData();
						// エラーなくロードできる場合はファイルを読み込んでステージ選択画面へ
						if (message == "")
						{
							SaveLoadFile.instance.LoadDataFromFile();

							transition.StartFadeOut();
							wentNextScene = true;

							// ステージ選択画面へ
							title.StartToContinueGame();
						}
						// エラーが出た場合は入力フォームの色を変えて無効化
						else SetActiveDataNameInput(false);
					}
				}
			}
			// ニューゲームでセーブデータを上書きする恐れがある状態で押した場合はファイルを上書き
			else
			{
				if (!isContinue && (message == SaveLoadFile.rewriteMessage
				|| message == SaveLoadFile.rewriteToBrokenFileMessage))
				{
					SaveLoadFile.instance.SaveDataToFile();

					transition.StartFadeOut();
					wentNextScene = true;

					// ゲーム冒頭画面へ
					title.StartToNewGame();
				}
				// ロードエラーの時はメッセージを初期状態に戻す
				else if (isContinue && (message == SaveLoadFile.notExistFileMessage
					|| message == SaveLoadFile.cannotLoadBrokenFileMessage))
				{
					message = "";
					SetActiveDataNameInput(true);
				}
			}
		}
	}

	/// <summary>
	/// キャンセルボタンを押した時の処理です
	/// </summary>
	public void PressCancelButton()
	{
		if (isActive && !wentNextScene)
		{
			// セーブデータ選択機能をスキップする場合はシーンを再読み込み
			if (isSkippedSelection)
			{
				SceneManager.LoadScene(thisSceneName); // シーンを再読み込み
				wentNextScene = true;
			}
			// メッセージが何もない（＝プレイヤー名/セーブデータ名を待ち受け中）場合はシーンを再読み込み
			else if (message == "")
			{
				SceneManager.LoadScene(thisSceneName); // シーンを再読み込み
				wentNextScene = true;
			}
			// ニューゲームで上書きをキャンセルした場合は戻る
			else if (!isContinue && (message == SaveLoadFile.rewriteMessage
				|| message == SaveLoadFile.rewriteToBrokenFileMessage))
			{
				message = "";
				SetActiveDataNameInput(true);
			}
			// ロードエラーの時はメッセージを初期状態に戻す
			else if (isContinue && (message == SaveLoadFile.notExistFileMessage
				|| message == SaveLoadFile.cannotLoadBrokenFileMessage))
			{
				message = "";
				SetActiveDataNameInput(true);
			}
		}
	}

	/// <summary>
	/// プレイヤー名の入力ができるかどうかを切り替えます
	/// </summary>
	/// <param name="isActive">入力が有効かどうか</param>
	private void SetActiveDataNameInput(bool isActive)
	{
		if (!isActive)
		{
			virtualKeyboard.isActive = false;
			dataNameField.color = Color.red;
		}
		else
		{
			if (!isSkippedSelection) virtualKeyboard.isActive = true;
			dataNameField.color = Color.black;
		}
	}

	/// <summary>
	/// セーブデータ選択機能を行わない時のUI表示処理です
	/// </summary>
	/// <param name="isActive">表示するかどうか</param>
	private void SetDeactiveWhenSelectionSkkipped()
	{
		for (int i = 0; i < deactivateObjectsToSkippingSelection.Length; ++i)
		{
			deactivateObjectsToSkippingSelection[i].SetActive(false);
		}
	}
}
