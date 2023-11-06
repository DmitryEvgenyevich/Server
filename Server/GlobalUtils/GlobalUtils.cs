using Server.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server.GlobalUtils
{
    static class GlobalUtils
    {
        public static string TryToGetCommandFromJson(string json, string proparty)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(json);
            JsonElement root = jsonDocument.RootElement;
            root.TryGetProperty(proparty, out JsonElement nameElement);
            return nameElement.ToString();
        }

        public static Response GetErrorMessage(Exception ex)
        {
            string error = TryToGetCommandFromJson(ex.Message, "details");

            if (error.ToString() != string.Empty)
                return new Response { ErrorMessage = error };

            error = TryToGetCommandFromJson(ex.Message, "message");

            if (error.ToString() != string.Empty)
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
