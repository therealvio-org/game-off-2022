using Godot;
using System;

public class Turn
{
    public int Id { get; private set; }
    public BaseCard Card { get => CardLibrary.Get(Id); }
    public PlayerTypes Owner { get; private set; }

    public Turn(int id, PlayerTypes owner)
    {
        Id = id;
        Owner = owner;
    }
}