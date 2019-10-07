using System;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MangerInstructions.ViewModel
{
    public class HashPassword
    {
        public static String GetHash(String password)
        {
            return password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: new byte[] { 0 },
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 4));
        }
    }
}