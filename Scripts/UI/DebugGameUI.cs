using System.Text;
using UnityEngine;

public class DebugGameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private bool autoFindGameManager = true;
    [SerializeField] private bool autoStartRoundIfNeeded = true;

    [Header("Layout")]
    [SerializeField] private Rect windowRect = new Rect(16f, 16f, 460f, 560f);
    [SerializeField] private bool startVisible = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    private string _roundResult = string.Empty;
    private string _actionHint = "-";
    private bool _visible;
    private int _windowId;

    private void Awake()
    {
        _windowId = GetInstanceID();
    }

    private void OnEnable()
    {
        _visible = startVisible;
        EnsureManager();

        if (gameManager != null)
        {
            gameManager.OnRoundEnded.AddListener(OnRoundEnded);
            if (autoStartRoundIfNeeded && !gameManager.HasStartedRound)
            {
                gameManager.StartGame();
            }
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnRoundEnded.RemoveListener(OnRoundEnded);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            _visible = !_visible;
            _actionHint = _visible ? "Debug opened" : "Debug hidden";
        }

        if (!_visible)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.D)) TryDraw();
        if (Input.GetKeyDown(KeyCode.P)) TryPass();
        if (Input.GetKeyDown(KeyCode.R)) TryRestart();

        if (Input.GetKeyDown(KeyCode.Alpha1)) TryTrump(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryTrump(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryTrump(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) TryTrump(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) TryTrump(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) TryTrump(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) TryTrump(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) TryTrump(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) TryTrump(8);
    }

    private void OnGUI()
    {
        EnsureManager();

        if (!_visible)
        {
            if (GUI.Button(new Rect(12f, 12f, 120f, 28f), $"Open ({toggleKey})"))
            {
                _visible = true;
                _actionHint = "Debug opened";
            }

            return;
        }

        windowRect = GUI.Window(_windowId, windowRect, DrawWindow, "Debug Game UI (OnGUI)");
    }

    private void DrawWindow(int id)
    {
        float x = 12f;
        float y = 28f;
        float width = windowRect.width - 24f;

        if (gameManager == null)
        {
            GUI.Label(new Rect(x, y, width, 24f), "GameManager not found.");
            y += 26f;

            if (GUI.Button(new Rect(x, y, width, 28f), "Find GameManager"))
            {
                EnsureManager(true);
                _actionHint = gameManager != null ? "GameManager found" : "GameManager still not found";
            }

            y += 34f;
            GUI.Label(new Rect(x, y, width, 24f), $"Press {toggleKey} to hide/show.");
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
            return;
        }

        PlayerState player = gameManager.GetPlayer();
        PlayerState opponent = gameManager.GetOpponent();

        GUI.Label(new Rect(x, y, width, 22f), $"Turn: {gameManager.GetCurrentTurn()}"); y += 22f;
        GUI.Label(new Rect(x, y, width, 22f), $"Target: {gameManager.TargetScore}"); y += 22f;
        GUI.Label(new Rect(x, y, width, 22f), $"Player Score: {player.TotalAll()}"); y += 22f;
        GUI.Label(new Rect(x, y, width, 22f), $"Opponent Score: {FormatOpponentScore(opponent)}"); y += 26f;

        GUI.Label(new Rect(x, y, width, 22f), "Player Cards: " + FormatCards(player, true)); y += 22f;
        GUI.Label(new Rect(x, y, width, 22f), "Opponent Cards: " + FormatCards(opponent, false)); y += 30f;

        float buttonWidth = (width - 8f) / 3f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 30f), "Draw")) TryDraw();
        if (GUI.Button(new Rect(x + buttonWidth + 4f, y, buttonWidth, 30f), "Pass")) TryPass();
        if (GUI.Button(new Rect(x + (buttonWidth + 4f) * 2f, y, buttonWidth, 30f), "Restart")) TryRestart();
        y += 38f;

        GUI.Label(new Rect(x, y, width, 22f), "Available Trumps:");
        y += 24f;

        if (player.Trumps.Count == 0)
        {
            GUI.Label(new Rect(x, y, width, 22f), "(no trumps)");
            y += 24f;
        }
        else
        {
            for (int i = 0; i < player.Trumps.Count; i++)
            {
                int index = i;
                TrumpCard trump = player.Trumps[index];
                if (GUI.Button(new Rect(x, y, width, 26f), $"Use [{index + 1}] {trump.Name}"))
                {
                    TryTrump(index);
                }

                y += 30f;
            }
        }

        GUI.Label(new Rect(x, y, width, 22f), "Round Result:");
        y += 24f;

        string resultText = string.IsNullOrEmpty(_roundResult) ? "-" : _roundResult;
        GUI.TextArea(new Rect(x, y, width, 72f), resultText);
        y += 78f;

        GUI.Label(new Rect(x, y, width, 22f), "Hint: " + _actionHint);
        y += 22f;
        GUI.Label(new Rect(x, y, width, 22f), $"Hotkeys: {toggleKey}, D, P, R, 1..9");

        GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
    }

    private static string FormatCards(PlayerState state, bool revealAll)
    {
        if (state == null || state.Hand.Count == 0)
        {
            return "(none)";
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < state.Hand.Count; i++)
        {
            Card card = state.Hand[i];
            if (i > 0)
            {
                sb.Append(", ");
            }

            bool visible = revealAll || card.IsFaceUp;
            sb.Append(visible ? card.Value.ToString() : "?");
        }

        return sb.ToString();
    }

    private static string FormatOpponentScore(PlayerState opponent)
    {
        if (opponent == null)
        {
            return "-";
        }

        int visible = opponent.TotalVisible();
        int hiddenCount = 0;
        for (int i = 0; i < opponent.Hand.Count; i++)
        {
            if (!opponent.Hand[i].IsFaceUp)
            {
                hiddenCount++;
            }
        }

        return hiddenCount > 0 ? $"{visible} + ?" : visible.ToString();
    }

    private void TryDraw()
    {
        if (!EnsureManagerAndRound())
        {
            return;
        }

        bool ok = gameManager.PlayerDraw();
        _actionHint = ok ? "Draw OK" : "Draw blocked (not your turn or round ended).";
        Debug.Log($"DebugGameUI Draw => {ok}");
    }

    private void TryPass()
    {
        if (!EnsureManagerAndRound())
        {
            return;
        }

        bool ok = gameManager.PlayerPass();
        _actionHint = ok ? "Pass OK" : "Pass blocked (not your turn or round ended).";
        Debug.Log($"DebugGameUI Pass => {ok}");
    }

    private void TryRestart()
    {
        if (gameManager == null)
        {
            EnsureManager(true);
        }

        if (gameManager == null)
        {
            _actionHint = "GameManager not found.";
            return;
        }

        _roundResult = string.Empty;
        gameManager.StartGame();
        _actionHint = "Round restarted";
        Debug.Log("DebugGameUI Restart => round restarted");
    }

    private void TryTrump(int index)
    {
        if (!EnsureManagerAndRound())
        {
            return;
        }

        PlayerState player = gameManager.GetPlayer();
        if (player == null || index < 0 || index >= player.Trumps.Count)
        {
            _actionHint = "Trump index unavailable.";
            return;
        }

        TrumpCard trump = player.Trumps[index];
        bool ok = gameManager.PlayerUseTrump(index);
        _actionHint = ok ? $"Trump used: {trump.Name}" : "Trump blocked (not your turn or invalid).";
        Debug.Log($"DebugGameUI Trump[{index}] => {ok}");
    }

    private bool EnsureManagerAndRound()
    {
        EnsureManager();
        if (gameManager == null)
        {
            _actionHint = "GameManager not found.";
            return false;
        }

        if (autoStartRoundIfNeeded && !gameManager.HasStartedRound)
        {
            gameManager.StartGame();
        }

        return true;
    }

    private void OnRoundEnded(string result)
    {
        _roundResult = result;
    }

    private void EnsureManager(bool force = false)
    {
        if (!force && gameManager != null)
        {
            return;
        }

        if (autoFindGameManager || force)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }
}
