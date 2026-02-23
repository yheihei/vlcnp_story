namespace VLCNP.Core
{
    // ゲームの進行状況を管理するフラグ
    // 絶対に順番を変えないこと 既存のセーブデータが壊れる
    public enum Flag
    {
        None, // なし
        OpeningDone, // オープニング済
        BabyLongOnceChat, // ベビロンと一度会話済
        JoinedLeelee, // リーリーと仲間になった
        LeeleeOnceChat, // リーリーと一度会話済
        GetSowrd, // 剣を手に入れた
        LeeleeFukkatuDone, // リーリー復活済
        LeeleeRakubanMaeChat, // リーリーと一緒にお昼寝部屋の落盤を見た
        LeeleeRakubanMaeFinished, // リーリーと一緒にお昼寝部屋の落盤をこわしたあと
        VeryLongGunEquipped, // ベリーロングガンを装備した
        VeryEnemyAnimalsBossDefeated, // ベリーエネミーアニマルズのボスを倒した
        OhirunebeyaBeforeEscapeChat, // お昼寝部屋の脱出直前にリーリーと会話した
        OhirunebeyaBlockBroken, // お昼寝部屋の落盤をこわした
        LetsKabeKick, // かべキックを教えてもらった
        ArrivedVLFarm, // ベリーロングファームに到着した
        PunchTutorial1, // パンチの使い方を教えてもらった
        MillcoFirstChated, // Millcoと初めて会話した
        HayatoFirstChated, // Hayatoと初めて会話した
        AllowToUniHouse, // ウニの家に入れるようになった
        DragDefeated, // ドラァグクイーンを倒した
        IkehayaBlockChainChated, // イケハヤからブロックチェーンの話を聞いた
        HPUpOhirunebeyaBoss, // お昼寝部屋のボスを倒した際のHPアップ
        HPUpDragDefeated, // ドラァグクイーンを倒した際のHPアップ
        AkimVeryLong, // Akimがベリーロングになった
        KabeKickTutorial, // かべキックの使い方を教えてもらった
        OhiruneBeyaTutiTyukan, // お昼寝部屋土の中間地点に到達した
        VLOrochiJoined, // オロチが仲間になった
        HPUpAfterVLOrochiJoined, // オロチ加入後のHPアップ
        NoEnemies, // 敵がいない
        IsGamePad, // ゲームパッドでプレイしている
        SkeletonEnemyDoorClosed1, // おひるねべや(闇)のスケルトン部屋の扉が閉まっている
        SkeltonEnemyDoorClosed2,  // おひるねべや(闇)のB1Fのスケルトン部屋の扉が閉まっている
        B1FLightSwitched1, // おひるねべや(闇)のB1Fの真ん中のライトがついている
        B1FLightSwitched2, // おひるねべや(闇)のB1Fの右上のライトがついている
        B1FLightSwitched3, // おひるねべや(闇)のB1Fの一番上のスイッチが降りている
        B1FTojikome,  // おひるねべや(闇)のB1Fの閉じ込めイベントが発生した
        B1FBossDefeated, // おひるねべや(闇)のB1Fのボスを倒した
        TrialEnd2,  // 体験版終了のメッセージが出た
        Yami1FDoorClosed,  
    }
}
