using Aeonpulse.Attributes;
using Aeonpulse.Models;
using Microsoft.Maui.Graphics;

namespace Aeonpulse.Views;

/// <summary>
/// <see cref="IDrawable"/> implementation for the Human Birth Rank history curve.
///
/// <para>
/// <b>Axes:</b>
/// X - linear, domain [-5000, 2050] (Year AD).
/// Y - linear, domain [0, 125,000,000,000].
/// </para>
/// <para>
/// <b>Curve clipping strategy:</b>
/// The PRB dataset contains points at years -190000, -50000 and -8000, all of
/// which are to the left of XMin=-5000. Rather than simply skipping them (which
/// would leave the segment from x=-5000 to x=1 undrawn), the polyline loop
/// detects the first in-range point and, if the immediately preceding raw data
/// point was out of range, interpolates an entry point exactly at x=XMin.
/// The path starts with MoveTo at that interpolated entry so:
/// - the segment from x=-5000 to x=1 is drawn, and
/// - no vertical baseline drop from (x=-5000, EverBorn=0) is produced.
/// </para>
/// <para>
/// <b>Theming:</b>
/// Scheme detected via SpaceDarker resource:
/// Black = HC-Dark (all foreground white, marker white);
/// White = HC-Light (all foreground black, marker black);
/// other = DefaultDark (TextGray curve/ticks, TextDim labels, JubileeAccent marker).
/// </para>
/// </summary>
[AIContext("UIPresentation")]
internal sealed class BirthRankChartDrawable : IDrawable
{
    private readonly HumanBirthRankResult _result;

    private const double XMin = -5000;
    private const double XMax =  2050;
    private const double YMin =     0;
    private const double YMax = 125_000_000_000.0;

    private const float MarginLeft   = 36f;
    private const float MarginRight  =  6f;
    private const float MarginTop    =  6f;
    private const float MarginBottom = 28f;

    private static readonly (double Year, float ExtraOffset)[] _annotations =
    {
        (1850,  0f),
        (1900, 30f),
        (1950,  0f),
        (2000, 30f),
        (2022,  0f),
    };

    internal BirthRankChartDrawable(HumanBirthRankResult result) => _result = result;

    /// <inheritdoc/>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        if (w <= 0 || h <= 0) return;

        float px0 = MarginLeft;
        float px1 = w - MarginRight;
        float py0 = MarginTop;
        float py1 = h - MarginBottom;
        float pw  = px1 - px0;
        float ph  = py1 - py0;
        if (pw <= 0 || ph <= 0) return;

        // Resolve theme colours via SpaceDarker discriminator.
        var res = Application.Current?.Resources;
        Color bg = GetColor(res, "SpaceDarker", Color.FromArgb("#060812"));

        Color foreground;
        Color labelColor;
        Color markerColor;
        if (bg == Colors.Black)
        {
            foreground = labelColor = markerColor = Colors.White;
        }
        else if (bg == Colors.White)
        {
            foreground = labelColor = markerColor = Colors.Black;
        }
        else
        {
            foreground  = GetColor(res, "TextGray",      Color.FromArgb("#B0B0B0"));
            labelColor  = GetColor(res, "TextDim",       Color.FromArgb("#E5E5E5"));
            markerColor = GetColor(res, "JubileeAccent", Color.FromArgb("#FFD700"));
        }

        float MapX(double year)   => px0 + (float)((year   - XMin) / (XMax - XMin)) * pw;
        float MapY(double births) => py1 - (float)((births - YMin) / (YMax - YMin)) * ph;

        // ------------------------------------------------------------------
        // Y-axis labels only (no grid lines).
        // ------------------------------------------------------------------
        canvas.FontSize  = 8.5f;
        canvas.FontColor = labelColor;
        var yTicks = new (double Value, string Label)[]
        {
            ( 20_000_000_000, "20B"),
            ( 40_000_000_000, "40B"),
            ( 60_000_000_000, "60B"),
            ( 80_000_000_000, "80B"),
            (100_000_000_000, "100B"),
            (120_000_000_000, "120B"),
        };
        foreach (var (val, lbl) in yTicks)
        {
            float gy = MapY(val);
            canvas.DrawString(lbl, 0, gy - 5.5f, MarginLeft - 2f, 11f,
                              HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        // ------------------------------------------------------------------
        // X-axis tick marks and labels.
        // ------------------------------------------------------------------
        var xTicks = new (double Year, string Label)[]
        {
            (-5000, "5000 B.C.E."),
            (-2500, "2500 B.C.E."),
            (    1, "1 C.E."),
            ( 1200, "1200"),
            ( 2022, "2022"),
        };
        canvas.FontSize  = 8f;
        canvas.FontColor = labelColor;
        foreach (var (year, lbl) in xTicks)
        {
            float gx = MapX(year);
            canvas.StrokeColor = foreground.WithAlpha(0.5f);
            canvas.StrokeSize  = 0.5f;
            canvas.DrawLine(gx, py1, gx, py1 + 3f);
            canvas.DrawString(lbl, gx - 22f, py1 + 4f, 44f, MarginBottom - 4f,
                              HorizontalAlignment.Center, VerticalAlignment.Top);
        }

        // ------------------------------------------------------------------
        // Curve polyline with correct left-edge clipping.
        //
        // Problem: the PRB data has points at years -190000, -50000, -8000
        // (all < XMin=-5000) and then at year 1. A simple "skip if year < XMin"
        // filter starts the path at MoveTo(year=1) which leaves the entire
        // segment from x=-5000 to x=1 undrawn.
        //
        // Fix: when the first in-range point is encountered and its immediate
        // predecessor was out of range, linearly interpolate an entry point
        // at exactly x=XMin from the two bracketing raw data points, and use
        // that as the MoveTo. This draws the [-5000, 1] segment without
        // producing a vertical drop from (XMin, EverBorn=0).
        // ------------------------------------------------------------------
        var pts = _result.ChartPoints;
        if (pts.Count >= 2)
        {
            canvas.StrokeColor    = foreground;
            canvas.StrokeSize     = 1.5f;
            canvas.StrokeLineCap  = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;

            var path = new PathF();
            bool started = false;

            for (int i = 0; i < pts.Count; i++)
            {
                var (year, everBorn) = pts[i];

                if (year > XMax) break;   // past the right edge - stop

                if (year < XMin)          // still to the left of the visible range
                {
                    // Keep iterating; remember this point as the potential left bracket.
                    continue;
                }

                // First point that is >= XMin.
                if (!started)
                {
                    // If there is a predecessor that was out of range (year < XMin),
                    // interpolate an entry point at exactly x = XMin.
                    if (i > 0 && pts[i - 1].Year < XMin)
                    {
                        var (prevYear, prevBorn) = pts[i - 1];
                        double span = year - prevYear;
                        double t    = span > 0 ? (XMin - prevYear) / span : 0;
                        double entryBorn = prevBorn + t * (everBorn - prevBorn);
                        path.MoveTo(MapX(XMin), MapY(Math.Clamp(entryBorn, YMin, YMax)));
                        path.LineTo(MapX(year),  MapY(Math.Clamp(everBorn, YMin, YMax)));
                    }
                    else
                    {
                        path.MoveTo(MapX(year), MapY(Math.Clamp(everBorn, YMin, YMax)));
                    }
                    started = true;
                }
                else
                {
                    path.LineTo(MapX(year), MapY(Math.Clamp(everBorn, YMin, YMax)));
                }
            }

            if (started) canvas.DrawPath(path);
        }

        // ------------------------------------------------------------------
        // Left-side point annotations with right-pointing arrows.
        // ------------------------------------------------------------------
        var yearToBorn = new System.Collections.Generic.Dictionary<double, double>();
        foreach (var (yr, eb) in pts)
            if (!yearToBorn.ContainsKey(yr)) yearToBorn[yr] = eb;

        canvas.FontSize  = 8f;
        const float ArrowLen  = 14f;
        const float ArrowHead =  3.5f;
        const float LabelW    = 30f;

        foreach (var (annoYear, extraOffset) in _annotations)
        {
            if (!yearToBorn.TryGetValue(annoYear, out double annoEverBorn)) continue;
            float dotX = MapX(annoYear);
            float dotY = MapY(Math.Clamp(annoEverBorn, YMin, YMax));

            float arrowEndX   = dotX - 2f;
            float arrowStartX = arrowEndX - (ArrowLen + extraOffset);

            canvas.FontColor   = labelColor;
            canvas.StrokeColor = labelColor;
            canvas.StrokeSize  = 0.8f;

            canvas.DrawString(((int)annoYear).ToString(),
                              arrowStartX - LabelW - 1f, dotY - 5f, LabelW, 11f,
                              HorizontalAlignment.Right, VerticalAlignment.Center);
            canvas.DrawLine(arrowStartX, dotY, arrowEndX, dotY);
            canvas.DrawLine(arrowEndX, dotY, arrowEndX - ArrowHead, dotY - ArrowHead);
            canvas.DrawLine(arrowEndX, dotY, arrowEndX - ArrowHead, dotY + ArrowHead);
        }

        // ------------------------------------------------------------------
        // User marker: filled ellipse at (MarkerYear, EstimatedRank).
        // ------------------------------------------------------------------
        double markerYear = _result.MarkerYear;
        double markerBorn = _result.EstimatedRank;
        if (!double.IsNaN(markerYear) && markerBorn > 0)
        {
            float mx = MapX(Math.Clamp(markerYear, XMin, XMax));
            float my = MapY(Math.Clamp(markerBorn, YMin, YMax));
            canvas.FillColor = markerColor;
            canvas.FillEllipse(mx - 4f, my - 4f, 8f, 8f);
        }
    }

    private static Color GetColor(ResourceDictionary? res, string key, Color fallback)
    {
        if (res != null && res.TryGetValue(key, out var raw) && raw is Color c)
            return c;
        return fallback;
    }
}
