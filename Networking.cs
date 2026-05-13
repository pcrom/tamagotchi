using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HanaJotchi
{
    public partial class Networking : VPScreen
    {

        /// <summary>
        /// Synchronizes the local pet's statistics with the latest values from the server.
        /// </summary>
        /// <remarks>This method updates the local pet's hunger value to match the server's data. Call
        /// this method to ensure the local state reflects any changes made remotely. If the server is unreachable or
        /// returns invalid data, the local state may not be updated.</remarks>
        /// <returns>A task that represents the asynchronous synchronization operation.</returns>
        public async Task SyncPetStats()
        {
            string responseJson = await UpdatePetStats();
            // Use a JSON parser to get the corrected hunger from the server
            var serverData = JsonConvert.DeserializeObject<dynamic>(responseJson);
            hanaJotchiPet.Hunger = int.Parse(serverData.new_hunger); // Update local hunger with server's response
        }

        /// <summary>
        /// Sends the current pet statistics to the remote API and retrieves the server response as a string.
        /// </summary>
        /// <remarks>This method posts the pet's name, token, and hunger level to the API endpoint using
        /// an HTTP POST request. The returned string contains the raw response from the server, which may require
        /// further parsing or validation depending on the API contract.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response from the server as
        /// a string.</returns>
        public async Task<string> UpdatePetStats()
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "name", hanaJotchiPet.Name },
                    { "token", hanaJotchiPet.Token },
                    { "hunger", hanaJotchiPet.Hunger.ToString() }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(Program.VirtualPet.apiEndpoint + "/petstats.php", content);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
