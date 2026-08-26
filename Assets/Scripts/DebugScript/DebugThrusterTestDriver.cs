using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using VLCNP.Core;

namespace VLCNP.DebugSctipt
{
    /**
     * #649 スラスター飛行のプレイモード検証用ドライバ。
     * 仮想ゲームパッドを生成し、scenario フィールドに番号を設定すると
     * 対応する入力シーケンス(ジャンプ→空中ジャンプ等)を再生する。
     * UniCli の Component.SetProperty からプレイモード中に駆動する想定。
     */
    public class DebugThrusterTestDriver : MonoBehaviour
    {
        // 実行したいシナリオ番号を設定すると1回実行して0に戻る
        [SerializeField]
        int scenario = 0;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Gamepad pad;
        bool isRunning = false;

        void Start()
        {
            pad = InputSystem.AddDevice<Gamepad>("DebugThrusterTestPad");
        }

        void OnDestroy()
        {
            if (pad != null)
            {
                InputSystem.RemoveDevice(pad);
            }
        }

        void Update()
        {
            if (scenario == 0 || isRunning)
                return;
            int current = scenario;
            scenario = 0;
            StartCoroutine(RunScenario(current));
        }

        IEnumerator RunScenario(int number)
        {
            isRunning = true;
            Debug.Log($"[DebugThrusterTestDriver] シナリオ{number} 開始");
            switch (number)
            {
                case 1:
                    // 地上ジャンプ→空中でジャンプ再押下(ニュートラル=上方噴射 or 未加入なら発動不可)
                    yield return GroundJump();
                    yield return HoldSouth(1.6f);
                    break;
                case 2:
                    // 地上ジャンプ→右入力しながら空中でジャンプ再押下(右方噴射)
                    yield return GroundJump();
                    SetState(dpad: GamepadButton.DpadRight);
                    yield return new WaitForSeconds(0.05f);
                    yield return HoldSouth(1.6f, GamepadButton.DpadRight);
                    SetState();
                    break;
                case 3:
                    // 地上ジャンプ→下入力しながら空中でジャンプ再押下(下方噴射)
                    yield return GroundJump();
                    SetState(dpad: GamepadButton.DpadDown);
                    yield return new WaitForSeconds(0.05f);
                    yield return HoldSouth(0.4f, GamepadButton.DpadDown);
                    SetState();
                    break;
                case 4:
                    // 噴射途中でキャラ切替→切替先で再噴射(燃料引き継ぎ確認)
                    yield return GroundJump();
                    yield return HoldSouth(0.5f);
                    yield return TapNorth();
                    yield return new WaitForSeconds(0.1f);
                    yield return HoldSouth(1.0f);
                    break;
                case 5:
                    // 地上でキャラ切替
                    yield return TapNorth();
                    break;
                case 6:
                    // ジャンプ後に押しっぱなし→スラスター燃料切れ後の浮遊移行確認(ミタマ用)
                    yield return GroundJump();
                    yield return HoldSouth(2.5f);
                    break;
            }
            Debug.Log($"[DebugThrusterTestDriver] シナリオ{number} 終了");
            isRunning = false;
        }

        IEnumerator GroundJump()
        {
            SetState(south: true);
            yield return new WaitForSeconds(0.2f);
            SetState();
            yield return new WaitForSeconds(0.25f);
        }

        IEnumerator HoldSouth(float seconds, GamepadButton? dpad = null)
        {
            SetState(south: true, dpad: dpad);
            yield return new WaitForSeconds(seconds);
            SetState(dpad: dpad);
            yield return new WaitForSeconds(0.1f);
        }

        IEnumerator TapNorth()
        {
            SetState(north: true);
            yield return new WaitForSeconds(0.1f);
            SetState();
            yield return new WaitForSeconds(0.1f);
        }

        void SetState(bool south = false, bool north = false, GamepadButton? dpad = null)
        {
            GamepadState state = new();
            if (south)
                state = state.WithButton(GamepadButton.South);
            if (north)
                state = state.WithButton(GamepadButton.North);
            if (dpad != null)
                state = state.WithButton(dpad.Value);
            InputSystem.QueueStateEvent(pad, state);
        }
#endif
    }
}
