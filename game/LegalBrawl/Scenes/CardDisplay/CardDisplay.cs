using Godot;
using System;

public class CardDisplay : NinePatchRect
{
    private BaseCard _cardResource;
    private Label _nameLabel;
    private Label _costLabel;
    private TextureRect _cardTexture;
    private Label _descriptionLabel;

    public override void _Ready()
    {
        _nameLabel = FindNode("CardName") as Label;
        _costLabel = FindNode("CardCost") as Label;
        _cardTexture = FindNode("CardTexture") as TextureRect;
        _descriptionLabel = FindNode("Description") as Label;
    }

    public void Display(BaseCard resource)
    {
        _cardResource = resource;
        _nameLabel.Text = resource.Name;
        _costLabel.Text = $"${resource.Cost}k";
        // _cardTexture
        _descriptionLabel.Text = resource.Description;
    }
}
