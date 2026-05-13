using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HanaJotchi
{
    public partial class VPScreen : Form
    {
        public GotchaGotchiPet hanaJotchiPet;
        private Timer HanaJotchiHeartbeat;

        public readonly string apiEndpoint = "https://ewaygames.com/GameApi/VirtuPet";

        private bool isDragging = false;
        private Point lastCursorPos;
        private Point lastFormPos;
        
        // 0 (invisible) to 255 (solid)
        private int uiAlpha = 100; 


        // Current stats (default values until API loads)

        // Position
        private int petX = 150;
        private int petY = 200;


        // Where the pet wants to go
        private int targetX = 210;

        // Tracks when to blink
        private int blinkTimer = 0;

        private Random rnd = new Random();
        private string petState = "Idle"; // e.g., "Idle", "Eating", "Sleeping"

        // Define button hitboxes (x, y, width, height)
        const int y  = 40;
        private Rectangle feedBtn = new Rectangle(160, y + 20, 80, 40);
        private Rectangle playBtn = new Rectangle(360, y + 10, 80, 40);
        private Rectangle sleepBtn = new Rectangle(260, y - 20, 80, 40);
        private Rectangle exitBtn;

        /// <summary>
        /// Initializes a new instance of the VPScreen class, optionally enabling private server mode.
        /// </summary>
        /// <remarks>This constructor sets the window to remain on top of other windows. If private server
        /// mode is enabled, the UsePrivateServer property is set accordingly.</remarks>
        /// <param name="enablePrivateServer">true to enable private server mode; otherwise, false. The default is false.</param>
        public VPScreen(bool enablePrivateServer = false)
        {
            InitializeComponent();
            
            // Keep the window on top
            this.TopMost = true;

            // Check for private server configuration
            if (enablePrivateServer)
            {
                // Load lines from the config file, we expect the private server URL to be on line 6 (index 5)
                string[] array = File.ReadAllLines("HanaJotchiPrivate.cfg");

                // We loop through each line in the configuration file to find the server URL. This allows for flexible configuration and the ability to add additional settings in the future without changing the code, as we can simply look for specific keys in the config file.
                foreach (string line in array)
                {
                    // If the line starts with "//", we treat it as a comment and ignore it, otherwise we use the provided URL as the API endpoint.
                    if (!line.StartsWith("//") && line.Contains("="))
                    {
                        // We split the line on the '=' character and take the second part as the API endpoint URL, allowing for flexible configuration without hardcoding the server address.
                        switch (line.Split('=')[0])
                        {
                            case "server": // If the line starts with "server=", we take the value after the '=' as the API endpoint URL and set it to the apiEndpoint variable, allowing the application to connect to a private server instead of the default official server.
                                apiEndpoint = line.Split('=')[1];
                                break;
                            default:
                                MessageBox.Show($"Found missing config line: \n{line}");
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the timer tick event to update the virtual pet's state and refresh the game display.
        /// </summary>
        /// <remarks>This method is intended to be used as an event handler for a timer that periodically
        /// updates the virtual pet's logic and redraws the game interface. It should be attached to the timer's Tick
        /// event to ensure regular updates.</remarks>
        /// <param name="sender">The source of the event, typically the timer that triggered the tick.</param>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        private void HanaJotchiHeartbeat_Tick(object sender, EventArgs e)
        {
            // Handle hunger/happiness over time
            UpdatePetLogic();

            // Draw the new frame
            RenderGame();
        }

        /// <summary>
        /// Update the pet's logic, such as hunger and happiness levels, based on time and interactions.
        /// </summary>
        private void UpdatePetLogic()
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
        private void DrawStatBar(Graphics g, string label, float value, int x, int y, int ax, int ay, Color color)
        {
            g.DrawString(label, new Font("Arial", 10), GetUIBrush(Color.Black), x - ax, y - ay);
            g.FillRectangle(GetUIBrush(color), x + 60, y + 2, value, 10);
            g.DrawRectangle(GetUIPen(Color.Black), x + 60, y + 2, 100, 10); // Background outline
        }

        /// <summary>
        /// Determines the color to use for a stat bar based on its value. For inverse stats like Hunger, 
        /// the logic is flipped so that lower values are green and higher values are red, 
        /// providing a consistent visual cue for the player regardless of whether a higher or lower value is desirable.
        /// </summary>
        /// <param name="value">0-100</param>
        /// <param name="isInverse">If it's an inverse stat (Hunger), 0 is "Full" and 100 is "Starving"</param>
        /// <returns></returns>
        private Color GetStatColor(float value, bool isInverse)
        {
            // We flip the logic so the visual remains consistent for the user
            int checkValue = isInverse ? (int)Math.Round(100 - value) : (int)Math.Round(value);
            if (checkValue <= 20) return Color.FromArgb(uiAlpha, Color.Red); // Danger zone
            if (checkValue >= 50) return Color.FromArgb(uiAlpha, Color.LimeGreen); // Healthy zone

            return Color.FromArgb(uiAlpha, Color.Gold); // Caution/Normal
        }

        /// <summary>
        /// Simple render engine gives you total control over the "retro" look.
        /// </summary>
        private void RenderGame()
        {
            // Create a new bitmap to draw on, matching the size of the PictureBox
            Bitmap canvas = new Bitmap(CanvasBox.Width, CanvasBox.Height);

            // Use a Graphics object to draw on the bitmap
            using (Graphics g = Graphics.FromImage(canvas))
            {
                // Transparent Background
                g.Clear(Color.FromArgb(255,0,1,0));


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


                //Draw Stats Bars
                DrawStatBar(g, "Hunger", hanaJotchiPet.Hunger, 340, 360, 0, 3, GetStatColor(hanaJotchiPet.Hunger, true));
                DrawStatBar(g, "Happiness", hanaJotchiPet.Happiness, 340, 375, 10, 3, GetStatColor(hanaJotchiPet.Happiness, false));

                // Debug: Show targetX as a number and bar for testing movement logic
                int stateUpdate = targetX % 100;
                g.DrawString($"{stateUpdate}", new Font("Courier New", 12), Brushes.Black, 270, 409);

                // Shows a pre-dertimed move from random logic, this is to help visualize the movement code and ensure the pet moves toward the targetX as expected.
                DrawStatBar(g, "Debug Move", stateUpdate, 99, 410, 26, 1, Color.Aqua);

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
                using (SolidBrush expBrush = GetUIBrush(Color.BlueViolet))
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
        /// Creates a SolidBrush using the current global uiAlpha and a base color.
        /// </summary>
        private SolidBrush GetUIBrush(Color baseColor)
        {
            // Color.FromArgb(Alpha, Color) handles the conversion automatically
            return new SolidBrush(Color.FromArgb(uiAlpha, baseColor));
        }

        /// <summary>
        /// Creates a Pen using the current global uiAlpha and a base color.
        /// </summary>
        private Pen GetUIPen(Color baseColor)
        {
            // Color.FromArgb(Alpha, Color) handles the conversion automatically
            return new Pen(Color.FromArgb(uiAlpha, baseColor));
        }



        /// <summary>
        /// Helper function to create a pill-shaped GraphicsPath for buttons or other UI elements.
        /// </summary>
        /// <param name="rect">The height of the rectangle determines the diameter of the rounded ends, creating a smooth, capsule-like appearance. </param>
        /// <returns>This method returns a GraphicsPath that forms a pill shape based on the provided rectangle.</returns>
        private GraphicsPath CreatePillPath(Rectangle rect)
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

        /// <summary>
        /// Handles the Shown event of the VPScreen form, initializing UI elements, loading configuration settings, and
        /// starting the main game loop timer.
        /// </summary>
        /// <remarks>This method sets up the initial state of the VPScreen, including positioning UI
        /// controls, reading private server configuration if enabled, and initializing the pet and game loop timer. It
        /// is intended to be called automatically when the form is first displayed.</remarks>
        /// <param name="sender">The source of the event, typically the VPScreen form instance.</param>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        private void VPScreen_Shown(object sender, EventArgs e)
        {
            // Position the exit button in the top-right corner of the PictureBox
            exitBtn = new Rectangle(CanvasBox.Width - 40, 0, 40, 40);


            // Initialize pet data (in a real game, this would come from the API)
            hanaJotchiPet = new GotchaGotchiPet
            {
                Name = "Fluffy",
                Token = "abc123", //This Token would be a unique identifier from the API, it is the lifeline to your pet's data and must be included in all API calls to update or retrieve stats.
                Age = 2,
                Description = "A happy little pet.",
                Experience = 79,
                Happiness = 67,
                Hunger = 60,
                IsAwake = true,
                Species = "Cat",
            };

            // Create the timer that will drive the game loop
            HanaJotchiHeartbeat = new Timer
            {
                // Set this timer to 100ms or 200ms for a "retro" frame rate
                Interval = 100,
                Enabled = true
            };

            // Start the game loop
            HanaJotchiHeartbeat.Tick += HanaJotchiHeartbeat_Tick;

        }

        /// <summary>
        /// Handles the MouseClick event for the PictureBox, triggering actions when interactive buttons within the
        /// PictureBox are clicked, determining if any interactive buttons were clicked and performing the corresponding actions.
        /// </summary>
        /// <remarks>This method determines which interactive button, if any, was clicked within the
        /// PictureBox and performs the corresponding action. Ensure that the button regions are properly defined to
        /// enable correct interaction.</remarks>
        /// <param name="sender">The source of the event, typically the PictureBox control.</param>
        /// <param name="e">A MouseEventArgs that contains the event data, including the location of the mouse click.</param>
        private void CanvasBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (feedBtn.Contains(e.Location))
            {
                PerformAction("feed");
            }
            else if (playBtn.Contains(e.Location))
            {
                PerformAction("play");
            }
            else if (sleepBtn.Contains(e.Location))
            {
                PerformAction("sleep");
            }
            else if (exitBtn.Contains(e.Location))
            {
                PerformAction("exit");
            }
        }

        /// <summary>
        /// Performs the specified action on the virtual pet, such as feeding, playing, or putting it to sleep.
        /// </summary>
        /// <remarks>If the action is "exit", the application will close. For other supported actions, the
        /// pet's state is updated and the corresponding behavior is triggered. After performing the action, the pet's
        /// state resets to idle after a short delay.</remarks>
        /// <param name="action">The action to perform. Supported values are "feed", "play", "sleep", and "exit". The value is
        /// case-sensitive.</param>
        private async void PerformAction(string action)
        {
            petState = "[Debug State] " + action;
            RenderGame();

            // Update stats via API

            switch(action)
            {
                case "feed":
                    hanaJotchiPet.Feed();
                    break;
                case "play":
                    hanaJotchiPet.Play();
                    break;
                case "sleep":
                    hanaJotchiPet.Sleep();
                    break;
                case "exit":
                    Application.Exit();
                    break;
            }

            // Reset state after a moment
            await Task.Delay(1000);
            petState = "Idle";
        }
  
        /// <summary>
        /// Handles the MouseDown event for the PictureBox to initiate a drag operation when the left mouse button is
        /// pressed.
        /// </summary>
        /// <remarks>Dragging is initiated only when the left mouse button is pressed. This event is
        /// commonly used to enable moving or interacting with the PictureBox via mouse input.</remarks>
        /// <param name="sender">The source of the event, typically the PictureBox control.</param>
        /// <param name="e">A MouseEventArgs that contains the event data, including information about which mouse button was pressed.</param>
        private void CanvasBox_MouseDown(object sender, MouseEventArgs e)
        {
            // Drag as we are pressing on any image render of the object,
            // the side effect of a picturebox will drag everything except transparent
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastCursorPos = Cursor.Position;
                lastFormPos = Location;
            }
        }

        /// <summary>
        /// Handles the MouseMove event for the PictureBox control to update the form's position while dragging.
        /// </summary>
        /// <remarks>This method enables the user to move the form by clicking and dragging the PictureBox
        /// control when dragging is active. Dragging behavior depends on the state of the isDragging flag and the
        /// tracking of previous cursor and form positions.</remarks>
        /// <param name="sender">The source of the event, typically the PictureBox control.</param>
        /// <param name="e">A MouseEventArgs that contains the event data, including the current mouse position.</param>
        private void CanvasBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Calculate how much the mouse has moved since the last frame
                int diffX = Cursor.Position.X - lastCursorPos.X;
                int diffY = Cursor.Position.Y - lastCursorPos.Y;

                // Update the form's position
                Location = new Point(lastFormPos.X + diffX, lastFormPos.Y + diffY);
            }
        }

        /// <summary>
        /// Handles the MouseUp event for the picture box to end a drag operation.
        /// </summary>
        /// <param name="sender">The source of the event, typically the picture box control.</param>
        /// <param name="e">A MouseEventArgs that contains the event data for the mouse button release.</param>
        private void CanvasBox_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }
    }
}