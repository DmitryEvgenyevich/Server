using Newtonsoft.Json;
using Server.Message;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Server.GlobalUtilities
{
    static class GlobalUtilities
    {
        public static string TryToGetValueFromJsonByProperty(string json, string property)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(json);
            JsonElement root = jsonDocument.RootElement;
            root.TryGetProperty(property, out var nameElement);
            return nameElement.ToString();
        }

        public static string HashString(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hash;
            }
        }
        public static int CreateRandomNumber(int num1, int num2)
        {
            return new Random().Next(num1, num2);
        }

        public static async Task SendRequest(NetworkStream stream, IMessage message)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new IMessageConverter() }
            };

            string json = JsonConvert.SerializeObject(message);

            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await stream.WriteAsync(bytes, 0, bytes.Length);
            await stream.FlushAsync();
        }

        public static string GetErrorMessage(Exception ex)
        {
            string error = TryToGetValueFromJsonByProperty(ex.Message, "details");

            if (error.ToString() != "" || error.ToString() != null)
                return error ;

            error = TryToGetValueFromJsonByProperty(ex.Message, "message");

            if (error.ToString() != "")
                return ex.Message ;

            return "Error";
        }

        public static string ConvertBytesToString(byte[] buffer, int bytesRead)
        {
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }

        public static bool isStringEmpty(string str)
        {
            return str == string.Empty;
        }
    }
}
