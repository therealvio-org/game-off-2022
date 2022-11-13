using Godot;
using System;

public class Card : Control
{
    private Control _fixedAnchor;
    private Control _cardAnchor;
    private CanvasLayer _layer;
    private CardDisplay _cardBody;
    private Control _cardHover;
    private bool _isHeld;
    public bool IsHeld { get => _isHeld; }
    private bool _isHover;
    public bool IsHovered { get => _isHover; }
    private bool _isMoving;
    public bool IsMoving { get => _isMoving; }
    private bool _isHighlight { get => !List.IsHeld && _isHover && List.LastHovered == this; }
    private bool _isGrabbable;
    private AudioStreamPlayer _audioPlayer;
    public CardList List;
    public CardDisplay Display { get => _cardBody; }

    public override void _Ready()
    {
        _layer = GetNode<CanvasLayer>("Layer");
        _layer.Layer = Layers.CARD;

        _fixedAnchor = _layer.GetNode<Control>("FixedAnchor");
        _cardAnchor = _layer.GetNode<Control>("CardAnchor");
        _cardBody = _cardAnchor.GetNode<CardDisplay>("CardDisplay");
        _cardBody.SuppressClick();

        _isHeld = false;
        _isHover = false;
        _isMoving = false;

        _cardHover = _cardBody.GetNode<Control>("HoverBox");
        _cardHover.Connect("gui_input", this, "OnGuiInput");

        //_audioPlayer = GetNode<AudioStreamPlayer>("AudioPlayer");
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouse mouseEvent)
        {
            if (_cardHover.GetGlobalRect().HasPoint(mouseEvent.GlobalPosition))
            {
                if (_isGrabbable && !_isHeld)
                {
                    _isHover = true;
                }
            }
            else if (_isHover)
                _isHover = false;
        }
        if (inputEvent is InputEventMouseButton mouseClickEvent && mouseClickEvent.ButtonIndex == (int)ButtonList.Left)
        {
            if (_isHeld)
                HoldCard(false);
        }
    }

    public void OnGuiInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouse mouseEvent)
        {
            if (_cardHover.GetRect().HasPoint(mouseEvent.Position))
            {

            }
        }
        if (inputEvent is InputEventMouseButton mouseClickEvent && mouseClickEvent.ButtonIndex == (int)ButtonList.Left)
        {
            HoldCard(mouseClickEvent.Pressed);
        }
    }

    private void HoldCard(bool pressed)
    {
        if (!_isGrabbable)
            return;

        _isHeld = pressed;
        _cardHover.MouseFilter = _isHeld ? MouseFilterEnum.Ignore : MouseFilterEnum.Stop;
    }

    public override void _Process(float delta)
    {
        if (_isHeld)
            MoveCardToMouse(delta);
        else
            MoveCardToAnchor(delta);

        ScaleTowardsDesired(_isHeld || _isHighlight, delta);
        UpdateLayer(_isHeld || _isHighlight);

        if (_isHeld)
        {
            List.LastHeld = this;
            _isHover = false;
        }

        if (_isHover && !List.IsHovered)
        {
            List.LastHovered = this;
        }
    }

    public void SetCardPosition(CardPosition cardPosition)
    {
        _cardAnchor.RectGlobalPosition = cardPosition.Position;
        _cardAnchor.RectRotation = cardPosition.Rotation;
    }

    public void SetFixedPosition(CardPosition cardPosition)
    {
        _fixedAnchor.RectGlobalPosition = cardPosition.Position;
        _fixedAnchor.RectRotation = cardPosition.Rotation;
    }

    public Vector2 GetAnchorOffset()
    {
        if (_isHighlight)
            return (Vector2.Up * 200).Rotated(Mathf.Deg2Rad(_cardBody.RectRotation));
        return Vector2.Zero;
    }

    public void MoveCardToAnchor(float delta)
    {
        MoveTowardsDesired(_fixedAnchor.RectPosition + GetAnchorOffset(), delta * 5);
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
        _isMoving = _cardAnchor.RectPosition.DistanceSquaredTo(desiredPosition) > 20;
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

    public void MakeGrabbable()
    {
        _isGrabbable = true;
    }
}
