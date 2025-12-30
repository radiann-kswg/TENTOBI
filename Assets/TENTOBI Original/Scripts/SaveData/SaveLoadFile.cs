// 引用URL: https://gametukurikata.com/program/savedata (2020.09.01)
// 引用URL: https://dkrevel.com/makegame-beginner/make-2d-action-game-manager/ (2020.09.12)

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class SaveLoadFile : MonoBehaviour
{
    #region シングルトン化
    public static SaveLoadFile instance = null;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }
    #endregion

    #region データを保存/反映する際に用いる変数
    /// <summary>
    /// 保存時に正常なファイルに上書きするかを確認するメッセージです
    /// </summary>
    public const string rewriteMessage = "すでにセーブデータが存在します。\n上書きしますか?";
    /// <summary>
    /// 保存時に破損の恐れがあるファイルに上書きするかを確認するメッセージです
    /// </summary>
    public const string rewriteToBrokenFileMessage
        = "セーブデータを読み込めません。\n新しく上書きしますか？";
    /// <summary>
    /// 読み込み時にファイルが存在しない場合に表示するメッセージです
    /// </summary>
    public const string notExistFileMessage = "セーブデータが存在しません。";
    /// <summary>
    /// 読み込み時に破損の恐れなどによりファイルを読み込めない場合に表示するメッセージです
    /// </summary>
    public const string cannotLoadBrokenFileMessage
        = "セーブデータを読み込めませんでした。\nセーブデータが破損している恐れがあります。";
    #endregion ここまで

    /// <summary>
    /// セーブデータ名です
    /// </summary>
    public string savedataName;

    /// <summary>
    /// セーブデータファイルの処理時に使用するFileStreamクラスです
    /// </summary>
    private FileStream fileStream;

    /// <summary>
    /// バイナリフォーマッターです
    /// </summary>
    private BinaryFormatter bf;

    /// <summary>
    /// SaveData(セーブデータ)クラスです
    /// ここにセーブデータが保存されます
    /// </summary>
    public Savedata savedata;

    /// <summary>
    /// ステージ総数です(新規セーブデータの作成に用いるのでむやみに変更しないこと)
    /// 今後のステージ実装を見越し多めに取っています(0番目はカウントしないので最大255ステージ)
    /// </summary>
    private const int stageRength = 256;
    
    private void Start()
    {
        InitializeData2NewGame();
    }

    #region セーブデータクラスの処理
    /// <summary>
    /// セーブデータクラス
    /// </summary>
    [Serializable]
    public class Savedata
    {
        /// <summary>
        /// ステージクリア進捗（チュートリアル含む, ステージ選択画面で使用）
        /// </summary>
        public int stageProgressNum;
        /// <summary>
        /// 入手スコア総数（チュートリアルを除く）
        /// </summary>
        public int gottenScore;
        /// <summary>
        /// ステージごとの入手スコア（チュートリアルを除く；ステージ番号と添え字を合わせるため、添え字0は使用しない)
        /// ※↑ステージ番号0はテスト用ステージに使用
        /// (ステージ番号の指定を行う必要がない & ステージクリア進捗0はステージをすべて攻略していない時として扱う ため)
        /// </summary>
        public int[] scorePerSatge;
    }

    /// <summary>
    /// ニューゲームを開始する際にセーブデータクラスを初期化します
    /// </summary>
    public void InitializeData2NewGame()
    {
        instance.savedata.stageProgressNum = 0;
        instance.savedata.gottenScore = 0;
        instance.savedata.scorePerSatge = new int[stageRength];

        instance.savedataName = "";
    }

    /// <summary>
    /// 入手スコア総数を再計算します
    /// </summary>
    public void UpdateGottenScore()
    {
        int progress = instance.savedata.stageProgressNum;
        var stage = MainMenuDirector.main.stageViewList[progress];
        while (stage.isTutrial)
        {
            --progress;
            stage = MainMenuDirector.main.stageViewList[progress];
        }
        int result = 0;
        for (int i = 1; i <= stage.stageNum; ++i)
        {
            result += instance.savedata.scorePerSatge[i];
        }
        if (result > instance.savedata.gottenScore)
        {
            instance.savedata.gottenScore = result;
            //instance.SaveDataToFile();
        }
    }

    #endregion ここまで

    /// <summary>
    /// セーブデータを保存する処理に移行します
    /// </summary>
    ///<returns>保存する際の確認メッセージ(何もない場合は確認なし)</returns>
    public string ReturnMessageToSaveData()
    {
        bf = new BinaryFormatter();
        fileStream = null;

        // 上書きになるかどうかの判定
        bool[] isSaveDataExist = IsSaveDataExist();

        if (isSaveDataExist[0])
        {
            if (!isSaveDataExist[1]) return rewriteMessage;
            else return rewriteToBrokenFileMessage;
        }
        return "";
    }

    /// <summary>
    /// ファイルにセーブデータを保存します
    /// </summary>
    public void SaveDataToFile()
    {
        try
        {
            // ゲームフォルダにセーブデータファイルを作成
            fileStream = File.Create(Application.dataPath + "/savedata_" + instance.savedataName + ".dat");

            // ファイルにクラスを保存
            bf.Serialize(fileStream, savedata);
        }
        catch (IOException e1)
        {
            Debug.Log("ファイルオープンエラー");
        }
        finally
        {
            if (fileStream != null)
            {
                fileStream.Close();
            }
        }
    }

    /// <summary>
    /// セーブデータを読み込む処理に移行します
    /// </summary>
    ///<returns>ロード時のエラーメッセージ(何もない場合は正常にロードできる)</returns>
    public string ReturnMessageToLoadData()
    {
        bf = new BinaryFormatter();
        fileStream = null;

        // 上書きになるかどうかの判定
        bool[] isSaveDataExist = IsSaveDataExist();

        if (!isSaveDataExist[0]) return notExistFileMessage;
        if (isSaveDataExist[1]) return cannotLoadBrokenFileMessage;
        return "";
    }

    /// <summary>
    /// セーブデータをファイルからロードします
    /// </summary>
    public void LoadDataFromFile()
    {
        bf = new BinaryFormatter();
        fileStream = null;

        try
        {
            // ファイルを読み込む
            fileStream = File.Open(Application.dataPath + "/savedata_" + instance.savedataName + ".dat", FileMode.Open);

            // 読み込んだデータをデシリアライズ
            instance.savedata = bf.Deserialize(fileStream) as Savedata;
        }
        catch (FileNotFoundException e1)
        {
            Debug.Log("ファイルがありません");
        }
        catch (IOException e2)
        {
            Debug.Log("ファイルオープンエラー");
        }
        finally
        {
            if (fileStream != null)
            {
                fileStream.Close();
            }
        }
    }

    /// <summary>
    /// すでに同じ名前のセーブデータファイルについて確認します
    /// </summary>
    /// <returns>(配列長さ:2) ファイルが存在するかどうか(#0)；データが破損している恐れがあるか(#1)</returns>
    private bool[] IsSaveDataExist()
    {
        bool[] res = { true, false };
        try
        {
            // ファイルを読み込む
            fileStream = File.Open(Application.dataPath + "/savedata_" + savedataName + ".dat", FileMode.Open);

            // 読み込んだデータを仮のセーブデータクラスでデシリアライズ
            Savedata dataCheck = bf.Deserialize(fileStream) as Savedata;
        }
        catch (FileNotFoundException e1)
        {
            Debug.Log("ファイルがありませんでした");
            res[0] = false;
        }
        catch (IOException e2)
        {
            Debug.Log("ファイルオープン検証の結果、読み込みに失敗しました");
            res[1] = true;
        }
        finally
        {
            if (fileStream != null)
            {
                fileStream.Close();
            }
        }

        return res;
    }
}