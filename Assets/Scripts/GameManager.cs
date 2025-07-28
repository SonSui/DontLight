using UnityEngine;
using DG.Tweening;
using static GameEvents;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// ゲームの状態を定義する列挙型
/// </summary>
[SerializeField]
public enum GameState
{
    MainMenu,
    LocalPreparation,
    OnlinePreparation,
    OnlineAddWaitingForHost,
    OnlineAddWaitingForPlayers,
    OnlineRoom,
    LocalPlaying,
    OnlinePlaying,
    GameOver,
    OnlinePlayingResults,
    LocalPlayingResults,
    Paused,
    Quit,
    None
};
/// <summary>
/// ゲーム状態とシーン名のマッピング
/// </summary>
[System.Serializable]
public class SceneState
{
    public GameState state;
    public string sceneName;
};
public class GameManager : MonoBehaviour
{
    public GameState CurrentGameState = GameState.MainMenu; // 現在の状態
    private GameState _previousGameState = GameState.None; // 直前の状態を保持

    [Header("GameState to String")]
    public List<SceneState> SceneStatesMap;  // Inspectorで設定されたマッピングリスト
    private Dictionary<GameState, string> _sceneState;  // 実行時に使用する辞書
    
    private List<InputOnlyPlayer> joinedPlayers = new List<InputOnlyPlayer>();
    private int maxPlayers = 4; // 最大プレイヤー数
    private int registeredPlayerCount = 0; // 登録済みプレイヤー数
    public List<Color> playerColors = new List<Color>();
    public int maxBulbCount = 32; // 最大電球数

    public PlayerData winner = new PlayerData();

    public static GameManager Instance { get; private set; }   // シングルトンインスタンス
    private Tween stateTween; // 状態遷移アニメーション（未使用）

    private void Awake()
    {
        // シングルトンの初期化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン間で保持
        }
        else
        {
            Destroy(gameObject);
        }

        Application.targetFrameRate = 60; // フレームレートを60に設定

        // マッピングリストから辞書を構築
        _sceneState = new Dictionary<GameState, string>();
        foreach (var sceneState in SceneStatesMap)
        {
            _sceneState[sceneState.state] = sceneState.sceneName;
        }
    }

    private void OnEnable()
    {
        // イベント購読
        UIEvents.OnLocalStart += HandleLocalStart;
        UIEvents.OnOnlineStart += HandleOnlineStart;
        UIEvents.OnGameClose += HandleGameQuit;
        UIEvents.OnLocalGameStart += HandleLocalGameStart; // ローカルゲーム開始イベント
        UIEvents.OnOnlineGameStart += HandleOnlineGameStart; // オンラインゲーム開始イベント
        UIEvents.OnReturnToTitleScene += HandleOnReturnToTitle; // タイトル画面に戻るイベント
        UIEvents.OnOnlineRoomEnter += HandleOnEnterOnlineRoom;
        UIEvents.OnGameStateChange += HandleOnGameStateChange;


        PlayerEvents.OnPlayerRegistered += RegisterPlayer; // プレイヤー登録イベント
        PlayerEvents.OnWinnerSet += WinnerSetted; // 勝者設定イベント
        PrepareUIEvents.OnSetBulbCount += SetMaxBulbCount; // 最大電球数設定イベント


    }

    private void OnDisable()
    {
        // イベント購読解除
        UIEvents.OnLocalStart -= HandleLocalStart;
        UIEvents.OnOnlineStart -= HandleOnlineStart;
        UIEvents.OnGameClose -= HandleGameQuit;
        UIEvents.OnLocalGameStart -= HandleLocalGameStart; // ローカルゲーム開始イベント
        UIEvents.OnOnlineGameStart -= HandleOnlineGameStart; // オンラインゲーム開始イベント
        UIEvents.OnReturnToTitleScene -= HandleOnReturnToTitle; // タイトル画面に戻るイベント
        UIEvents.OnOnlineRoomEnter -= HandleOnEnterOnlineRoom;
        UIEvents.OnGameStateChange -= HandleOnGameStateChange;
        PlayerEvents.OnPlayerRegistered -= RegisterPlayer; // プレイヤー登録イベント
        PlayerEvents.OnWinnerSet -= WinnerSetted; // 勝者設定イベント
        PrepareUIEvents.OnSetBulbCount -= SetMaxBulbCount; // 最大電球数設定イベント

    }
    private void Start()
    {
        DOVirtual.DelayedCall(0.1f, () =>
        {
            GameSceneEvents.OnBeforeSceneChange?.Invoke(CurrentGameState);
        });
    }

    public void RegisterPlayer(InputOnlyPlayer player)
    {
        if (registeredPlayerCount >= maxPlayers)
        {
            Debug.LogWarning("最大プレイヤー数に達しました。新しいプレイヤーを登録できません。");
            return;
        }

        if (joinedPlayers.Exists(p => p.playerData.input.user == player.playerData.input.user))
        {
            Debug.LogWarning("このプレイヤーはすでに登録されています。");
            return;
        }

        // 情報をセット
        player.playerData.playerIndex = registeredPlayerCount;
        player.playerData.playerName = $"P{registeredPlayerCount + 1}";
        player.playerData.playerColor = playerColors[registeredPlayerCount % playerColors.Count];

        joinedPlayers.Add(player);
        registeredPlayerCount++;

        PrepareUIEvents.OnPlayerDataCreated?.Invoke(player.playerData);
    }

    /// <summary>
    /// ローカル対戦の開始処理
    /// </summary>
    private void HandleLocalStart()
    {
        ChangeState(GameState.LocalPreparation);
    }
    private void HandleLocalGameStart()
    {
        ChangeState(GameState.LocalPlaying);
    }

    /// <summary>
    /// オンライン対戦の開始処理
    /// </summary>
    private void HandleOnlineStart()
    {
        ChangeState(GameState.OnlinePreparation);
    }
    private void HandleOnlineGameStart()
    {
        ChangeState(GameState.OnlinePlaying);
    }

    private void HandleOnReturnToTitle()
    {
        ChangeState(GameState.MainMenu);
    }

    private void HandleOnEnterOnlineRoom()
    {
        ChangeState(GameState.OnlineRoom);
    }
    private void HandleOnGameStateChange(GameState state)
    {
        ChangeState(state);
    }

    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    private void HandleGameQuit()
    {
        ChangeState(GameState.Quit);
    }

    /// <summary>
    /// 状態遷移処理（対応するシーンの読み込み）
    /// </summary>
    private void ChangeState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                if (CurrentGameState == GameState.MainMenu) return;
                if (_sceneState.ContainsKey(CurrentGameState))
                {
                    _previousGameState = CurrentGameState;
                    CurrentGameState = GameState.MainMenu;
                    
                    ClearJoinedPlayers(); // プレイヤーリストをクリア
                    registeredPlayerCount = 0; // 登録済みプレイヤー数をリセット

                    winner = null; // 勝者をリセット

                    GameSceneEvents.OnBeforeSceneChange?.Invoke(CurrentGameState);
                    SceneTransitionManager.Instance.LoadScene(_sceneState[CurrentGameState]);
                }
                else
                {
                    Debug.LogError("MainMenuのシーンが設定されていません。");
                }
                break;

            case GameState.LocalPreparation:
                if (CurrentGameState == GameState.MainMenu)
                {
                    if (_sceneState.ContainsKey(CurrentGameState))
                    {
                        _previousGameState = CurrentGameState;
                        CurrentGameState = GameState.LocalPreparation;
                        GameSceneEvents.OnBeforeSceneChange?.Invoke(CurrentGameState);
                        SceneTransitionManager.Instance.LoadScene(_sceneState[CurrentGameState]);
                    }
                    else
                    {
                        Debug.LogError("LocalPreparationのシーンが設定されていません。");
                    }
                }
                else
                {
                    Debug.LogWarning("ローカル準備状態はメインメニューからのみ開始できます。");
                }
                break;

            case GameState.LocalPlaying:
                if (CurrentGameState == GameState.LocalPreparation && joinedPlayers.Count >= 2)
                {
                    if (_sceneState.ContainsKey(CurrentGameState))
                    {
                        _previousGameState = CurrentGameState;
                        CurrentGameState = GameState.LocalPlaying;
                        GameSceneEvents.OnBeforeSceneChange?.Invoke(CurrentGameState);
                        SceneTransitionManager.Instance.LoadScene(_sceneState[CurrentGameState]);
                    }
                    else
                    {
                        Debug.LogError("LocalPlayingのシーンが設定されていません。");
                    }
                }
                else
                {
                    Debug.LogWarning("ローカルプレイ状態はローカル準備状態からのみ開始できます。参加プレイヤーが2人以上必要です。");
                }
                break;


            case GameState.OnlinePreparation:
                if (CurrentGameState == GameState.MainMenu || CurrentGameState == GameState.OnlineRoom)

                {
                    if (_sceneState.ContainsKey(CurrentGameState))
                    {
                        _previousGameState = CurrentGameState;
                        CurrentGameState = GameState.OnlinePreparation;
                        GameSceneEvents.OnBeforeSceneChange?.Invoke(CurrentGameState);
                        SceneTransitionManager.Instance.LoadScene(_sceneState[CurrentGameState]);
                    }
                    else
                    {
                        Debug.LogError("OnlinePreparationのシーンが設定されていません。");
                    }
                }
                else
                {
                    Debug.LogWarning("オンライン準備状態はMainMenu/OnlineRoomからのみ開始できます。");
                }
                break;
            case GameState.OnlineRoom:
                if(CurrentGameState == GameState.OnlinePreparation)
                {
                    if (_sceneState.ContainsKey(CurrentGameState))
                    {
                        _previousGameState = CurrentGameState;
                        CurrentGameState = GameState.OnlineRoom;
                        GameSceneEvents.OnBeforeSceneChange?.Invoke(CurrentGameState);
                        SceneTransitionManager.Instance.LoadScene(_sceneState[CurrentGameState]);
                    }
                    else
                    {
                        Debug.LogError("OnlineRoomのシーンが設定されていません。");
                    }
                }
                else
                {
                    Debug.LogWarning("OnlineRoom状態はOnlinePreparationからのみ開始できます。");

                }
                break;

            case GameState.LocalPlayingResults:
                if (CurrentGameState == GameState.LocalPlaying)
                {
                    if (_sceneState.ContainsKey(CurrentGameState))
                    {
                        _previousGameState = CurrentGameState;
                        CurrentGameState = GameState.LocalPlayingResults;
                        DOVirtual.DelayedCall(3f, () =>
                        {
                            GameSceneEvents.OnBeforeSceneChange?.Invoke(CurrentGameState);
                            SceneTransitionManager.Instance.LoadScene(_sceneState[CurrentGameState]);
                        });
                    }
                    else
                    {
                        Debug.LogError("LocalPlayingResultsのシーンが設定されていません。");
                    }
                }
                else
                {
                    Debug.LogWarning("ローカルプレイ結果状態はローカルプレイ状態からのみ開始できます。");
                }
                break;


            case GameState.Quit:
                if (CurrentGameState != GameState.Quit)
                {
                    _previousGameState = CurrentGameState;
                    CurrentGameState = GameState.Quit;
                    Application.Quit(); // アプリケーション終了
                }
                else
                {
                    Debug.LogWarning("ゲームはすでに終了しています。");
                }
                break;
            default:
                Debug.LogWarning($"未対応のゲーム状態: {state}");
                break;

        }
    }
    public List<InputOnlyPlayer> GetAllJoinedPlayers()
    {
        return joinedPlayers;
    }

    public void ClearJoinedPlayers()
    {
        foreach (var player in joinedPlayers)
        {
            player.Delete(); // 参加プレイヤーを削除
        }
        joinedPlayers.Clear();
    }

    public int GetMaxBulbCount()
    {
        return maxBulbCount;
    }
    private void SetMaxBulbCount(int count)
    {
        maxBulbCount = count;
        Debug.Log($"最大電球数を{maxBulbCount}に設定しました。");
    }
    private void WinnerSetted(PlayerData data)
    {
        winner = data;
        Debug.Log($"勝者が設定されました: {winner.playerName}");
        ChangeState(GameState.LocalPlayingResults);  
    }
    public PlayerData GetWinner()
    {
        return winner;
    }
}