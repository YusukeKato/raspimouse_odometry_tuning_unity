# raspimouse_odometry_tuning_unity
Unity用パッケージ

## 動作時の参考動画
YouTube : https://youtu.be/QYnU6PeEx8s

<img src=https://github.com/YusukeKato/Images_Repository/blob/master/ROCLinearTest_1on3_10fps.gif width="640px">

## 動作確認した環境

- Unity 2018.3.0f2
- Vuforia7.5.26 (Assets>Resources>VuforiaConfigurationで確認)
- iPad第六世代(iOS12)

すでにVuforiaがインストールされていることが前提

## パッケージをUnityプロジェクトへインポート
1. Unityで新規プロジェクト作成
2. https://github.com/YusukeKato/raspimouse_odometry_tuning_unityをダウンロード
3. そのディレクトリの中にあるROTPackage>rot.unitypackageをインポート

## websocket-sharpをインポート
1. websocket-sharpをダウンロード
2. websocket-sharp.slnをビルドしてインポート(以下の記事を参考)

https://qiita.com/oishihiroaki/items/bb2977c72052f5dd5bd9

## Vuforiaのオブジェクトを配置
1. (Project)Assets>Scenes>MainSceneを開く 
2. (上部タブ)GameObject>Vuforia>ARCameraを選択(Import)
3. Assets>Vuforia>Scripts>DefaultTrackableEventHandler.csを削除
4. ROTPackageに入っている二つのスクリプトをAssets>Vuforia>Scriptsの中へコピー
5. GameObject>Vuforia>Imageを選択(Import)

## VuforiaConfiguration
1. (Hierarchy)ARCameraをクリック。Inspectorの下の方にある「Open Vuforia Configuration」ボタンをクリック。
2. 「Track Device Pose」という項目にチェック（バージョンによって名前が違うかも）。また、このときに「Tracking mode」が「POSITIONAL」になっていることを確認。

## ROSConnectorへの関連付け
1. (Hierarchy)ROSConnectorをクリック。InspectorのScriptの項目の右側にある二重丸みたいなのをクリック。選択画面が出てくるので「ROSConnector」を選択。
2. Inspectorで以下のように設定

＊ロボット側のIPアドレスもここで入力できる(はず)

- ImageTarget -> ImageTarget
- ParentObject -> ParentObject
- ArrowBluePrefab -> Arrows-blue
- ArrowRedPrefab -> Arrows-red

- ConnectButtonText -> ConnectText
- StartButtonText -> StartText
- XZText -> TextXZ
- YText -> TextY
- IsStartText -> TextIsStart
- IsMarkerFoundText -> TextIsFound
- CalibrationFlagText -> CalibrationFlagText

＊同じ名前があったらどちらでも大丈夫なはず

## ImageTargetへの関連付け
1. (Hierarchy)ImageTargetをクリック。
2. Inspectorで以下のように設定。

- ImageTarget -> ImageTarget
- ParentObject -> ParentObject
- MarkerPrefab -> Marker
- RosConnector -> ROSConnector

3. 親がImageTarget、子がAssets>Prefabs>Markerとなるように設定。Markerをクリックで掴んでHierarchyのImageTargetに入れる。

## ImageTargetのスケール調整
実際に使用する画像の大きさを計測して、ImageTargetのScaleに入力（単位はメートル）

## ビルドセッティング
1. File>BuildSettingを開く
2. PlatformをiOSに変更
3. PlayerSettingsボタンをクリック
4. Inspectorの一番上にある(1)CompanyNameと(2)ProductNameを適当に入力
5. OtherSettingの中のBundleIdentifierの項目に「com.(1).(2)」となるように入力
6. XRSettingsでVuforiaAugmentedRealityにチェック

## ビルド
ビルドする

## アプリの説明

右のパネル

- CONNECTボタン：ロボットへ接続（rosbridgeのサーバを立てておく）
- STARTボタン：矢印のオブジェクトを出せるようになる
- RESETボタン：矢印のオブジェクトを全て削除
- Signalボタン：ROSへ合図を送る(topic)
- Anchorボタン：絶対座標系の原点をマーカの中心に合わせ直す

左のパネル

- 絶対座標におけるマーカの中心の座標
	- 一行目：XとZ（単位はメートル）
	- 二行目：Y（単位はdegree、度）
- 三行目：スタートしているか
- 四行目：マーカを認識したか
