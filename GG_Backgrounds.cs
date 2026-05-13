using System.Drawing;

namespace HanaJotchi
{
    public class GG_Backgrounds : GotchaGotchi
    {
        public static void DrawStandardBackground(Graphics g, Rectangle lcdBounds, int width, int height, int size, int petX, int petY)
        {
            // --- PALETTE CONFIGURATION ---
            Color skyColor = Color.FromArgb(184, 216, 248);       // Light Blue
            Color mountainColor = Color.FromArgb(144, 168, 192);  // Muted Slate Blue
            Color grassColor = Color.FromArgb(88, 176, 56);       // Vibrant Grass Green
            Color dirtColor = Color.FromArgb(200, 152, 88);       // Dirt Brown
            Color platformColor = Color.FromArgb(120, 200, 80);   // Lighter Grass Circle

            // Sky & Ground Base Fill
            int horizonY = lcdBounds.Top + (int)(lcdBounds.Height * 0.45);

            using (SolidBrush skyBrush = new SolidBrush(skyColor))
            {
                g.FillRectangle(skyBrush, lcdBounds.X, lcdBounds.Top, lcdBounds.Width, horizonY - lcdBounds.Top);
            }

            using (SolidBrush dirtBrush = new SolidBrush(dirtColor))
            {
                g.FillRectangle(dirtBrush, lcdBounds.X, horizonY, lcdBounds.Width, lcdBounds.Bottom - horizonY);
            }

            //  Distant Mountains (Zigzag horizontal color band)
            using (SolidBrush mtBrush = new SolidBrush(mountainColor))
            {
                Point[] points = {
                  new Point(lcdBounds.Left, horizonY),
                  new Point(lcdBounds.Left + 20, horizonY - 15),
                  new Point(lcdBounds.Left + 45, horizonY - 5),
                  new Point(lcdBounds.Left + 80, horizonY - 22),
                  new Point(lcdBounds.Left + 115, horizonY - 8),
                  new Point(lcdBounds.Right, horizonY - 18),
                  new Point(lcdBounds.Right, horizonY)
                };

                g.FillPolygon(mtBrush, points);
            }

            // Main Foreground Grass Layer
            int grassTopY = horizonY + 12;
            using (SolidBrush grassBrush = new SolidBrush(grassColor))
                g.FillRectangle(grassBrush, lcdBounds.X, grassTopY, lcdBounds.Width, lcdBounds.Bottom - grassTopY);

            // Oval Active Base (Drawn directly under the pet coordinates)
            using (SolidBrush platBrush = new SolidBrush(platformColor))
            {
                using (Pen platPen = new Pen(Color.FromArgb(56, 120, 32), 2)) // Dark green outline
                {
                    // Anchors an oval underneath the pet's floating bounds
                    Rectangle platformBounds = new Rectangle(petX - 10, petY + 40, size + 20, 20);
                    g.FillEllipse(platBrush, platformBounds);
                    g.DrawEllipse(platPen, platformBounds);
                }
            }

        }
    }
}