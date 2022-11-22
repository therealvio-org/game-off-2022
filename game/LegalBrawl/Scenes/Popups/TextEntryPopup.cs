using Godot;
using System;
using System.Text.RegularExpressions;

public class TextEntryPopup : PopupPanel
{
    [Signal] public delegate void Finished(string text);
    [Signal] public delegate void InvalidInput(string reason);
    private Label _hint;
    private LineEdit _entry;
    private Button _doneButton;
    public override void _Ready()
    {
        _hint = FindNode("HintLabel") as Label;
        _entry = FindNode("LineEdit") as LineEdit;
        _doneButton = FindNode("DoneButton") as Button;

        _doneButton.Connect("pressed", this, "ValidateEntry");

        Connect("InvalidInput", this, "OnInvalidInput");
    }

    public void ValidateEntry()
    {
        string text = Regex.Replace(_entry.Text, @"[ ]+", " ");

        if (Regex.IsMatch(text, @"[^A-Za-z ]"))
        {
            EmitSignal("InvalidInput", "Alphabet and spaces only");
            return;
        }

        if (text.Length < 4)
        {
            EmitSignal("InvalidInput", "Must have 4+ alphabet characters");
            return;
        }

        // if (RudeWords.IsRude(text))
        // {
        //     EmitSignal("InvalidInput", "Contains an inappropriate word");
        //     return;
        // }

        EmitSignal("Finished", text);
    }

    public void OnInvalidInput(string reason)
    {
        _hint.Text = reason;
    }

}
