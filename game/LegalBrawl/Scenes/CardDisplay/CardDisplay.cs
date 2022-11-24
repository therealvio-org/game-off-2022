using Godot;
using System;

public class CardDisplay : NinePatchRect
{
    [Signal] public delegate void Reveal();
    [Signal] public delegate void Click(CardDisplay card);
    private BaseCard _cardResource;
    private Label _nameLabel;
    private Label _costLabel;
    private TextureRect _cardTexture;
    private Label _descriptionLabel;
    private Control _frontFace;
    private Control _backFace;
    private AnimationPlayer _animator;
    private bool _selected;
    private bool _flipped;
    private bool _suppressClick;
    private BaseCard _queueResource;
    private bool _queueActive;

    public BaseCard Resource { get => _cardResource; }
    public int Id { get => _cardResource.Id; }
    public bool IsSelected { get => _selected; }
    public bool IsFlipped { get => _flipped; }

    public override void _Ready()
    {
        _nameLabel = FindNode("CardName") as Label;
        _costLabel = FindNode("CardCost") as Label;
        _cardTexture = FindNode("CardTexture") as TextureRect;
        _descriptionLabel = FindNode("Description") as Label;
        _frontFace = FindNode("FrontFace") as Control;
        _backFace = FindNode("BackFace") as Control;
        _animator = FindNode("Animator") as AnimationPlayer;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (_cardResource == null || _suppressClick)
            return;

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.Pressed && mouseEvent.ButtonIndex == (int)ButtonList.Left)
                EmitSignal("Click", this);
        }
    }

    public void Display(BaseCard resource, bool active = true)
    {
        _cardResource = resource;
        _nameLabel.Text = resource.Name;
        _costLabel.Text = $"${resource.Cost}k";
        _cardTexture.Texture = resource.Art;
        _descriptionLabel.Text = CardEffectHelper.GenerateDescription(resource.Effects);

        if (active) Unselect();
        else Select();
    }

    public void QueueDisplay(BaseCard resource, bool active = true)
    {
        _queueResource = resource;
        _queueActive = active;
    }

    public void FlipUp()
    {
        AudioManager.Play("Flip");

        if (!_flipped)
            _animator.Play("FlipDown");

        _flipped = false;
        _animator.Queue("FlipUp");
    }

    public void FlipDown()
    {
        _flipped = true;
        _animator.Play("FlipDown");
    }

    public void ShowFace()
    {
        _flipped = false;
        _frontFace.Show();
        _backFace.Hide();

        if (_queueResource != null)
        {
            Display(_queueResource, _queueActive);
            _queueResource = null;
        }
    }

    public void WhenRevealed()
    {
        EmitSignal("Reveal");
    }

    public void ShowBack()
    {
        _flipped = true;
        _frontFace.Hide();
        _backFace.Show();
    }

    public void Select()
    {
        Modulate = new Color(1, 1, 1, 0.5f);
        _selected = true;
    }

    public void Unselect()
    {
        Modulate = new Color(1, 1, 1, 1);
        _selected = false;
    }

    public void SuppressClick()
    {
        _suppressClick = true;
    }

    public void OffsetRotation()
    {
        RectRotation += Randy.Range(-4f, 4f);
    }

    public void ResetRotation()
    {
        RectRotation = 0;
    }

    public void OnHoverStart()
    {
        if (_flipped || _selected)
            return;

        OffsetRotation();
        AudioManager.Play("Hover");
    }

    public void OnHoverEnd()
    {
        ResetRotation();
    }
}
