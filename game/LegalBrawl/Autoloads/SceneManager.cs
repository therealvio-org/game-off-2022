using Godot;
using System;
using System.Collections.Generic;

public class SceneManager : Node
{
    private static SceneManager _instance = null;
    public enum Scenes { Game, Card } // Also add item to SceneManagerData or it won't generate correctly
    private string[] _references;
    private Dictionary<Scenes, PackedScene> _loadedScenes;

    public override void _Ready()
    {
        _instance = this;
        // Scenes are not preloaded
        _loadedScenes = new Dictionary<Scenes, PackedScene>();
        _references = SceneManagerData.GenerateReferences();
    }

    public static T Create<T>(Scenes scene, Node parent) where T : Node
    {
        Node node = Create(scene, parent);
        if (node is T typedNode)
            return typedNode;

        throw new ArgumentException($"Node {node.Name} does not support {nameof(T)} param cast");
    }

    public static Node Create(Scenes scene, Node parent)
    {
        if (_instance == null)
        {
            throw new Exception("Trying to use Scene Manager before it's ready");
        }

        Node node = _instance.GetPackedScene(scene).Instance();
        parent.AddChild(node);
        return node;
    }

    public PackedScene GetPackedScene(Scenes scene)
    {
        if (!_loadedScenes.ContainsKey(scene))
            _loadedScenes.Add(scene, GD.Load<PackedScene>(_references[(int)scene]));
        return _loadedScenes[scene];
    }
}