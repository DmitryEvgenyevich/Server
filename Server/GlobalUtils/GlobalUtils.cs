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
        public static string _tryToGetCommandFromJson(string json, string proparty)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(json);
            JsonElement root = jsonDocument.RootElement;
            root.TryGetProperty(proparty, out JsonElement nameElement);
            return nameElement.ToString();
        }

        static public Response GetErrorMessage(Exception ex)
        {
            string error = _tryToGetCommandFromJson(ex.Message, "details");

            if (error.ToString() != string.Empty)
                return new Response { ErrorMessage = error };

            error = _tryToGetCommandFromJson(ex.Message, "message");

            if (error.ToString() != string.Empty)
                return new Response { ErrorMessage = ex.Message };

            return new Response { ErrorMessage = ex.Message };
        }
    }
}
