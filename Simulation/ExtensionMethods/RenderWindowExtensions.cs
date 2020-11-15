using Game.SFML_Text;
using SFML.Graphics;
using SFML.System;

namespace Game.ExtensionMethods
{
    public static class RenderWindowExtensions
    {
        public static void Draw(this RenderWindow window, Sprite texture, float scale, Vector2f position)
        {
            texture.Scale = new Vector2f(scale, scale);
            texture.Position = new Vector2f(position.X, position.Y);
            window.Draw(texture);
        }

        public static void DrawString(this RenderWindow window, FontText fontText, bool centre = true)
        {
            var text = new Text(fontText.StringText, fontText.Font);
            var size = text.GetLocalBounds();
            var scale = fontText.Scale;
            var textWidth = size.Width * scale;
            var textHeight = size.Height * scale;
            text.Scale = new Vector2f(scale, scale);
            text.FillColor = fontText.TextColour;
            text.OutlineColor = fontText.TextColour;
            window.Draw(text);
        }
    }
}
