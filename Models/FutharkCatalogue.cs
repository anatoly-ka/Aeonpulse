using Aeonpulse.Attributes;
using Aeonpulse.Resources;

namespace Aeonpulse.Models;

/// <summary>
/// Builds and caches the complete 24-rune Elder Futhark catalogue used by the
/// Web of Wyrd Explorer. Each call to <see cref="Build"/> creates a fresh list
/// so all strings reflect the currently active locale.
///
/// <para>
/// <b>Grid reference (row-major, 3 cols x 5 rows, aspect 1:2 W:H):</b>
/// <code>
///   p[0]  p[1]  p[2]    row 0
///   p[3]  p[4]  p[5]    row 1
///   p[6]  p[7]  p[8]    row 2
///   p[9]  p[10] p[11]   row 3
///   p[12] p[13] p[14]   row 4
/// </code>
/// Each rune's <c>Segments</c> array lists (A,B) point-index pairs that
/// together trace the rune glyph shape on the grid.
/// </para>
/// <para>
/// <b>Glyph encoding:</b> all symbol strings use explicit C# Unicode escapes
/// (\uXXXX) so the source file stays ASCII-clean.
/// </para>
/// <para>
/// <b>Localisation:</b> all <see cref="AppResources"/> lookups happen inside
/// <see cref="Build"/> so the current culture is sampled on every call.
/// </para>
/// </summary>
[AIContext("DataTransferObject")]
public static class FutharkCatalogue
{
    // U+16A0=Fehu   U+16A2=Uruz   U+16A6=Thurisaz U+16A8=Ansuz  U+16B1=Raidho
    // U+16B2=Kenaz  U+16B7=Gebo   U+16B9=Wunjo    U+16BB=Hagalaz U+16BE=Nauthiz
    // U+16C1=Isa    U+16C3=Jera   U+16C7=Eihwaz   U+16C8=Perthro U+16C9=Algiz
    // U+16CB=Sowilo U+16CF=Tiwaz  U+16D2=Berkano  U+16D6=Ehwaz  U+16D7=Mannaz
    // U+16DA=Laguz  U+16DD=Ingwaz U+16DF=Othala   U+16DE=Dagaz
    private static readonly (string Symbol, (int A, int B)[] Segs)[] _static =
    {
        // Fehu: p1-p13, p4-p2, p7-p5
        ("\u16A0", new[]{ (1,13), (4,2), (7,5) }),

        // Uruz: p0-p12, p0-p8, p8-p14
        ("\u16A2", new[]{ (0,12), (0,8), (8,14) }),

        // Thurisaz: p1-p13, p4-p8, p10-p8
        ("\u16A6", new[]{ (1,13), (4,8), (10,8) }),

        // Ansuz: p1-p13, p1-p5, p4-p8
        ("\u16A8", new[]{ (1,13), (1,5), (4,8) }),

        // Raidho: p1-p13, p1-p5, p7-p5, p7-p11
        ("\u16B1", new[]{ (1,13), (1,5), (7,5), (7,11) }),

        // Kenaz: p7-p5, p7-p11
        ("\u16B2", new[]{ (7,5), (7,11) }),

        // Gebo: p3-p11, p9-p5
        ("\u16B7", new[]{ (3,11), (9,5) }),

        // Wunjo: p1-p13, p1-p5, p7-p5
        ("\u16B9", new[]{ (1,13), (1,5), (7,5) }),

        // Hagalaz: p0-p12, p2-p14, p3-p11
        ("\u16BB", new[]{ (0,12), (2,14), (3,11) }),

        // Nauthiz: p1-p13, p3-p11
        ("\u16BE", new[]{ (1,13), (3,11) }),

        // Isa: p1-p13
        ("\u16C1", new[]{ (1,13) }),

        // Jera: p1-p3, p3-p7, p4-p8, p8-p10  [glyph U+16C3]
        ("\u16C3", new[]{ (1,3), (3,7), (4,8), (8,10) }),

        // Eihwaz: p1-p13, p1-p5, p9-p13  [glyph U+16C7]
        ("\u16C7", new[]{ (1,13), (1,5), (9,13) }),

        // Perthro: p0-p12, p0-p4, p4-p2, p12-p10, p10-p14
        ("\u16C8", new[]{ (0,12), (0,4), (4,2), (12,10), (10,14) }),

        // Algiz: p1-p13, p0-p4, p4-p2
        ("\u16C9", new[]{ (1,13), (0,4), (4,2) }),

        // Sowilo: p0-p9, p9-p5, p5-p14  [glyph U+16CB]
        ("\u16CB", new[]{ (0,9), (9,5), (5,14) }),

        // Tiwaz: p3-p1, p1-p5, p1-p13
        ("\u16CF", new[]{ (3,1), (1,5), (1,13) }),

        // Berkano: p1-p13, p1-p5, p5-p7, p7-p11, p11-p13
        ("\u16D2", new[]{ (1,13), (1,5), (5,7), (7,11), (11,13) }),

        // Ehwaz: p0-p12, p0-p4, p4-p2, p2-p14
        ("\u16D6", new[]{ (0,12), (0,4), (4,2), (2,14) }),

        // Mannaz: p0-p12, p0-p8, p6-p2, p2-p14
        ("\u16D7", new[]{ (0,12), (0,8), (6,2), (2,14) }),

        // Laguz: p1-p13, p1-p5
        ("\u16DA", new[]{ (1,13), (1,5) }),

        // Ingwaz: p6-p2, p0-p8, p8-p12, p14-p6
        ("\u16DD", new[]{ (6,2), (0,8), (8,12), (14,6) }),

        // Othala: p9-p5, p5-p1, p1-p3, p3-p11
        ("\u16DF", new[]{ (9,5), (5,1), (1,3), (3,11) }),

        // Dagaz: p3-p9, p9-p5, p5-p11, p11-p3
        ("\u16DE", new[]{ (3,9), (9,5), (5,11), (11,3) }),
    };

    /// <summary>
    /// Builds a fresh list of all 24 runes with localised strings read from
    /// <see cref="AppResources"/> at call time. Call after any locale change
    /// so the Explorer descriptions refresh immediately.
    /// </summary>
    public static IReadOnlyList<FutharkRune> Build()
    {
        var localised = new (string Name, string Brief, string Full)[]
        {
            (AppResources.Rune_Fehu_Name,     AppResources.Rune_Fehu_Brief,     AppResources.Rune_Fehu_Full),
            (AppResources.Rune_Uruz_Name,     AppResources.Rune_Uruz_Brief,     AppResources.Rune_Uruz_Full),
            (AppResources.Rune_Thurisaz_Name, AppResources.Rune_Thurisaz_Brief, AppResources.Rune_Thurisaz_Full),
            (AppResources.Rune_Ansuz_Name,    AppResources.Rune_Ansuz_Brief,    AppResources.Rune_Ansuz_Full),
            (AppResources.Rune_Raidho_Name,   AppResources.Rune_Raidho_Brief,   AppResources.Rune_Raidho_Full),
            (AppResources.Rune_Kenaz_Name,    AppResources.Rune_Kenaz_Brief,    AppResources.Rune_Kenaz_Full),
            (AppResources.Rune_Gebo_Name,     AppResources.Rune_Gebo_Brief,     AppResources.Rune_Gebo_Full),
            (AppResources.Rune_Wunjo_Name,    AppResources.Rune_Wunjo_Brief,    AppResources.Rune_Wunjo_Full),
            (AppResources.Rune_Hagalaz_Name,  AppResources.Rune_Hagalaz_Brief,  AppResources.Rune_Hagalaz_Full),
            (AppResources.Rune_Nauthiz_Name,  AppResources.Rune_Nauthiz_Brief,  AppResources.Rune_Nauthiz_Full),
            (AppResources.Rune_Isa_Name,      AppResources.Rune_Isa_Brief,      AppResources.Rune_Isa_Full),
            (AppResources.Rune_Jera_Name,     AppResources.Rune_Jera_Brief,     AppResources.Rune_Jera_Full),
            (AppResources.Rune_Eihwaz_Name,   AppResources.Rune_Eihwaz_Brief,   AppResources.Rune_Eihwaz_Full),
            (AppResources.Rune_Perthro_Name,  AppResources.Rune_Perthro_Brief,  AppResources.Rune_Perthro_Full),
            (AppResources.Rune_Algiz_Name,    AppResources.Rune_Algiz_Brief,    AppResources.Rune_Algiz_Full),
            (AppResources.Rune_Sowilo_Name,   AppResources.Rune_Sowilo_Brief,   AppResources.Rune_Sowilo_Full),
            (AppResources.Rune_Tiwaz_Name,    AppResources.Rune_Tiwaz_Brief,    AppResources.Rune_Tiwaz_Full),
            (AppResources.Rune_Berkano_Name,  AppResources.Rune_Berkano_Brief,  AppResources.Rune_Berkano_Full),
            (AppResources.Rune_Ehwaz_Name,    AppResources.Rune_Ehwaz_Brief,    AppResources.Rune_Ehwaz_Full),
            (AppResources.Rune_Mannaz_Name,   AppResources.Rune_Mannaz_Brief,   AppResources.Rune_Mannaz_Full),
            (AppResources.Rune_Laguz_Name,    AppResources.Rune_Laguz_Brief,    AppResources.Rune_Laguz_Full),
            (AppResources.Rune_Ingwaz_Name,   AppResources.Rune_Ingwaz_Brief,   AppResources.Rune_Ingwaz_Full),
            (AppResources.Rune_Othala_Name,   AppResources.Rune_Othala_Brief,   AppResources.Rune_Othala_Full),
            (AppResources.Rune_Dagaz_Name,    AppResources.Rune_Dagaz_Brief,    AppResources.Rune_Dagaz_Full),
        };

        var list = new List<FutharkRune>(_static.Length);
        for (int i = 0; i < _static.Length; i++)
        {
            var (sym, segs)         = _static[i];
            var (name, brief, full) = localised[i];
            list.Add(new FutharkRune(sym, name, brief, full, segs));
        }
        return list;
    }

    /// <summary>
    /// Returns the 0-based index in the catalogue whose <c>Name</c> matches
    /// <paramref name="runeName"/>, or 0 if not found.
    /// </summary>
    public static int IndexOf(IReadOnlyList<FutharkRune> catalogue, string runeName)
    {
        for (int i = 0; i < catalogue.Count; i++)
            if (catalogue[i].Name == runeName) return i;
        return 0;
    }
}
