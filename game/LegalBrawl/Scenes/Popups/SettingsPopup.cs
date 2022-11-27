using Godot;
using System;

public class SettingsPopup : PopupPanel
{
    private Slider _musicSlider;
    private Slider _sfxSlider;
    private Button _deleteButton;
    private Button _doneButton;
    private bool _confirmDelete = true;
    public override void _Ready()
    {
        _musicSlider = FindNode("MusicSlider") as Slider;
        _sfxSlider = FindNode("SfxSlider") as Slider;
        _deleteButton = FindNode("DeleteButton") as Button;
        _doneButton = FindNode("DoneButton") as Button;

        _musicSlider.Connect("value_changed", this, "OnSliderChange", new Godot.Collections.Array() { "Music" });
        _sfxSlider.Connect("value_changed", this, "OnSliderChange", new Godot.Collections.Array() { "SFX" });
        _deleteButton.Connect("pressed", this, "OnDeleteClicked");
        _doneButton.Connect("pressed", this, "OnDoneClicked");

        Connect("about_to_show", this, "OnShow");
    }

    public void OnShow()
    {
        _musicSlider.Value = GameStats.Settings.MusicLevel;
        _sfxSlider.Value = GameStats.Settings.SFXLevel;
        _confirmDelete = true;
        _deleteButton.Text = "Reset player data";
        _deleteButton.Disabled = !GameStats.HasPlayerName();
    }

    public void OnSliderChange(float value, string channel)
    {
        int level = (int)value;
        AudioManager.SetVolumeLevel(channel, level);
        switch (channel)
        {
            case "Music": GameStats.Settings.SetMusicLevel(level); break;
            case "SFX": GameStats.Settings.SetSFXLevel(level); break;
        }
    }

    public void OnDeleteClicked()
    {
        if (_confirmDelete)
        {
            _deleteButton.Text = "Are you sure?";
            _confirmDelete = false;
            return;
        }

        _confirmDelete = true;
        GameStats.Player.Reset();
        Owner.EmitSignal("DataDeleted");
        _deleteButton.Text = "Reset player data";
        _deleteButton.Disabled = true;
    }

    public void OnDoneClicked()
    {
        Hide();
    }
}
