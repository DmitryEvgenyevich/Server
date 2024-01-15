using Server.Message;

namespace Server.Authentication
{
    internal class Authentication
    {
        static Dictionary<int, (int, DateTime)> _authenticationList = new Dictionary<int, (int, DateTime)>();

        static int min = 10;

        static public async Task UpdateOrAddNewUser(int userId,  int code)
        {
            await Task.Run(() =>
            {
                DateTime time = DateTime.Now.AddMinutes(min);

                if (_authenticationList.ContainsKey(userId))
                {
                    _authenticationList[userId] = (code, time);
                }
                else
                {
                    _authenticationList.Add(userId, (code, time));
                    _ = _automaticDeletion(userId, time);
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

                return new Response { ErrorMessage = "not" };
            }

            return new Response { ErrorMessage = "You are to late" };
        }

        static async Task _automaticDeletion(int userId, DateTime test)
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    if (DateTime.Compare(DateTime.Now, test) >= 0 && _authenticationList.ContainsKey(userId))
                    {
                      _authenticationList.Remove(userId);
                        return;
                    }
                    else if (!_authenticationList.ContainsKey(userId))
                    {
                        return;
                    }
                }
            });
        }
    }
}
