using Godot;
using System;
using System.Collections.Generic;

public enum CardEffects
{
    ModifySelf = 0,
    ModifyOther = 1
}

public static class CardEffectHelper
{
    public static string GenerateDescription(Dictionary<int, int> effects)
    {
        string description = "";
        foreach (var kvp in effects)
        {
            description += GetDescription((CardEffects)kvp.Key, kvp.Value);
        }
        return description;
    }

    public static string GetDescription(CardEffects effect, int value)
    {
        switch (effect)
        {
            case CardEffects.ModifySelf:
                if (value >= 0)
                    return $"Gain {value} credibility. ";
                else
                    return $"Lose {-value} credibility. ";
            case CardEffects.ModifyOther:
                if (value >= 0)
                    return $"Opponent gains {value} credibility. ";
                else
                    return $"Opponent loses {-value} credibility. ";
            default: return "";
        }
    }
}