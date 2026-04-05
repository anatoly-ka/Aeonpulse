using Aeonpulse.Attributes;
using Microsoft.Maui.Graphics;

namespace Aeonpulse.Views;

/// <summary>
/// <see cref="IDrawable"/> for the Taxonomy Flow (Sankey-style) diagram in the
/// Vibrant Nature expanded card view.
/// Rendered by <c>GraphicsView x:Name="TaxonomyFlowView"</c> (HeightRequest=160).
/// The centre circle and taxonomy icon are drawn by XAML elements layered over the
/// GraphicsView in the same Grid (TaxonomyCircle Border + Image with ImageTint).
///
/// <para>
/// <b>Visual concept:</b>
/// Four inflow streams (Insects, Plants, Vertebrates, Others discoveries) converge
/// from the left edge to a central node; three outflow streams (Insects, Vertebrates,
/// Others extinctions) diverge from the same centre to the right edge.
/// All streams are cubic Bezier curves with fixed, scheme-independent colours.
/// </para>
/// <para>
/// <b>Stream colours (fixed, scheme-independent):</b>
/// Discoveries: Insects=#FFFF00, Plants=#00FF00, Vertebrates=#0000FF, Others=#00FFFF.
/// Extinctions: Insects=#C0C0C0, Vertebrates=#808000, Others=#808080.
/// </para>
/// <para>
/// <b>Label colours (scheme-aware):</b>
/// Resolved via SpaceDarker discriminator at draw time: TextDim (DefaultDark),
/// White (HC-Dark), Black (HC-Light).
/// Labels are drawn above each stream at the left/right edge anchors.
/// </para>
/// <para>
/// <b>Width scaling (proportional):</b>
/// OthersDiscovered = TotalDiscovered - Insects - Plants - Vertebrates.
/// OthersExtinct    = TotalExtinct    - Insects - Vertebrates.
/// MinValue = smallest non-zero value across all 7 counts.
/// Factor = 100 / max(sum-of-inflow-ratios, sum-of-outflow-ratios).
/// Width = clamp(round(ratio * factor), MinWidth=4, MaxWidth=40).
/// </para>
/// <para>
/// <b>Y-position layout:</b>
/// Streams stacked top-to-bottom with StreamGap=20 px between each pair.
/// Each stack is centred vertically in the canvas.
/// </para>
/// </summary>
[AIContext("UIPresentation")]
internal sealed class TaxonomyFlowDrawable : IDrawable
{
    private const float MinWidth          = 4f;
    private const float MaxWidth          = 40f;
    private const float StreamGap         = 20f;
    private const float LabelFontSize     = 8.5f;
    private const float LabelOffsetAbove  = 3f;

    // Fixed, scheme-independent stream colours (70% alpha for organic overlaps).
    private static readonly Color ColInsIn  = Color.FromArgb("#FFFF00").WithAlpha(0.70f);
    private static readonly Color ColPltIn  = Color.FromArgb("#00FF00").WithAlpha(0.70f);
    private static readonly Color ColVrtIn  = Color.FromArgb("#0000FF").WithAlpha(0.70f);
    private static readonly Color ColOthIn  = Color.FromArgb("#00FFFF").WithAlpha(0.70f);
    private static readonly Color ColInsOut = Color.FromArgb("#C0C0C0").WithAlpha(0.70f);
    private static readonly Color ColVrtOut = Color.FromArgb("#808000").WithAlpha(0.70f);
    private static readonly Color ColOthOut = Color.FromArgb("#808080").WithAlpha(0.70f);

    internal double TotalDiscovered       { get; set; }
    internal double TotalExtinct          { get; set; }
    internal double InsectsDiscovered     { get; set; }
    internal double PlantsDiscovered      { get; set; }
    internal double VertebratesDiscovered { get; set; }
    internal double InsectsExtinct        { get; set; }
    internal double VertebratesExtinct    { get; set; }

    /// <summary>
    /// Localised stream labels set by code-behind from <c>AppResources</c>.
    /// Index order: 0=InsectsIn, 1=PlantsIn, 2=VertsIn, 3=OthersIn.
    /// </summary>
    internal string[] InLabels  { get; set; } = { "Insects/invertebrates", "Plants", "Vertebrates", "Others" };

    /// <summary>
    /// Localised stream labels set by code-behind from <c>AppResources</c>.
    /// Index order: 0=InsectsOut, 1=VertsOut, 2=OthersOut.
    /// </summary>
    internal string[] OutLabels { get; set; } = { "Invertebrates", "Vertebrates", "Others" };

    /// <summary>
    /// Computes proportional stroke widths for all streams.
    /// Returns widths in order: InsIn, PltIn, VrtIn, OthIn, InsOut, VrtOut, OthOut.
    /// </summary>
    internal static float[] ComputeWidths(
        double insIn, double pltIn, double vrtIn, double othIn,
        double insOut, double vrtOut, double othOut)
    {
        double[] vals = { insIn, pltIn, vrtIn, othIn, insOut, vrtOut, othOut };

        double min = double.MaxValue;
        foreach (var v in vals)
            if (v > 0 && v < min) min = v;
        if (min == double.MaxValue || min <= 0) min = 1.0;

        double[] ratios = new double[7];
        for (int i = 0; i < 7; i++)
            ratios[i] = vals[i] > 0 ? vals[i] / min : 0.0;

        double sumIn  = ratios[0] + ratios[1] + ratios[2] + ratios[3];
        double sumOut = ratios[4] + ratios[5] + ratios[6];
        double maxSum = Math.Max(sumIn > 0 ? sumIn : 1.0, sumOut > 0 ? sumOut : 1.0);
        double factor = 100.0 / maxSum;

        var widths = new float[7];
        for (int i = 0; i < 7; i++)
            widths[i] = (float)Math.Clamp(Math.Round(ratios[i] * factor), MinWidth, MaxWidth);

        return widths;
    }

    /// <summary>
    /// Computes the Y centre positions for a stack of streams, centred within
    /// <paramref name="totalH"/>. Streams ordered top-to-bottom.
    /// </summary>
    private static float[] StackCentres(float[] widths, float totalH)
    {
        float totalUsed = 0;
        for (int i = 0; i < widths.Length; i++)
            totalUsed += widths[i] + (i > 0 ? StreamGap : 0);

        var centres = new float[widths.Length];
        float cursor = (totalH - totalUsed) / 2f;
        for (int i = 0; i < widths.Length; i++)
        {
            centres[i] = cursor + widths[i] / 2f;
            cursor += widths[i] + StreamGap;
        }
        return centres;
    }

    /// <inheritdoc/>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        if (w <= 0 || h <= 0) return;

        float cx = w / 2f;
        float cy = h / 2f;

        double othIn  = Math.Max(0, TotalDiscovered - InsectsDiscovered - PlantsDiscovered - VertebratesDiscovered);
        double othOut = Math.Max(0, TotalExtinct    - InsectsExtinct    - VertebratesExtinct);

        float[] widths = ComputeWidths(
            InsectsDiscovered, PlantsDiscovered, VertebratesDiscovered, othIn,
            InsectsExtinct, VertebratesExtinct, othOut);

        float wInsIn  = widths[0]; float wPltIn  = widths[1];
        float wVrtIn  = widths[2]; float wOthIn  = widths[3];
        float wInsOut = widths[4]; float wVrtOut = widths[5]; float wOthOut = widths[6];

        float[] inC  = StackCentres(new[] { wInsIn, wPltIn, wVrtIn, wOthIn }, h);
        float[] outC = StackCentres(new[] { wInsOut, wVrtOut, wOthOut }, h);

        float yInsIn  = inC[0]; float yPltIn  = inC[1];
        float yVrtIn  = inC[2]; float yOthIn  = inC[3];
        float yInsOut = outC[0]; float yVrtOut = outC[1]; float yOthOut = outC[2];

        float cpInX  = w * 0.35f;
        float cpOutX = w * 0.65f;

        // Scheme-aware label colour.
        var res = Application.Current?.Resources;
        Color bg = GetColor(res, "SpaceDarker", Color.FromArgb("#060812"));
        Color labelColour = bg == Colors.Black  ? Colors.White.WithAlpha(0.90f)
                          : bg == Colors.White  ? Colors.Black.WithAlpha(0.90f)
                          : GetColor(res, "TextDim", Color.FromArgb("#E5E5E5")).WithAlpha(0.90f);

        canvas.Antialias     = true;
        canvas.StrokeLineCap = LineCap.Round;

        // Draw streams.
        DrawStream(canvas, 0,  yInsIn,  cx, cy, cpInX, yInsIn,  cpInX, cy, wInsIn,  ColInsIn);
        DrawStream(canvas, 0,  yPltIn,  cx, cy, cpInX, yPltIn,  cpInX, cy, wPltIn,  ColPltIn);
        DrawStream(canvas, 0,  yVrtIn,  cx, cy, cpInX, yVrtIn,  cpInX, cy, wVrtIn,  ColVrtIn);
        DrawStream(canvas, 0,  yOthIn,  cx, cy, cpInX, yOthIn,  cpInX, cy, wOthIn,  ColOthIn);
        DrawStream(canvas, cx, cy, w, yInsOut, cpOutX, cy, cpOutX, yInsOut, wInsOut, ColInsOut);
        DrawStream(canvas, cx, cy, w, yVrtOut, cpOutX, cy, cpOutX, yVrtOut, wVrtOut, ColVrtOut);
        DrawStream(canvas, cx, cy, w, yOthOut, cpOutX, cy, cpOutX, yOthOut, wOthOut, ColOthOut);

        // Stream labels above each anchor.
        float labelW = cx * 0.88f;
        canvas.FontSize  = LabelFontSize;
        canvas.FontColor = labelColour;

        float[]  inY      = { yInsIn, yPltIn, yVrtIn, yOthIn };
        float[]  inW      = { wInsIn, wPltIn, wVrtIn, wOthIn };
        for (int i = 0; i < 4; i++)
        {
            float labelY = inY[i] - inW[i] / 2f - LabelFontSize - LabelOffsetAbove;
            canvas.DrawString(InLabels[i], 2f, labelY, labelW, LabelFontSize + 2f,
                              HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        float[]  outY      = { yInsOut, yVrtOut, yOthOut };
        float[]  outW      = { wInsOut, wVrtOut, wOthOut };
        for (int i = 0; i < 3; i++)
        {
            float labelY = outY[i] - outW[i] / 2f - LabelFontSize - LabelOffsetAbove;
            canvas.DrawString(OutLabels[i], cx + 2f, labelY, labelW, LabelFontSize + 2f,
                              HorizontalAlignment.Right, VerticalAlignment.Top);
        }
    }

    private static void DrawStream(
        ICanvas canvas,
        float x0, float y0, float x1, float y1,
        float cp1x, float cp1y, float cp2x, float cp2y,
        float thickness, Color color)
    {
        canvas.StrokeColor = color;
        canvas.StrokeSize  = thickness;
        var path = new PathF();
        path.MoveTo(x0, y0);
        path.CurveTo(cp1x, cp1y, cp2x, cp2y, x1, y1);
        canvas.DrawPath(path);
    }

    private static Color GetColor(ResourceDictionary? res, string key, Color fallback)
    {
        if (res != null && res.TryGetValue(key, out var raw) && raw is Color c) return c;
        return fallback;
    }
}