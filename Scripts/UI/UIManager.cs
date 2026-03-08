using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CardArtLibrary artLibrary;
    [SerializeField] private bool autoStartRound = true;
    [SerializeField] private bool disableLegacyOnGuiOverlaysWhenCanvasUiActive = false;

    [Header("Hands")]
    [SerializeField] private HandView playerHandView;
    [SerializeField] private HandView opponentHandView;
    [SerializeField] private RectTransform playerCardsRoot;
    [SerializeField] private RectTransform opponentCardsRoot;

    [Header("Counters")]
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text opponentScoreText;

    [Header("Top text")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text resultText;

    [Header("Deck / center")]
    [SerializeField] private RectTransform deckZone;
    [SerializeField] private RectTransform trumpPreviewRoot;
    [SerializeField] private Image sawImage;
    [SerializeField] private CardDealAnimator cardDealAnimator;

    [Header("Buttons")]
    [SerializeField] private Button drawButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button trumpButton;
    [SerializeField] private Button restartButton;

    [Header("Trump menu")]
    [SerializeField] private TrumpMenuToggle trumpMenuToggle;
    [SerializeField] private RectTransform sideTrumpMenuPanel;
    [SerializeField] private Transform trumpMenuRoot;
    [SerializeField] private Button trumpButtonPrefab;

    private readonly List<Button> _menuButtons = new List<Button>();

    private Card _lastPlayerDraw;
    private Card _lastOpponentDraw;
    private Image _previewImage;

    private void OnEnable()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogWarning("UIManager: GameManager not found.");
            return;
        }

        AutoBindByNames();
        EnsureEventSystemExists();
        if (disableLegacyOnGuiOverlaysWhenCanvasUiActive)
        {
            DisableLegacyDebugOverlays();
        }

        gameManager.OnStateChanged.AddListener(OnStateChanged);
        gameManager.OnRoundEnded.AddListener(OnRoundEnded);

        if (drawButton != null)
        {
            drawButton.onClick.AddListener(OnDrawClicked);
        }

        if (passButton != null)
        {
            passButton.onClick.AddListener(OnPassClicked);
        }

        if (trumpButton != null)
        {
            trumpButton.onClick.AddListener(OnTrumpClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(StartRound);
        }

        if (playerHandView != null)
        {
            playerHandView.CardClicked += OnPlayerCardClicked;
        }

        EnsureTrumpPreviewImage();
        InitializeTrumpMenuPanel();

        if (autoStartRound && !gameManager.HasStartedRound)
        {
            gameManager.StartGame();
        }

        RefreshAll();
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged.RemoveListener(OnStateChanged);
            gameManager.OnRoundEnded.RemoveListener(OnRoundEnded);
        }

        if (drawButton != null)
        {
            drawButton.onClick.RemoveListener(OnDrawClicked);
        }

        if (passButton != null)
        {
            passButton.onClick.RemoveListener(OnPassClicked);
        }

        if (trumpButton != null)
        {
            trumpButton.onClick.RemoveListener(OnTrumpClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(StartRound);
        }

        if (playerHandView != null)
        {
            playerHandView.CardClicked -= OnPlayerCardClicked;
        }
    }

    public void StartRound()
    {
        if (gameManager == null)
        {
            return;
        }

        _lastPlayerDraw = null;
        _lastOpponentDraw = null;

        if (resultText != null)
        {
            resultText.text = string.Empty;
        }

        HideTrumpMenu();
        gameManager.StartGame();
    }

    public void ToggleTrumpMenu()
    {
        if (trumpMenuToggle != null)
        {
            trumpMenuToggle.Toggle();
            return;
        }

        if (sideTrumpMenuPanel != null)
        {
            bool next = !sideTrumpMenuPanel.gameObject.activeSelf;
            sideTrumpMenuPanel.gameObject.SetActive(next);
        }
    }

    public void HideTrumpMenu()
    {
        if (trumpMenuToggle != null)
        {
            trumpMenuToggle.Hide();
            return;
        }

        if (sideTrumpMenuPanel != null)
        {
            sideTrumpMenuPanel.gameObject.SetActive(false);
        }
    }

    private void OnDrawClicked()
    {
        if (gameManager == null)
        {
            return;
        }

        bool ok = gameManager.PlayerDraw();
        if (!ok && statusText != null)
        {
            statusText.text = "Draw blocked: not your turn or round ended.";
        }
    }

    private void OnPassClicked()
    {
        if (gameManager == null)
        {
            return;
        }

        bool ok = gameManager.PlayerPass();
        if (!ok && statusText != null)
        {
            statusText.text = "Pass blocked: not your turn or round ended.";
        }
    }

    private void OnTrumpClicked()
    {
        ToggleTrumpMenu();
    }

    private void OnPlayerCardClicked(int index, Card card)
    {
        if (statusText != null && card != null)
        {
            statusText.text = $"Selected card: {card.Value}";
        }
    }

    private void OnStateChanged()
    {
        AnimateNewDrawsFromDeck();
        RefreshAll();
    }

    private void OnRoundEnded(string result)
    {
        if (resultText != null)
        {
            resultText.text = result;
        }
    }

    private void RefreshAll()
    {
        if (gameManager == null || artLibrary == null)
        {
            return;
        }

        PlayerState player = gameManager.GetPlayer();
        PlayerState opponent = gameManager.GetOpponent();
        if (player == null || opponent == null)
        {
            return;
        }

        if (playerHandView != null)
        {
            playerHandView.Refresh(player.Hand, artLibrary, false);
            playerHandView.SetCardInteractable(gameManager.GetCurrentTurn() == TurnOwner.Player);
        }

        if (opponentHandView != null)
        {
            opponentHandView.Refresh(opponent.Hand, artLibrary, false);
            opponentHandView.SetCardInteractable(false);
        }

        RefreshScoreTexts(player, opponent);
        RefreshStatusText(player, opponent);
        RefreshActionButtons();
        RefreshTrumpPreview(player);
        RebuildTrumpMenu(player.Trumps);
    }

    private void RefreshScoreTexts(PlayerState player, PlayerState opponent)
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = $"Player: {player.TotalAll()}";
        }

        if (opponentScoreText != null)
        {
            int visible = opponent.TotalVisible();
            int hidden = opponent.Hand.Count - opponent.Hand.FindAll(card => card.IsFaceUp).Count;
            opponentScoreText.text = hidden > 0
                ? $"Opponent: {visible} + ?"
                : $"Opponent: {visible}";
        }
    }

    private void RefreshStatusText(PlayerState player, PlayerState opponent)
    {
        if (statusText != null)
        {
            statusText.text = $"Turn: {gameManager.GetCurrentTurn()} | Target: {gameManager.TargetScore} | P:{player.TotalAll()} O:{opponent.TotalVisible()}+?";
        }
    }

    private void RefreshActionButtons()
    {
        bool isPlayerTurn = gameManager.GetCurrentTurn() == TurnOwner.Player;

        if (drawButton != null)
        {
            drawButton.interactable = isPlayerTurn;
        }

        if (passButton != null)
        {
            passButton.interactable = isPlayerTurn;
        }

        if (trumpButton != null)
        {
            trumpButton.interactable = isPlayerTurn;
        }
    }

    private void RebuildTrumpMenu(List<TrumpCard> trumps)
    {
        if (trumpMenuRoot == null || trumpButtonPrefab == null)
        {
            return;
        }

        for (int i = 0; i < _menuButtons.Count; i++)
        {
            if (_menuButtons[i] != null)
            {
                Destroy(_menuButtons[i].gameObject);
            }
        }

        _menuButtons.Clear();

        bool isPlayerTurn = gameManager.GetCurrentTurn() == TurnOwner.Player;
        for (int i = 0; i < trumps.Count; i++)
        {
            int trumpIndex = i;
            TrumpCard trump = trumps[i];

            Button button = Instantiate(trumpButtonPrefab, trumpMenuRoot);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = $"[{i + 1}] {trump.Name}";
            }

            button.interactable = isPlayerTurn;
            button.onClick.AddListener(() =>
            {
                if (!isPlayerTurn)
                {
                    return;
                }

                bool used = gameManager.PlayerUseTrump(trumpIndex);
                if (used)
                {
                    HideTrumpMenu();
                }
            });

            _menuButtons.Add(button);
        }
    }

    private void RefreshTrumpPreview(PlayerState player)
    {
        if (_previewImage == null)
        {
            return;
        }

        if (player.Trumps.Count > 0)
        {
            _previewImage.enabled = true;
            _previewImage.sprite = artLibrary.GetTrumpSprite(player.Trumps[0]);
        }
        else
        {
            _previewImage.enabled = false;
            _previewImage.sprite = null;
        }
    }

    private void AnimateNewDrawsFromDeck()
    {
        if (cardDealAnimator == null || deckZone == null || artLibrary == null || gameManager == null)
        {
            return;
        }

        PlayerState player = gameManager.GetPlayer();
        PlayerState opponent = gameManager.GetOpponent();
        if (player == null || opponent == null)
        {
            return;
        }

        if (player.LastDrawnCard != null && player.LastDrawnCard != _lastPlayerDraw)
        {
            RectTransform target = playerCardsRoot != null ? playerCardsRoot : playerHandView?.transform as RectTransform;
            cardDealAnimator.AnimateFromDeck(player.LastDrawnCard, artLibrary, deckZone, target, !player.LastDrawnCard.IsFaceUp);
            _lastPlayerDraw = player.LastDrawnCard;
        }

        if (opponent.LastDrawnCard != null && opponent.LastDrawnCard != _lastOpponentDraw)
        {
            RectTransform target = opponentCardsRoot != null ? opponentCardsRoot : opponentHandView?.transform as RectTransform;
            cardDealAnimator.AnimateFromDeck(opponent.LastDrawnCard, artLibrary, deckZone, target, !opponent.LastDrawnCard.IsFaceUp);
            _lastOpponentDraw = opponent.LastDrawnCard;
        }
    }

    private void InitializeTrumpMenuPanel()
    {
        if (trumpMenuToggle == null && sideTrumpMenuPanel != null)
        {
            trumpMenuToggle = sideTrumpMenuPanel.GetComponent<TrumpMenuToggle>();
            if (trumpMenuToggle == null)
            {
                trumpMenuToggle = sideTrumpMenuPanel.gameObject.AddComponent<TrumpMenuToggle>();
            }
        }

        if (trumpMenuToggle != null)
        {
            trumpMenuToggle.Initialize(sideTrumpMenuPanel != null ? sideTrumpMenuPanel.gameObject : null, false);
        }
        else if (sideTrumpMenuPanel != null)
        {
            sideTrumpMenuPanel.gameObject.SetActive(false);
        }
    }

    private void EnsureTrumpPreviewImage()
    {
        if (trumpPreviewRoot == null || _previewImage != null)
        {
            return;
        }

        _previewImage = trumpPreviewRoot.GetComponentInChildren<Image>();
        if (_previewImage != null)
        {
            return;
        }

        GameObject preview = new GameObject("TrumpPreviewImage", typeof(RectTransform), typeof(Image));
        preview.transform.SetParent(trumpPreviewRoot, false);

        _previewImage = preview.GetComponent<Image>();
        _previewImage.preserveAspect = true;

        RectTransform rect = preview.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(4f, 4f);
        rect.offsetMax = new Vector2(-4f, -4f);
    }

    private void EnsureEventSystemExists()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystem.transform.SetAsLastSibling();
        Debug.Log("UIManager: Auto-created EventSystem for UI buttons.");
    }

    private void DisableLegacyDebugOverlays()
    {
        // Structured UI and old OnGUI overlays should not run together.
        DebugGameUI legacyDebug = FindFirstObjectByType<DebugGameUI>();
        if (legacyDebug != null && legacyDebug.enabled)
        {
            legacyDebug.enabled = false;
        }

    }

    private void AutoBindByNames()
    {
        // Optional safety: if inspector links are missing, try find by layout names.
        if (drawButton == null)
        {
            drawButton = GameObject.Find("DrawButton")?.GetComponent<Button>();
        }

        if (passButton == null)
        {
            passButton = GameObject.Find("PassButton")?.GetComponent<Button>();
        }

        if (trumpButton == null)
        {
            trumpButton = GameObject.Find("TrumpButton")?.GetComponent<Button>();
        }

        if (restartButton == null)
        {
            restartButton = GameObject.Find("RestartButton")?.GetComponent<Button>();
        }

        if (sideTrumpMenuPanel == null)
        {
            sideTrumpMenuPanel = GameObject.Find("SideTrumpMenuPanel")?.GetComponent<RectTransform>();
        }

        if (trumpMenuRoot == null)
        {
            trumpMenuRoot = GameObject.Find("TrumpMenuRoot")?.transform;
        }

        if (deckZone == null)
        {
            deckZone = GameObject.Find("DeckZone")?.GetComponent<RectTransform>();
        }

        if (playerCardsRoot == null)
        {
            playerCardsRoot = GameObject.Find("PlayerCardsRoot")?.GetComponent<RectTransform>();
        }

        if (opponentCardsRoot == null)
        {
            opponentCardsRoot = GameObject.Find("OpponentCardsRoot")?.GetComponent<RectTransform>();
        }
    }

}
