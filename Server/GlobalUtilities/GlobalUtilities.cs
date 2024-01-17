using Server.Message;
using System.Net.Sockets;
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
            root.TryGetProperty(property, out JsonElement nameElement);
            return nameElement.ToString();
        }

        public static int CreateRandomNumber(int num1, int num2)
        {
            Random random = new Random();
            return random.Next(num1, num2);
        }

        public static async Task SendRequest(NetworkStream stream, IMessage message)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new IMessageConverter() }
            };

            string json = JsonSerializer.Serialize(message, options);

            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await stream.WriteAsync(bytes, 0, bytes.Length);
            await stream.FlushAsync();
        }

        public static Response GetErrorMessage(Exception ex)
        {
            string error = TryToGetValueFromJsonByProperty(ex.Message, "details");

            if (error.ToString() != "" || error.ToString() != null)
                return new Response { ErrorMessage = error };

            error = TryToGetValueFromJsonByProperty(ex.Message, "message");

            if (error.ToString() != "")
                return new Response { ErrorMessage = ex.Message };

            return new Response { ErrorMessage = ex.Message };
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
