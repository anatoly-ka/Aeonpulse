using Aeonpulse.Attributes;
using Microsoft.Maui.Graphics;

namespace Aeonpulse.Views;

/// <summary>
/// <see cref="IDrawable"/> for the population growth chart in the Global Crowd
/// expanded card view. Rendered by <c>GraphicsView x:Name="PopulationChartView"</c>.
///
/// <para>
/// <b>Dataset:</b> 10 reference points from 1950 to 2050, Y axis in billions.
/// X maps Year linearly to canvas width; Y maps Population linearly to canvas height
/// (0,0 = top-left, so higher population = smaller Y value).
/// </para>
/// <para>
/// <b>Drawing passes:</b>
/// 1. Subtle horizontal gridlines at 2B, 4B, 6B, 8B, 10B with Y-axis labels.
/// 2. Population curve (PathF through all dataset points).
/// 3. Base-date marker (filled gold circle).
/// 4. Current-date marker (white circle with gold stroke).
/// 5. Scrubber: vertical line + interpolated hover dot at <see cref="ScrubX"/>.
/// </para>
/// <para>
/// <b>Theming:</b> colours read from <c>Application.Current.Resources</c>
/// at draw time via SpaceDarker discriminator.
/// DefaultDark: TextGray 25% grid, CyberCyan curve, JubileeAccent base dot.
/// HC-Dark: white 30% grid, white curve, white dots.
/// HC-Light: black 30% grid, black curve, black dots.
/// </para>
/// </summary>
[AIContext("UIPresentation")]
internal sealed class PopulationChartDrawable : IDrawable
{
    // Historical and projected population dataset (Year, Billions).
    // Covers 1950-2050 to give the chart a forward-looking horizon.
    internal static readonly (double Year, double PopBillions)[] Data =
    {
        (1950, 2.5), (1960, 3.0), (1970, 3.7), (1980, 4.4),
        (1990, 5.3), (2000, 6.1), (2010, 6.9), (2020, 7.8),
        (2026, 8.15),(2050, 9.7),
    };

    private const double YearMin = 1950;
    private const double YearMax = 2050;
    private const double PopMin  = 2.0;
    private const double PopMax  = 10.0;

    // Padding inside the canvas (left wider to fit Y-axis labels).
    private const float PadLeft   = 36f;
    private const float PadRight  = 10f;
    private const float PadTop    = 10f;
    private const float PadBottom = 20f;

    internal int    BaseYear        { get; set; }
    internal double BasePopBillions { get; set; }
    internal int    CurrentYear     { get; set; }
    internal double CurrentPopBillions { get; set; }

    /// <summary>
    /// X pixel coordinate of the scrubber line. Set by the interaction handler
    /// in MainPage.xaml.cs. Clamped to the chart area in <see cref="Draw"/>.
    /// -1 means no scrubber shown (initial state before first touch).
    /// </summary>
    internal float ScrubX { get; set; } = -1f;

    /// <inheritdoc/>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        if (w <= 0 || h <= 0) return;

        float chartLeft   = PadLeft;
        float chartRight  = w - PadRight;
        float chartTop    = PadTop;
        float chartBottom = h - PadBottom;
        float chartW      = chartRight  - chartLeft;
        float chartH      = chartBottom - chartTop;
        if (chartW <= 0 || chartH <= 0) return;

        // Coordinate helpers.
        float ToX(double year)       => chartLeft  + (float)((year - YearMin) / (YearMax - YearMin)) * chartW;
        float ToY(double popBillions) => chartBottom - (float)((popBillions - PopMin) / (PopMax - PopMin)) * chartH;

        // Resolve colours.
        var res = Application.Current?.Resources;
        Color bg = GetColor(res, "SpaceDarker", Color.FromArgb("#060812"));

        Color gridColor;
        Color curveColor;
        Color labelColor;
        Color accentColor;     // base-date dot
        Color currentDotFill;  // current-date dot fill
        Color scrubColor;

        if (bg == Colors.Black)
        {
            gridColor      = Colors.White.WithAlpha(0.20f);
            curveColor     = Colors.White.WithAlpha(0.80f);
            labelColor     = Colors.White.WithAlpha(0.60f);
            accentColor    = Colors.White;
            currentDotFill = Colors.Black;
            scrubColor     = Colors.White.WithAlpha(0.70f);
        }
        else if (bg == Colors.White)
        {
            gridColor      = Colors.Black.WithAlpha(0.20f);
            curveColor     = Colors.Black.WithAlpha(0.80f);
            labelColor     = Colors.Black.WithAlpha(0.60f);
            accentColor    = Colors.Black;
            currentDotFill = Colors.White;
            scrubColor     = Colors.Black.WithAlpha(0.70f);
        }
        else
        {
            gridColor      = GetColor(res, "TextGray",      Color.FromArgb("#B0B0B0")).WithAlpha(0.20f);
            curveColor     = GetColor(res, "CyberCyan",     Color.FromArgb("#00E5FF")).WithAlpha(0.85f);
            labelColor     = GetColor(res, "TextGray",      Color.FromArgb("#B0B0B0")).WithAlpha(0.70f);
            accentColor    = GetColor(res, "JubileeAccent", Color.FromArgb("#FFD700"));
            currentDotFill = GetColor(res, "CardDark",      Color.FromArgb("#121628"));
            scrubColor     = GetColor(res, "TextDim",       Color.FromArgb("#E5E5E5")).WithAlpha(0.70f);
        }

        // 1. Gridlines + Y-axis labels at 2B, 4B, 6B, 8B, 10B.
        canvas.StrokeSize  = 0.5f;
        canvas.StrokeColor = gridColor;
        canvas.FontColor   = labelColor;
        canvas.FontSize    = 9f;
        for (double pop = 2.0; pop <= 10.0; pop += 2.0)
        {
            float gy = ToY(pop);
            canvas.DrawLine(chartLeft, gy, chartRight, gy);
            canvas.DrawString($"{(int)pop}B", 0f, gy - 7f, PadLeft - 2f, 14f,
                              HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        // 2. Population curve as a PathF through all dataset points.
        canvas.StrokeColor = curveColor;
        canvas.StrokeSize  = 2.0f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;
        var path = new PathF();
        path.MoveTo(ToX(Data[0].Year), ToY(Data[0].PopBillions));
        for (int i = 1; i < Data.Length; i++)
            path.LineTo(ToX(Data[i].Year), ToY(Data[i].PopBillions));
        canvas.DrawPath(path);

        // 3. Base-date marker: filled gold circle (only if within chart year range).
        //    Y is computed via InterpolatePopulation so the centre lies exactly on the
        //    drawn polyline, regardless of the live-model population value stored in
        //    BasePopBillions.
        if (BaseYear >= YearMin && BaseYear <= YearMax)
        {
            float bx = ToX(BaseYear);
            float by = ToY(InterpolatePopulation(BaseYear));
            canvas.FillColor = accentColor;
            canvas.FillCircle(bx, by, 5f);
        }

        // 4. Current-date marker: CardDark fill + accent stroke.
        //    Same reasoning: Y from InterpolatePopulation keeps the dot on the line.
        if (CurrentYear >= YearMin && CurrentYear <= YearMax)
        {
            float cx2 = ToX(CurrentYear);
            float cy2 = ToY(InterpolatePopulation(CurrentYear));
            canvas.FillColor   = currentDotFill;
            canvas.StrokeColor = accentColor;
            canvas.StrokeSize  = 2.0f;
            canvas.FillCircle(cx2, cy2, 5f);
            canvas.DrawCircle(cx2, cy2, 5f);
        }

        // 5. Scrubber: vertical line + hover dot at ScrubX.
        if (ScrubX >= chartLeft && ScrubX <= chartRight)
        {
            // Interpolate Y on the polyline at ScrubX.
            double scrubYear = YearMin + (ScrubX - chartLeft) / chartW * (YearMax - YearMin);
            double scrubPop  = InterpolatePopulation(scrubYear);
            float  scrubY    = ToY(scrubPop);

            canvas.StrokeColor = scrubColor;
            canvas.StrokeSize  = 1.0f;
            canvas.DrawLine(ScrubX, chartTop, ScrubX, chartBottom);

            canvas.FillColor   = scrubColor;
            canvas.StrokeColor = scrubColor;
            canvas.StrokeSize  = 1.5f;
            canvas.FillCircle(ScrubX, scrubY, 5.5f);
        }
    }

    /// <summary>
    /// Linearly interpolates the population (in billions) for <paramref name="year"/>
    /// from the <see cref="Data"/> dataset. Clamps to the dataset's first/last values
    /// for out-of-range years.
    /// </summary>
    internal static double InterpolatePopulation(double year)
    {
        if (year <= Data[0].Year)      return Data[0].PopBillions;
        if (year >= Data[^1].Year)     return Data[^1].PopBillions;

        for (int i = 0; i < Data.Length - 1; i++)
        {
            if (year >= Data[i].Year && year <= Data[i + 1].Year)
            {
                double t = (year - Data[i].Year) / (Data[i + 1].Year - Data[i].Year);
                return Data[i].PopBillions + t * (Data[i + 1].PopBillions - Data[i].PopBillions);
            }
        }
        return Data[^1].PopBillions;
    }

    private static Color GetColor(ResourceDictionary? res, string key, Color fallback)
    {
        if (res != null && res.TryGetValue(key, out var raw) && raw is Color c)
            return c;
        return fallback;
    }
}
