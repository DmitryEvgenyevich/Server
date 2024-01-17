using Server.Message;

namespace Server.Authentication
{
    class Authentication
    {
        private static Dictionary<int, (int, DateTime)> _authenticationList = new Dictionary<int, (int, DateTime)>();
        private const short WAIT_IN_MINUTES = 5;

        public static async Task UpdateOrAddNewUser(int userId,  int code)
        {
            await Task.Run(() =>
            {
                DateTime time = DateTime.Now.AddMinutes(WAIT_IN_MINUTES);

                if (_authenticationList.ContainsKey(userId))
                {
                    _authenticationList[userId] = (code, time);
                }
                else
                {
                    _authenticationList.Add(userId, (code, time));
                    _ = _startAutomaticDeletionTimer(userId, time);
                }
            });
        }

        public static Response IsCodeRight_DeleteFromList(int userId, int code)
        {
            if (_authenticationList.ContainsKey(userId))
            { 
                if (code == _authenticationList[userId].Item1)
                {
                    _authenticationList.Remove(userId);
                    return new Response { };
                }

                return new Response { ErrorMessage = "Wrong code" };
            }

            return new Response { ErrorMessage = "The code is no longer valid" };
        }

        private static async Task _startAutomaticDeletionTimer(int userId, DateTime expirationTime)
        {
            while (true)
            {
                await Task.Delay(1000);

                if (DateTime.Compare(DateTime.Now, expirationTime) >= 0 && _authenticationList.ContainsKey(userId))
                {
                    _authenticationList.Remove(userId);
                    return;
                }
                else if (!_authenticationList.ContainsKey(userId))
                {
                    return;
                }
            }
        }
    }
}
