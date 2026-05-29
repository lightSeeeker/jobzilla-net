using System.Text.RegularExpressions;

namespace jobzilla_net.Application.Common.Utils;

public static class AtsScoringUtility
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "a", "an", "in", "to", "for", "with", "on", "of", "by", "at",
        "is", "are", "was", "were", "be", "been", "being", "have", "has", "had", "do",
        "does", "did", "will", "would", "shall", "should", "may", "might", "must", "can",
        "could", "from", "as", "about", "into", "through", "during", "before", "after",
        "above", "below", "over", "under", "between", "among", "this", "that", "these",
        "those", "then", "than", "or", "but", "if", "because", "while", "until", "how",
        "what", "when", "where", "why", "who", "whom", "which", "there", "here", "all",
        "any", "both", "each", "few", "more", "most", "other", "some", "such", "no", "nor",
        "not", "only", "own", "same", "so", "than", "too", "very", "s", "t", "can", "will",
        "just", "don", "should", "now"
    };

    public static int CalculateScore(string? jobText, string? resumeText)
    {
        if (string.IsNullOrWhiteSpace(jobText) || string.IsNullOrWhiteSpace(resumeText))
        {
            return 0;
        }

        var jobWords = ExtractKeywords(jobText);
        var resumeWords = ExtractKeywords(resumeText);

        if (jobWords.Count == 0)
        {
            return 0;
        }

        int matchCount = 0;
        foreach (var jobWord in jobWords)
        {
            if (resumeWords.Contains(jobWord))
            {
                matchCount++;
            }
        }

        double score = (double)matchCount / jobWords.Count * 100.0;
        return (int)Math.Round(score);
    }

    private static HashSet<string> ExtractKeywords(string text)
    {
        var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var matches = Regex.Matches(text, @"\b[a-zA-Z]{3,}\b");

        foreach (Match match in matches)
        {
            var word = match.Value;
            if (!StopWords.Contains(word))
            {
                words.Add(word);
            }
        }

        return words;
    }
}
