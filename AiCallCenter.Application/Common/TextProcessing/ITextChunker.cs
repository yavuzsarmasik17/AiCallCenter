namespace AiCallCenter.Application.Common.TextProcessing;

public interface ITextChunker
{
    IReadOnlyList<string> Split(
        string text,
        int maxChunkLength = 1000,
        int overlapLength = 150);
}