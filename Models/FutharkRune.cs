using Aeonpulse.Attributes;

namespace Aeonpulse.Models;

/// <summary>
/// Immutable descriptor for one of the 24 Elder Futhark runes, used by the
/// Web of Wyrd Explorer in the Birth Rune expanded card view.
///
/// <para>
/// <b>Web of Wyrd geometry:</b> the 15-point grid (3 cols x 5 rows, aspect 1:2)
/// is indexed p[0]..p[14] in row-major order:
/// <code>
///   p[0]  p[1]  p[2]    row 0
///   p[3]  p[4]  p[5]    row 1
///   p[6]  p[7]  p[8]    row 2
///   p[9]  p[10] p[11]   row 3
///   p[12] p[13] p[14]   row 4
/// </code>
/// <c>Segments</c> is an array of (A, B) pairs where A and B are point indices.
/// <c>WyrdWebDrawable</c> draws each segment as a straight line between p[A]
/// and p[B], overlaid on the dim skeleton of the 9 Web lines.
/// </para>
/// <para>
/// <b>Localisation:</b> <c>Symbol</c> is the Unicode glyph stored as a C#
/// \uXXXX escape in <c>FutharkCatalogue</c>. <c>Name</c>, <c>Brief</c>, and
/// <c>Full</c> are read from <c>AppResources</c> at construction time by
/// <c>FutharkCatalogue.Build()</c> so the Explorer always reflects the active
/// locale.
/// </para>
/// </summary>
[AIContext("DataTransferObject")]
public sealed class FutharkRune
{
    /// <summary>Unicode Elder Futhark glyph.</summary>
    public string Symbol { get; }

    /// <summary>Localised rune name from <c>AppResources.Rune_Xxx_Name</c>.</summary>
    public string Name { get; }

    /// <summary>Concise localised meaning from <c>AppResources.Rune_Xxx_Brief</c>.</summary>
    public string Brief { get; }

    /// <summary>Full localised interpretation from <c>AppResources.Rune_Xxx_Full</c>.</summary>
    public string Full { get; }

    /// <summary>
    /// Point-pair segments that draw this rune on the Web of Wyrd canvas.
    /// Each element is (A, B) where A and B are 0-based indices into the
    /// 15-point grid (row-major, 3 cols x 5 rows).
    /// </summary>
    public (int A, int B)[] Segments { get; }

    /// <summary>Constructs an immutable rune descriptor.</summary>
    public FutharkRune(string symbol, string name, string brief, string full,
                       (int A, int B)[] segments)
    {
        Symbol   = symbol;
        Name     = name;
        Brief    = brief;
        Full     = full;
        Segments = segments;
    }
}
