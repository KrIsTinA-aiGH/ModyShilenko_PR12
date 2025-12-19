using System;

namespace Shilenko_wpf1.Services
{
    public static class SimpleCaptcha
    {
        private static Random rnd = new Random();

        public static string Create()
        {
            string result = "";
            for (int i = 0; i < 6; i++)
            {
                result += "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[rnd.Next(36)];
            }
            return result;
        }
    }
}