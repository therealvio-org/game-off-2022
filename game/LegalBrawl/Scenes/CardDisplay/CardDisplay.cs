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
    private bool _suppressClick;

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
        if (_cardResource == null || _suppressClick)
            return;

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

        if (active) Unselect();
        else Select();
    }

    public void FlipUp()
    {
        _flipped = false;
        _nameLabel.Show();
        _costLabel.Show();
        _cardTexture.Show();
        _descriptionLabel.Show();
    }

    public void FlipDown()
    {
        _flipped = true;
        _nameLabel.Hide();
        _costLabel.Hide();
        _cardTexture.Hide();
        _descriptionLabel.Hide();
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

    public void SelfClick(int id, bool selected, bool flipped)
    {
        if (flipped)
            return;

        if (selected)
            Unselect();
        else
            Select();
    }

    public void SuppressClick()
    {
        _suppressClick = true;
    }
}
