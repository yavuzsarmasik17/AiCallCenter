using AiCallCenter.Application.Common.TextProcessing;
using System.Text.RegularExpressions;

namespace AiCallCenter.Infrastructure.TextProcessing;

public sealed class TextChunker : ITextChunker
{
    public IReadOnlyList<string> Split(
        string text,
        int maxChunkLength = 1000,
        int overlapLength = 150)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        if (maxChunkLength < 200)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxChunkLength),
                "Parça uzunluğu en az 200 karakter olmalıdır.");
        }

        if (overlapLength < 0 ||
            overlapLength >= maxChunkLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(overlapLength),
                "Örtüşme uzunluğu geçersiz.");
        }

        var normalizedText = NormalizeText(text);

        var chunks = new List<string>();

        var startIndex = 0;

        while (startIndex < normalizedText.Length)
        {
            var remainingLength =
                normalizedText.Length - startIndex;

            var currentLength = Math.Min(
                maxChunkLength,
                remainingLength);

            var endIndex = startIndex + currentLength;

            if (endIndex < normalizedText.Length)
            {
                endIndex = FindSuitableEndIndex(
                    normalizedText,
                    startIndex,
                    endIndex);
            }

            var chunk = normalizedText
                .Substring(
                    startIndex,
                    endIndex - startIndex)
                .Trim();

            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }

            if (endIndex >= normalizedText.Length)
            {
                break;
            }

            startIndex = Math.Max(
                endIndex - overlapLength,
                startIndex + 1);
        }

        return chunks;
    }

    private static string NormalizeText(string text)
    {
        var normalized = text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Trim();

        normalized = Regex.Replace(
            normalized,
            @"[ \t]+",
            " ");

        normalized = Regex.Replace(
            normalized,
            @"\n{3,}",
            "\n\n");

        return normalized;
    }

    private static int FindSuitableEndIndex(
        string text,
        int startIndex,
        int proposedEndIndex)
    {
        var minimumEndIndex =
            startIndex + ((proposedEndIndex - startIndex) / 2);

        var sentenceEnd = text.LastIndexOfAny(
            new[] { '.', '!', '?', '\n' },
            proposedEndIndex - 1,
            proposedEndIndex - minimumEndIndex);

        if (sentenceEnd >= minimumEndIndex)
        {
            return sentenceEnd + 1;
        }

        var spaceIndex = text.LastIndexOf(
            ' ',
            proposedEndIndex - 1,
            proposedEndIndex - minimumEndIndex);

        return spaceIndex >= minimumEndIndex
            ? spaceIndex
            : proposedEndIndex;
    }
}