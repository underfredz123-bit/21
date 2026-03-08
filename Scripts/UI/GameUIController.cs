using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CardArtLibrary artLibrary;

    [Header("Bootstrap")]
    [SerializeField] private bool autoStartRoundIfNeeded = true;

    [Header("Hands")]
    [SerializeField] private HandView playerHandView;
    [SerializeField] private HandView opponentHandView;
    [SerializeField] private TrumpHandView playerTrumpView;
    [SerializeField] private TrumpHandView opponentTrumpView;

    [Header("Text")]
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private TMP_Text targetLabel;
    [SerializeField] private TMP_Text betLabel;
    [SerializeField] private TMP_Text resultLabel;

    [Header("Buttons")]
    [SerializeField] private Button drawButton;
    [SerializeField] private Button passButton;

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged.AddListener(Refresh);
            gameManager.OnRoundEnded.AddListener(OnRoundEnded);

            if (autoStartRoundIfNeeded && !gameManager.HasStartedRound)
            {
                gameManager.StartGame();
            }
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged.RemoveListener(Refresh);
            gameManager.OnRoundEnded.RemoveListener(OnRoundEnded);
        }
    }

    public void StartRound()
    {
        if (gameManager == null)
        {
            return;
        }

        if (resultLabel != null)
        {
            resultLabel.text = string.Empty;
        }

        gameManager.StartGame();
        Refresh();
    }

    public void Refresh()
    {
        if (gameManager == null || artLibrary == null)
        {
            return;
        }

        if (!gameManager.HasStartedRound)
        {
            SetInteractable(false);
            if (statusLabel != null)
            {
                statusLabel.text = "Waiting for round start";
            }

            return;
        }

        PlayerState player = gameManager.GetPlayer();
        PlayerState opponent = gameManager.GetOpponent();

        if (playerHandView != null)
        {
            playerHandView.Refresh(player.Hand, artLibrary, true);
        }

        if (opponentHandView != null)
        {
            opponentHandView.Refresh(opponent.Hand, artLibrary, true);
        }

        if (playerTrumpView != null)
        {
            playerTrumpView.Refresh(player.Trumps, artLibrary);
        }

        if (opponentTrumpView != null)
        {
            opponentTrumpView.Refresh(opponent.Trumps, artLibrary);
        }

        if (statusLabel != null)
        {
            statusLabel.text = $"Turn: {gameManager.GetCurrentTurn()}";
        }

        if (targetLabel != null)
        {
            targetLabel.text = $"Target: {gameManager.TargetScore}";
        }

        if (betLabel != null)
        {
            int playerBet = gameManager.GetEffectiveBetDistance(player);
            int opponentBet = gameManager.GetEffectiveBetDistance(opponent);
            betLabel.text = $"Bet: P {playerBet} | O {opponentBet}";
        }

        SetInteractable(gameManager.GetCurrentTurn() == TurnOwner.Player);
    }

    private void OnRoundEnded(string result)
    {
        if (resultLabel != null)
        {
            resultLabel.text = result;
        }
    }

    private void SetInteractable(bool value)
    {
        if (drawButton != null)
        {
            drawButton.interactable = value;
        }

        if (passButton != null)
        {
            passButton.interactable = value;
        }
    }
}
