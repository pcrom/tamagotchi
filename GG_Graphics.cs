using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HanaJotchi
{
    public class GG_Graphics : GotchaGotchi
    {

        /// <summary>
        /// Draws a labeled horizontal bar representing a statistic at the specified location using the given color and
        /// value.Helper to draw simple bars
        /// </summary>
        /// <remarks>The bar is drawn with a fixed height and a maximum width of 100 pixels. Values
        /// greater than 100 may cause the bar to extend beyond the outline.</remarks>
        /// <param name="g">The graphics surface on which to draw the bar and label.</param>
        /// <param name="label">The text label to display next to the bar.</param>
        /// <param name="value">The length of the bar, in pixels. Typically represents the value of the statistic to visualize.</param>
        /// <param name="x">The x-coordinate, in pixels, of the starting position for the label and bar.</param>
        /// <param name="y">The y-coordinate, in pixels, of the starting position for the label and bar.</param>
        /// <param name="color">The color used to fill the bar.</param>
        public static void DrawStatBar(Graphics g, string label, float value, int x, int y, int ax, int ay, Color color)
        {
            g.DrawString(label, new Font("Arial", 10), GetUIBrush(Color.Black, uiAlpha), x - ax, y - ay);
            g.FillRectangle(GetUIBrush(color, uiAlpha), x + 60, y + 2, value, 10);
            g.DrawRectangle(GetUIPen(Color.Black, uiAlpha), x + 60, y + 2, 100, 10); // Background outline
        }

        /// <summary>
        /// Determines the color to use for a stat bar based on its value. For inverse stats like Hunger, 
        /// the logic is flipped so that lower values are green and higher values are red, 
        /// providing a consistent visual cue for the player regardless of whether a higher or lower value is desirable.
        /// </summary>
        /// <param name="value">0-100</param>
        /// <param name="isInverse">If it's an inverse stat (Hunger), 0 is "Full" and 100 is "Starving"</param>
        /// <returns></returns>
        public static Color GetStatColor(float value, bool isInverse)
        {
            // We flip the logic so the visual remains consistent for the user
            int checkValue = isInverse ? (int)Math.Round(100 - value) : (int)Math.Round(value);
            if (checkValue <= 20) return Color.FromArgb(uiAlpha, Color.Red); // Danger zone
            if (checkValue >= 50) return Color.FromArgb(uiAlpha, Color.LimeGreen); // Healthy zone

            return Color.FromArgb(uiAlpha, Color.Gold); // Caution/Normal
        }



        /// <summary>
        /// Creates a SolidBrush using the current global uiAlpha and a base color.
        /// </summary>
        public static SolidBrush GetUIBrush(Color baseColor, int _uiAlpha)
        {
            // Color.FromArgb(Alpha, Color) handles the conversion automatically
            return new SolidBrush(Color.FromArgb(_uiAlpha, baseColor));
        }

        /// <summary>
        /// Creates a Pen using the current global uiAlpha and a base color.
        /// </summary>
        public static Pen GetUIPen(Color baseColor, int _uiAlpha)
        {
            // Color.FromArgb(Alpha, Color) handles the conversion automatically
            return new Pen(Color.FromArgb(_uiAlpha, baseColor));
        }



        /// <summary>
        /// Helper function to create a pill-shaped GraphicsPath for buttons or other UI elements.
        /// </summary>
        /// <param name="rect">The height of the rectangle determines the diameter of the rounded ends, creating a smooth, capsule-like appearance. </param>
        /// <returns>This method returns a GraphicsPath that forms a pill shape based on the provided rectangle.</returns>
        public static GraphicsPath CreatePillPath(Rectangle rect)
        {
            var path = new GraphicsPath();
            int diameter = rect.Height; // Use the height as the diameter for the rounded ends
            Rectangle leftArc = new Rectangle(rect.X, rect.Y, diameter, diameter);
            Rectangle rightArc = new Rectangle(rect.Right - diameter, rect.Y, diameter, diameter);

            path.AddArc(leftArc, 90, 180);   // Left semi-circle
            path.AddArc(rightArc, 270, 180); // Right semi-circle
            path.CloseFigure();              // Joins them with straight lines

            return path;
        }
    }
}
