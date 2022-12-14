using Godot;
using System;
using System.Collections.Generic;

public enum CardEffects
{
    ModifySelf = 0,
    ModifyOther = 1,
    TurnCondition = 2,
    GainEvidence = 3,
    GainWitness = 4,
    Blunder = 5,
    WitnessCondition = 6, // 1 = Yours, 2 = Opponents, 4 = GainValue
    EvidenceCondition = 7,
    RemoveWitness = 8, // 1 = Yours, 2 = Opponents
    SkipTurn = 9, // 1 = Yours, 2 = Opponents
    GiveEvidence = 10,
}

public static class CardEffectHelper
{
    public static string GenerateDescription(Dictionary<int, int> effects)
    {
        string description = "";

        if (effects.Count == 0)
            return "This card has no ability";

        foreach (CardEffects effect in CardLogic.EffectOrder)
        {
            if (!effects.ContainsKey((int)effect))
                continue;

            description += GetDescription(effect, effects[(int)effect], effects.Count == 1);
        }

        return description;
    }

    public static string GetDescription(CardEffects effect, int value, bool verbose)
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
            case CardEffects.TurnCondition:
                return $"If this is your {FindNth(value + 1)} card, ";
            case CardEffects.GainEvidence:
                return $"Gain a{EvidenceStrength(value)} evidence. " + (verbose ? $"(Adds {value} credibility each turn.) " : "");
            case CardEffects.GiveEvidence:
                return $"Your opponent gains a{EvidenceStrength(value)} evidence. " + (verbose ? $"(Adds {value} credibility each turn.) " : "");
            case CardEffects.GainWitness:
                return $"Gain a{WitnessStrength(value)}witness. " + (verbose ? $"(Reduces your opponent's credibility by {value} at the end of the case.) " : "");
            case CardEffects.Blunder:
                return $"Blunder. " + (verbose ? $"(Taking this card earns you ${value})." : "");
            case CardEffects.WitnessCondition:
                string text = "If ";
                if (IsOwner(value))
                    text += "you have ";
                if (IsOther(value))
                    text += "your opponent has ";
                text += "a witness, ";
                if ((value & 4) != 0)
                    text += "gain credibility equal to the witness value. ";
                return text;
            case CardEffects.RemoveWitness:
                if (IsOwner(value))
                    return "You lose a witness. ";
                if (IsOther(value))
                    return "Your opponent loses a witness. ";
                return "";
            case CardEffects.SkipTurn:
                if (IsOwner(value))
                    return "You lose your next turn. ";
                if (IsOther(value))
                    return "Your opponent loses their next turn. ";
                return "";
            default: return "";
        }
    }

    public static string FindNth(int n)
    {
        switch (n)
        {
            case 1: return "first";
            case 2: return "second";
            case 3: return "third";
            case 4: return "fourth";
            case 5: return "fifth";
            case 6: return "sixth";
            case 7: return "last";
            default: return "????";
        }
    }

    public static string EvidenceStrength(int value)
    {
        switch (Math.Abs(value))
        {
            case 1: return " weak";
            case 3: return " strong";
            default: return "n";
        }
    }

    public static bool IsOwner(int value)
    {
        return (value & 1) != 0;
    }
    public static bool IsOther(int value)
    {
        return (value & 2) != 0;
    }

    public static string GetWho(int value)
    {
        bool you = IsOwner(value);
        bool opp = IsOther(value);
        if (you && opp)
            return "both players";
        if (you)
            return "you";
        if (opp)
            return "your opponent";
        return "nobody";
    }

    public static string WitnessStrength(int value)
    {
        switch (value)
        {
            case 5: return " character ";
            case 10: return "n expert ";
            case 15: return "n eye-";
            default: return "n ";
        }
    }
}