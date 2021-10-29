using System;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Client.System.Components
{
    public class NihilInputBox
    {
        public Rectangle Rectangle;
        public bool Selected = false;
        public string Text = string.Empty;
        public int CharCount {get { return Text.Length;}}
        private Color _color;
        private Color _textColor;
        private int _fontSize;
        private bool _autoSize = false;

        public NihilInputBox(int x, int y, int width, int height, Color color, Color textColor, int fontSize = 0)
        {
            Rectangle = new Rectangle(x, y, width, height);
            _color = color;
            _textColor = textColor;
            _fontSize = fontSize;
        }

        public void Draw()
        {
            DrawRectangleRec(Rectangle, _color);    

            if (Selected)
                DrawRectangleLines((int)Rectangle.x, (int)Rectangle.y, (int)Rectangle.width, (int)Rectangle.height, Color.YELLOW);  

            if ((_fontSize == 0) || (_autoSize))
            {
                _autoSize = true;
                _fontSize = (int)Math.Floor(Rectangle.width / Text.Length);
            }

            DrawText(Text, (int)Rectangle.x, (int)Rectangle.y, _fontSize, _textColor);   
        }
    }
}