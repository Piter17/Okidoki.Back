namespace Okidoki.App.Extensions;

public static class StringExtensions
{
    extension(string value)
    {
        public string CapitalizeFirstLetter(bool restSmall = false) => value switch
        {
            [] => value,
            [var first] => char.ToUpperInvariant(first).ToString(),
            [var first, .. var rest] => char.ToUpperInvariant(first)
                + (restSmall ? rest.ToLowerInvariant() : rest),
        };

        public string SkipLastS() => value switch
        {
            [.. var rest, 's' or 'S'] => rest,
            [] => value,
            _ => value
        };
    }

}