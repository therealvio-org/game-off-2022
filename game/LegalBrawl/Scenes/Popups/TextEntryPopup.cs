using Godot;
using System;
using System.Text.RegularExpressions;
using ProfanityFilter;

public class TextEntryPopup : PopupPanel
{
    [Signal] public delegate void Finished(string text);
    [Signal] public delegate void InvalidInput(string reason);
    private Label _hint;
    private LineEdit _entry;
    private Button _doneButton;
    private ProfanityFilter.ProfanityFilter _filter;
    public override void _Ready()
    {
        _hint = FindNode("HintLabel") as Label;
        _entry = FindNode("LineEdit") as LineEdit;
        _doneButton = FindNode("DoneButton") as Button;

        _doneButton.Connect("pressed", this, "ValidateEntry");

        Connect("InvalidInput", this, "OnInvalidInput");

        _filter = new ProfanityFilter.ProfanityFilter();
    }

    public void ValidateEntry()
    {
        string text = Regex.Replace(_entry.Text, @"[ ]+", " ");

        if (Regex.IsMatch(text, @"[^A-Za-z ]"))
        {
            EmitSignal("InvalidInput", "Alphabet and spaces only");
            return;
        }

        if (text.Length < 4 && text.Length <= 18)
        {
            EmitSignal("InvalidInput", "Must be 4-18 alphabet characters");
            return;
        }

        string noSpaces = Regex.Replace(text.ToLower(), @"[ ]+", "");

        if (_filter.ContainsProfanity(noSpaces))
        {
            EmitSignal("InvalidInput", "Contains an inappropriate word");
            return;
        }

        EmitSignal("Finished", text);
    }

    public void OnInvalidInput(string reason)
    {
        _hint.Text = reason;
    }

}
