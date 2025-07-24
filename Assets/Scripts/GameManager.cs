using UnityEngine;
using DG.Tweening;
using static GameEvents;
using UnityEditor.Experimental.GraphView;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// �Q�[���̏�Ԃ��`����񋓌^
/// </summary>
[SerializeField]
public enum GameState
{
    MainMenu,
    LocalPreparation,
    OnlinePreparation,
    OnlineAddWaitingForHost,
    OnlineAddWaitingForPlayers,
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
/// �Q�[����ԂƃV�[�����̃}�b�s���O
/// </summary>
[System.Serializable]
public class SceneState
{
    public GameState state;
    public string sceneName;
};
public class GameManager : MonoBehaviour
{
    public GameState CurrentGameState = GameState.MainMenu; // ���݂̏��
    private GameState _previousGameState = GameState.None; // ���O�̏�Ԃ�ێ�

    [Header("GameState to String")]
    public List<SceneState> SceneStatesMap;  // Inspector�Őݒ肳�ꂽ�}�b�s���O���X�g
    private Dictionary<GameState, string> _sceneState;  // ���s���Ɏg�p���鎫��
    
    private List<InputOnlyPlayer> joinedPlayers = new List<InputOnlyPlayer>();
    private int maxPlayers = 4; // �ő�v���C���[��
    private int registeredPlayerCount = 0; // �o�^�ς݃v���C���[��
    public List<Color> playerColors = new List<Color>();
    public int maxBulbCount = 32; // �ő�d����

    public static GameManager Instance { get; private set; }   // �V���O���g���C���X�^���X
    private Tween stateTween; // ��ԑJ�ڃA�j���[�V�����i���g�p�j

    private void Awake()
    {
        // �V���O���g���̏�����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �V�[���Ԃŕێ�
        }
        else
        {
            Destroy(gameObject);
        }

        Application.targetFrameRate = 60; // �t���[�����[�g��60�ɐݒ�

        // �}�b�s���O���X�g���玫�����\�z
        _sceneState = new Dictionary<GameState, string>();
        foreach (var sceneState in SceneStatesMap)
        {
            _sceneState[sceneState.state] = sceneState.sceneName;
        }
    }

    private void OnEnable()
    {
        // �C�x���g�w��
        UIEvents.OnLocalStart += HandleLocalStart;
        UIEvents.OnOnlineStart += HandleOnlineStart;
        UIEvents.OnGameClose += HandleGameQuit;
        UIEvents.OnLocalGameStart += HandleLocalGameStart; // ���[�J���Q�[���J�n�C�x���g
        UIEvents.OnOnlineGameStart += HandleOnlineGameStart; // �I�����C���Q�[���J�n�C�x���g
        UIEvents.OnReturnToTitleScene += HandleOnReturnToTitle; // �^�C�g����ʂɖ߂�C�x���g
        PlayerEvents.OnPlayerRegistered += RegisterPlayer; // �v���C���[�o�^�C�x���g
        PrepareUIEvents.OnSetBulbCount += SetMaxBulbCount; // �ő�d�����ݒ�C�x���g

    }

    private void OnDisable()
    {
        // �C�x���g�w�ǉ���
        UIEvents.OnLocalStart -= HandleLocalStart;
        UIEvents.OnOnlineStart -= HandleOnlineStart;
        UIEvents.OnGameClose -= HandleGameQuit;
        UIEvents.OnLocalGameStart -= HandleLocalGameStart; // ���[�J���Q�[���J�n�C�x���g
        UIEvents.OnOnlineGameStart -= HandleOnlineGameStart; // �I�����C���Q�[���J�n�C�x���g
        UIEvents.OnReturnToTitleScene -= HandleOnReturnToTitle; // �^�C�g����ʂɖ߂�C�x���g
        PlayerEvents.OnPlayerRegistered -= RegisterPlayer; // �v���C���[�o�^�C�x���g
        PrepareUIEvents.OnSetBulbCount -= SetMaxBulbCount; // �ő�d�����ݒ�C�x���g
    }


    public void RegisterPlayer(InputOnlyPlayer player)
    {
        if (registeredPlayerCount >= maxPlayers)
        {
            Debug.LogWarning("�ő�v���C���[���ɒB���܂����B�V�����v���C���[��o�^�ł��܂���B");
            return;
        }

        if (joinedPlayers.Exists(p => p.playerData.input.user == player.playerData.input.user))
        {
            Debug.LogWarning("���̃v���C���[�͂��łɓo�^����Ă��܂��B");
            return;
        }

        // �����Z�b�g
        player.playerData.playerIndex = registeredPlayerCount;
        player.playerData.playerName = $"P{registeredPlayerCount + 1}";
        player.playerData.playerColor = playerColors[registeredPlayerCount % playerColors.Count];

        joinedPlayers.Add(player);
        registeredPlayerCount++;

        PrepareUIEvents.OnPlayerDataCreated?.Invoke(player.playerData);
    }

    /// <summary>
    /// ���[�J���ΐ�̊J�n����
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
    /// �I�����C���ΐ�̊J�n����
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

    /// <summary>
    /// �Q�[���I������
    /// </summary>
    private void HandleGameQuit()
    {
        ChangeState(GameState.Quit);
    }

    /// <summary>
    /// ��ԑJ�ڏ����i�Ή�����V�[���̓ǂݍ��݁j
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
                    
                    ClearJoinedPlayers(); // �v���C���[���X�g���N���A
                    registeredPlayerCount = 0; // �o�^�ς݃v���C���[�������Z�b�g
                    SceneTransitionManager.Instance.LoadScene(_sceneState[CurrentGameState]);
                }
                else
                {
                    Debug.LogError("MainMenu�̃V�[�����ݒ肳��Ă��܂���B");
                }
                break;

            case GameState.LocalPreparation:
                if (CurrentGameState == GameState.MainMenu)
                {
                    _previousGameState = CurrentGameState;
                    CurrentGameState = GameState.LocalPreparation;

                    if (_sceneState.ContainsKey(CurrentGameState))
                    {
                        SceneTransitionManager.Instance.LoadScene(_sceneState[CurrentGameState]);
                    }
                    else
                    {
                        CurrentGameState = _previousGameState; // �V�[�����ݒ肳��Ă��Ȃ��ꍇ�͑O�̏�Ԃɖ߂�
                        Debug.LogError("LocalPreparation�̃V�[�����ݒ肳��Ă��܂���B");
                    }
                }
                else
                {
                    Debug.LogWarning("���[�J��������Ԃ̓��C�����j���[����̂݊J�n�ł��܂��B");
                }
                break;

            case GameState.LocalPlaying:
                if (CurrentGameState == GameState.LocalPreparation && joinedPlayers.Count >= 2)
                {
                    _previousGameState = CurrentGameState;
                    CurrentGameState = GameState.LocalPlaying;
                    if (_sceneState.ContainsKey(CurrentGameState))
                    {
                        SceneTransitionManager.Instance.LoadScene(_sceneState[CurrentGameState]);
                    }
                    else
                    {
                        CurrentGameState = _previousGameState; // �V�[�����ݒ肳��Ă��Ȃ��ꍇ�͑O�̏�Ԃɖ߂�
                        Debug.LogError("LocalPlaying�̃V�[�����ݒ肳��Ă��܂���B");
                    }
                }
                else
                {
                    Debug.LogWarning("���[�J���v���C��Ԃ̓��[�J��������Ԃ���̂݊J�n�ł��܂��B�Q���v���C���[��2�l�ȏ�K�v�ł��B");
                }
                break;


            case GameState.OnlinePreparation:
                if (CurrentGameState == GameState.MainMenu)
                {
                    _previousGameState = CurrentGameState;
                    CurrentGameState = GameState.OnlinePreparation;

                    if (_sceneState.ContainsKey(CurrentGameState))
                    {
                        SceneTransitionManager.Instance.LoadScene(_sceneState[CurrentGameState]);
                    }
                    else
                    {
                        CurrentGameState = _previousGameState; // �V�[�����ݒ肳��Ă��Ȃ��ꍇ�͑O�̏�Ԃɖ߂�
                        Debug.LogError("OnlinePreparation�̃V�[�����ݒ肳��Ă��܂���B");
                    }
                }
                else
                {
                    Debug.LogWarning("�I�����C��������Ԃ̓��C�����j���[����̂݊J�n�ł��܂��B");
                }
                break;


            case GameState.Quit:
                if (CurrentGameState != GameState.Quit)
                {
                    _previousGameState = CurrentGameState;
                    CurrentGameState = GameState.Quit;
                    Application.Quit(); // �A�v���P�[�V�����I��
                }
                else
                {
                    Debug.LogWarning("�Q�[���͂��łɏI�����Ă��܂��B");
                }
                break;
            default:
                Debug.LogWarning($"���Ή��̃Q�[�����: {state}");
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
            player.Delete(); // �Q���v���C���[���폜
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
        Debug.Log($"�ő�d������{maxBulbCount}�ɐݒ肵�܂����B");
    }
}