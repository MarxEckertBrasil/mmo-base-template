using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Client.System.Components
{
    public class NihilButton
    {
        public Rectangle Rectangle;
        public string Text = string.Empty;
        public int State = 0;
        private Color _color;
        private Color _selectedColor;
        private Color _pressedColor;
        private Color _textColor;
        private int _fontSize;


        public NihilButton(int x, int y, int width, int height, Color color, Color colorSelected, Color colorPressed, Color textColor, int fontSize)
        {
            Rectangle = new Rectangle(x, y, width, height);
            _color = color;
            _selectedColor = colorSelected;
            _pressedColor = colorPressed;
            _textColor = textColor;
            _fontSize = fontSize;
        }

        public void Draw()
        {
            if (State == 0)
                DrawRectangleRec(Rectangle, _color);
            else if (State == 1)
                DrawRectangleRec(Rectangle, _selectedColor);
            else
                DrawRectangleRec(Rectangle, _pressedColor);    

            DrawText(Text, (int)(Rectangle.x + Rectangle.width/4), (int)(Rectangle.y + Rectangle.height/4), _fontSize, _textColor);        
        }

    }
}