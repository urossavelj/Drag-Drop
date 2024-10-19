namespace _UI.Helpers
{
    public static class Parsers
    {
        public static Dictionary<string, string> ParseStringToDictionary(string input)
        {
            var result = new Dictionary<string, string>();
            var pairs = input.Split(';');

            foreach (var pair in pairs)
            {
                if (string.IsNullOrWhiteSpace(pair)) continue;

                var keyValue = pair.Split(new[] { ',' }, 2);
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();
                    result[key] = value;
                }
            }

            return result;
        }
    }
}
