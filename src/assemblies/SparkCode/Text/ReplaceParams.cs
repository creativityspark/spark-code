using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkCode.Text
{
    public static class ReplaceParams
    {
        public static string Replace(Context ctx, string text, string[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var value = parameters[i];
                if (value != null)
                {
                    text = text.Replace($"{{{{param{i + 1}}}}}", value);
                }
            }
            return text;
        }
    }
}
