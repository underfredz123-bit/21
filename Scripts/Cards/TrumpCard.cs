using System;

public enum TrumpEffectType
{
    DrawSpecific,
    PeekOpponentHidden,
    ForceOpponentPass,
    SwapHands,
    Destroy,
    Hush,
    PerfectDraw,
    Refresh,
    RemoveOpponentLastDraw,
    ReturnLastDraw,
    ExchangeLastDraw,
    Disservice,
    Cuckoo,
    ShieldPlusOne,
    ShieldPlusTwo,
    SwordPlusOne,
    SwordPlusTwo,
    GoFor17,
    GoFor21,
    GoFor24,
    GoFor27,
    Bless,
    Bloodshed,
    Friendship,
    Reincarnation
}

[Serializable]
public class TrumpCard
{
    public string Name;
    public TrumpEffectType EffectType;
    public int Parameter;

    public TrumpCard(string name, TrumpEffectType effectType, int parameter = 0)
    {
        Name = name;
        EffectType = effectType;
        Parameter = parameter;
    }
}
