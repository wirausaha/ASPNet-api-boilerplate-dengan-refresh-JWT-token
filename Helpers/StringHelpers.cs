using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AspApi.Helpers
{
    public static class StringHelpers
    {
        public static string Truncate(this string? input, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            var trimmed = input.Trim();
            return trimmed.Length <= maxLength
                ? trimmed
                : trimmed.Substring(0, maxLength);
        }
    }
}
