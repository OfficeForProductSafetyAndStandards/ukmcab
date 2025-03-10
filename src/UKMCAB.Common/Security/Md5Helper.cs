﻿using System.Security.Cryptography;
using System.Text;

namespace UKMCAB.Common.Security;

public static class Md5Helper
{
    public static string? CalculateMD5(string input)
    {
        if (input != null)
        {
            using var md5Hasher = MD5.Create();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            return BitConverter.ToString(data);
        }
        else
        {
            return null;
        }
    }
}
