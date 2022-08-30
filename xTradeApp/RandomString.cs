using System;
using System.Text;

namespace xTrade
{
    public static class RandomString
    {
        public static string NextString(Random r, int size)
        {
            var data = new byte[size];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)r.Next(0, 128);
            }
            
            return Encoding.UTF8.GetString(data, 0, size);
        }
    }
}