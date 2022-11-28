using Godot;
using System;

public class GameSettings : Resource
{
    public const string LOCATION = "user://settings.tres";
    [Export] public int MusicLevel = 4;
    [Export] public int SFXLevel = 4;
    public void Init()
    {
        AudioManager.SetVolumeLevel("Music", MusicLevel);
        AudioManager.SetVolumeLevel("SFX", SFXLevel);
    }

    public void SetMusicLevel(int value)
    {
        MusicLevel = value;
        ResourceSaver.Save(LOCATION, this);
    }

    public void SetSFXLevel(int value)
    {
        SFXLevel = value;
        ResourceSaver.Save(LOCATION, this);
    }

}