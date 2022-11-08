using Godot;
using System;

public class CardDisplay : NinePatchRect
{
    [Signal] public delegate void OnClick(int id, bool selected, bool flipped);
    private BaseCard _cardResource;
    private Label _nameLabel;
    private Label _costLabel;
    private TextureRect _cardTexture;
    private Label _descriptionLabel;
    private bool _selected;
    private bool _flipped;

    public BaseCard Resource { get => _cardResource; }

    public override void _Ready()
    {
        _nameLabel = FindNode("CardName") as Label;
        _costLabel = FindNode("CardCost") as Label;
        _cardTexture = FindNode("CardTexture") as TextureRect;
        _descriptionLabel = FindNode("Description") as Label;

        Connect("OnClick", this, "SelfClick");
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.Pressed && mouseEvent.ButtonIndex == (int)ButtonList.Left)
                EmitSignal("OnClick", _cardResource.Id, _selected, _flipped);
        }
    }

    public void Display(BaseCard resource, bool active = true)
    {
        FlipUp();

        _cardResource = resource;
        _nameLabel.Text = resource.Name;
        _costLabel.Text = $"${resource.Cost}k";
        // _cardTexture
        _descriptionLabel.Text = resource.Description;
    }

    public void FlipUp()
    {
        _flipped = false;
    }

    public void FlipDown()
    {
        _flipped = true;
    }

    public void SelfClick(int id, bool selected, bool flipped)
    {
        if (flipped)
            return;

        _selected = !selected;
    }
}
