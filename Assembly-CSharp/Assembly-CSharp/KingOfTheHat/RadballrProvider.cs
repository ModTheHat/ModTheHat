// Decompiled with JetBrains decompiler
// Type: RadballrProvider
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7A049970-C314-4BB9-8469-3CD036A16C96
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\King of the Hat\KingOfTheHat_Data\Managed\Assembly-CSharp.dll

using Controllers;
using Cysharp.Threading.Tasks;
using ModTheHat;
using Rewired;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VioletUI;

public class RadballrProvider : KothInputListener
{
    public string debugCode = "radballr";
    private int index;
    public static Action DebugCodeEntered;
    [NonSerialized]
    public static bool isDebug;
    private float lastEntry;

    protected override bool respectInputLock => false;

    protected override bool respectsModal => false;

    protected override void OnInput(InputTick inputTick)
    {
        if (!inputTick.HasMovement && !inputTick.HasInput || this.State.screenId == ScreenId.Game)
            return;
        if (this.IsAccurate(inputTick, this.debugCode[this.index]))
        {
            this.lastEntry = Time.time;
            ++this.index;
            if (this.index < this.debugCode.Length)
                return;
            Action debugCodeEntered = RadballrProvider.DebugCodeEntered;
            if (debugCodeEntered != null)
                debugCodeEntered();
            this.Dispatcher.Run(UIActions.Alert("Hat God Mode Unlocked. Enter ? for commands"));
            RadballrProvider.isDebug = true;
            this.index = 0;
        }
        else if (this.index > 0 && (double)Time.time - (double)this.lastEntry > 1.0)
            this.index = 0;
        else if (this.index > 0 && inputTick.IsAnyUIButtonDown)
        {
            this.index = 0;
        }
        else
        {
            if (this.index <= 0 || !inputTick.HasMovement || this.IsAccurate(inputTick, this.debugCode[this.index - 1]))
                return;
            this.index = 0;
        }

    }

    private async UniTaskVoid Update()
    {
        RadballrProvider radballrProvider = this;
        if (!RadballrProvider.isDebug)
            return;
        UniTask uniTask;
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
        {
            Unlockables.AddPlaytime(1);
            uniTask = SaveDataManager.SaveSetting("PLAY_TIME", radballrProvider.State.playTime, radballrProvider.token);
            await uniTask;
            radballrProvider.Dispatcher.Run(UIActions.DismissNotification);
            uniTask = UniTask.DelayFrame(1);
            await uniTask;
            radballrProvider.Dispatcher.Run(UIActions.Alert(string.Format("Games played: {0}", (object)radballrProvider.State.playTime)));
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            uniTask = radballrProvider.StoryEnding();
            await uniTask;
        }
        if (radballrProvider.State.screenId == ScreenId.LevelSelect && Input.GetKeyDown(KeyCode.V))
        {
            Unlockable unlockable = Unlockables.NextLockedLevel(radballrProvider.State.level.id, radballrProvider.State.levelPack);
            if (unlockable == null)
            {
                Koth.LogWarning("No more levels to unlock");
            }
            else
            {
                Unlockables.Unlock(unlockable);
                uniTask = KothNavigator.Go(ScreenId.CharacterSelect);
                await uniTask;
                uniTask = KothNavigator.Go(ScreenId.LevelSelect);
                await uniTask;
            }
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            StateMonobehaviour<UIState>.Singleton.Dispatcher.Run(UIActions.Alert("Unlock everything"));
            UnlockableQuery.UnlockEverything(new CancellationToken());
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            StateMonobehaviour<UIState>.Singleton.Dispatcher.Run(UIActions.Alert("Lock everything"));
            Unlockables.LockAll();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StateMonobehaviour<UIState>.Singleton.Dispatcher.Run(UIActions.Alert("Doubletime"));
            Time.timeScale = 2f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StateMonobehaviour<UIState>.Singleton.Dispatcher.Run(UIActions.Alert("Normal Speed"));
            Time.timeScale = 0.96f;
        }
        if (Input.GetKey(KeyCode.Alpha1) && Input.GetKeyUp(KeyCode.Alpha4) || Input.GetKey(KeyCode.Alpha4) && Input.GetKeyUp(KeyCode.Alpha1))
        {
            if ((UnityEngine.Object)DevKeyboardPort.Instance != (UnityEngine.Object)null)
            {
                Debug.LogError((object)"Tried to reset 4p controls but devkeyboardport is not null");
                return;
            }
            if (!ReInput.isReady)
            {
                Debug.LogError((object)"Tried to reset 4p controls but rewired is not ready");
                return;
            }
            StateMonobehaviour<UIState>.Singleton.Dispatcher.Run(UIActions.Alert("Set 4p keyboard controls"));
            radballrProvider.Set4pDebugControls();
        }
        if (Input.GetKey(KeyCode.Alpha6) && Input.GetKeyUp(KeyCode.Alpha9) || Input.GetKey(KeyCode.Alpha9) && Input.GetKeyUp(KeyCode.Alpha6))
            radballrProvider.Win();
        if (!Input.GetKeyDown(KeyCode.Question))
            return;
        radballrProvider.Dispatcher.Run(UIActions.Alert(string.Join("\n", new string[3]
        {
      "+ => Simulate one game played",
      "l => Lock everything",
      "u => Unlock everything"
        })));
    }

    private async UniTask StoryEnding()
    {
        RadballrProvider radballrProvider = this;
        GameController.instance.CancelGame();
        UniTask uniTask = UniTask.Delay(420);
        await uniTask;
        radballrProvider.Dispatcher.Run(UIActions.ResetPlayerStates);
        radballrProvider.Dispatcher.Run(UIActions.SetPlayerStatus(0, PlayerStatus.READY));
        radballrProvider.Dispatcher.Run(UIActions.SetGameMode(GameModeBlueprints.ARCADE));
        radballrProvider.Dispatcher.Run(UIActions.SetCurrentBattleIndex(7));
        radballrProvider.Dispatcher.Run(UIActions.SetArcadeDifficulty(ArcadeDifficulty.NORMAL));
        uniTask = Story.StartArcade(ScreenTransition.None);
        await uniTask;
        uniTask = radballrProvider.Press(Buttons.Y);
        await uniTask;
        while (KothNavigator.Singleton.CurrentScreen != ScreenId.Game)
        {
            uniTask = UniTask.DelayFrame(1);
            await uniTask;
        }
        uniTask = UniTask.Delay(1500);
        await uniTask;
        radballrProvider.Dispatcher.Run(UIActions.ClearMinorDialogue);
        radballrProvider.Dispatcher.Run(UIActions.ClearDialogue);
        uniTask = UniTask.Delay(420);
        await uniTask;
        uniTask = radballrProvider.WinFinalBattle();
        await uniTask;
        uniTask = UniTask.Delay(420);
        await uniTask;
        while (radballrProvider.State.dialogue.HasLine)
        {
            uniTask = radballrProvider.Press(Buttons.A);
            await uniTask;
        }
    }

    private async UniTask WinFinalBattle()
    {
        GameController.MetaContext.GetEntityWithMetaID(0).ReplaceScore(StateMonobehaviour<UIState>.Singleton.State.game.config.maxScore - 1);
        await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate);
        foreach (LogicEntity logicEntity in Contexts.sharedInstance.logic.GetGroup(LogicMatcher.PlayerID))
        {
            if (logicEntity.playerID.id != 0)
            {
                logicEntity.Kill();
                logicEntity.GetHat().Kill();
            }
        }
    }

    private void Set4pDebugControls()
    {
        ActionElementMap actionElementMap = new ActionElementMap(5, ControllerElementType.Button, Pole.Positive, KeyboardKeyCode.I, ModifierKey.None, ModifierKey.None, ModifierKey.None);
        this.MapDebugControls(1, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.A, KeyCode.S);
        this.MapDebugControls(2, KeyCode.Alpha6, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.Q, KeyCode.W);
        this.MapDebugControls(3, KeyCode.Alpha9, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2);
        IList<ControllerMap> maps = ReInput.players.GetPlayer(0).controllers.maps.GetMaps(ControllerType.Keyboard, 0);
        List<int> intList = new List<int>();
        foreach (ControllerMap controllerMap in (IEnumerable<ControllerMap>)maps)
        {
            foreach (ActionElementMap allMap in (IEnumerable<ActionElementMap>)controllerMap.AllMaps)
            {
                switch (allMap.keyCode)
                {
                    case KeyCode.A:
                    case KeyCode.D:
                    case KeyCode.S:
                    case KeyCode.W:
                        intList.Add(allMap.id);
                        continue;
                    default:
                        continue;
                }
            }
            foreach (int elementMapId in intList)
                controllerMap.DeleteElementMap(elementMapId);
        }
        for (int playerId = 0; playerId < 4; ++playerId)
        {
            foreach (ControllerMap map in (IEnumerable<ControllerMap>)ReInput.players.GetPlayer(playerId).controllers.maps.GetMaps(ControllerType.Keyboard, 0))
            {
                foreach (ActionElementMap allMap in (IEnumerable<ActionElementMap>)map.AllMaps)
                    ReInput.mapping.GetAction(allMap.actionId);
            }
        }
    }

    private bool IsAccurate(InputTick inputTick, char c)
    {
        switch (c)
        {
            case 'a':
                return inputTick.IsAButtonDown;
            case 'b':
                return inputTick.IsBButtonDown;
            case 'd':
                return inputTick.IsMoveDown;
            case 'l':
                return inputTick.IsMoveLeft;
            case 'r':
                return inputTick.IsMoveRight;
            case 'u':
                return inputTick.IsMoveUp;
            default:
                Debug.LogError((object)string.Format("Tried to check cheat code for unknown character {0}", (object)c));
                return false;
        }
    }

    private void MapDebugControls(
      int playerId,
      KeyCode up,
      KeyCode left,
      KeyCode down,
      KeyCode right,
      KeyCode a,
      KeyCode b)
    {
        ControllerMap map = ReInput.players.GetPlayer(playerId).controllers.maps.GetMaps(ControllerType.Keyboard, 0)[0];
        map.CreateElementMap(1, Pole.Positive, up, ModifierKey.None, ModifierKey.None, ModifierKey.None);
        map.CreateElementMap(0, Pole.Negative, left, ModifierKey.None, ModifierKey.None, ModifierKey.None);
        map.CreateElementMap(1, Pole.Negative, down, ModifierKey.None, ModifierKey.None, ModifierKey.None);
        map.CreateElementMap(0, Pole.Positive, right, ModifierKey.None, ModifierKey.None, ModifierKey.None);
        map.CreateElementMap(5, Pole.Positive, a, ModifierKey.None, ModifierKey.None, ModifierKey.None);
        map.CreateElementMap(6, Pole.Positive, b, ModifierKey.None, ModifierKey.None, ModifierKey.None);
    }

    private void Win()
    {
        Contexts.sharedInstance.meta.GetEntityWithMetaID(0).ReplaceScore(StateMonobehaviour<UIState>.Singleton.State.game.config.maxScore - 1);
        foreach (LogicEntity logicEntity in Contexts.sharedInstance.logic.GetGroup(LogicMatcher.PlayerID))
        {
            if (logicEntity.playerID.id != 0)
            {
                logicEntity.Kill();
                logicEntity.GetHat().Kill();
            }
        }
    }

    private async UniTask Press(Buttons button, int playerId = 0, int delay = 420)
    {
        RadballrProvider radballrProvider = this;
        UniTask uniTask;
        while (radballrProvider.State.inputLocked)
        {
            uniTask = UniTask.DelayFrame(1);
            await uniTask;
        }
        uniTask = UniTask.Delay(delay);
        await uniTask;
        InputSnapshot inputSnapshot = new InputSnapshot();
        inputSnapshot.PressButton(button);
        InputTick inputTick = new InputTick()
        {
            snapshot = inputSnapshot,
            playerId = playerId,
            tick = GameController.Tick,
            controllerType = ControllerTypes.KEYBOARD
        };
        radballrProvider.TriggerInput(inputTick);
    }

    private void TriggerInput(InputTick inputTick)
    {
        if ((UnityEngine.Object)ControllerPort.instance != (UnityEngine.Object)null)
        {
            ControllerPort.instance.TriggerInput(inputTick, -1);
        }
        else
        {
            if (!((UnityEngine.Object)DevKeyboardPort.Instance != (UnityEngine.Object)null))
                throw new Exception("Can't find InputPort in scene");
            DevKeyboardPort.Instance.TriggerInput(inputTick, -1);
        }
    }

    protected override bool IsDirty(UIState state, UIState lastState) => false;
}
