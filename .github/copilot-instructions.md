# Copilot Instructions for TENTOBI (Unity)

## このドキュメントについて

- このファイルは GitHub Copilot / Copilot Chat が本リポジトリの文脈（ゲーム仕様・設計・慣習）を理解しやすくするための指示書です。
- 本リポジトリはサークル「グラビティ(Gravity)」が Unity 6 で開発している球体化ジャイロアクションゲーム
  「天童説を唱えた罰がぶっ飛んでいる件について」（略称：てんとび / TENTOBI）の開発データです。
- 現在の開発メンバーは `RadianN_kswg` と `yuufoor` の 2 名です。

---

## 前提条件（Copilot の基本ルール）

- 回答は必ず日本語で行ってください。
- 変更が大きくなりそうな場合（目安：差分 200 行以上 / 影響範囲が複数シーン・複数 Prefab に及ぶ / 既存挙動を変える可能性が高い）は、先に「やることリスト（計画）」を提示し、実行可否を確認してください。
- 不確かな点（対象シーン、タグ名、Prefab 参照、どのステージ番号に関係するか等）がある場合は、勝手に決めずに 1〜3 個の確認質問をしてください。
- 既存方針に合わせ、最小変更で目的を達成してください（過剰なリファクタリングや命名整理の一括変更をしない）。

---

## アプリ概要（ゲーム仕様の要点）

- 本作はスマートフォンの加速度（ジャイロ的操作）で重力方向を制御し、球体（Rigidbody2D）を転がして進行するアクションです。
- ステージには以下の要素が存在します。
  - リスポーン地点（ステージ中継ポイント）
  - ゴール（到達でステージクリア）
  - 地面（接地判定）
  - トラップ：即死（Kill）/ 継続ダメージ（Damage）
  - オブジェクト操作（CP 消費で対象物を動かす）
- 主要パラメータ
  - Life（残機）
  - HP（継続ダメージ耐久）
  - CP（操作力：オブジェクト操作のリソース）

---

## 技術スタック（エコシステム）

- Unity: 6000.2.10f1（Unity 6）
- 言語: C#（Unity 標準の MonoBehaviour 中心）
- UI: uGUI + TextMeshPro（`TMPro`）
- 2D: Rigidbody2D / Collider2D / Tilemap 系
- 入力:
  - ジャイロ/加速度: `Input.acceleration`
  - 汎用入力: `Input.GetMouseButtonDown`（タップ相当）
  - 仮想スティック等: SimpleInput（`Assets/Plugins/SimpleInput`）
- 主なパッケージ（抜粋）: Cinemachine, AI Navigation, Unity Test Framework

---

## ディレクトリ構成（重要）

### スクリプト

- ゲーム本体のスクリプトは主に以下にあります。
  - `Assets/TENTOBI Original/Scripts/Stage/` : ステージ進行・プレイヤー・重力・UI
    - `Gimmicks/` : ギミック系（`Hopper.cs`, `PressTrap.cs`, `RotatingBar.cs`）
    - `Objects/` : ステージ上のオブジェクト系（`Apple.cs`, `GoalPoint.cs`, `Infoboard.cs`, `Life.cs`, `ObjectControlChecker.cs`, `RespownPoint.cs`, `Score.cs`）
    - `PhysicsFields/` : 物理フィールド系（`WindTile.cs`）
    - `Tutorials/` : チュートリアル進行（`TutorialProgress.cs`）
    - 直下: `GameDirector.cs`, `GravityController.cs`, `GroundChecker.cs`, `ObjectController.cs`, `PlayerDirector.cs`, `StagePointDirector.cs`, `UIDirector.cs`
  - `Assets/TENTOBI Original/Scripts/Menu/` : ステージ選択等（`MainMenuDirector.cs`, `StageButtonDirector.cs`）
  - `Assets/TENTOBI Original/Scripts/Title/` : タイトル・セーブ選択（`TitleDirector.cs`, `SaveDataSelectDirector.cs`）
  - `Assets/TENTOBI Original/Scripts/SaveData/` : セーブ/ロード（`SaveLoadFile.cs`）
  - `Assets/TENTOBI Original/Scripts/` 直下: `TransitionDirector.cs`, `VersionViewer.cs`, `VirtualKeyboard.cs`

### シーン

- `Assets/TENTOBI Original/Scenes/`
  - `Title.unity` : タイトル画面
  - `Menu/` : メニュー・ステージ選択画面
  - `Stages/` : 正式ステージ（`Stage1.unity`, `Stage2.unity`, `Stage3.unity`, `SampleStage.unity`）
  - `Tutorials/` : チュートリアル（`Tutorial1.unity`, `Tutorial2.unity`）
  - `TrialStages forCreators/` : 制作者向けトライアルステージ（`TrialStage1 forCreators.unity`）
  - `TestSatge.unity` : テスト用シーン（typo のまま維持）

### Prefab

- `Assets/TENTOBI Original/Prefab/`
  - `Stage/` : ステージ用 Prefab
    - `Player.prefab`, `UIs.prefab`
    - `Gimmicks/` : `GoalPoint.prefab`, `Hopper (Power35).prefab`, `Press.prefab`, `RespownPoint (1).prefab`, `RotatingBar.prefab`
    - `Objects/` : `Apple (+50).prefab`, `Container #0.prefab`, `Infoboad (1).prefab`, `Life (+2).prefab`, `Score (+5).prefab`
    - `Objects/Controlables/` : `Container(Controlable) #0.prefab`
    - `Tutorials/` : チュートリアル用 Prefab
  - `Menu/` : メニュー用 Prefab
  - `SaveLoadFile.prefab` : セーブデータ管理（`DontDestroyOnLoad`）

### 開発者作業フォルダ（重要）

- `Assets/TENTOBI Original/_RadianN/` と `Assets/TENTOBI Original/_yuufoor/` は各メンバーの作業用フォルダです。
- **`_RadianN/` 内のデータを優先的に正として扱ってください**（同名シーン等が両フォルダに存在する場合、`_RadianN/` 側を参照してください）。
- これらフォルダ内のファイルを無断で編集・削除しないでください。
- `_RadianN/` には `Stage2.unity`, `_testStage_RadianN.unity` が存在します。

### 変更を避ける領域

- `Assets/Plugins/**`（例：SimpleInput）
- `Assets/Standard Assets/**`
- これらに手を入れる必要がある場合は、まず代替案（ラッパー/拡張/自前実装の追加）を検討し、必要性を説明してください。

---

## 既存アーキテクチャ / 重要コンポーネント

このプロジェクトは「シーン内のオブジェクトを Tag で検索し、必要な Director/Controller を取得して連携する」構成が中心です。

- ステージ中の中核

  - `GameDirector` : ステージ状態（`stageNum`, `scoreInStage`, `respownNum`, `life`, `HP`, `CP`）とゲームオーバー/クリア遷移。パラメータ上限（`life_max`, `HP_max`, `CP_max`）もここで管理。
  - `GravityController` : `Input.acceleration` から重力ベクトルを計算し `Rigidbody2D.AddForce` で印加（ポーズ時は速度を退避/復帰）。`rigitbody`（綴り揺れ）フィールドで `Rigidbody2D` を保持。
  - `GroundChecker` / `PlayerDirector` : 接地・ダメージ接触の判定
  - `StagePointDirector` : リスポーン位置と、動くオブジェクト（`movingObject[]`）の初期化
  - `UIDirector` : HUD（スコア/タイム/ライフ/HP バー/CP バー）、ポーズ、ゲームオーバー/クリア UI、ステージ遷移の UI 側制御
  - `TransitionDirector` : フェード等のトランジション（多くの箇所が前提にしている）
  - `ObjectController` : CP 消費によるオブジェクト操作（`ObjectControlChecker` と連携）

- ステージオブジェクト（主要）
  - `RespownPoint` : リスポーン地点（`pointIndex` で番号管理）
  - `GoalPoint` : ゴール判定（到達で `GameDirector.SwitchToGameClear()` 呼び出し）
  - `Hopper` : プレイヤーに反発力（`AddForce` + `ForceMode2D.Impulse`）を与えるギミック
  - `PressTrap` : プレスギミック
  - `RotatingBar` : 回転バーギミック
  - `WindTile` : 風力フィールド（Tilemap + `OnTrigger` 系 + `FixedUpdate` で `AddForce`）
  - `Apple` / `Life` / `Score` : アイテム系
  - `Infoboard` : 情報掲示板

- データ
  - `SaveLoadFile.instance` : セーブデータ（BinaryFormatter + `Application.dataPath`）を保持し `DontDestroyOnLoad`。シングルトンパターン（`Awake` で重複チェック）。
  - `MainMenuDirector.main` : ステージ選択表示のための保持クラス（`DontDestroyOnLoad`）

---

## シーン/タグ連携のルール（壊しやすいので重要）

多くのスクリプトが `GameObject.FindGameObjectWithTag(...)` を前提にしています。新規実装や改修では以下を厳守してください。

- 既存タグ名・シーン名を勝手に変更しない（変更が必要なら、影響範囲と移行手順を先に提示）。
- 新しい参照を増やす場合は、可能なら Inspector 参照（`[SerializeField]`）を優先し、Tag 探索追加は最小限にする。
- 既存の主要タグ（例）
  - `GameController`（`GameDirector` / `TitleDirector` 等の中核）
  - `Player`（プレイヤー球体。`Rigidbody2D`, `GravityController` 等を持つ）
  - `UI`（`UIDirector` を持つ UI ルートオブジェクト）
  - `TransitionDirector`
  - `StagePointDirector`
  - `GroundChecker`
  - `SaveDataSelecter`（綴り揺れあり、変更不可）
  - `PlayerRending`（`MeshRenderer` 参照。綴り揺れあり、変更不可）
  - `Ground`（地形 Tilemap の接地判定用）
  - `Kill Trap`（即死トラップ Tilemap）
  - `Damage Trap`（継続ダメージトラップ Tilemap）

---

## 実装スタイル（既存コードに合わせる）

- 既存スクリプトは日本語コメント、`#region`、`[Header]` を多用しています。新規/改修でも同じ粒度・書き方に寄せてください。
- 物理挙動（Rigidbody2D 操作）は基本 `FixedUpdate` で行う（例：`AddForce`, 速度代入）。
- Update/FixedUpdate の責務分離
  - 入力や状態遷移判定: `Update`
  - 力・速度の反映: `FixedUpdate`
- `Debug.Log` はデバッグ用途に限定し、恒常ログにならないよう配慮してください（必要ならフラグ化や条件付き出力）。
- スクリプトファイル冒頭の引用 URL コメント: 既存コードには `// 引用URL: https://... (YYYY.MM.DD)` 形式の参考文献コメントが付いているものがあります。新規スクリプトで外部資料を参照した場合は同形式で記載してください。
- 角度ベースの力計算は `CalcurateForceAndAngle()` という命名で `#region` 内に実装するパターンが多用されています（例：`GravityController`, `Hopper`, `WindTile`）。同様の計算を追加する場合はこの形に寄せてください（`Calcurate` の綴りは既存の誤字に統一）。
- `SaveLoadFile` のシングルトンは `instance` フィールドと `Awake` での重複チェック + `DontDestroyOnLoad` パターンで実装されています。同様のシングルトンを追加する場合もこの形に合わせてください。

---

## 命名法則・コーディング文法（`Assets/TENTOBI Original/Scripts` 分析ベース）

### 命名（既存コード準拠）

- **クラス名**: PascalCase（例：`GameDirector`, `GravityController`, `GroundChecker`）。
- **役割サフィックス**（よく出る命名）
  - `*Director`: ゲーム進行・UI・画面遷移などの統括（例：`GameDirector`, `UIDirector`, `TitleDirector`）
  - `*Controller`: 入力/操作/制御（例：`GravityController`, `ObjectController`）
  - `*Checker`: 判定系（例：`GroundChecker`, `ObjectControlChecker`）
  - `*Point`, `*Tile`, `*Bar`, `*Hopper` など、ステージ上のオブジェクトは名詞中心
- **メソッド名**:
  - Unity イベントは `Start` / `Update` / `FixedUpdate` を中心に使用。
  - public メソッドは PascalCase が多い（例：`SwitchToGameClear`, `ReturnPauseFlag`, `IsGround`）。
  - 一部で camelCase の public メソッドも存在（例：`returnThisStageViewID`）。既存クラスを拡張する場合は、そのクラス内の流儀に合わせてください。
- **フィールド名**:
  - private フィールドは lowerCamelCase が基本（例：`playerDir`, `isGameOver`, `wentNextScene`）。
  - bool は `isXxx` / `hasXxx` 系が多い（例：`isTutorial`, `isGround`, `isDamaged`）。
  - Inspector 公開用の `public` フィールドも lowerCamelCase が多い（例：`stageNum`, `hpBar`, `mainMenuSceneName`）。
- **定数（const）**:
  - `private const float flashT` のように lowerCamelCase の定数が既にあります。新規追加も「そのファイル内の既存スタイル」に寄せてください。

### 綴り揺れ・既存識別子の扱い（重要）

- 既存コードには綴り揺れ/誤字が含まれます。以下は確認済みの主要なものです：

  | 識別子 | 正しい綴り | 種別 |
  |---|---|---|
  | `Respown*` / `respownNum` | Respawn | フィールド名・メソッド名 |
  | `rigitbody` | rigidbody | `Rigidbody2D` フィールド名（複数ファイル） |
  | `Calcurate*` | Calculate | メソッド名（`CalcurateForceAndAngle` 等） |
  | `TestSatge` | TestStage | シーン名（`Scenes/TestSatge.unity`） |
  | `stageRength` | stageLength | `const` フィールド名（`SaveLoadFile.cs`） |
  | `Pless*` | Press | ギミック名（`PressTrap` 等） |
  | `isTutrial` | isTutorial | bool フィールド名 |
  | `SaveDataSelecter` | SaveDataSelector | クラス名・タグ名 |
  | `clockwize` | clockwise | 変数名 |

- これらはシーン参照・Prefab・他スクリプト呼び出し・タグ文字列と結びついている可能性があるため、**ユーザー確認なしにリネーム/修正しない**でください。
- 新規実装で同じ概念を参照する場合は、まず既存の綴りに合わせる（互換優先）。将来修正する場合は、影響範囲（参照先、Prefab、タグ、シーン）を調査して移行計画を提示してください。

### コーディング文法・記述スタイル

- **`#region` の使い方**: 以下のパターンが頻出します。新規コードでも同系統の命名で区切ってください。
  - `private変数定義 1/2`（複数ブロックに分割する場合は番号付き）
  - `Prefab内アタッチ用public変数定義`
  - `ステージ情報` / `パラメータの上限`
  - `〜計算`（例：`重力計算用変数`, `反発力計算`, `風力計算`）
  - `当たり判定`
  - `シングルトン化`
  - `セーブデータクラスの処理`
- **コメント**: `/// <summary>` の日本語コメントが多く、public フィールド/主要メソッドには説明が付く傾向があります。コメントは `/// <summary>` を `[Header]` の前ではなく後に書くケースも存在します（既存スタイルに合わせてください）。
- **Inspector 向け注釈**: `public` フィールドには `[Header("...")]` を付け、用途・単位・注意事項を日本語で明示する流儀です。
- **当たり判定フラグ**: `Enter/Stay/Exit` を別フラグで保持して、メソッド呼び出し時に集約して状態更新 → フラグをリセットするパターンが使われています（例：`IsGround`, `IsDamaged`, `IsInCollision`, `IsInWind`）。同様の判定ロジックを追加する場合はこの形に寄せてください。
- **`[SerializeField]` の使い分け**: Inspector 公開が必要な private フィールドには `[SerializeField]` を使用してください。ただし既存クラスでは `public` フィールドで Inspector 公開しているケースが大半のため、既存クラスに追加する際はそのクラス内の慣習に合わせてください。
- **`const` フィールド**: `private const int stageRength = 256` のように lowerCamelCase が使われています（PascalCase の `const` も一部存在するため、ファイル内既存スタイルに寄せてください）。

---

## テスト方針（現状に合わせた現実的運用）

- 現状はプレイ挙動中心のため、自動テストは必須ではありません。
- ただし「純粋関数化できる計算（例：重力ベクトル計算、スコア集計）」を新規に追加する場合は、Unity Test Framework（EditMode）での最小テスト追加を提案してもよいです。
- テストを追加する場合は `Assets/TENTOBI Original/Tests/EditMode/` 配下に `*Tests.cs` の命名で配置してください（ディレクトリが存在しない場合は作成してください）。
- パッケージ: `com.unity.test-framework` は既に `Packages/manifest.json` に含まれています（追加不要）。

---

## アンチパターン（やらないでほしいこと）

- 既存挙動を変える大規模リファクタ（入力系の総入れ替え、タグ参照の全面撤去、シリアライズ形式変更など）を、ユーザー確認なしに実施しない。
- 新しい外部パッケージ/アセットを勝手に導入しない。
- `Assets/Plugins` や `Assets/Standard Assets` を理由なく編集しない。
- セーブデータの形式（BinaryFormatter）や保存先（`Application.dataPath`）を、互換性検討なしに変更しない。
- `Assets/TENTOBI Original/_yuufoor/` のシーン/アセットを `_RadianN/` の確認なしに正として扱わない（同名リソースは `_RadianN/` 側を優先）。
- 綴り揺れ識別子（`Respown*`, `rigitbody`, `Calcurate*`, `Satge`, `isTutrial` 等）を、ユーザー確認なしに一括リネームしない。
- `MonoBehaviour` のライフサイクル外（`Awake`/`Start` より前）でコンポーネント参照を取得しない。
- `Update` 内で毎フレーム `GetComponent<T>()` や `FindGameObjectWithTag()` を呼び出さない（必ず `Start` でキャッシュする）。

---

## 変更提案時の出力フォーマット（推奨）

- 何をどこに変更するか（ファイル単位）
- 既存挙動への影響
- 動作確認手順（Unity Editor で再生して確認できるチェック項目）
