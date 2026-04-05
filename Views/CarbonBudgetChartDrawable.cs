using Aeonpulse.Attributes;
using Microsoft.Maui.Graphics;

namespace Aeonpulse.Views;

/// <summary>
/// <see cref="IDrawable"/> for the 1.5-degree carbon budget chart in the Global Exhale
/// expanded card view. Rendered by <c>GraphicsView x:Name="CarbonBudgetChartView"</c>
/// (HeightRequest=220).
///
/// <para>
/// <b>Visual concept:</b>
/// A time-series curve of cumulative CO2 emissions (Gt) from <see cref="ChartStartYear"/>
/// to <see cref="DepletionYear"/> + 2% padding. A horizontal dashed red limit line marks
/// <see cref="TotalBudgetGt"/>. Three milestone nodes sit on or at the curve:
/// base-date (hollow circle), today (filled cyan), depletion (filled pink).
/// Past emissions are drawn as a solid thick line; the future projection is dashed.
/// </para>
/// <para>
/// <b>Colour strategy:</b> colours are read from <c>Application.Current.Resources</c>
/// at draw time via the SpaceDarker discriminator, matching the pattern established in
/// <see cref="PopulationChartDrawable"/>.
/// DefaultDark: CyberCyan curve, CyberPink limit/depletion, JubileeAccent base-date,
///   TextGray grid, CardDark today-dot fill.
/// HC-Dark / HC-Light: falls back to white / black.
/// </para>
/// </summary>
[AIContext("UIPresentation")]
internal sealed class CarbonBudgetChartDrawable : IDrawable
{
    // Polynomial integral: cumulative Gt emitted from 1900 to x years after 1900.
    // Matches the formula in CalculationService exactly.
    private const double PreIndustrialGt = 11.77;
    private static double CumFromPoly(double x) =>
        0.0008 / 3.0 * x * x * x - 0.0122 / 2.0 * x * x + 0.6859 * x;
    internal static double CumCO2AtYear(double year) =>
        PreIndustrialGt + CumFromPoly(Math.Max(0, year - 1900.0));

    // Padding inside the canvas.
    private const float PadLeft   = 38f;
    private const float PadRight  = 10f;
    private const float PadTop    = 10f;
    private const float PadBottom = 22f;

    internal double ChartStartYear  { get; set; }
    internal double DepletionYear   { get; set; }
    internal double TotalBudgetGt   { get; set; }
    internal double BaseDateCumGt   { get; set; }
    internal double TodayCumGt      { get; set; }
    internal double BaseYear        { get; set; }
    internal double TodayYear       { get; set; }

    /// <inheritdoc/>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        if (w <= 0 || h <= 0 || TotalBudgetGt <= 0 || DepletionYear <= ChartStartYear) return;

        float chartLeft   = PadLeft;
        float chartRight  = w - PadRight;
        float chartTop    = PadTop;
        float chartBottom = h - PadBottom;
        float chartW      = chartRight - chartLeft;
        float chartH      = chartBottom - chartTop;
        if (chartW <= 0 || chartH <= 0) return;

        // X axis: ChartStartYear .. DepletionYear + 2% padding.
        double xMin = ChartStartYear;
        double xMax = DepletionYear + (DepletionYear - ChartStartYear) * 0.02;
        // Y axis: 0 .. TotalBudgetGt * 1.05 padding.
        double yMin = 0;
        double yMax = TotalBudgetGt * 1.05;

        float ToX(double year)   => chartLeft + (float)((year - xMin) / (xMax - xMin)) * chartW;
        float ToY(double cumGt)  => chartBottom - (float)((cumGt - yMin) / (yMax - yMin)) * chartH;

        // Resolve colours at draw time via SpaceDarker discriminator.
        var res = Application.Current?.Resources;
        Color bg = GetColor(res, "SpaceDarker", Color.FromArgb("#060812"));

        Color gridColor, curveColor, limitColor, labelColor, accentColor, todayFill, depletionColor, bgColor;
        if (bg == Colors.Black)
        {
            gridColor      = Colors.White.WithAlpha(0.20f);
            curveColor     = Colors.White.WithAlpha(0.80f);
            limitColor     = Colors.White.WithAlpha(0.70f);
            labelColor     = Colors.White.WithAlpha(0.55f);
            accentColor    = Colors.White;
            todayFill      = Colors.Black;
            depletionColor = Colors.White;
            bgColor        = Colors.Black;
        }
        else if (bg == Colors.White)
        {
            gridColor      = Colors.Black.WithAlpha(0.20f);
            curveColor     = Colors.Black.WithAlpha(0.80f);
            limitColor     = Colors.Black.WithAlpha(0.70f);
            labelColor     = Colors.Black.WithAlpha(0.55f);
            accentColor    = Colors.Black;
            todayFill      = Colors.White;
            depletionColor = Colors.Black;
            bgColor        = Colors.White;
        }
        else
        {
            gridColor      = GetColor(res, "TextGray",      Color.FromArgb("#B0B0B0")).WithAlpha(0.20f);
            curveColor     = GetColor(res, "CyberCyan",     Color.FromArgb("#00E5FF")).WithAlpha(0.85f);
            limitColor     = GetColor(res, "CyberPink",     Color.FromArgb("#FF79C6")).WithAlpha(0.85f);
            labelColor     = GetColor(res, "TextGray",      Color.FromArgb("#B0B0B0")).WithAlpha(0.65f);
            accentColor    = GetColor(res, "JubileeAccent", Color.FromArgb("#FFD700"));
            todayFill      = GetColor(res, "CardDark",      Color.FromArgb("#121628"));
            depletionColor = GetColor(res, "CyberPink",     Color.FromArgb("#FF79C6"));
            bgColor        = GetColor(res, "CardDark",      Color.FromArgb("#121628"));
        }

        // 1. Horizontal gridlines + Y-axis labels.
        canvas.StrokeSize  = 0.5f;
        canvas.StrokeColor = gridColor;
        canvas.FontColor   = labelColor;
        canvas.FontSize    = 9f;
        // Choose ~4 gridline steps covering 0..TotalBudgetGt.
        double step = Math.Pow(10, Math.Floor(Math.Log10(TotalBudgetGt / 4.0)));
        if (step < 50) step = 50;
        for (double g = step; g < yMax; g += step)
        {
            float gy = ToY(g);
            if (gy < chartTop || gy > chartBottom) continue;
            canvas.DrawLine(chartLeft, gy, chartRight, gy);
            canvas.DrawString($"{(int)g}", 0f, gy - 7f, PadLeft - 2f, 14f,
                              HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        // 2. Dashed limit line at TotalBudgetGt.
        float limitY = ToY(TotalBudgetGt);
        canvas.StrokeColor       = limitColor;
        canvas.StrokeSize        = 1.2f;
        canvas.StrokeDashPattern = new float[] { 6f, 4f };
        canvas.DrawLine(chartLeft, limitY, chartRight, limitY);
        canvas.StrokeDashPattern = null;

        // 3. Emission curve in two passes: past (solid) and future (dashed).
        //    Sample ~1 point per year for a smooth curve.
        int totalSteps = (int)Math.Ceiling(xMax - xMin) + 2;
        double stepSize = (xMax - xMin) / Math.Max(totalSteps, 2);

        // Past segment: ChartStartYear .. TodayYear.
        canvas.StrokeColor   = curveColor;
        canvas.StrokeSize    = 2.2f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeDashPattern = null;
        {
            var path = new PathF();
            bool started = false;
            for (double yr = xMin; yr <= Math.Min(TodayYear, xMax) + stepSize * 0.5; yr += stepSize)
            {
                double clampedYr = Math.Min(yr, TodayYear);
                float px = ToX(clampedYr);
                float py = ToY(CumCO2AtYear(clampedYr));
                if (!started) { path.MoveTo(px, py); started = true; }
                else           path.LineTo(px, py);
                if (clampedYr >= TodayYear) break;
            }
            if (started) canvas.DrawPath(path);
        }

        // Future segment: TodayYear .. DepletionYear (dashed).
        canvas.StrokeSize        = 1.5f;
        canvas.StrokeDashPattern = new float[] { 5f, 4f };
        {
            var path = new PathF();
            bool started = false;
            for (double yr = TodayYear; yr <= DepletionYear + stepSize * 0.5; yr += stepSize)
            {
                double clampedYr = Math.Min(yr, DepletionYear);
                float px = ToX(clampedYr);
                float py = ToY(CumCO2AtYear(clampedYr));
                if (!started) { path.MoveTo(px, py); started = true; }
                else           path.LineTo(px, py);
                if (clampedYr >= DepletionYear) break;
            }
            if (started) canvas.DrawPath(path);
        }
        canvas.StrokeDashPattern = null;

        // 4. Base-date node: hollow circle with accent stroke.
        if (BaseYear >= xMin && BaseYear <= xMax)
        {
            float bx = ToX(BaseYear);
            float by = ToY(BaseDateCumGt);
            canvas.FillColor   = bgColor;
            canvas.StrokeColor = accentColor;
            canvas.StrokeSize  = 2.0f;
            canvas.FillCircle(bx, by, 5f);
            canvas.DrawCircle(bx, by, 5f);
        }

        // 5. Today node: filled cyan with dark stroke.
        if (TodayYear >= xMin && TodayYear <= xMax)
        {
            float tx = ToX(TodayYear);
            float ty = ToY(TodayCumGt);
            canvas.FillColor   = curveColor;
            canvas.StrokeColor = todayFill;
            canvas.StrokeSize  = 1.5f;
            canvas.FillCircle(tx, ty, 6f);
            canvas.DrawCircle(tx, ty, 6f);
        }

        // 6. Depletion node: filled pink at the limit line intersection.
        if (DepletionYear >= xMin && DepletionYear <= xMax)
        {
            float dx = ToX(DepletionYear);
            float dy = ToY(TotalBudgetGt);
            canvas.FillColor   = depletionColor;
            canvas.StrokeColor = bgColor;
            canvas.StrokeSize  = 1.5f;
            canvas.FillCircle(dx, dy, 6f);
            canvas.DrawCircle(dx, dy, 6f);
        }

        // 7. X-axis year labels.
        canvas.FontColor = labelColor;
        canvas.FontSize  = 9f;
        if (BaseYear >= xMin && BaseYear <= xMax)
            canvas.DrawString($"{(int)BaseYear}", ToX(BaseYear) - 16f, chartBottom + 3f, 32f, 14f,
                              HorizontalAlignment.Center, VerticalAlignment.Top);
        canvas.DrawString($"{(int)TodayYear}", ToX(TodayYear) - 16f, chartBottom + 3f, 32f, 14f,
                          HorizontalAlignment.Center, VerticalAlignment.Top);
        if (DepletionYear >= xMin)
            canvas.DrawString($"{(int)DepletionYear}", ToX(DepletionYear) - 16f, chartBottom + 3f, 32f, 14f,
                              HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    private static Color GetColor(ResourceDictionary? res, string key, Color fallback)
    {
        if (res != null && res.TryGetValue(key, out var raw) && raw is Color c) return c;
        return fallback;
    }
}
