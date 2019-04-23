# raspimouse_odometry_tuning_unity
ARライブラリを用いて移動ロボットの動きを補正するアプリケーションです。

## 機能
- ロボットのオドメトリを可視化（画像の赤色の矢印）
- ロボットが実際に移動した経路を可視化（画像の青色の矢印）
- ロボットの動きをキャリブレーション（修正中のため使用できません）

<img src=https://github.com/YusukeKato/Images_Repository/blob/master/ROC_SampleImage.png width=34%>

## 動画
YouTube : https://youtu.be/QYnU6PeEx8s

## 動作確認した環境
- Unity
	- 2018.3.0f2
	- 2019.1.0f2
- Vuforia
	- 7.5.26
	- 8.1.7
- デバイス
	- iPad 第六世代(iOS12)

すでに Vuforia がインストールされていることが前提

## パッケージを Unity プロジェクトへインポート
1. Unity で新規プロジェクト作成
2. https://github.com/YusukeKato/raspimouse_odometry_tuning_unity をダウンロード
3. そのディレクトリの中にある RaspimouseOdomTuning.unitypackage をインポート

## Vuforiaのオブジェクトを配置
1. (Project) Assets > Scenes > MainScene を開く 
2. (上部タブ) GameObject > Vuforia > ARCamera を選択 (Importする)
3. 同じようにGameObject > Vuforia > Image を選択 (Importする)
4. ImageTarget の コンポーネントである
"DefaultTrackableEventHandler"スクリプトを削除（歯車マークを押してremove）
5. Assets > Scripts > CustomTrackableEventHandler を 
ImageTarget へアタッチ（Add Component でもできる）

## VuforiaConfiguration
1. Window > Vuforia Configuration を選択
2. 「Track Device Pose」という項目にチェック
（バージョンによって名前が違うかもしれない）。
また、このときに「Tracking mode」が「POSITIONAL」になっていることを確認。

* Vuforia Configuration が見つからない、設定できない場合は、
 File > Build Settings > Player Settings > XR Settings > Vuforia... にチェック

## ROSConnector への関連付け
1. (Hierarchy) ROSConnectorをクリック。
Inspector の Script の項目の右側にある二重丸みたいなのをクリック。
選択画面が出てくるので「ROSConnector」を選択。
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

## ImageTarget への関連付け
1. (Hierarchy) ImageTarget をクリック
2. Inspector で以下のように設定

- ImageTarget -> ImageTarget
- ParentObject -> ParentObject
- MarkerPrefab -> Marker
- RosConnector -> ROSConnector

3. 親がImageTarget、子がAssets > Prefabs > Marker となるように設定する。
Marker をクリックで掴んで Hierarchy の ImageTarget に入れる。

## ImageTarget のスケール調整
実際に使用する画像の大きさを計測して、
ImageTarget の Scale に入力（単位はメートル）

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
