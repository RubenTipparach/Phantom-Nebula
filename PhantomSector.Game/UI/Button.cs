using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PhantomSector.Game.UI;

/// <summary>
/// Button UI component with mouse hover and click support
/// </summary>
public class Button
{
    private const int BUTTON_BUFFER_MARGIN = 20;

    public string DisplayText { get; set; }
    public Vector2 Position { get; private set; }
    public Rectangle Bounds { get; private set; }
    public bool IsHovered { get; private set; }
    public bool IsEnabled { get; set; } = true;

    public event EventHandler OnClick;

    private SpriteFont _font;
    private Texture2D _whiteTexture;
    private bool _lastHover;

    public Button(string displayText, SpriteFont font, Texture2D whiteTexture)
    {
        DisplayText = displayText;
        _font = font;
        _whiteTexture = whiteTexture;

        Vector2 textSize = _font.MeasureString(displayText);
        Bounds = new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            (int)textSize.X + BUTTON_BUFFER_MARGIN * 2,
            (int)textSize.Y
        );
    }

    public void SetPosition(Vector2 position)
    {
        Position = position;
        Bounds = new Rectangle(
            (int)position.X,
            (int)position.Y,
            Bounds.Width,
            Bounds.Height
        );
    }

    public void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        if (!IsEnabled)
        {
            IsHovered = false;
            return;
        }

        // Check if mouse is over button
        IsHovered = Bounds.Contains(mouseState.X, mouseState.Y);

        // Check for click
        if (IsHovered && mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
        {
            OnClick?.Invoke(this, EventArgs.Empty);
        }

        _lastHover = IsHovered;
    }

    public void Draw(SpriteBatch spriteBatch, float alpha = 1f)
    {
        if (!IsEnabled)
        {
            // Draw disabled state
            spriteBatch.Draw(_whiteTexture, Bounds, Color.Gray * 0.5f * alpha);
            spriteBatch.DrawString(_font, DisplayText, Position + new Vector2(BUTTON_BUFFER_MARGIN, 0), Color.DarkGray * alpha);
        }
        else
        {
            // Draw normal or hovered state
            Color backgroundColor = IsHovered ? Color.Goldenrod : Color.Chocolate;
            spriteBatch.Draw(_whiteTexture, Bounds, backgroundColor * alpha);
            spriteBatch.DrawString(_font, DisplayText, Position + new Vector2(BUTTON_BUFFER_MARGIN, 0), Color.White * alpha);
        }
    }
}
