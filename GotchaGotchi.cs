using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HanaJotchi
{
    [DesignerCategory("Disabled")]
    public partial class GotchaGotchi : Form
    {
        public GotchaGotchiPet hanaJotchiPet;

        public string apiEndpoint = "https://ewaygames.com/GameApi/VirtuPet";

        public bool isDragging = false;
        public Point lastCursorPos;
        public Point lastFormPos;

        // 0 (invisible) to 255 (solid)
        public static int uiAlpha = 100;


        // Current stats (default values until API loads)

        // Position
        public int petX = 150;
        public int petY = 200;


        // Where the pet wants to go
        public int targetX = 210;

        // Tracks when to blink
        public int blinkTimer = 0;

        public Random rnd = new Random();
        public string petState = "Idle"; // e.g., "Idle", "Eating", "Sleeping"

        // Define button hitboxes (x, y, width, height)
        public const int y = 40;
        public Rectangle feedBtn = new Rectangle(160, y + 20, 80, 40);
        public Rectangle playBtn = new Rectangle(360, y + 10, 80, 40);
        public Rectangle sleepBtn = new Rectangle(260, y - 20, 80, 40);
        public Rectangle exitBtn;


        /// <summary>
        /// Simple render engine gives you total control over the "retro" look.
        /// </summary>
        public void RenderGame(PictureBox CanvasBox)
        {
            // Create a new bitmap to draw on, matching the size of the PictureBox
            Bitmap canvas = new Bitmap(CanvasBox.Width, CanvasBox.Height);

            // Use a Graphics object to draw on the bitmap
            using (Graphics g = Graphics.FromImage(canvas))
            {
                // Transparent Background
                g.Clear(Color.FromArgb(255, 0, 1, 0));


                // Enable anti-aliasing for smoother shapes
                g.SmoothingMode = SmoothingMode.AntiAlias;


                // Define the bounding area for the egg
                Rectangle rect = new Rectangle(10, 10, CanvasBox.Width - 40, CanvasBox.Height - 40);


                // Create the egg shape using Bezier curves
                using (GraphicsPath path = new GraphicsPath())
                {
                    // Start at the top center then end at the bottom of the rectangle
                    Point top = new Point(rect.X + rect.Width / 2, rect.Y);
                    Point bottom = new Point(rect.X + rect.Width / 2, rect.Bottom);

                    // RIGHT SIDE (Wide at top, Narrow at bottom)
                    path.AddBezier(
                        top,
                        new Point(rect.Right + (rect.Width / 3), rect.Top),      // Pushes the TOP out (wide A)
                        new Point(rect.Right - (rect.Width / 6), rect.Bottom),   // Pulls the BOTTOM in (narrow B)
                        bottom);

                    // LEFT SIDE (Wide at top, Narrow at bottom)
                    path.AddBezier(
                        bottom,
                        new Point(rect.Left + (rect.Width / 6), rect.Bottom),    // Pulls the BOTTOM in (narrow A)
                        new Point(rect.Left - (rect.Width / 3), rect.Top),       // Pushes the TOP out (wide B)
                        top);

                    // Fill the egg with a light color and draw a border
                    g.FillPath(new SolidBrush(Color.FromArgb(200, 215, 180)), path);

                    // Draw a thicker border
                    g.DrawPath(new Pen(Color.Black, 3), path);
                }

                // Define the 'Screen' area inside the egg
                // We'll place it in the center, slightly above the buttons
                int screenWidth = (int)(rect.Width * 0.7);
                int screenHeight = (int)(rect.Height * 0.4);
                int screenX = rect.X + (rect.Width - screenWidth) / 2;
                int screenY = rect.Y + (rect.Height / 6); // Positioned in the upper-middle

                // Create a rectangle for the screen area
                Rectangle screenRect = new Rectangle(screenX, screenY, screenWidth, screenHeight);

                // Draw the LCD background (classic greenish-grey)
                Color lcdColor = Color.FromArgb(255, 170, 185, 150);
                g.FillRectangle(new SolidBrush(lcdColor), screenRect);

                // Draw a double border for depth
                g.DrawRectangle(new Pen(Color.DimGray, 2), screenRect); // Inner frame
                g.DrawRectangle(new Pen(Color.Black, 1), Rectangle.Inflate(screenRect, 2, 2)); // Outer plastic edge

                // Use a slightly darker shade of your LCD color for high-contrast visibility
                using (Pen gridPen = GG_Graphics.GetUIPen(Color.FromArgb(0, 0, 0, 0), uiAlpha/16))
                {
                    int gridSize = 41; // Distance between grid lines in pixels

                    // Draw Vertical Lines
                    for (int x = screenRect.Left + gridSize; x < screenRect.Right; x += gridSize)
                    {
                        g.DrawLine(gridPen, x, screenRect.Top, x, screenRect.Bottom);
                    }

                    // Draw Horizontal Lines
                    for (int y = screenRect.Top + gridSize; y < screenRect.Bottom; y += gridSize)
                    {
                        g.DrawLine(gridPen, screenRect.Left, y, screenRect.Right, y);
                    }
                }

                //Draw Stats Bars
                GG_Graphics.DrawStatBar(g, "Hunger", hanaJotchiPet.Hunger, 340, 360, 0, 3, GG_Graphics.GetStatColor(hanaJotchiPet.Hunger, true));
                GG_Graphics.DrawStatBar(g, "Happiness", hanaJotchiPet.Happiness, 340, 375, 10, 3, GG_Graphics.GetStatColor(hanaJotchiPet.Happiness, false));

                // Debug: Show targetX as a number and bar for testing movement logic
                int stateUpdate = targetX % 100;
                g.DrawString($"{stateUpdate}", new Font("Courier New", 12), Brushes.Black, 270, 409);

                // Shows a pre-dertimed move from random logic, this is to help visualize the movement code and ensure the pet moves toward the targetX as expected.
                GG_Graphics.DrawStatBar(g, "Debug Move", stateUpdate, 99, 410, 26, 1, Color.Aqua);

                // Draw Body (centered on petX/petY)
                int size = 60;
                g.FillEllipse(Brushes.Black, petX - (size / 2), petY - (size / 2), size, size);
                g.FillEllipse(Brushes.White, petX - (size / 2) + 2, petY - (size / 2) + 2, size - 4, size - 4);

                // Draw Eyes
                if (hanaJotchiPet.IsBlinking)
                {
                    // Draw horizontal lines for closed eyes
                    g.DrawLine(new Pen(Color.Black, 2), petX - 15, petY - 5, petX - 5, petY - 5);
                    g.DrawLine(new Pen(Color.Black, 2), petX + 5, petY - 5, petX + 15, petY - 5);
                }
                else
                {
                    // Draw normal circles
                    g.FillEllipse(Brushes.Black, petX - 15, petY - 10, 6, 6);
                    g.FillEllipse(Brushes.Black, petX + 9, petY - 10, 6, 6);
                }


                // Draw Buttons
                g.FillRectangle(Brushes.LightGray, feedBtn);
                g.DrawString("FEED", new Font("Courier New", 12), Brushes.Black, feedBtn.X + 10, feedBtn.Y + 10);

                g.FillRectangle(Brushes.LightGray, playBtn);
                g.DrawString("PLAY", new Font("Courier New", 12), Brushes.Black, playBtn.X + 10, playBtn.Y + 10);

                g.FillRectangle(Brushes.LightGray, sleepBtn);
                g.DrawString("SLEEP", new Font("Courier New", 12), Brushes.Black, sleepBtn.X + 10, sleepBtn.Y + 10);

                // Draw Exit Button
                g.FillRectangle(Brushes.DarkRed, exitBtn);
                g.DrawString("X", new Font("Courier New", 20, FontStyle.Bold), Brushes.White, exitBtn.X + 6, exitBtn.Y + 5);


                // Draw Debug State
                g.DrawString($"STATUS: {petState}", new Font("Courier New", 12), Brushes.Black, 111, 470);


                // Draw Experience as a pie chart
                using (SolidBrush expBrush = GG_Graphics.GetUIBrush(Color.BlueViolet, uiAlpha))
                {
                    float sweepAngle = (hanaJotchiPet.Experience % 100) * 3.6f;
                    g.FillPie(expBrush, 99, 340, 50, 50, 0, sweepAngle);
                }

                // Convert screen coordinates to be relative to the PictureBox
                Point clientCursor = CanvasBox.PointToClient(Cursor.Position);

                if (screenRect.Contains(clientCursor))
                {
                    // Fade in to solid when hovering the screen
                    uiAlpha = Math.Min(255, uiAlpha + 35);
                }
                else
                {
                    // Fade out to translucent when the mouse leaves the screen
                    uiAlpha = Math.Max(60, uiAlpha - 20);
                }
            }

            // Dispose of the old image to free memory, then set the new canvas
            CanvasBox.Image?.Dispose();

            // Set the rendered canvas as the PictureBox image
            CanvasBox.Image = canvas;

        }

        /// <summary>
        /// Update the pet's logic, such as hunger and happiness levels, based on time and interactions.
        /// </summary>
        public void UpdatePetLogic(PictureBox CanvasBox)
        {
            //Move slowly toward targetX
            if (petX < targetX)
            {
                petX += 2;
            }

            if (petX > targetX)
            {
                petX -= 2;
            }

            //Occasionally pick a new random spot
            if (rnd.Next(0, 50) == 1)
            {
                targetX = rnd.Next(150, CanvasBox.Width - 150);
            }



            /* Blinking Logic */

            //Increment blink timer each tick
            blinkTimer++;


            // Blink every ~3 seconds
            if (blinkTimer > 30)
            {
                hanaJotchiPet.IsBlinking = true;

                // Keep eyes shut for 3 frames
                if (blinkTimer > 33)
                {
                    hanaJotchiPet.IsBlinking = false;
                    blinkTimer = 0;
                }
            }

            hanaJotchiPet.Update();
        }
    }
}
