using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class ResultSceneController : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;
    public PlayerData winnerData = null;

    public TextMeshProUGUI nameText;

    public float countdownTime = 3f;
    public GameObject firstSelectedButton;

    private GameObject spawnedPlayer;

    private void Start()
    {
        winnerData = GameManager.Instance.GetWinner();
        if (winnerData != null)
        {
            nameText.text = winnerData.playerName;
            SpawnWinnerPlayer();
        }
        else
        {
            nameText.text = "No Winner";
        }
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    void Update()
    {
        if (countdownTime > 0f)
        {
            countdownTime -= Time.deltaTime;
        }   
    }
    public void OnReturnButtonSelected()
    {
        if(countdownTime>0f)return;

        GameEvents.UIEvents.OnReturnToTitleScene.Invoke();
    }
    private void SpawnWinnerPlayer()
    {
        if (winnerData == null || winnerData.devices == null || winnerData.devices.Count == 0)
        {
            Debug.LogWarning("Winner data invalid or no devices.");
            return;
        }

        var inputPlayer = PlayerInput.Instantiate(
            playerPrefab,
            controlScheme: winnerData.controlScheme,
            pairWithDevices: winnerData.devices.ToArray(),
            playerIndex: winnerData.playerIndex
        );

        inputPlayer.neverAutoSwitchControlSchemes = true; // ‘¼‚Ì“ü—Í‹ÖŽ~
        inputPlayer.transform.position = spawnPoint.position;

        var testScript = inputPlayer.GetComponent<PlayerTestSon>();
        if (testScript != null)
        {
            testScript.SetPlayerData(winnerData);
        }

        spawnedPlayer = inputPlayer.gameObject;
    }

    private void OnDisable()
    {
        if (spawnedPlayer != null)
        {
            var input = spawnedPlayer.GetComponent<PlayerInput>();
            if (input != null && input.user.valid)
            {
                input.user.UnpairDevicesAndRemoveUser();
            }

            Destroy(spawnedPlayer);
            spawnedPlayer = null;
        }
    }
}
