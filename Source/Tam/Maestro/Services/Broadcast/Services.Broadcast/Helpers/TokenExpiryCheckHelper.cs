using System;

namespace Services.Broadcast.Helpers
{
    public static class TokenExpiryCheckHelper
    {
        /// <summary>
        /// check weather token expired or not
        /// </summary>
        /// <param name="expirationDate">Token expiration date time</param>
        /// <param name="currentDate">current date time</param>
        /// <returns></returns>
        public static bool HasTokenExpired(DateTime? expirationDate, DateTime currentDate)
        {
            if (expirationDate <= currentDate)
            {
                return true;
            }
            return false;
        }
    }
}
