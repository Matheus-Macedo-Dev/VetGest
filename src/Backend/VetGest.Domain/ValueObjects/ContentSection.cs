namespace VetGest.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a content section within a pregnancy phase.
/// Used to organize fetal development and maternal changes content.
/// </summary>
public readonly struct ContentSection : IEquatable<ContentSection>
{
    /// <summary>
    /// Section title (e.g., "Fetal Development", "Maternal Changes").
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Section content (Markdown-formatted text).
    /// </summary>
    public string Content { get; }

    public ContentSection(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        Title = title;
        Content = content;
    }

    public override string ToString() => $"{Title}: {Content[..Math.Min(50, Content.Length)]}...";

    public bool Equals(ContentSection other)
    {
        return Title == other.Title && Content == other.Content;
    }

    public override bool Equals(object? obj) => obj is ContentSection other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Title, Content);

    public static bool operator ==(ContentSection left, ContentSection right) => left.Equals(right);
    public static bool operator !=(ContentSection left, ContentSection right) => !left.Equals(right);
}
