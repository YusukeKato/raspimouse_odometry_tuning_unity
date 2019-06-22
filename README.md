# raspimouse_odometry_tuning_unity
ARライブラリを用いて移動ロボットの動きを補正するアプリケーションです。  
https://github.com/YusukeKato/raspimouse_odometry_tuning_ros と組み合わせて使用します。

## 機能
- ロボットのオドメトリを可視化（画像の赤色の矢印）
- ロボットが実際に移動した経路を可視化（画像の青色の矢印）
- ロボットの動きをキャリブレーション（修正中のため使用できません）

<img src=https://github.com/YusukeKato/Images_Repository/blob/master/DemoMovieSquare4.gif width=34%>

## 動画

* アプリ使用デモ
  * [![YouTube](https://img.youtube.com/vi/QYnU6PeEx8s/0.jpg)](https://www.youtube.com/watch?v=QYnU6PeEx8s)
  * [![YouTube](https://img.youtube.com/vi/WzJxbKnnwyw/0.jpg)](https://www.youtube.com/watch?v=WzJxbKnnwyw)
* 環境構築方法
  * [![YouTube](https://img.youtube.com/vi/bdhoh1FPSFs/0.jpg)](https://www.youtube.com/watch?v=bdhoh1FPSFs)

## 動作確認した環境
- Unity
	- 2019.1.0f2
- Vuforia
	- 7.5.26
	- 8.1.7
- デバイス
	- iPad 第5世代(iOS12)
	- iPad 第6世代(iOS12)

すでに Vuforia がインストールされていることが前提です。インストールは以下を参考にしてください。

* https://docs.unity3d.com/ja/current/Manual/GettingStartedInstallingHub.html
* https://library.vuforia.com/articles/Training/getting-started-with-vuforia-in-unity.html

## パッケージを Unity プロジェクトへインポート
1. https://github.com/YusukeKato/raspimouse_odometry_tuning_unity をダウンロード
2. ダウンロードしたUnityプロジェクトをUnityから開く

## Vuforiaのオブジェクトを配置
1. (Project) Assets > Scenes > MainScene を開く 
2. (上部タブ) GameObject > Vuforia > ARCamera を選択 (Importする)
3. 同じようにGameObject > Vuforia > Image を選択 (Importする)
4. ImageTarget の コンポーネントである
"DefaultTrackableEventHandler"スクリプトを削除（歯車マークを押してremove）
5. Assets > Scripts > CustomTrackableEventHandler を 
ImageTarget へアタッチ（Add Component でもできる）

<img src=./Figs/fig1.png width=40%>

## ビルドセッティング
1. File > BuildSetting を開く
2. 「Add Open Scenes」ボタンを押してシーンを登録
3. Platform を iOS に変更
4. Player Settings ボタンをクリック
5. Inspector の一番上にある (1)CompanyName と (2)ProductName を適当に入力
6. OtherSetting の中の BundleIdentifier の項目に「com.(1).(2)」となるように入力
7. XRSettings で Vuforia Augmented Reality Supportedにチェック

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

## ImageTarget への関連付け
1. (Hierarchy) ImageTarget をクリック
2. Inspector で以下のように設定

- ImageTarget -> ImageTarget
- ParentObject -> ParentObject
- MarkerPrefab -> Marker
- RosConnector -> ROSConnector

<img src=./Figs/fig3.png width=40%>

3. 親がImageTarget、子がAssets > Prefabs > Marker となるように設定する。
Marker をクリックで掴んで Hierarchy の ImageTarget に入れる。

## 各ボタンのイベント設定
Canvas > Panel の中にある各Button の Inspector の一番下の項目
「On Click ()」で図のようにイベントを設定する。

（図はConnectButtonの場合）

<img src=./Figs/fig4.png width=40%>

- ConnectButton -> ROSConnector > ConnectButton
- StartButton -> ROSConnector > StartButton
- MenuButton -> ROSConnector > ArrowResetButton
- CalibrationFlagButton -> ROSConnector > CalibrationFlagButton
- WorldAnchorButton -> ROSConnector > WorldAnchorButton

## ImageTarget の設定
Image Target BehhaviourのDatabaseをVuforiaMars_Imagesにする

## ImageTarget のスケール調整
実際に使用する画像の大きさを計測して、
ImageTarget の Scale に入力（単位はメートル）

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
