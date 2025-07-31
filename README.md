作品名「灯すな(Don't Light)」
---------------------------------------------------------------------------
作品概要：

>暗闇の中で光を避けながら戦う、2～4人対戦型のアクションゲームです。
>
>プレイヤーは俯瞰視点で操作され、一定時間ごとに手に入る電球を投げてマップを照らすことができます。
>
>また、常時使用可能な懐中電灯も持っていますが、いずれの光源にも触れるとダメージを受けるため、戦略的な光の使い方と回避行動が求められます。
>
>時間が経過するにつれて安全地帯が減少し、最終的に最後まで生き残った1人が勝者となります。
>
>シンプルなルールながら、攻防における光と影の駆け引きが熱い駆け引きを生み出します。

開発環境：

>Unity　6000.0.23f1

----------------------------------------------------------------------------
操作方法：

マウス/キーボード			 
---------------------------------------	
>WASD		移動
>
>マウス		照準
>
>左クリック	充電
>
>右クリック	懐中電灯点滅
>
>Space		電球投げ
>
>F		投げキャンセル		
					
ゲームパッド（Xbox例）
---------------------------------------	
>左スティック		移動
>
>右スティック		照準
>
>R1/X			充電
>
>R2			懐中電灯点滅
>
>L1			電球投げ
>
>L2			ロックオン切り替え
>
>B			投げキャンセル

----------------------------------------------------------------------------
**メンバー・担当箇所・PRポイント**

*******************************************
SonSui
======================
担当箇所

>1．電球・懐中電灯、LightManagerによる光源管理と当たり判定
>
>2．イベントバス
>
>3．ゲーム状態管理システム（GameManager）
>
>4．UI(タイトル、ローカルロビー、メインゲーム、リザルト)
>
>5．プレイヤーの入力デバイス管理システム
>
>6．プレイヤーのパラメーター管理、照準機能（一部）、操作（充電、照準）、アニメーション
>
>7．サウンド
>
>8．BGM管理システム
>
>9．背景レイアウト（タイトル、リザルト）
>
>10．エフェクト（被弾、回復、リザルト）

======================

アピールポイント

>1．モジュール間の疎結合を実現するため、イベントバス（GameEvents.cs）を実装。
ゲーム状態管理、UIイベント、プレイヤー生成や状態管理、当たり判定効率化など、多くの機能をイベントバス経由で統合し、拡張性を向上させました。
>
>2．電球生成時にEventを利用してLightManagerに登録し、Colliderを使わずに当たり判定を効率化しました。
>
>3．ステートマシンを用いてゲーム状態管理およびBGM管理システムを実装し、状態遷移の明確化と拡張性の向上を実現しました。
>
>4．UIアニメーション効果や充電用シェーダを実装し、演出面の向上を図りました。

********************************************
dai8862
======================
担当箇所

>1．マップ生成システム
>
>2．ゲームシーンの背景レイアウト
>
>3．マップ素材整理

======================

アピールポイント

>外部ファイルを用いた地面タイル・壁の自動生成システムを構築。
Prefabの差し替えだけでマップ素材の調整が可能な柔軟な設計にしました。

********************************************
blackpaper-art
======================
担当箇所

>1．プレイヤー機能実装
>
>2．電球投擲機能実装

======================

アピールポイント

>1．ゲームパッドとキーボード/マウスの両方に対応したデュアル入力システムをサポートし、プレイヤーの入力に応じて自動的に適切な入力方式に切り替える。
>
>2．グレネードの投擲範囲インジケーターと、ターゲット位置を示すポイントインジケーターを作成しました。 さらに、LineRenderer を使って投擲軌道のラインインジケーターも実装。
>
>3．ゲームパッドプレイヤーに優しい機能として、敵への自動ロックオン機能も対応。

********************************************
88814588nujam
======================
担当箇所

>1．オンライン対戦機能
>
>2．オンライン対戦ロビーのUI、レイアウト
>
>3．オンライン対戦ルーム
>
>4．シーン遷移時のフェード機能
>
>5．ローカル対戦ロビーのUI（一部）

======================

アピールポイント

>1．LAN環境では、各プレイヤーはゲームロビーで作成されたルームからのリアルタイムブロードキャストを受信できる（リスニングポート8888）。
>
>2．LAN環境では、ゲームルームを作成したプレイヤーは、ルーム情報をロビーにリアルタイムでブロードキャストする（プレイヤー数、マップ情報、プレーヤーたちが待ってしているかゲーム中のかなどの情報、画面に表示されないIPなどの情報を暗黙的に提供もできる）。
>
>3．LAN 環境のルームシーンでは、3D モデリングを使用して、プレイヤーがゲームの前にウォームアップのやり取りを行えるようにする。
>
>4．2Dシーンでも3Dシーンでも、プレイヤーの楽しみを高めるために細部まで表現されている。




----------------------------------------------------------------------------
**利用する素材・プラグイン・ツール**

Ethereal URP 2024 - Volumetric Lighting & Fog
https://assetstore.unity.com/packages/tools/particles-effects/ethereal-urp-2024-volumetric-lighting-fog-274279

DOTween (HOTween v2)
https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676

Ghost character Free
https://assetstore.unity.com/packages/3d/characters/creatures/ghost-character-free-267003

Free-FlashLight
https://assetstore.unity.com/packages/3d/props/electronics/free-flashlight-293680

LightBulb
https://assetstore.unity.com/packages/3d/props/interior/lightbulb-218258


Halloween Pack | Cemetery Snap
https://assetstore.unity.com/packages/3d/environments/fantasy/halloween-pack-cemetery-snap-235573

graveyard-kit
https://kenney.nl/assets/graveyard-kit

Hyper Casual FX
https://assetstore.unity.com/packages/vfx/particles/hyper-casual-fx-200333

FX Lightning II free
https://assetstore.unity.com/packages/vfx/particles/fx-lightning-ii-free-25210

Paper UI Pack
https://loudeyes.itch.io/paper-ui-pack-for-games

魔女たちの茶会:
https://dova-s.jp/bgm/play21654.html

Ambivalent_Queen:
https://dova-s.jp/bgm/download21215.html

レトロな夢はお好き？:
https://dova-s.jp/bgm/play21473.html

カーソル移動2:
https://soundeffect-lab.info/sound/button/

決定ボタンを押す7:
https://soundeffect-lab.info/sound/button/

かわいい効果音_6:
https://dova-s.jp/se/play1013.html

可愛い足音:
https://dova-s.jp/se/play1305.html

ポイ捨て:
https://dova-s.jp/se/play320.html

卓上ベル・カウンターベル:
https://dova-s.jp/se/play1138.html

シートベルトをかける・外す音:
https://dova-s.jp/se/play955.html

光ライトサーベル剣３:
https://dova-s.jp/se/play1086.html

落下・物を投げる音:
https://dova-s.jp/se/play684.html

紙のページをめくる音:
https://dova-s.jp/se/play1191.html

次のシーン:
https://dova-s.jp/se/play443.html

しょげる:
https://soundeffect-lab.info/
