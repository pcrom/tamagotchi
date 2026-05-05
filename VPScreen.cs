using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HanaJotchi
{
    public partial class VPScreen : Form
    {
        private HanaJotchiGotchaGotcha pet;
        private Timer HanaJotchiHeartbeat;

        private const string apiEndpoint = "https://ewaygames.com/GameApi/VirtuPet";


        // Current stats (default values until API loads)

        // Position
        private int petX = 100;
        private int petY = 100;


        // Where the pet wants to go
        private int targetX = 100;

        // Tracks when to blink
        private int blinkTimer = 0;

        private Random rnd = new Random();
        private string petState = "Idle"; // e.g., "Idle", "Eating", "Sleeping"

        // Define button hitboxes (x, y, width, height)
        const int y  = 340;
        private Rectangle feedBtn = new Rectangle(20, y, 80, 40);
        private Rectangle playBtn = new Rectangle(120, y, 80, 40);
        private Rectangle sleepBtn = new Rectangle(220, y, 80, 40);

        public VPScreen()
        {
            InitializeComponent();
        }

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
                targetX = rnd.Next(50, pictureBox1.Width - 50);
            }



            /* Blinking Logic */

            //Increment blink timer each tick
            blinkTimer++;

            
            // Blink every ~3 seconds
            if (blinkTimer > 30)
            { 
                
                pet.IsBlinking = true;

                // Keep eyes shut for 3 frames
                if (blinkTimer > 33)
                {
                    pet.IsBlinking = false;
                    blinkTimer = 0;
                }
            }
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
        private void DrawStatBar(Graphics g, string label, int value, int x, int y, int ax, int ay, Color color)
        {
            g.DrawString(label, new Font("Arial", 10), Brushes.Black, x - ax, y - ay);
            g.FillRectangle(new SolidBrush(color), x + 60, y + 2, value, 10);
            g.DrawRectangle(Pens.Black, x + 60, y + 2, 100, 10); // Background outline
        }

        /// <summary>
        /// Simple render engine gives you total control over the "retro" look.
        /// </summary>
        private void RenderGame()
        {
            Bitmap canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.FromArgb(200, 215, 180)); // Background

                // Simple rectangles representing the 4 hearts/bars often used in Tamagotchi
                DrawStatBar(g, "Hunger",pet.Hunger, 10, 10, 0, 3, Color.Red);
                DrawStatBar(g, "Happiness", pet.Happiness, 10, 35, 10, 3, Color.Gold);


                int stateUpdate = targetX % 100;
                g.DrawString($"{stateUpdate}", new Font("Courier New", 12), Brushes.Black, 194, 269);

                DrawStatBar(g, "Debug Move", stateUpdate, 30, 270, 26, 1, Color.Aqua);

                // Draw Body (centered on petX/petY)
                int size = 60;
                g.FillEllipse(Brushes.Black, petX - (size / 2), petY - (size / 2), size, size);
                g.FillEllipse(Brushes.White, petX - (size / 2) + 2, petY - (size / 2) + 2, size - 4, size - 4);

                // Draw Eyes
                if (pet.IsBlinking)
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

                g.FillRectangle(Brushes.LightGray, feedBtn);
                g.DrawString("FEED", new Font("Courier New", 12), Brushes.Black, feedBtn.X + 10, feedBtn.Y + 10);

                g.FillRectangle(Brushes.LightGray, playBtn);
                g.DrawString("PLAY", new Font("Courier New", 12), Brushes.Black, playBtn.X + 10, playBtn.Y + 10);

                g.FillRectangle(Brushes.LightGray, sleepBtn);
                g.DrawString("SLEEP", new Font("Courier New", 12), Brushes.Black, sleepBtn.X + 10, sleepBtn.Y + 10);


                g.DrawString($"STATUS: {petState}", new Font("Courier New", 12), Brushes.Black, 10, 150);
            }
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = canvas;

        }

        private void VPScreen_Shown(object sender, EventArgs e)
        {
            // Initialize pet data (in a real game, this would come from the API)
            pet = new HanaJotchiGotchaGotcha
            {
                Name = "Fluffy",
                Token = "abc123", //This Token would be a unique identifier from the API, it is the lifeline to your pet's data and must be included in all API calls to update or retrieve stats.
                Age = 2,
                Description = "A happy little pet.",
                Experience = 150,
                Happiness = 97,
                Hunger = 10,
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

        public async Task SyncPetStats()
        {
            string responseJson = await UpdatePetStats();
            // Use a JSON parser to get the corrected hunger from the server
            var serverData = JsonConvert.DeserializeObject<dynamic>(responseJson);
            pet.Hunger = int.Parse(serverData.new_hunger); // Update local hunger with server's response
        }

        public async Task<string> UpdatePetStats()
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string> 
                {
                    { "name", pet.Name },
                    { "token", pet.Token },
                    { "hunger", pet.Hunger.ToString() }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(apiEndpoint + "/petstats.php", content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
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
        }

        private async void PerformAction(string action)
        {
            petState = "Debug State";
            RenderGame();

            // Update stats via API

            switch(action)
            {
                case "feed":
                    pet.Feed();
                    break;
                case "play":
                    pet.Play();
                    break;
                case "sleep":
                    pet.Sleep();
                    break;
            }

            // Reset state after a moment
            await Task.Delay(1000);
            petState = "Idle";
        }
    }
}