using System;

public static class SceneManagerData
{
    public static string[] GenerateReferences()
    {
        string[] references = new string[Enum.GetNames(typeof(SceneManager.Scenes)).Length];

        // Add references here:
        references[(int)SceneManager.Scenes.Card] = "res://Scenes/Card/Card.tscn";

        return references;
    }
}