using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StageButtonDirector : MonoBehaviour
{
	#region Prefab用変数定義
	[Header("ステージ名表記用Text")]
	/// <summary>
	/// ステージ名が表記されるTextです
	/// </summary>
	public TextMeshProUGUI stageNameText;

	[Header("ハイスコア表記用Text")]
	/// <summary>
	/// ステージのハイスコアが表記されるTextです
	/// </summary>
	public TextMeshProUGUI stageScoreText;

	[Header("ステージ表記用サムネイル（Image）")]
	/// <summary>
	/// ステージ表記に使用するサムネイル画像です
	/// </summary>
	public Image stageImage = null;
	#endregion

	#region private変数定義
	[Header("識別用ID")]
	/// <summary>
	/// このボタンを識別するIDです
	/// </summary>
	public int buttonID = 0;

	/// <summary>
	/// このボタンのステージを識別するIDです
	/// </summary>
	private int thisStageViewID;

	/// <summary>
	/// 次のシーンに遷移させているか
	/// </summary>
	private bool wentNextScene;

	/// <summary>
	/// TransitionDirectorコンポーネント
	/// </summary>
	private TransitionDirector transition;

	/// <summary>
	/// 次に遷移するシーン名
	/// </summary>
	private string nextSceneName;
	#endregion

	// Start is called before the first frame update
	void Start()
	{
		transition = GameObject.FindGameObjectWithTag("TransitionDirector").GetComponent<TransitionDirector>();

		// ステージ情報を反映
		SetStageButtonActive();
		wentNextScene = false;
	}

	// Update is called once per frame
	void Update()
	{
		if (wentNextScene)
		{
			if (transition.IsFadeOutComplete()) SceneManager.LoadScene(nextSceneName);
		}
	}

	/// <summary>
	/// このステージ選択ボタンが押された時の処理です
	/// </summary>
	public void OnStageButtonClick()
	{
		if (transition.IsFadeInComplete())
		{
			// ボタンの持つステージ情報を入手
			var thisStage = MainMenuDirector.main.stageViewList[thisStageViewID];

			// シーン名を計算
			string sceneName;
			if (thisStage.isTutrial) sceneName = "Tutorial" + thisStage.stageNum.ToString();
			else sceneName = "Stage" + thisStage.stageNum.ToString();
			nextSceneName = sceneName;

			// 次のシーンへ
			transition.StartFadeOut();
			wentNextScene = true;
		}
	}

	/// <summary>
	/// ページ変更時などにステージ選択ボタンの表示を切り替えます
	/// </summary>
	private void SetStageButtonActive()
	{
		// ステージ選択画面表示用クラスの添え字を計算
		thisStageViewID = MainMenuDirector.main.returnThisStageViewID(buttonID);

		// ステージ選択画面表示用クラスのデータ数以内 かつステージ攻略範囲内or1面先 である場合
		if (thisStageViewID < MainMenuDirector.main.stageViewList.Length
			&& thisStageViewID <= SaveLoadFile.instance.savedata.stageProgressNum)
		{
			// 表示
			gameObject.SetActive(true);
			// ボタンの持つステージ情報を入手
			var thisStage = MainMenuDirector.main.stageViewList[thisStageViewID];

			// ステージ名を反映
			if (!thisStage.isTutrial)
			{
				stageNameText.text = "Stage " + thisStage.stageNum.ToString("D2");
				if (thisStageViewID == SaveLoadFile.instance.savedata.stageProgressNum) // 未攻略（=ステージ攻略範囲内1面先）の場合
					stageScoreText.text = "Next >>";
				else
					stageScoreText.text = "HighScore: " + SaveLoadFile.instance.savedata.scorePerStage[thisStage.stageNum].ToString("D4");
			}
			else
			{
				stageNameText.text = "Tutorial " + thisStage.stageNum.ToString("D2");
				if (thisStageViewID == SaveLoadFile.instance.savedata.stageProgressNum) // 未攻略（=ステージ攻略範囲内1面先）の場合
					stageScoreText.text = "Next >>";
				else
					stageScoreText.text = "Completed";
			}
			// サムネイル画像があれば反映
			if (thisStage.stageImage) stageImage = thisStage.stageImage;
		}
		// 範囲外だった場合
		else
		{
			// 非表示
			gameObject.SetActive(false);
		}
	}
}
