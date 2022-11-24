using Godot;
using System;

public static class CardLogic
{
    public static BoardState Apply(BaseCard card, BoardState state, PlayerTypes owner)
    {
        // Perform all the card's properties to the state
        foreach (var kvp in card.Effects)
        {
            state = ApplyEffect((CardEffects)kvp.Key, kvp.Value, state, owner);
        }

        return state;
    }

    public static BoardState ApplyEffect(CardEffects effect, int value, BoardState state, PlayerTypes owner)
    {
        PlayerTypes other = PlayerTypeHelper.GetOther(owner);
        PlayerState ownerState = state[(int)owner];
        PlayerState otherState = state[(int)other];

        switch (effect)
        {
            case CardEffects.ModifySelf:
                ownerState.Credibility += value; break;
            case CardEffects.ModifyOther:
                otherState.Credibility += value; break;
        }

        if (owner == PlayerTypes.Player)
            return new BoardState(ownerState, otherState);
        return new BoardState(otherState, ownerState);
    }
}