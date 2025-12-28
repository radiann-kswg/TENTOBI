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

- ゲーム本体のスクリプトは主に以下にあります。
  - `Assets/TENTOBI Original/Scripts/Stage` : ステージ進行・プレイヤー・重力・UI・ギミック
  - `Assets/TENTOBI Original/Scripts/Menu` : ステージ選択等
  - `Assets/TENTOBI Original/Scripts/Title` : タイトル・セーブ選択
  - `Assets/TENTOBI Original/Scripts/SaveData` : セーブ/ロード
- 変更を避ける領域（原則）
  - `Assets/Plugins/**`（例：SimpleInput）
  - `Assets/Standard Assets/**`
  - これらに手を入れる必要がある場合は、まず代替案（ラッパー/拡張/自前実装の追加）を検討し、必要性を説明してください。

---

## 既存アーキテクチャ / 重要コンポーネント

このプロジェクトは「シーン内のオブジェクトを Tag で検索し、必要な Director/Controller を取得して連携する」構成が中心です。

- ステージ中の中核

  - `GameDirector` : ステージ状態（stageNum, score, respawn, life, HP, CP）とゲームオーバー/クリア遷移
  - `GravityController` : `Input.acceleration` から重力ベクトルを計算し `Rigidbody2D.AddForce` で印加（ポーズ時は速度を退避/復帰）
  - `GroundChecker` / `PlayerDirector` : 接地・ダメージ接触の判定
  - `StagePointDirector` : リスポーン位置と、動くオブジェクトの初期化
  - `UIDirector` : HUD、ポーズ、ゲームオーバー/クリア UI、ステージ遷移の UI 側制御
  - `TransitionDirector` : フェード等のトランジション（多くの箇所が前提にしている）

- データ
  - `SaveLoadFile.instance` : セーブデータ（BinaryFormatter）を保持し `DontDestroyOnLoad`
  - `MainMenuDirector.main` : ステージ選択表示のための保持クラス（`DontDestroyOnLoad`）

---

## シーン/タグ連携のルール（壊しやすいので重要）

多くのスクリプトが `GameObject.FindGameObjectWithTag(...)` を前提にしています。新規実装や改修では以下を厳守してください。

- 既存タグ名・シーン名を勝手に変更しない（変更が必要なら、影響範囲と移行手順を先に提示）。
- 新しい参照を増やす場合は、可能なら Inspector 参照（`[SerializeField]`）を優先し、Tag 探索追加は最小限にする。
- 既存の主要タグ（例）
  - `GameController`（GameDirector / TitleDirector 等の中核）
  - `Player`
  - `UI`
  - `TransitionDirector`
  - `StagePointDirector`
  - `GroundChecker`
  - `SaveDataSelecter`
  - `PlayerRending`（MeshRenderer 参照）

---

## 実装スタイル（既存コードに合わせる）

- 既存スクリプトは日本語コメント、`#region`、`[Header]` を多用しています。新規/改修でも同じ粒度・書き方に寄せてください。
- 物理挙動（Rigidbody2D 操作）は基本 `FixedUpdate` で行う（例：`AddForce`, 速度代入）。
- Update/FixedUpdate の責務分離
  - 入力や状態遷移判定: `Update`
  - 力・速度の反映: `FixedUpdate`
- `Debug.Log` はデバッグ用途に限定し、恒常ログにならないよう配慮してください（必要ならフラグ化や条件付き出力）。

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

- 既存コードには綴り揺れ/誤字が含まれます（例：`Respown*`, `rigitbody`, `Calcurate*`, `Satge`, `Pless*`, `isTutrial`, `SaveDataSelecter`, `clockwize`）。
- これらはシーン参照・Prefab・他スクリプト呼び出し・タグ文字列と結びついている可能性があるため、**ユーザー確認なしにリネーム/修正しない**でください。
- 新規実装で同じ概念を参照する場合は、まず既存の綴りに合わせる（互換優先）。将来修正する場合は、影響範囲（参照先、Prefab、タグ、シーン）を調査して移行計画を提示してください。

### コーディング文法・記述スタイル

- **`#region` の使い方**: `private変数定義 1/2`、`Prefab内アタッチ用public変数定義`、`〜計算`、`当たり判定` のように、まとまりで区切るパターンが多いです。
- **コメント**: `/// <summary>` の日本語コメントが多く、public フィールド/主要メソッドには説明が付く傾向があります。
- **Inspector 向け注釈**: `public`フィールドには `[Header("...")]` を付け、用途や単位、注意事項を日本語で明示する流儀です。
- **当たり判定フラグ**: `Enter/Stay/Exit` を別フラグで保持して、メソッド呼び出し時に集約して状態更新 → フラグをリセットするパターンが使われています（例：`IsGround`, `IsDamaged`, `IsInCollision`）。同様の判定ロジックを追加する場合はこの形に寄せてください。

---

## テスト方針（現状に合わせた現実的運用）

- 現状はプレイ挙動中心のため、自動テストは必須ではありません。
- ただし「純粋関数化できる計算（例：重力ベクトル計算、スコア集計）」を新規に追加する場合は、Unity Test Framework（EditMode）での最小テスト追加を提案してもよいです。

---

## アンチパターン（やらないでほしいこと）

- 既存挙動を変える大規模リファクタ（入力系の総入れ替え、タグ参照の全面撤去、シリアライズ形式変更など）を、ユーザー確認なしに実施しない。
- 新しい外部パッケージ/アセットを勝手に導入しない。
- `Assets/Plugins` や `Assets/Standard Assets` を理由なく編集しない。
- セーブデータの形式（BinaryFormatter）や保存先（`Application.dataPath`）を、互換性検討なしに変更しない。

---

## 変更提案時の出力フォーマット（推奨）

- 何をどこに変更するか（ファイル単位）
- 既存挙動への影響
- 動作確認手順（Unity Editor で再生して確認できるチェック項目）
