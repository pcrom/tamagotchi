using System;

namespace HanaJotchi
{
    public class GotchaGotchiPet
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public string Description { get; set; }
        public int Age { get; set; }
        public string Species { get; set; }
        public bool IsAwake { get; set; }

        // Are eyes closed?
        public bool IsBlinking { get; set; }
        public long Experience { get; set; }
        public float Hunger { get; set; }
        public float Happiness { get; set; }
        public int Hair { get; set; }
        public int Hat { get; set; }
        public int Hold { get; set; }
        public int Face { get; set; }
        public int Horn { get; set; }
        public int Tail { get; set; }
        public int Ears { get; set; }
        public int Eyes { get; set; }
        public int Nose { get; set; }
        public int Mouth { get; set; }


        public float DefaultDecayRate { get { return 0.001f; }}

        public void Update()
        {
            if (!IsAwake)
            {
                return;
            }

            if (Hunger > 70)
            {
                Happiness -= DefaultDecayRate * 16;
            }
            else if (Hunger > 55)
            {
                Happiness -= DefaultDecayRate * 8;
            }
            else if (Hunger > 45)
            {
                Happiness -= DefaultDecayRate * 4;
            }
            else if (Hunger > 35)
            {
                Happiness -= DefaultDecayRate * 2;
            }
            else if (Hunger > 25)
            {
                Happiness -= DefaultDecayRate * 1;
            }

            /* Run Each Update Cycle */

            Hunger = Math.Min(100, Hunger + DefaultDecayRate);

            //Console.WriteLine($"Updated pet status: Hunger={Hunger}, Happiness={Happiness}");
        }

        public void Feed()
        {
            WakeUp();
            Hunger = Math.Max(0, Hunger - 10);
            Happiness = Math.Min(100, Happiness + 5);
            Experience += 1;
        }

        public void Play()
        {
            WakeUp();
            Happiness = Math.Min(100, Happiness + 10);
            Hunger = Math.Min(100, Hunger + 5);
            Experience += 10;
        }

        public void Sleep()
        {
            IsAwake = false;
            Hunger = Math.Min(100, Hunger + 5);
            Experience += 1;
        }

        public void WakeUp()
        {
            IsAwake = true;
        }

        public void GainExperience(long amount)
        {
            Experience += amount;
        }

        public void AgeUp()
        {
            Age++;
        }

        public void Evolve()
        {
            if (Experience >= 1000)
            {
                Species = "Evolved " + Species;
                Experience = 0; // Reset experience after evolution
            }
        }

        public void DisplayStatus()
        {
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Species: {Species}");
            Console.WriteLine($"Age: {Age}");
            Console.WriteLine($"Hunger: {Hunger}");
            Console.WriteLine($"Happiness: {Happiness}");
            Console.WriteLine($"Experience: {Experience}");
            Console.WriteLine($"Is Awake: {IsAwake}");
            Console.WriteLine($"Is Blinking: {IsBlinking}");
        }

        public void Reset()
        {
            Age = 0;
            Hunger = 50;
            Happiness = 50;
            Experience = 0;
            IsAwake = true;
            IsBlinking = false;
        }

        public void Rename(string newName)
        {
            Name = newName;
        }

        public void ChangeDescription(string newDescription)
        {
            Description = newDescription;
        }

        public void ChangeToken(string newToken)
        {
            throw new NotSupportedException("Token change is not allowed for security reasons.");
        }

        public void Delete()
        {
            // Simulate deletion by resetting all properties
            Name = null;
            Token = null;
            Description = null;
            Age = 0;
            Species = null;
            IsAwake = false;
            IsBlinking = false;
            Experience = 0;
            Hunger = 0;
            Happiness = 0;
        }

        public void Greeting()
        {
            Console.WriteLine("TODO: Invoke greeting, properly display on screen!");
        }

        public GotchaGotchiPet()
        {

        }
    }
}
