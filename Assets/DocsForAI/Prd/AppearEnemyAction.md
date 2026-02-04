# 目的

最初隠れていて、そのアクションが行われると姿が現れるIEnemyActiont作りたい

# 詳細

- Assets/Scripts/Combat/EnemyAction 配下に作る
- EnemyActionを継承し、EnemyV2Controllerから呼ばれる想定
- Startで姿を消しておき Executeで現れる
- Start
  - 姿を消す。ダメージ判定と表示を無効化させたい。どうやってやるべきか相談したい
- Execute
  - Startで無効化したダメージ判定と表示を有効化する
  - IsAppearのAnimationをSetBool true させ、OnAppearAnimationFinished をAnimationEventで呼ばれると行動完了になる RangeDetect と似たような感じ