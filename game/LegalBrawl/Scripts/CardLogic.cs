using Godot;
using System;

public static class CardLogic
{
    public static CardEffects[] EffectOrder =  {
        CardEffects.Blunder,
        CardEffects.TurnCondition,
        CardEffects.ModifySelf,
        CardEffects.ModifyOther,
        CardEffects.GainEvidence,
        CardEffects.GainWitness,
    };

    public static BoardState Apply(BoardState state, Turn turn)
    {
        // Perform all the card's properties to the state
        bool skipNext = false;
        foreach (CardEffects effect in EffectOrder)
        {
            if (!turn.Card.Effects.ContainsKey((int)effect))
                continue;

            if (!ConditionMet(effect, turn.Card.Effects[(int)effect], state, turn))
            {
                skipNext = true;
                continue;
            }

            if (skipNext)
            {
                skipNext = false;
                continue;
            }

            state = ApplyEffect(effect, turn.Card.Effects[(int)effect], state, turn);
        }

        foreach (int evidence in state[(int)turn.Owner].Evidence)
        {
            ApplyEffect(CardEffects.ModifySelf, evidence, state, turn);
        }

        if (turn.Round == 6)
        {
            foreach (int witness in state[(int)turn.Owner].Witnesses)
            {
                ApplyEffect(CardEffects.ModifyOther, witness, state, turn);
            }
        }

        return state;
    }

    public static BoardState ApplyEffect(CardEffects effect, int value, BoardState state, Turn turn)
    {
        PlayerTypes other = PlayerTypeHelper.GetOther(turn.Owner);
        PlayerState ownerState = state[(int)turn.Owner];
        PlayerState otherState = state[(int)other];

        switch (effect)
        {
            case CardEffects.ModifySelf:
                ownerState.AddCredibility(value); break;
            case CardEffects.ModifyOther:
                otherState.AddCredibility(value); break;
            case CardEffects.GainEvidence:
                ownerState.AddEvidence(value); break;
            case CardEffects.GainWitness:
                ownerState.AddWitness(value); break;
        }

        if (turn.Owner == PlayerTypes.Player)
            return new BoardState(ownerState, otherState);
        return new BoardState(otherState, ownerState);
    }

    public static bool ConditionMet(CardEffects effect, int value, BoardState state, Turn turn)
    {
        switch (effect)
        {
            case CardEffects.TurnCondition:
                return turn.Round == value;
            default: return true;
        }
    }
}