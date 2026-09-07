using System.Text;

namespace CampusTrade.Backend.Services;

public class AiKnowledgeService : IAiKnowledgeService
{
    private sealed class AiKnowledgeOptions
    {
        public string Path { get; set; } = "Knowledge";

        public string[] ExtraFiles { get; set; } = [];

        public int ChunkSize { get; set; } = 600;

        public int MaxChunks { get; set; } = 4;

        public int MaxContextChars { get; set; } = 2500;
    }

    private sealed class IndexedChunk(KnowledgeChunk chunk, string lowerText)
    {
        public KnowledgeChunk Chunk { get; } = chunk;

        public string LowerText { get; } = lowerText;
    }

    private readonly ILogger<AiKnowledgeService> _logger;
    private readonly AiKnowledgeOptions _options;
    private readonly IReadOnlyList<IndexedChunk> _chunks;
    private readonly string? _manualText;

    public AiKnowledgeService(IConfiguration configuration, IWebHostEnvironment environment, ILogger<AiKnowledgeService> logger)
    {
        _logger = logger;
        _options = configuration.GetSection("AiKnowledge").Get<AiKnowledgeOptions>() ?? new AiKnowledgeOptions();
        _chunks = Load(environment.ContentRootPath);
        _manualText = LoadManualText(environment.ContentRootPath);
    }

    public string? GetManualText() => _manualText;

    public IReadOnlyList<KnowledgeChunk> Retrieve(string query)
    {
        var terms = ExtractTerms(query);
        if (terms.Count == 0 || _chunks.Count == 0)
        {
            return Array.Empty<KnowledgeChunk>();
        }

        var n = _chunks.Count;
        var df = new Dictionary<string, int>(terms.Count);
        foreach (var term in terms)
        {
            var count = 0;
            foreach (var chunk in _chunks)
            {
                if (chunk.LowerText.Contains(term, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            df[term] = count;
        }

        var scored = new List<(double Score, IndexedChunk Chunk)>();
        foreach (var chunk in _chunks)
        {
            double score = 0;
            foreach (var term in terms)
            {
                var tf = CountOccurrences(chunk.LowerText, term);
                if (tf == 0)
                {
                    continue;
                }

                var idf = Math.Log(1 + (double)n / (1 + df[term]));
                var mult = chunk.Chunk.SectionTitle.Contains(term, StringComparison.OrdinalIgnoreCase) ? 3 : 1;
                score += (1 + 0.5 * Math.Log(tf)) * idf * mult;
            }

            if (score > 0)
            {
                scored.Add((score, chunk));
            }
        }

        var selected = new List<KnowledgeChunk>();
        var totalChars = 0;
        foreach (var (_, chunk) in scored.OrderByDescending(s => s.Score).Take(_options.MaxChunks))
        {
            if (selected.Count > 0 && totalChars + chunk.Chunk.Text.Length > _options.MaxContextChars)
            {
                break;
            }

            selected.Add(chunk.Chunk);
            totalChars += chunk.Chunk.Text.Length;
        }

        return selected;
    }

    private string? LoadManualText(string contentRoot)
    {
        var manualPath = Path.Combine(contentRoot, _options.Path, "使用手册.md");
        if (!File.Exists(manualPath))
        {
            _logger.LogWarning("AI usage manual not found: {Path}", manualPath);
            return null;
        }

        try
        {
            var text = File.ReadAllText(manualPath);
            if (text.Contains('�'))
            {
                _logger.LogError("AI usage manual skipped (invalid encoding): {Path}", manualPath);
                return null;
            }

            return text;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load AI usage manual: {Path}", manualPath);
            return null;
        }
    }

    private IReadOnlyList<IndexedChunk> Load(string contentRoot)
    {
        var files = new List<string>();

        var knowledgeDir = Path.Combine(contentRoot, _options.Path);
        if (Directory.Exists(knowledgeDir))
        {
            files.AddRange(Directory.GetFiles(knowledgeDir, "*.md", SearchOption.AllDirectories));
        }
        else
        {
            _logger.LogWarning("AI knowledge folder not found: {Path}", knowledgeDir);
        }

        foreach (var extra in _options.ExtraFiles)
        {
            var fullPath = Path.GetFullPath(Path.Combine(contentRoot, extra));
            if (File.Exists(fullPath))
            {
                files.Add(fullPath);
            }
            else
            {
                _logger.LogWarning("AI knowledge file not found: {Path}", fullPath);
            }
        }

        var chunks = new List<IndexedChunk>();
        foreach (var file in files)
        {
            try
            {
                var text = File.ReadAllText(file);
                if (text.Contains('�'))
                {
                    _logger.LogError("AI knowledge file skipped (invalid encoding): {Path}", file);
                    continue;
                }

                chunks.AddRange(ChunkFile(file, text));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load AI knowledge file: {Path}", file);
            }
        }

        _logger.LogInformation("AI knowledge loaded: {ChunkCount} chunk(s) from {FileCount} file(s).", chunks.Count, files.Count);
        return chunks;
    }

    private IEnumerable<IndexedChunk> ChunkFile(string filePath, string text)
    {
        var docName = Path.GetFileNameWithoutExtension(filePath);
        var docTitle = docName;
        var h2 = string.Empty;
        var sectionTitle = docName;

        var chunks = new List<IndexedChunk>();
        var buffer = new List<string>();
        var length = 0;

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r').Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith("###", StringComparison.Ordinal))
            {
                Flush();
                var h3 = line[3..].Trim();
                sectionTitle = string.IsNullOrEmpty(h2) ? h3 : $"{h2} / {h3}";
                continue;
            }

            if (line.StartsWith("##", StringComparison.Ordinal))
            {
                Flush();
                h2 = line[2..].Trim();
                sectionTitle = h2;
                continue;
            }

            if (line.StartsWith("#", StringComparison.Ordinal))
            {
                Flush();
                docTitle = line[1..].Trim();
                h2 = string.Empty;
                sectionTitle = docName;
                continue;
            }

            // Hard-split oversized lines so no chunk exceeds ChunkSize.
            var remaining = line;
            while (remaining.Length > _options.ChunkSize)
            {
                Flush();
                buffer.Add(remaining[.._options.ChunkSize]);
                Flush();
                remaining = remaining[_options.ChunkSize..];
            }

            if (buffer.Count > 0 && length + remaining.Length + 1 > _options.ChunkSize)
            {
                Flush();
            }

            buffer.Add(remaining);
            length += remaining.Length + 1;
        }

        Flush();
        return chunks;

        void Flush()
        {
            if (buffer.Count == 0)
            {
                return;
            }

            var chunkText = string.Join("\n", buffer);
            chunks.Add(new IndexedChunk(
                new KnowledgeChunk(docName, docTitle, sectionTitle, chunkText),
                chunkText.ToLowerInvariant()));
            buffer.Clear();
            length = 0;
        }
    }

    private static List<string> ExtractTerms(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<string>();
        }

        var normalized = new StringBuilder(query.Length);
        foreach (var c in query.Trim().ToLowerInvariant())
        {
            if ((c >= '一' && c <= '鿿') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                normalized.Append(c);
            }
        }

        var text = normalized.ToString();
        if (text.Length == 0)
        {
            return new List<string>();
        }

        var terms = new HashSet<string>();

        // Very short queries: match the whole string literally.
        if (text.Length <= 2)
        {
            terms.Add(text);
            return terms.ToList();
        }

        // Character bigrams (covers CJK and digit-CJK boundaries like "6位").
        for (var i = 0; i < text.Length - 1; i++)
        {
            terms.Add(text.Substring(i, 2));
        }

        // ASCII letter/digit runs of length >= 2.
        var current = new StringBuilder();
        foreach (var c in text)
        {
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                current.Append(c);
            }
            else if (current.Length >= 2)
            {
                terms.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Clear();
            }
        }

        if (current.Length >= 2)
        {
            terms.Add(current.ToString());
        }

        return terms.ToList();
    }

    private static int CountOccurrences(string text, string term)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(term, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += term.Length;
        }

        return count;
    }
}
