using Server.Message;
using System.Text;
using System.Text.Json;

namespace Server.GlobalUtilities
{
    static class GlobalUtilities
    {
        public static string TryToGetCommandFromJson(string json, string property)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(json);
            JsonElement root = jsonDocument.RootElement;
            root.TryGetProperty(property, out JsonElement nameElement);
            return nameElement.ToString();
        }

        public static Response GetErrorMessage(Exception ex)
        {
            string error = TryToGetCommandFromJson(ex.Message, "details");

            if (error.ToString() != "" || error.ToString() != null)
                return new Response { ErrorMessage = error };

            error = TryToGetCommandFromJson(ex.Message, "message");

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
