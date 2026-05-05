using System;

namespace HanaJotchi
{
    class HanaJotchiGotchaGotcha
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
        public int Hunger { get; set; }
        public int Happiness { get; set; }
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

        public void Update()
        {
            if (IsAwake)
            {
                Hunger = Math.Min(100, Hunger + 1);
                Happiness = Math.Max(0, Happiness - 1);
            }
        }

        public void Feed()
        {
            Hunger = Math.Max(0, Hunger - 10);
            Happiness = Math.Min(100, Happiness + 5);
        }

        public void Play()
        {
            Happiness = Math.Min(100, Happiness + 10);
            Hunger = Math.Min(100, Hunger + 5);
        }

        public void Sleep()
        {
            IsAwake = false;
            Hunger = Math.Min(100, Hunger + 5);
        }

        public void WakeUp()
        {
            IsAwake = true;
        }

        public void ForceBlink()
        {
            IsBlinking = true;
            // Simulate blinking for a short duration
            System.Threading.Thread.Sleep(500);
            IsBlinking = false;
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

        public HanaJotchiGotchaGotcha()
        {

        }
    }
}
