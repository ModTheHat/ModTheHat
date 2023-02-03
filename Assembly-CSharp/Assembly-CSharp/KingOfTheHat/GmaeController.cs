// Decompiled with JetBrains decompiler
// Type: GameController
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7A049970-C314-4BB9-8469-3CD036A16C96
// Assembly location: E:\dev\ModTheHat\libs\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Determinism;
using Entitas;
using FastMath;
using KothNetwork;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using VioletUI;

public class GameController : BCIBehaviour
{
    public Action<GamePacket> OnGameStart;
    public Action OnGameEnd;
    public static bool isInMenus = false;
    public static readonly int FPS = 50;
    public static readonly long DELTA_TIME = FixedMath.Create(1L, (long)(GameController.FPS - 1));
    public static GameController instance;
    public static Action<LogicEntity> OnRespawn;
    public static Action<LogicEntity> OnWorldWrap;
    private Contexts _contexts;
    private GameLogicSystem gameLogicSystem;
    private GGPOSystem ggpoSystem;
    private Systems destroySystem;
    private FindActivePlayersSystem findActivePlayersSystem;
    private Ticker ticker;
    public GameModeBlueprints gmBlueprint;
    public GameModeBlueprints tutorialToTest;
    private static int nextId = 0;

    public static event GameController.FXDelegate FXFunctions;

    public bool arcadeOrGolf
    {
        get
        {
            if (!((UnityEngine.Object)this.state.game.mode != (UnityEngine.Object)null))
                return false;
            return this.state.game.mode.id == GameModeBlueprints.ARCADE || this.state.game.mode.id == GameModeBlueprints.GOLF;
        }
    }

    public static TimeContext TimeContext => Contexts.sharedInstance?.time;

    public static LogicContext LogicContext => Contexts.sharedInstance?.logic;

    public static MetaContext MetaContext => Contexts.sharedInstance?.meta;

    public static GameModeContext ModeContext => Contexts.sharedInstance?.gameMode;

    public static int Tick => GameController.TimeContext == null || !GameController.TimeContext.hasTick ? Time.frameCount : GameController.TimeContext.tick.value;

    public static LogicEntity Find(int id)
    {
        Contexts sharedInstance = Contexts.sharedInstance;
        return sharedInstance == null ? (LogicEntity)null : sharedInstance.logic.GetEntityWithId(id);
    }

    public static LogicEntity FindPlayer(int playerId)
    {
        foreach (LogicEntity e in GameController.LogicContext.GetEntitiesWithPlayerID(playerId))
        {
            if (e.IsPlayer())
                return e;
        }
        return (LogicEntity)null;
    }

    public static LogicEntity FindHat(int playerId)
    {
        LogicEntity player = GameController.FindPlayer(playerId);
        return player == null ? (LogicEntity)null : player.GetHat();
    }

    private Dispatch.Dispatcher<UIState> Dispatcher => StateMonobehaviour<UIState>.Singleton.Dispatcher;

    private UIState state => StateMonobehaviour<UIState>.Singleton.State;

    [Button]
    public void StartTutorial()
    {
        PlayerState allPlayer = StateMonobehaviour<UIState>.Singleton.State.allPlayers[0];
        allPlayer.character = Characters.BIRTHDAY;
        allPlayer.status = PlayerStatus.READY;
        this.Dispatcher.Run(UIActions.SetGameMode(this.tutorialToTest));
        this.StartGame(StateMonobehaviour<UIState>.Singleton.State);
        KothNavigator.Go(ScreenId.Game);
    }

    private void OnEnable()
    {
        if ((UnityEngine.Object)GameController.instance == (UnityEngine.Object)null)
            GameController.instance = this;
        this.StartCoroutine(this.ResetScenario());
    }

    public void OnDisable()
    {
        RayCastSystem.grid.Clear();
        GameController.DestroyAllGameEntities();
        this.DestroyAllMetaEntities();
        GameController.DestroyAllInputs();
        GameController.DestroyAllAIs();
        this.DestroyAllTimeEntities();
        this.gameLogicSystem.TearDown();
        this.destroySystem.ClearReactiveSystems();
        this.ggpoSystem.TearDown();
        try
        {
            this._contexts.input.DestroyAllEntities();
        }
        catch (ContextStillHasRetainedEntitiesException ex)
        {
            Koth.LogWarning(ex.Message ?? "");
        }
        try
        {
            this._contexts.logic.DestroyAllEntities();
        }
        catch (ContextStillHasRetainedEntitiesException ex)
        {
            Koth.LogWarning(ex.Message ?? "");
        }
        try
        {
            this._contexts.meta.DestroyAllEntities();
        }
        catch (ContextStillHasRetainedEntitiesException ex)
        {
            Koth.LogWarning(ex.Message ?? "");
        }
        try
        {
            this._contexts.gameMode.DestroyAllEntities();
        }
        catch (ContextStillHasRetainedEntitiesException ex)
        {
            Koth.LogWarning(ex.Message ?? "");
        }
        try
        {
            this._contexts.aI.DestroyAllEntities();
        }
        catch (ContextStillHasRetainedEntitiesException ex)
        {
            Koth.LogWarning(ex.Message ?? "");
        }
        try
        {
            this._contexts.fX.DestroyAllEntities();
        }
        catch (ContextStillHasRetainedEntitiesException ex)
        {
            Debug.LogWarning((object)(ex.Message ?? ""));
        }
        this.ticker.ClearSystems();
        this.destroySystem.ClearReactiveSystems();
        this.DestroyAllTimeEntities();
        this.destroySystem.ClearReactiveSystems();
        try
        {
            this._contexts.time.DestroyAllEntities();
        }
        catch (ContextStillHasRetainedEntitiesException ex)
        {
            Koth.LogWarning(ex.Message ?? "");
        }
        try
        {
            this._contexts.fX.DestroyAllEntities();
        }
        catch (ContextStillHasRetainedEntitiesException ex)
        {
            Koth.LogWarning(ex.Message ?? "");
        }
        this.Dispatcher.Run(UIActions.ChooseDefaultLevelPack);
        Assets.UnloadAllAddressables();
    }

    public async UniTaskVoid Start()
    {
        GameController gameController = this;
        if (!Release.Flag.SOCK_MOUSE_SUPPORT)
            Cursor.visible = false;
        StadiaWrapper.GgpSetStreamProfile();
        StadiaWrapper.ForceSDR();
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        AnalyticsController.ReportEvent("Platform", Application.platform.ToString());
        AnalyticsController.ReportEvent("Refresh Rate", Application.targetFrameRate.ToString());
        AnalyticsController.ReportEvent("Version Number", Release.Version);
        gameController._contexts = Contexts.sharedInstance;
        gameController.AddIdEvents(gameController._contexts);
        gameController.ticker = gameController.gameObject.AddComponent<Ticker>();
        gameController.ticker.SetContexts(gameController._contexts);
        gameController._contexts.meta.SetNetwork(gameController.state.network);
        gameController.destroySystem = new Feature("Destroy System").Add((ISystem)new DestroyEventSystem(gameController._contexts)).Add((ISystem)new DestroySystem(gameController._contexts));
        gameController.findActivePlayersSystem = new FindActivePlayersSystem(gameController._contexts);
        gameController.gameLogicSystem = new GameLogicSystem(gameController._contexts);
        gameController.ggpoSystem = new GGPOSystem(gameController._contexts, gameController.gameLogicSystem);
        gameController._contexts.time.ReplaceDestroySystem(gameController.destroySystem);
        gameController._contexts.time.ReplaceLogicSystem((Systems)gameController.gameLogicSystem);
        gameController.gameLogicSystem.Initialize();
        Contexts.sharedInstance.logic.ReplaceLocus(FixedVector2.ZERO, false);
        await Assets.allAssetsLoader.LoadComplete();
        gameController.SetLevel(Assets.GetLevelById("4144e5c5-0a47-45b9-9e53-2ba13a269f31"));
        gameController.Dispatcher.Run(UIActions.SetMaxGameTime(gameController.state.game.isTimed, 150));
        gameController.Dispatcher.Run(UIActions.SetItemFrequency(ItemFrequency.NONE));
        gameController.Dispatcher.Run(UIActions.SetMaxItemsOnScreen(2));
        if ((UnityEngine.Object)gameController.state.game.mode == (UnityEngine.Object)null)
            gameController.Dispatcher.Run(UIActions.SetGameMode(Release.Flag.HAT_HUNTERS_DEFAULT ? GameModeBlueprints.HAT_HUNTERS : GameModeBlueprints.LAST_HAT_STANDING));
        gameController.Dispatcher.Run(UIActions.SetTeams(false));
        gameController.Dispatcher.Run(UIActions.ToggleSpecialMeter(Release.Flag.SOCK_SPECIAL_METER));
        gameController.Dispatcher.Run(UIActions.SetGlobalKnockback(65536L));
        gameController.Dispatcher.Run(UIActions.SetGameSpeed(62880L));
        gameController.PrepGarbageCollector();
        gameController.Dispatcher.Run(UIActions.ChooseDefaultLevelPack);
    }

    private void Update()
    {
    }

    public void FixedUpdate()
    {
        if (GameController.TimeContext.isDroppingTick || GameController.TimeContext.isNetpaused)
            return;
        this.RunSystems();
    }

    private bool IsOnlineGame
    {
        get
        {
            if (GameController.MetaContext.isGameOver && !GameController.MetaContext.isEndGameTimer)
                return false;
            return this.state.network.isConnected || this.state.network.isSimulatingRollback;
        }
    }

    private void RunSystems()
    {
        if (GameController.isInMenus && !this.arcadeOrGolf)
            return;
        if (this.IsOnlineGame)
        {
            this.ggpoSystem.Execute();
        }
        else
        {
            this.gameLogicSystem.Execute();
            this.gameLogicSystem.Cleanup();
        }
    }

    public void TriggerFX()
    {
        if (!GameController.TimeContext.isConfirmTick)
            return;
        try
        {
            GameController.FXDelegate fxFunctions = GameController.FXFunctions;
            if (fxFunctions == null)
                return;
            fxFunctions();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void ReloadMetaPlayers()
    {
        if (this.findActivePlayersSystem == null)
            Koth.LogWarning("GameController can't ReloadMetaPlayers bc findActivePlayersSystem == null");
        else
            this.findActivePlayersSystem.Execute(GameController.MetaContext);
    }

    private void AddId(IContext context, IEntity entity)
    {
        if (GameController.nextId == 2147483645)
            throw new Exception("Tried to assign id, but we're at max value.");
        (entity as IIdEntity).ReplaceId(GameController.nextId++);
    }

    private void AddIdEvents(Contexts contexts)
    {
        foreach (IContext allContext in contexts.allContexts)
        {
            if (((IEnumerable<System.Type>)allContext.contextInfo.componentTypes).Contains<System.Type>(typeof(IdComponent)))
            {
                allContext.OnEntityCreated -= new ContextEntityChanged(this.AddId);
                allContext.OnEntityCreated += new ContextEntityChanged(this.AddId);
            }
        }
    }

    public static void DestroyAllGameEntities(bool clearLogicSystem = true, bool includeCreationEntities = false)
    {
        foreach (LogicEntity entity in Contexts.sharedInstance.logic.GetEntities())
        {
            if (entity.isGameElement)
                entity.isDestroyed = true;
        }
        if (includeCreationEntities)
        {
            foreach (Entity entity in Contexts.sharedInstance.creation.GetEntities())
                entity.Destroy();
            foreach (InputEntity entity in Contexts.sharedInstance.input.GetEntities())
            {
                if (entity.hasPlayerData || entity.hasPlayerAIData)
                    entity.Destroy();
            }
        }
        if (!clearLogicSystem)
            return;
        GameController.instance.destroySystem.Execute();
        GameController.instance.destroySystem.Cleanup();
    }

    private void DestroyAllMetaEntities()
    {
        foreach (MetaEntity entity in this._contexts.meta.GetEntities())
            entity.isDestroyed = true;
        this.destroySystem.Execute();
        this.destroySystem.Cleanup();
    }

    private void DestroyAllTimeEntities()
    {
        foreach (TimeEntity entity in this._contexts.time.GetEntities())
            entity.isDestroyed = true;
        this.destroySystem.Execute();
        this.destroySystem.Cleanup();
    }

    public static void ResetAllGameEntities(bool clearLogicSystem = true)
    {
        foreach (LogicEntity entity in Contexts.sharedInstance.logic.GetEntities())
        {
            if (entity.isGameElement)
                entity.isReset = true;
        }
        if (!clearLogicSystem)
            return;
        GameController.instance.destroySystem.Execute();
        GameController.instance.destroySystem.Cleanup();
    }

    public static void DestroyAllSpawnedEntities(bool clearLogicSystem = true)
    {
        foreach (LogicEntity entity in Contexts.sharedInstance.logic.GetEntities())
        {
            if (entity.isSpawnedObject)
                entity.isDestroyed = true;
            else if (entity.hasObjectSpawnerTimer)
                entity.objectSpawnerTimer.framesLeft = 1;
        }
        if (!clearLogicSystem)
            return;
        GameController.instance.destroySystem.Execute();
        GameController.instance.destroySystem.Cleanup();
    }

    public static void DestroyOnRoundLevelObjects(bool clearLogicSystem = true)
    {
        if (Contexts.sharedInstance.logic == null || !Contexts.sharedInstance.time.hasLogicSystem || GameController.instance.gameLogicSystem == null)
            return;
        foreach (LogicEntity entity in Contexts.sharedInstance.logic.GetEntities())
        {
            if ((entity.hasLevelObject || entity.isSpawnedObject) && entity.isDestroyOnRound)
                entity.isDestroyed = true;
        }
        if (!clearLogicSystem)
            return;
        GameController.instance.destroySystem.Execute();
        GameController.instance.destroySystem.Cleanup();
    }

    public static void DestroyAllLevelObjects(bool clearLogicSystem = true)
    {
        if (Contexts.sharedInstance.logic == null || !Contexts.sharedInstance.time.hasLogicSystem || GameController.instance.gameLogicSystem == null)
            return;
        foreach (LogicEntity entity in Contexts.sharedInstance.logic.GetEntities())
        {
            if (!entity.isRequestedObject && (entity.hasLevelObject || entity.isSpawnedObject || !entity.hasPlayerID && entity.hasControllerID && entity.controllerID.id > 4))
                entity.isDestroyed = true;
        }
        if (!clearLogicSystem)
            return;
        GameController.instance.destroySystem.Execute();
        GameController.instance.destroySystem.Cleanup();
    }

    public static void DestroyAllNonPlayerGameEntities(bool clearLogicSystem = false)
    {
        if (Contexts.sharedInstance.logic == null || !Contexts.sharedInstance.time.hasLogicSystem || GameController.instance.gameLogicSystem == null)
            return;
        foreach (LogicEntity entity in Contexts.sharedInstance.logic.GetEntities())
        {
            if (entity.hasLevelObject || !entity.hasPlayerID && entity.hasControllerID && entity.controllerID.id > 4 || entity.hasPlayerID && entity.playerID.id > 4)
            {
                entity.isDestroyed = true;
                if (entity.hasHat)
                {
                    foreach (int num in entity.hat.entityID)
                        Contexts.sharedInstance.logic.GetEntityWithId(num).isDestroyed = true;
                }
            }
        }
        if (!clearLogicSystem)
            return;
        GameController.instance.destroySystem.Execute();
        GameController.instance.destroySystem.Cleanup();
    }

    public static void DestroyAllPlayerGameEntities(bool clearLogicSystem = true)
    {
        if (Contexts.sharedInstance.logic == null || !Contexts.sharedInstance.time.hasLogicSystem || Contexts.sharedInstance.time.logicSystem.system == null)
            return;
        foreach (LogicEntity entity in Contexts.sharedInstance.logic.GetEntities())
        {
            if (entity.hasPlayerID)
            {
                entity.isDestroyed = true;
                if (entity.hasHat)
                {
                    foreach (int num in entity.hat.entityID)
                        Contexts.sharedInstance.logic.GetEntityWithId(num).isDestroyed = true;
                }
            }
        }
        if (!clearLogicSystem)
            return;
        GameController.instance.destroySystem.Execute();
        GameController.instance.destroySystem.Cleanup();
    }

    public static void DestroyAllAIs()
    {
        foreach (AIEntity entity in Contexts.sharedInstance.aI.GetEntities())
            entity.isDestroyed = true;
        new DestoryAISystem(Contexts.sharedInstance).Cleanup();
    }

    public static void DestroyAllInputs()
    {
        foreach (InputEntity entity in Contexts.sharedInstance.input.GetEntities())
        {
            if (entity.hasControllerInput || entity.hasControllerID)
                entity.isDestroyed = true;
        }
        GameController.instance.destroySystem.Execute();
        GameController.instance.destroySystem.Cleanup();
    }

    private bool UnlockableMainLevelsCondition
    {
        get
        {
            if (!((UnityEngine.Object)this.state.game.Mode != (UnityEngine.Object)null))
                return false;
            return this.state.game.Mode.id == GameModeBlueprints.LAST_HAT_STANDING || this.state.game.Mode.id == GameModeBlueprints.HAT_HUNTERS;
        }
    }

    private bool UnlockableGolfCondition => (UnityEngine.Object)this.state.game.Mode != (UnityEngine.Object)null && this.state.game.Mode.id == GameModeBlueprints.GOLF;

    public void StartGame(UIState state, int startFrames)
    {
        if (!this._contexts.meta.isGameOver)
            throw new Exception("Attempted to start game while game is already playing.");
        if (!state.network.IsActive && state.network.isSimulatingRollback)
            Koth.Log(string.Format("Running game with local rollback. State.network.localRollbackFrames={0}", (object)state.network.localRollbackFrames));
        GameController.isInMenus = false;
        GameController.DestroyAllAIs();
        GameController.DestroyAllInputs();
        GameController.DestroyAllGameEntities();
        RayCastSystem.grid.Clear();
        this.ggpoSystem.ClearPredictionGrid();
        Contexts.sharedInstance.creation.ReplaceNextController(5);
        if (state.game.randomSeed != 0)
            Determinism.Random.SetSeed(state.game.randomSeed);
        else
            Determinism.Random.SetSeed((int)DateTimeOffset.Now.ToUnixTimeMilliseconds());
        state.levelPacks.jankWorld.ShuffleDeck();
        this.SetLevel(state.level);
        if (this.UnlockableMainLevelsCondition)
        {
            Unlockable unlockable = Unlockables.NextLockedLevel(state.level.id, state.levelPack);
            if (unlockable != null)
                this.Dispatcher.Run(UIActions.AddLevelToUnlockList(unlockable.id));
        }
        this.SummonPlayers(state.allPlayers);
        if (Release.Flag.JARUZ_TWOVSTWO && state.game.config.hasTwoVsTwo)
            this.SummonPlayers(state.allPlayers);
        this.SetMode(state.game.Mode.id, state);
        this.ApplyGameModifiers(state);
        this._contexts.time.ReplaceTick(0);
        this._contexts.logic.ReplaceChecksum("start");
        this._contexts.meta.isRequestingStart = true;
        this._contexts.time.isPaused = true;
        this._contexts.logic.ReplaceNextItemTime(750);
        this.Dispatcher.Run(UIActions.ClearDisconnectReason);
        this.Dispatcher.Run(UIActions.ResetScores());
        this.Dispatcher.Run(UIActions.ResetStats);
        if (state.game.Mode.id == GameModeBlueprints.LAST_HAT_STANDING || state.game.Mode.id == GameModeBlueprints.HAT_HUNTERS)
        {
            int num1 = 0;
            if (state.levelPack == null)
            {
                Koth.LogWarning("levelPack is null. not reporting level selection to AnalyticsController");
            }
            else
            {
                for (int index = 0; index < state.levelPack.levels.Count; ++index)
                {
                    if (state.level.id == state.levelPack.levels[index].data.id)
                        num1 = index % 7 + 1;
                }
                AnalyticsController.ReportEvent("Level Selection", state.levelPack.id.ToString(), state.level.environment.ToString(), num1.ToString());
                if (state.game.Mode.id == GameModeBlueprints.LAST_HAT_STANDING || state.game.Mode.id == GameModeBlueprints.HAT_HUNTERS)
                {
                    AnalyticsController.ReportEvent("Game Rules", state.game.Mode.id.ToString(), state.game.config.hasTeams ? "Teams" : "FFA", state.game.config.isTimed ? string.Format("{0}", (object)state.game.config.maxTime) : string.Format("{0}", (object)state.game.config.maxScore));
                    AnalyticsController.ReportEvent("Game Rules", state.game.Mode.id.ToString(), state.game.config.hasTeams ? "Teams" : "FFA", state.game.config.GetGameSpeedString());
                    if (state.game.config.itemFrequency > ItemFrequency.NONE)
                    {
                        int num2 = 0;
                        while (num2 < state.items.Count)
                            ++num2;
                    }
                }
            }
        }
        else if (state.game.Mode.id == GameModeBlueprints.GOLF)
        {
            int num = 0;
            for (int index = 0; index < state.levelPack.levels.Count; ++index)
            {
                if (state.level.id == state.levelPack.levels[index].data.id)
                    num = index + 1;
            }
            AnalyticsController.ReportEvent("Level Selection", state.levelPack.id.ToString(), num.ToString());
        }
        if (state.network.isConnected && GameController.IsLobbyOwner(state))
        {
            int activePlayerCount = state.allPlayers.ActivePlayerCount;
            AnalyticsController.ReportEvent("Game Mode Selection", "Online", state.game.Mode.id.ToString(), state.game.config.hasTeams ? "TEAMS" : "FFA", state.allPlayers.ActivePlayerCount.ToString());
            AnalyticsController.ReportProgression(AnalyticsProgressionStatus.START, "Online", state.network.matchMode.ToString());
        }
        else if (!state.network.isConnected)
        {
            AnalyticsController.ReportEvent("Game Mode Selection", "Offline", state.game.Mode.id.ToString(), state.game.config.hasTeams ? "TEAMS" : "FFA", state.allPlayers.ActivePlayerCount.ToString());
            AnalyticsController.ReportProgression(AnalyticsProgressionStatus.START, "Offline", state.game.Mode.id.ToString());
        }
        GamePacket gamePacket = new GamePacket(GameController.Tick, state);
        this.Dispatcher.Run(UIActions.SetOfflineMatchId(gamePacket.matchId));
        Action<GamePacket> onGameStart = this.OnGameStart;
        if (onGameStart == null)
            return;
        onGameStart(gamePacket);
    }

    public void StartGame(UIState state)
    {
        int startFrames = 3 * GameController.FPS;
        this.StartGame(state, startFrames);
    }

    public void StartRound(RoundPacket roundPacket, CancellationToken token = default(CancellationToken))
    {
        if (roundPacket.tick >= 0)
            GameController.TimeContext.ReplaceTick(roundPacket.tick);
        int newFramesLeft = this.state.game.Mode.endRoundFrames - 1;
        GameController.ModeContext.onRoundOverDetected.function();
        GameController.MetaContext.isRoundOver = true;
        GameController.MetaContext.isEndRoundTimer = true;
        GameController.MetaContext.ReplaceEndRoundTimeLeft(newFramesLeft);
        this.SetLevel(this.state.level);
        if (this.state.network.IsActive)
            throw new Exception("Not updated to work w GGPOSystem");
    }

    public void StartChallengerBattle(UIState state)
    {
        for (int index = 0; index < 4; ++index)
            state.allPlayers[index].IsReady = false;
        state.allPlayers[0].IsReady = true;
        state.allPlayers[1].character = state.unlockablesState.currentChallenger;
        state.allPlayers[1].IsReady = true;
        state.allPlayers[1].isAIControlled = true;
        state.allPlayers[0].score = 0;
        state.allPlayers[1].score = 0;
        state.allPlayers[2].score = 0;
        state.allPlayers[3].score = 0;
    }

    public void SummonPlayers(AllPlayerStates players, bool isLevelSelect = false)
    {
        foreach (PlayerState player in (AllDataHolder<PlayerState>)players)
        {
            if (player.IsInGame)
            {
                InputEntity inputEntity = GameController.SummonPlayer(player);
                if (player.isAIControlled)
                    inputEntity.AddPlayerAIData(new KothAISelection());
                if (!isLevelSelect)
                    AnalyticsController.ReportEvent("Character Selection", player.character.ToString(), player.colorIndex.ToString());
                if (Release.Flag.NULL_SHADOWGOLF && this.state.game.mode.id == GameModeBlueprints.GOLF)
                    break;
            }
        }
    }

    public static InputEntity SummonPlayer(PlayerState playerState)
    {
        InputEntity entity = Contexts.sharedInstance.input.CreateEntity();
        entity.AddControllerID(playerState.id);
        entity.AddPlayerData(new KothPlayerSelection(playerState.id, playerState.character, playerState.team, playerState.colorIndex));
        return entity;
    }

    public static async UniTask<LogicEntity> SummonPlayer(
      PlayerState playerState,
      CancellationToken token)
    {
        GameController.SummonPlayer(playerState);
        LogicEntity player;
        for (player = GameController.FindPlayer(playerState.id); player == null; player = GameController.FindPlayer(playerState.id))
            await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate, token);
        return player;
    }

    public void SetLevel(LevelData levelData) => this._contexts.logic.ReplaceLevel(levelData);

    public void ApplyGameModifiers(UIState state)
    {
        if (!Release.Flag.SOCK_IN_GAME_SETTINGS)
        {
            Contexts.sharedInstance.logic.ReplaceLocus(FixedVector2.ZERO, false);
            Contexts.sharedInstance.logic.ReplaceGlobalKnockBack(65536L);
        }
        else
        {
            if (state.game.mode.hasRuleModifiers)
            {
                Time.timeScale = state.game.config.gameSpeed.ToFloat().Clamp(0.5f, 2f);
                Contexts.sharedInstance.logic.ReplaceLocus(FixedVector2.ZERO, state.game.config.hasLocus);
                Contexts.sharedInstance.logic.ReplaceGlobalKnockBack(state.game.config.knockbackMultiplier);
            }
            else
            {
                Time.timeScale = 0.96f;
                this.Dispatcher.Run(UIActions.SetGameSpeed(62880L));
                Contexts.sharedInstance.logic.ReplaceGlobalKnockBack(65536L);
            }
            GameController.LogicContext.ReplaceItemFrequency(state.game.config.itemFrequency);
            GameController.LogicContext.ReplaceMaxItemsOnScreen(state.game.config.itemFrequency == ItemFrequency.LOW ? 1 : (state.game.config.itemFrequency == ItemFrequency.NORMAL ? 2 : 3));
            GameController.LogicContext.isUsingItems = state.game.config.hasItems;
        }
    }

    public void DoJankSequence() => this._contexts.logic.ReplaceLevel(this.state.levelPacks.jankWorld.NextCard());

    public void SetMode(GameModeBlueprints mode, UIState state)
    {
        GameModeBlueprint blueprint = Assets.Get(mode);
        Contexts.sharedInstance.gameMode.LoadRules(blueprint, state);
        StateMonobehaviour<UIState>.Singleton.Dispatcher.Run(UIActions.SetMaxScore(state.game.config.maxScore));
        StateMonobehaviour<UIState>.Singleton.Dispatcher.Run(UIActions.SetMaxGameTime(blueprint.useTime, state.game.config.maxTime));
    }

    public void Pause(bool paused) => this._contexts.time.isPaused = paused;

    public async void FinishGame()
    {
        if (!Contexts.sharedInstance.meta.isGameOver)
            return;
        if (this.state.network.IsActive)
        {
            if (this.state.network.LocalPeer != null && this.state.network.LocalPeer.playerIds.Contains(0))
            {
                ApiClient apiClient = NetworkController.instance.apiClient;
                EventJson body = new EventJson();
                body.eventType = "victoryOnline";
                CancellationToken token = new CancellationToken();
                apiClient.Put("/events", (object)body, token: token);
            }
        }
        else if (this.state.game.Mode.id != GameModeBlueprints.TUTORIAL_LEGACY)
        {
            ApiClient apiClient = NetworkController.instance.apiClient;
            EventJson body = new EventJson();
            body.eventType = "victoryOffline";
            CancellationToken token = new CancellationToken();
            apiClient.Put("/events", (object)body, token: token);
        }
        UniTask uniTask = OverlayFX.VictoryAnimation();
        await uniTask;
        if (this.state.network.isConnected && GameController.IsLobbyOwner(this.state))
            AnalyticsController.ReportProgression(AnalyticsProgressionStatus.COMPLETE, "Online", this.state.network.matchMode.ToString());
        else if (!this.state.network.isConnected)
            AnalyticsController.ReportProgression(AnalyticsProgressionStatus.COMPLETE, "Offline", this.state.game.Mode.id.ToString());
        this.CancelGame();
        this.EndGameEvents();
        if (this.state.game.Mode.victoryScreen == ScreenId.None)
            return;
        uniTask = KothNavigator.Go(this.state.game.Mode.victoryScreen);
        await uniTask;
    }

    public void EndGameEvents()
    {
        Action onGameEnd = this.OnGameEnd;
        if (onGameEnd != null)
            onGameEnd();
        this.Dispatcher.Run(UIActions.ClearOfflineMatchId);
    }

    public void CancelGame(bool sendAnalytics = true)
    {
        if (!Contexts.sharedInstance.meta.isGameOver & sendAnalytics)
        {
            if (this.state.network.isConnected && GameController.IsLobbyOwner(this.state))
                AnalyticsController.ReportProgression(AnalyticsProgressionStatus.FAILED, "Online", this.state.network.matchMode.ToString());
            else if (!this.state.network.isConnected)
                AnalyticsController.ReportProgression(AnalyticsProgressionStatus.FAILED, "Offline", this.state.game.Mode.id.ToString());
        }
        switch (StateMonobehaviour<UIState>.Singleton.State.game.Mode.id)
        {
            case GameModeBlueprints.TUTORIAL_LEGACY:
            case GameModeBlueprints.TUTORIAL:
                this.Dispatcher.Run(UIActions.HideButtonHints);
                this.Dispatcher.Run(UIActions.ClearDialogue);
                this.Dispatcher.Run(UIActions.ClearMinorDialogue);
                this.Dispatcher.Run(UIActions.SetGameModeIsUsingScore(false));
                break;
        }
        this._contexts.meta.isRoundOver = false;
        this._contexts.meta.isEndRoundTimer = false;
        this._contexts.meta.isGameOver = true;
        this._contexts.meta.isEndGameTimer = false;
        this._contexts.time.isPaused = false;
        this.gameLogicSystem.TearDown();
        this.ggpoSystem.TearDown();
        GameController.DestroyAllGameEntities();
    }

    public void PrepGarbageCollector()
    {
        GCLatencyMode latencyMode = GCSettings.LatencyMode;
        RuntimeHelpers.PrepareConstrainedRegions();
        try
        {
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
        }
        finally
        {
            GCSettings.LatencyMode = latencyMode;
        }
    }

    private static bool IsLobbyOwner(UIState state)
    {
        bool flag = false;
        foreach (Peer peer in state.network.peers)
        {
            if (peer.IsLocal && peer.isOwner)
                flag = true;
        }
        return flag;
    }

    public static void Trigger321HatsOn() => Contexts.sharedInstance.time.ReplaceCountdown(GameController.Tick + 150);

    private IEnumerator ResetScenario()
    {
        yield return (object)null;
        StateMonobehaviour<UIState>.Singleton.Dispatcher.Run(UIActions.SetTeams(false));
        StateMonobehaviour<UIState>.Singleton.Dispatcher.Run(UIActions.ResetPlayerStates);
        StateMonobehaviour<UIState>.Singleton.Dispatcher.Run(UIActions.SetMaxScore(5));
    }

    public delegate void FXDelegate();
}
