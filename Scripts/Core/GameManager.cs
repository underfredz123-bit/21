using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum TurnOwner
{
    Player,
    Opponent
}

public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool autoStartOnPlay = true;
    [SerializeField] private int startingTrumpCount = 1;
    [SerializeField] private float opponentThinkDelay = 0.75f;
    [SerializeField] private int baseTargetScore = 21;
    [SerializeField] private int baseBetDistance = 5;

    [Header("Events")]
    public UnityEvent OnStateChanged = new UnityEvent();
    public UnityEvent<string> OnRoundEnded = new UnityEvent<string>();

    private readonly Deck _deck = new Deck();
    private readonly List<PlacedTrump> _placedTrumps = new List<PlacedTrump>();
    private readonly List<TrumpCard> _trumpPool = new List<TrumpCard>();
    private PlayerState _player;
    private PlayerState _opponent;
    private TurnOwner _currentTurn;
    private bool _roundOver;
    private int _targetScore;
    private int _betDistance;

    private void Awake()
    {
        _player = new PlayerState("Player");
        _opponent = new PlayerState("Opponent");
    }

    private void Start()
    {
        if (autoStartOnPlay)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        _roundOver = false;
        _deck.Reset();
        _player.Reset();
        _opponent.Reset();
        _placedTrumps.Clear();
        BuildTrumpPool();

        _targetScore = baseTargetScore;
        _betDistance = baseBetDistance;

        DealStartingHand(_player);
        DealStartingHand(_opponent);

        DealStartingTrumps(_player);
        DealStartingTrumps(_opponent);

        _currentTurn = TurnOwner.Player;
        OnStateChanged?.Invoke();
    }

    public PlayerState GetPlayer() => _player;
    public PlayerState GetOpponent() => _opponent;
    public TurnOwner GetCurrentTurn() => _currentTurn;
    public int TargetScore => _targetScore;
    public int BetDistance => _betDistance;
    public bool HasStartedRound => _player != null && _opponent != null && _player.Hand.Count > 0 && _opponent.Hand.Count > 0;
    public int GetEffectiveBetDistance(PlayerState owner)
    {
        PlayerState opponent = owner == _player ? _opponent : _player;
        int modifier = owner.BetModifier;
        if (owner.BlessActive && IsLeading(owner, opponent))
        {
            modifier += 1;
        }

        return Mathf.Max(0, _betDistance + modifier);
    }

    public bool PlayerDraw()
    {
        if (!IsPlayerTurn() || _roundOver)
        {
            return false;
        }

        DrawCard(_player, true);
        EndTurn();
        return true;
    }

    public bool PlayerPass()
    {
        if (!IsPlayerTurn() || _roundOver)
        {
            return false;
        }

        _player.HasPassed = true;
        EndTurn();
        return true;
    }

    public bool PlayerUseTrump(int index)
    {
        if (!IsPlayerTurn() || _roundOver)
        {
            return false;
        }

        if (index < 0 || index >= _player.Trumps.Count)
        {
            Debug.LogWarning("Invalid trump index.");
            return false;
        }

        TrumpCard trump = _player.Trumps[index];
        _player.Trumps.RemoveAt(index);
        ResolveTrump(trump, _player, _opponent);
        EndTurn();
        return true;
    }

    private bool IsPlayerTurn() => _currentTurn == TurnOwner.Player;

    private void DealStartingHand(PlayerState target)
    {
        DrawCard(target, true);
        DrawCard(target, false);
    }

    private void DealStartingTrumps(PlayerState target)
    {
        target.Trumps.Clear();
        for (int i = 0; i < startingTrumpCount; i++)
        {
            TrumpCard trump = DrawTrumpFromPool();
            if (trump != null)
            {
                target.Trumps.Add(trump);
            }
        }
    }

    private void DrawCard(PlayerState target, bool faceUp)
    {
        int value = _deck.DrawRandom();
        if (value == -1)
        {
            return;
        }

        target.AddCard(new Card(value, faceUp));
    }

    private void EndTurn()
    {
        if (_player.HasPassed && _opponent.HasPassed)
        {
            EndRound();
            return;
        }

        _currentTurn = _currentTurn == TurnOwner.Player ? TurnOwner.Opponent : TurnOwner.Player;
        OnStateChanged?.Invoke();

        if (_currentTurn == TurnOwner.Opponent)
        {
            StartCoroutine(OpponentTurn());
        }
    }

    private IEnumerator OpponentTurn()
    {
        yield return new WaitForSeconds(opponentThinkDelay);
        if (_roundOver)
        {
            yield break;
        }

        if (TryOpponentUseTrump())
        {
            EndTurn();
            yield break;
        }

        bool shouldDraw = ShouldOpponentDraw();
        if (shouldDraw)
        {
            DrawCard(_opponent, true);
        }
        else
        {
            _opponent.HasPassed = true;
        }

        EndTurn();
    }

    private bool ShouldOpponentDraw()
    {
        if (_opponent.HasPassed || _deck.IsEmpty)
        {
            return false;
        }

        int opponentScore = _opponent.TotalAll();
        int playerScore = _player.TotalAll();

        // Strong immediate rules.
        if (opponentScore >= _targetScore)
        {
            return false;
        }

        if (opponentScore <= 13)
        {
            return true;
        }

        // If opponent is already ahead on a safe total, prefer pass.
        if (opponentScore >= 17 && opponentScore >= playerScore)
        {
            return false;
        }

        float bustRisk = EstimateBustRisk(opponentScore);

        // Draw only when the risk is acceptable or when the opponent is clearly behind.
        bool farBehind = playerScore - opponentScore >= 3;
        if (bustRisk >= 0.50f && !farBehind)
        {
            return false;
        }

        if (bustRisk >= 0.70f)
        {
            return false;
        }

        // If behind, allow a little more risk; if ahead, be more conservative.
        if (opponentScore > playerScore && bustRisk >= 0.35f)
        {
            return false;
        }

        return true;
    }

    private float EstimateBustRisk(int currentScore)
    {
        if (_deck.AvailableCount <= 0)
        {
            return 1f;
        }

        int safeDelta = _targetScore - currentScore;
        if (safeDelta <= 0)
        {
            return 1f;
        }

        int bustOptions = 0;
        for (int value = 1; value <= 11; value++)
        {
            if (_deck.HasValue(value) && value > safeDelta)
            {
                bustOptions++;
            }
        }

        return (float)bustOptions / _deck.AvailableCount;
    }

    private bool TryOpponentUseTrump()
    {
        if (_opponent.Trumps.Count == 0)
        {
            return false;
        }

        int trumpIndex = SelectBestOpponentTrumpIndex();
        if (trumpIndex < 0 || trumpIndex >= _opponent.Trumps.Count)
        {
            return false;
        }

        TrumpCard trump = _opponent.Trumps[trumpIndex];
        _opponent.Trumps.RemoveAt(trumpIndex);
        ResolveTrump(trump, _opponent, _player);
        return true;
    }

    private int SelectBestOpponentTrumpIndex()
    {
        int opponentScore = _opponent.TotalAll();
        int playerScore = _player.TotalAll();

        bool opponentBehind = opponentScore < playerScore;
        bool closeToBust = opponentScore >= _targetScore - 1;
        bool playerDangerous = playerScore >= _targetScore - 2;

        for (int i = 0; i < _opponent.Trumps.Count; i++)
        {
            TrumpCard trump = _opponent.Trumps[i];
            if (closeToBust && IsTargetShiftTrump(trump))
            {
                return i;
            }
        }

        for (int i = 0; i < _opponent.Trumps.Count; i++)
        {
            TrumpCard trump = _opponent.Trumps[i];
            if (opponentBehind && IsCardGainTrump(trump))
            {
                return i;
            }
        }

        for (int i = 0; i < _opponent.Trumps.Count; i++)
        {
            TrumpCard trump = _opponent.Trumps[i];
            if (playerDangerous && IsDisruptTrump(trump))
            {
                return i;
            }
        }

        for (int i = 0; i < _opponent.Trumps.Count; i++)
        {
            TrumpCard trump = _opponent.Trumps[i];
            if (trump.EffectType == TrumpEffectType.Bless || trump.EffectType == TrumpEffectType.ShieldPlusOne || trump.EffectType == TrumpEffectType.ShieldPlusTwo)
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsTargetShiftTrump(TrumpCard trump)
    {
        return trump.EffectType == TrumpEffectType.GoFor24 || trump.EffectType == TrumpEffectType.GoFor27;
    }

    private static bool IsCardGainTrump(TrumpCard trump)
    {
        return trump.EffectType == TrumpEffectType.PerfectDraw
            || trump.EffectType == TrumpEffectType.DrawSpecific
            || trump.EffectType == TrumpEffectType.Hush
            || trump.EffectType == TrumpEffectType.Refresh;
    }

    private static bool IsDisruptTrump(TrumpCard trump)
    {
        return trump.EffectType == TrumpEffectType.ForceOpponentPass
            || trump.EffectType == TrumpEffectType.RemoveOpponentLastDraw
            || trump.EffectType == TrumpEffectType.Destroy
            || trump.EffectType == TrumpEffectType.Reincarnation
            || trump.EffectType == TrumpEffectType.SwapHands;
    }

    private void EndRound()
    {
        _roundOver = true;
        RevealHiddenCards(_player);
        RevealHiddenCards(_opponent);

        int playerScore = _player.TotalAll();
        int opponentScore = _opponent.TotalAll();

        string result = ResolveWinner(playerScore, opponentScore);
        OnRoundEnded?.Invoke(result);
        OnStateChanged?.Invoke();
    }

    private void RevealHiddenCards(PlayerState target)
    {
        foreach (Card card in target.Hand)
        {
            card.Reveal();
        }
    }

    private string ResolveWinner(int playerScore, int opponentScore)
    {
        bool playerBust = playerScore > _targetScore;
        bool opponentBust = opponentScore > _targetScore;

        if (playerBust && opponentBust)
        {
            return "Draw: both exceeded target.";
        }

        if (playerBust)
        {
            return "Opponent wins: player exceeded target.";
        }

        if (opponentBust)
        {
            return "Player wins: opponent exceeded target.";
        }

        if (playerScore == opponentScore)
        {
            return "Draw: same score.";
        }

        string winner = playerScore > opponentScore ? "Player" : "Opponent";
        return $"{winner} wins: closer to target.";
    }

    private void BuildTrumpPool()
    {
        _trumpPool.Clear();

        
   
        
        AddTrumpToPool("Destroy", TrumpEffectType.Destroy);
        AddTrumpToPool("Hush", TrumpEffectType.Hush);
        AddTrumpToPool("Perfect Draw", TrumpEffectType.PerfectDraw);
        AddTrumpToPool("Refresh", TrumpEffectType.Refresh);
        AddTrumpToPool("Remove", TrumpEffectType.RemoveOpponentLastDraw);
        AddTrumpToPool("Return", TrumpEffectType.ReturnLastDraw);
        AddTrumpToPool("Exchange", TrumpEffectType.ExchangeLastDraw);
        AddTrumpToPool("Disservice", TrumpEffectType.Disservice);
        AddTrumpToPool("Cuckoo", TrumpEffectType.Cuckoo);
        AddTrumpToPool("Shield +1", TrumpEffectType.ShieldPlusOne);
        AddTrumpToPool("Shield +2", TrumpEffectType.ShieldPlusTwo);
        AddTrumpToPool("Sword +1", TrumpEffectType.SwordPlusOne);
        AddTrumpToPool("Sword +2", TrumpEffectType.SwordPlusTwo);
        AddTrumpToPool("Go for 17", TrumpEffectType.GoFor17);
        AddTrumpToPool("Go for 21", TrumpEffectType.GoFor21);
        AddTrumpToPool("Go for 24", TrumpEffectType.GoFor24);
        AddTrumpToPool("Go for 27", TrumpEffectType.GoFor27);
        AddTrumpToPool("Bless", TrumpEffectType.Bless);
        AddTrumpToPool("Bloodshed", TrumpEffectType.Bloodshed);
        AddTrumpToPool("Friendship", TrumpEffectType.Friendship);
        AddTrumpToPool("Reincarnation", TrumpEffectType.Reincarnation);
        AddTrumpToPool("2-Card", TrumpEffectType.DrawSpecific, 2);
        AddTrumpToPool("3-Card", TrumpEffectType.DrawSpecific, 3);
        AddTrumpToPool("4-Card", TrumpEffectType.DrawSpecific, 4);
        AddTrumpToPool("5-Card", TrumpEffectType.DrawSpecific, 5);
        AddTrumpToPool("6-Card", TrumpEffectType.DrawSpecific, 6);
        AddTrumpToPool("7-Card", TrumpEffectType.DrawSpecific, 7);
    }

    private void AddTrumpToPool(string name, TrumpEffectType effect, int parameter = 0)
    {
        _trumpPool.Add(new TrumpCard(name, effect, parameter));
    }

    private TrumpCard DrawTrumpFromPool()
    {
        if (_trumpPool.Count == 0)
        {
            return null;
        }

        int index = Random.Range(0, _trumpPool.Count);
        TrumpCard trump = _trumpPool[index];
        _trumpPool.RemoveAt(index);
        return trump;
    }

    private void ResolveTrump(TrumpCard trump, PlayerState owner, PlayerState opponent)
    {
        switch (trump.EffectType)
        {
            case TrumpEffectType.DrawSpecific:
            {
                int value = _deck.DrawSpecific(trump.Parameter);
                if (value != -1)
                {
                    owner.AddCard(new Card(value, true));
                }
                break;
            }
            case TrumpEffectType.PeekOpponentHidden:
            {
                foreach (Card card in opponent.Hand)
                {
                    if (!card.IsFaceUp)
                    {
                        card.Reveal();
                        break;
                    }
                }
                break;
            }
            case TrumpEffectType.ForceOpponentPass:
            {
                opponent.HasPassed = true;
                break;
            }
            case TrumpEffectType.SwapHands:
            {
                List<Card> temp = new List<Card>(owner.Hand);
                owner.Hand.Clear();
                owner.Hand.AddRange(opponent.Hand);
                opponent.Hand.Clear();
                opponent.Hand.AddRange(temp);
                break;
            }
            case TrumpEffectType.Destroy:
            {
                RemoveLastPlacedTrump(owner);
                break;
            }
            case TrumpEffectType.Hush:
            {
                int value = _deck.DrawRandom();
                if (value != -1)
                {
                    owner.AddCard(new Card(value, false));
                }
                break;
            }
            case TrumpEffectType.PerfectDraw:
            {
                int bestValue = GetBestValueForTarget(owner.TotalAll());
                if (bestValue != -1)
                {
                    owner.AddCard(new Card(bestValue, true));
                }
                break;
            }
            case TrumpEffectType.Refresh:
            {
                foreach (Card card in owner.Hand.ToList())
                {
                    owner.RemoveCard(card);
                    _deck.ReturnCard(card.Value);
                }

                DrawCard(owner, true);
                DrawCard(owner, true);
                break;
            }
            case TrumpEffectType.RemoveOpponentLastDraw:
            {
                if (_deck.AvailableCount <= 1 || opponent.LastDrawnCard == null)
                {
                    break;
                }

                opponent.RemoveCard(opponent.LastDrawnCard);
                break;
            }
            case TrumpEffectType.ReturnLastDraw:
            {
                if (owner.LastDrawnCard == null)
                {
                    break;
                }

                Card lastCard = owner.LastDrawnCard;
                owner.RemoveCard(lastCard);
                _deck.ReturnCard(lastCard.Value);
                break;
            }
            case TrumpEffectType.ExchangeLastDraw:
            {
                if (owner.LastDrawnCard == null || opponent.LastDrawnCard == null)
                {
                    break;
                }

                Card ownerCard = owner.LastDrawnCard;
                Card opponentCard = opponent.LastDrawnCard;

                owner.RemoveCard(ownerCard);
                opponent.RemoveCard(opponentCard);

                owner.AddCard(opponentCard);
                opponent.AddCard(ownerCard);
                break;
            }
            case TrumpEffectType.Disservice:
            {
                DrawCard(opponent, true);
                break;
            }
            case TrumpEffectType.Cuckoo:
            {
                if (owner.LastDrawnCard == null)
                {
                    break;
                }

                Card lastCard = owner.LastDrawnCard;
                owner.RemoveCard(lastCard);
                opponent.AddCard(lastCard);
                break;
            }
            case TrumpEffectType.ShieldPlusOne:
            {
                AddPlacedTrump(trump, owner, opponent);
                break;
            }
            case TrumpEffectType.ShieldPlusTwo:
            {
                AddPlacedTrump(trump, owner, opponent);
                break;
            }
            case TrumpEffectType.SwordPlusOne:
            {
                AddPlacedTrump(trump, owner, opponent);
                break;
            }
            case TrumpEffectType.SwordPlusTwo:
            {
                AddPlacedTrump(trump, owner, opponent);
                break;
            }
            case TrumpEffectType.GoFor17:
            case TrumpEffectType.GoFor21:
            case TrumpEffectType.GoFor24:
            case TrumpEffectType.GoFor27:
            {
                AddPlacedTrump(trump, owner, opponent);
                break;
            }
            case TrumpEffectType.Bless:
            {
                AddPlacedTrump(trump, owner, opponent);
                break;
            }
            case TrumpEffectType.Bloodshed:
            {
                AddPlacedTrump(trump, owner, opponent);
                TryGiveTrump(owner);
                break;
            }
            case TrumpEffectType.Friendship:
            {
                TryGiveTrump(owner);
                TryGiveTrump(owner);
                TryGiveTrump(opponent);
                TryGiveTrump(opponent);
                break;
            }
            case TrumpEffectType.Reincarnation:
            {
                if (RemoveLastPlacedTrump(owner))
                {
                    TryGiveTrump(owner);
                }
                break;
            }
        }

        OnStateChanged?.Invoke();
    }

    private void TryGiveTrump(PlayerState target)
    {
        TrumpCard trump = DrawTrumpFromPool();
        if (trump != null)
        {
            target.Trumps.Add(trump);
        }
    }

    private int GetBestValueForTarget(int currentTotal)
    {
        int bestValue = -1;
        int bestGap = int.MaxValue;
        for (int value = 1; value <= 11; value++)
        {
            int gap = _targetScore - (currentTotal + value);
            if (gap < 0)
            {
                continue;
            }

            if (_deck.AvailableCount == 0)
            {
                break;
            }

            if (gap < bestGap && TryPeekAvailable(value))
            {
                bestGap = gap;
                bestValue = value;
            }
        }

        if (bestValue != -1)
        {
            _deck.DrawSpecific(bestValue);
        }

        return bestValue;
    }

    private bool TryPeekAvailable(int value)
    {
        return _deck.HasValue(value);
    }

    private bool IsLeading(PlayerState owner, PlayerState opponent)
    {
        int ownerScore = owner.TotalAll();
        int opponentScore = opponent.TotalAll();

        if (ownerScore > _targetScore || opponentScore > _targetScore)
        {
            return ownerScore <= _targetScore && opponentScore > _targetScore;
        }

        return ownerScore > opponentScore;
    }

    private void AddPlacedTrump(TrumpCard trump, PlayerState owner, PlayerState opponent)
    {
        _placedTrumps.Add(new PlacedTrump(trump, owner));
        RecalculatePersistentEffects();
    }

    private bool RemoveLastPlacedTrump(PlayerState requester)
    {
        if (_placedTrumps.Count == 0)
        {
            return false;
        }

        PlacedTrump last = _placedTrumps[_placedTrumps.Count - 1];
        if (last.Owner == requester)
        {
            return false;
        }

        _placedTrumps.RemoveAt(_placedTrumps.Count - 1);
        RecalculatePersistentEffects();
        return true;
    }

    private void RecalculatePersistentEffects()
    {
        _targetScore = baseTargetScore;
        _betDistance = baseBetDistance;
        _player.BetModifier = 0;
        _opponent.BetModifier = 0;
        _player.BlessActive = false;
        _opponent.BlessActive = false;

        foreach (PlacedTrump placed in _placedTrumps)
        {
            ApplyPersistentEffect(placed.Trump, placed.Owner, placed.Owner == _player ? _opponent : _player);
        }
    }

    private void ApplyPersistentEffect(TrumpCard trump, PlayerState owner, PlayerState opponent)
    {
        switch (trump.EffectType)
        {
            case TrumpEffectType.ShieldPlusOne:
                owner.BetModifier -= 1;
                break;
            case TrumpEffectType.ShieldPlusTwo:
                owner.BetModifier -= 2;
                break;
            case TrumpEffectType.SwordPlusOne:
                opponent.BetModifier += 1;
                break;
            case TrumpEffectType.SwordPlusTwo:
                opponent.BetModifier += 2;
                break;
            case TrumpEffectType.GoFor17:
                _targetScore = 17;
                break;
            case TrumpEffectType.GoFor21:
                _targetScore = 21;
                break;
            case TrumpEffectType.GoFor24:
                _targetScore = 24;
                break;
            case TrumpEffectType.GoFor27:
                _targetScore = 27;
                break;
            case TrumpEffectType.Bless:
                owner.BlessActive = true;
                break;
            case TrumpEffectType.Bloodshed:
                _betDistance += 1;
                break;
        }
    }

    private class PlacedTrump
    {
        public TrumpCard Trump { get; }
        public PlayerState Owner { get; }

        public PlacedTrump(TrumpCard trump, PlayerState owner)
        {
            Trump = trump;
            Owner = owner;
        }
    }
}
