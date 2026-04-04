using Aeonpulse.Attributes;
using Aeonpulse.Models;
using Microsoft.Maui.Graphics;

namespace Aeonpulse.Views;

/// <summary>
/// <see cref="IDrawable"/> for the Web of Wyrd (Skuld's Net) visualization in
/// the Birth Rune expanded card view.
///
/// <para>
/// <b>Grid:</b> 15 points, 3 columns x 5 rows, canvas aspect 1:2 (W:H).
/// <code>
///   p[0]  p[1]  p[2]    row 0
///   p[3]  p[4]  p[5]    row 1
///   p[6]  p[7]  p[8]    row 2
///   p[9]  p[10] p[11]   row 3
///   p[12] p[13] p[14]   row 4
/// </code>
/// stepX = (width  - 2*padX) / 2
/// stepY = (height - 2*padY) / 4
/// </para>
/// <para>
/// <b>Skeleton (9 Web of Wyrd lines always drawn dim):</b>
/// <list type="bullet">
///   <item>3 verticals  : p[0]-p[12], p[1]-p[13], p[2]-p[14]</item>
///   <item>3 diags TL-BR: p[0]-p[8],  p[3]-p[11], p[6]-p[14]</item>
///   <item>3 diags BL-TR: p[6]-p[2],  p[9]-p[5],  p[12]-p[8]</item>
/// </list>
/// </para>
/// <para>
/// <b>Rune highlight:</b> each <see cref="FutharkRune.Segments"/> entry is a
/// (A,B) pair of point indices. The highlight pass draws those segments thick
/// in accent colour on top of the skeleton, then fills dots at their endpoints.
/// </para>
/// <para>
/// <b>Theming:</b> colours read from <c>Application.Current.Resources</c>
/// at draw time via SpaceDarker discriminator.
/// </para>
/// </summary>
[AIContext("UIPresentation")]
internal sealed class WyrdWebDrawable : IDrawable
{
    private readonly IReadOnlyList<FutharkRune> _catalogue;
    private readonly int _selectedIndex;

    // The 9 fixed skeleton line endpoints (point-index pairs into the 15-pt array).
    private static readonly (int A, int B)[] SkeletonSegs =
    {
        // 3 verticals
        ( 0, 12),
        ( 1, 13),
        ( 2, 14),
        // 3 diagonals TL->BR
        ( 0,  8),
        ( 3, 11),
        ( 6, 14),
        // 3 diagonals BL->TR
        ( 6,  2),
        ( 9,  5),
        (12,  8),
    };

    internal WyrdWebDrawable(IReadOnlyList<FutharkRune> catalogue, int selectedIndex)
    {
        _catalogue     = catalogue;
        _selectedIndex = Math.Clamp(selectedIndex, 0, catalogue.Count - 1);
    }

    /// <inheritdoc/>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        if (w <= 0 || h <= 0) return;

        float padX  = w * 0.08f;
        float padY  = h * 0.08f;
        float stepX = (w - padX * 2f) / 2f;
        float stepY = (h - padY * 2f) / 4f;

        // Build all 15 grid points (row-major, 3 cols x 5 rows).
        PointF Pt(int row, int col) => new PointF(padX + col * stepX, padY + row * stepY);
        var pts = new PointF[15]
        {
            Pt(0,0), Pt(0,1), Pt(0,2),   // p[ 0] p[ 1] p[ 2]
            Pt(1,0), Pt(1,1), Pt(1,2),   // p[ 3] p[ 4] p[ 5]
            Pt(2,0), Pt(2,1), Pt(2,2),   // p[ 6] p[ 7] p[ 8]
            Pt(3,0), Pt(3,1), Pt(3,2),   // p[ 9] p[10] p[11]
            Pt(4,0), Pt(4,1), Pt(4,2),   // p[12] p[13] p[14]
        };

        // Resolve colours.
        var res = Application.Current?.Resources;
        Color bg = GetColor(res, "SpaceDarker", Color.FromArgb("#060812"));

        Color skelColor;
        Color accentColor;
        if (bg == Colors.Black)
        {
            skelColor   = Colors.White.WithAlpha(0.30f);
            accentColor = Colors.White;
        }
        else if (bg == Colors.White)
        {
            skelColor   = Colors.Black.WithAlpha(0.30f);
            accentColor = Colors.Black;
        }
        else
        {
            skelColor   = GetColor(res, "TextGray", Color.FromArgb("#B0B0B0")).WithAlpha(0.25f);
            accentColor = GetColor(res, "JubileeAccent", Color.FromArgb("#FFD700"));
        }

        // Selected rune segments (may include points not on skeleton lines).
        var rune     = (_selectedIndex >= 0 && _selectedIndex < _catalogue.Count)
                       ? _catalogue[_selectedIndex] : null;
        var runeSegs = rune?.Segments ?? System.Array.Empty<(int, int)>();

        canvas.StrokeLineCap = LineCap.Round;

        // 1. Draw all 9 skeleton lines (dim, thin).
        canvas.StrokeColor = skelColor;
        canvas.StrokeSize  = 1.2f;
        foreach (var (a, b) in SkeletonSegs)
            canvas.DrawLine(pts[a], pts[b]);

        // 2. Draw rune segments (thick, accent colour) on top of skeleton.
        canvas.StrokeColor = accentColor;
        canvas.StrokeSize  = 3.0f;
        foreach (var (a, b) in runeSegs)
        {
            if (a >= 0 && a < pts.Length && b >= 0 && b < pts.Length)
                canvas.DrawLine(pts[a], pts[b]);
        }

        // 3. Fill accent dots at every endpoint of a rune segment.
        var litPts = new System.Collections.Generic.HashSet<int>();
        foreach (var (a, b) in runeSegs)
        {
            if (a >= 0 && a < pts.Length) litPts.Add(a);
            if (b >= 0 && b < pts.Length) litPts.Add(b);
        }
        canvas.FillColor = accentColor;
        const float dotR = 3.5f;
        foreach (int pi in litPts)
            canvas.FillCircle(pts[pi].X, pts[pi].Y, dotR);
    }

    private static Color GetColor(ResourceDictionary? res, string key, Color fallback)
    {
        if (res != null && res.TryGetValue(key, out var raw) && raw is Color c)
            return c;
        return fallback;
    }
}
