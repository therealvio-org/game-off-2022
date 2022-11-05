using Godot;
using System;

public class Card : Control
{
    private Control _fixedAnchor;
    private Control _cardAnchor;
    private CanvasLayer _layer;
    private Control _cardBody;
    private bool _isHeld;
    private bool _isHover;
    private AudioStreamPlayer _audioPlayer;

    public override void _Ready()
    {
        _layer = GetNode<CanvasLayer>("Layer");
        _layer.Layer = Layers.CARD;

        _fixedAnchor = _layer.GetNode<Control>("FixedAnchor");
        _cardAnchor = _layer.GetNode<Control>("CardAnchor");
        _cardBody = _cardAnchor.GetNode<Control>("Card");
        _isHeld = false;
        _isHover = false;

        _cardBody.GetNode<Control>("HoverBox").Connect("mouse_entered", this, "OnMouseEnter");
        _cardBody.GetNode<Control>("HoverBox").Connect("mouse_exited", this, "OnMouseExit");
        _cardBody.GetNode<Control>("HoverBox").Connect("gui_input", this, "OnGuiInput");

        //_audioPlayer = GetNode<AudioStreamPlayer>("AudioPlayer");
    }

    public void OnMouseEnter()
    {
        GD.Print("Entered", Name);
        _isHover = true;
        //_audioPlayer.Play();
    }

    public void OnMouseExit()
    {
        GD.Print("Exited", Name);
        _isHover = false;
    }

    public void OnGuiInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == (int)ButtonList.Left)
        {
            _isHeld = mouseEvent.Pressed;
        }
    }

    public override void _Process(float delta)
    {
        if (_isHeld)
            MoveCardToMouse(delta);
        else
            MoveCardToAnchor(delta);

        ScaleTowardsDesired(_isHover || _isHeld, delta);
        UpdateLayer(_isHover || _isHeld);
    }

    public void SetFixedPosition(CardPosition cardPosition)
    {
        _fixedAnchor.RectGlobalPosition = cardPosition.Position;
        _fixedAnchor.RectRotation = cardPosition.Rotation;
    }

    public Vector2 GetAnchorOffset()
    {

        return _fixedAnchor.RectPosition + (Vector2.Up * 200).Rotated(Mathf.Deg2Rad(_cardBody.RectRotation)); ;
    }

    public void MoveCardToAnchor(float delta)
    {
        MoveTowardsDesired(_isHover ? GetAnchorOffset() : _fixedAnchor.RectPosition, delta * 5);
        RotateTowardsDesired(_fixedAnchor.RectRotation, delta);
    }

    public void MoveCardToMouse(float delta)
    {
        MoveTowardsDesired(GetGlobalMousePosition(), 0.5f);
        RotateTowardsDesired(0, delta);
    }

    public void MoveTowardsDesired(Vector2 desiredPosition, float weight)
    {
        _cardAnchor.RectPosition = _cardAnchor.RectPosition.LinearInterpolate(desiredPosition, weight);
    }

    public void RotateTowardsDesired(float desiredRotation, float delta)
    {
        _cardBody.RectRotation = Mathf.Lerp(_cardBody.RectRotation, desiredRotation, delta * 3);
    }

    public void ScaleTowardsDesired(bool bigger, float delta)
    {
        _cardBody.RectScale = _cardBody.RectScale.LinearInterpolate(bigger ? Vector2.One * 1.2f : Vector2.One, delta * 3);
    }

    public void UpdateLayer(bool above)
    {
        _layer.Layer = above ? Layers.CARD_ABOVE : Layers.CARD;
    }
}
