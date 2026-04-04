using Aeonpulse.Attributes;
using Aeonpulse.Models;
using Microsoft.Maui.Graphics;

namespace Aeonpulse.Views;

/// <summary>
/// <see cref="IDrawable"/> for the two-ring donut chart in the Life Log expanded
/// card view. Rendered by <c>GraphicsView x:Name="LifeLogChartView"</c> (260x260).
///
/// <para>
/// <b>Geometry:</b>
/// Center = (cx, cy). Three concentric radii:
/// <list type="bullet">
///   <item>holeR  = outerR * 0.30  - transparent centre hole.</item>
///   <item>innerR = outerR * 0.58  - boundary between inner (Today) and outer (Forecast) ring.</item>
///   <item>outerR = min(w,h)/2 - padding - outer edge of the forecast ring.</item>
/// </list>
/// </para>
/// <para>
/// <b>Drawing passes (per slice, starting at -90 degrees = top):</b>
/// 1. Outer ring arc (holeR to outerR), slice colour at 70% opacity - covers both rings first.
/// 2. Inner ring fill (holeR to innerR), same colour full opacity - overwrites the inner zone.
/// 3. Separator line from holeR to outerR at the start angle of each slice (background colour,
///    thin) to create crisp visual separation between categories.
/// </para>
/// <para>
/// <b>Why path-based arcs:</b> <c>ICanvas.DrawArc</c> only strokes; to fill a ring
/// segment we build a <c>PathF</c> that traces the outer arc forward, then the inner
/// arc backward (donut wedge).
/// </para>
/// <para>
/// <b>Theming:</b> slice colours are data-palette values (fixed, scheme-independent).
/// The separator and hole fill use <c>CardDark</c> / <c>SpaceDarker</c> from
/// <c>Application.Current.Resources</c> so the gaps look correct in every scheme.
/// </para>
/// </summary>
[AIContext("UIPresentation")]
internal sealed class LifeLogChartDrawable : IDrawable
{
    private readonly IReadOnlyList<LifeLogSlice> _slices;

    private const double DegToRad  = Math.PI / 180.0;
    private const float  Padding   = 14f;
    private const float  StartDeg  = -90f;  // 12 o'clock

    internal LifeLogChartDrawable(IReadOnlyList<LifeLogSlice> slices)
    {
        _slices = slices;
    }

    /// <inheritdoc/>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        if (w <= 0 || h <= 0 || _slices.Count == 0) return;

        float cx     = w / 2f;
        float cy     = h / 2f;
        float outerR = Math.Min(w, h) / 2f - Padding;
        float innerR = outerR * 0.58f;
        float holeR  = outerR * 0.30f;
        if (outerR <= 0) return;

        // Background / separator colour: use CardDark so gaps blend with the card.
        var res = Application.Current?.Resources;
        Color separatorColor = GetColor(res, "CardDark", Color.FromArgb("#121628"));

        // --- Pass 1 & 2: draw each slice ---
        float angleDeg = StartDeg;
        foreach (var slice in _slices)
        {
            float sweepDeg = (float)(slice.DailyProportion * 360.0);
            Color sliceColor = Color.FromArgb(slice.ColorHex);

            // Pass 1 - outer ring (holeR..outerR) at reduced opacity.
            Color outerColor = sliceColor.WithAlpha(0.60f);
            FillDonutSegment(canvas, cx, cy, holeR, outerR, angleDeg, sweepDeg, outerColor);

            // Pass 2 - inner ring (holeR..innerR) at full opacity, overpaints inner zone.
            FillDonutSegment(canvas, cx, cy, holeR, innerR, angleDeg, sweepDeg, sliceColor);

            angleDeg += sweepDeg;
        }

        // --- Pass 3: separator lines between slices ---
        canvas.StrokeColor = separatorColor;
        canvas.StrokeSize  = 1.5f;
        canvas.StrokeLineCap = LineCap.Butt;
        float sepAngle = StartDeg;
        foreach (var slice in _slices)
        {
            double rad = sepAngle * DegToRad;
            float  cos = (float)Math.Cos(rad);
            float  sin = (float)Math.Sin(rad);
            canvas.DrawLine(cx + holeR  * cos, cy + holeR  * sin,
                            cx + outerR * cos, cy + outerR * sin);
            sepAngle += (float)(slice.DailyProportion * 360.0);
        }

        // --- Hole fill: paint the centre disc in CardDark so the donut hole is clean ---
        canvas.FillColor = separatorColor;
        canvas.FillCircle(cx, cy, holeR - 0.5f);
    }

    /// <summary>
    /// Fills a donut (annular) segment defined by two radii and an angular span.
    /// Builds a <see cref="PathF"/> that traces the outer arc clockwise then the
    /// inner arc counter-clockwise, closing to form a filled ring wedge.
    /// </summary>
    private static void FillDonutSegment(ICanvas canvas,
                                          float cx, float cy,
                                          float innerR, float outerR,
                                          float startDeg, float sweepDeg,
                                          Color color)
    {
        if (sweepDeg <= 0) return;

        float endDeg = startDeg + sweepDeg;

        // Number of line segments used to approximate each arc.
        // 1 segment per degree gives smooth curves without excess vertices.
        int   steps   = Math.Max(2, (int)Math.Ceiling(Math.Abs(sweepDeg)));
        float stepRad = (float)(sweepDeg * DegToRad / steps);

        var path = new PathF();

        // Outer arc start point.
        float startRad = (float)(startDeg * DegToRad);
        path.MoveTo(cx + outerR * (float)Math.Cos(startRad),
                    cy + outerR * (float)Math.Sin(startRad));

        // Outer arc (clockwise = increasing angle).
        for (int i = 1; i <= steps; i++)
        {
            float a = startRad + i * stepRad;
            path.LineTo(cx + outerR * (float)Math.Cos(a),
                        cy + outerR * (float)Math.Sin(a));
        }

        // Line from outer arc end to inner arc end.
        float endRad = (float)(endDeg * DegToRad);
        path.LineTo(cx + innerR * (float)Math.Cos(endRad),
                    cy + innerR * (float)Math.Sin(endRad));

        // Inner arc (counter-clockwise = decreasing angle).
        for (int i = steps - 1; i >= 0; i--)
        {
            float a = startRad + i * stepRad;
            path.LineTo(cx + innerR * (float)Math.Cos(a),
                        cy + innerR * (float)Math.Sin(a));
        }

        path.Close();

        canvas.FillColor = color;
        canvas.FillPath(path);
    }

    private static Color GetColor(ResourceDictionary? res, string key, Color fallback)
    {
        if (res != null && res.TryGetValue(key, out var raw) && raw is Color c)
            return c;
        return fallback;
    }
}
