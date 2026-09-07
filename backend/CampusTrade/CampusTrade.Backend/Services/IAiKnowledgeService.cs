namespace CampusTrade.Backend.Services;

public record KnowledgeChunk(string DocName, string DocTitle, string SectionTitle, string Text);

public interface IAiKnowledgeService
{
    /// <summary>
    /// Returns the most relevant knowledge chunks for the query, ordered by score.
    /// Returns an empty list when the knowledge base is empty or the query has no matchable terms.
    /// </summary>
    IReadOnlyList<KnowledgeChunk> Retrieve(string query);

    /// <summary>
    /// Returns the raw markdown of the usage manual (使用手册.md), or null when it is not available.
    /// </summary>
    string? GetManualText();
}
