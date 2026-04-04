using Aeonpulse.Attributes;
using Microsoft.Maui.Graphics;

namespace Aeonpulse.Views;

/// <summary>
/// <see cref="IDrawable"/> for the Pythagorean Enneagram visualization in the
/// Personal Year expanded card view.
///
/// <para>
/// <b>Geometry:</b>
/// Nine nodes are placed evenly around a circle (radius = shorter canvas
/// half minus padding). Node 1 is at the top (angle -90 degrees = 270 degrees).
/// Angle step = 40 degrees (360 / 9).
/// </para>
/// <para>
/// <b>Inner figures drawn as part of the skeleton:</b>
/// <list type="bullet">
///   <item>Outer circle connecting all 9 nodes.</item>
///   <item>{9/4} nonagram: each node connected to the node 4 steps ahead
///         (1-5-9-4-8-3-7-2-6-1), forming the classic Enneagram star.</item>
///   <item>{9/3} equilateral triangle: nodes 3-6-9-3.</item>
/// </list>
/// </para>
/// <para>
/// <b>Drawing passes:</b>
/// 1. Skeleton (dim, thin): outer circle + {9/4} star + {9/3} triangle.
/// 2. Node circles (small, dim) at all 9 positions.
/// 3. Number labels 1-9 drawn outside each node.
/// 4. Active highlight: larger filled circle in accent colour at the node
///    matching <c>PersonalYear</c> (1-9).
/// 5. Active label: number redrawn in accent colour over the highlight.
/// </para>
/// <para>
/// <b>Theming:</b> colours are read from <c>Application.Current.Resources</c>
/// at draw time. SpaceDarker discriminates DefaultDark / HC-Dark / HC-Light.
/// DefaultDark: TextGray 25% skeleton, TextDim nodes/labels, JubileeAccent highlight.
/// HC-Dark: white 30% skeleton, white nodes/labels, white highlight.
/// HC-Light: black 30% skeleton, black nodes/labels, black highlight.
/// </para>
/// </summary>
[AIContext("UIPresentation")]
internal sealed class EnneagramDrawable : IDrawable
{
    private readonly int _personalYear;   // 1-9

    private const double DegToRad = Math.PI / 180.0;

    internal EnneagramDrawable(int personalYear)
    {
        _personalYear = Math.Clamp(personalYear, 1, 9);
    }

    /// <inheritdoc/>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        if (w <= 0 || h <= 0) return;

        float cx  = w / 2f;
        float cy  = h / 2f;
        float pad = Math.Min(w, h) * 0.14f;
        float r   = Math.Min(w, h) / 2f - pad;      // outer circle radius
        if (r <= 0) return;

        // Resolve colours.
        var res = Application.Current?.Resources;
        Color bg = GetColor(res, "SpaceDarker", Color.FromArgb("#060812"));

        Color skelColor;
        Color nodeColor;
        Color labelColor;
        Color accentColor;

        if (bg == Colors.Black)
        {
            skelColor   = Colors.White.WithAlpha(0.30f);
            nodeColor   = Colors.White.WithAlpha(0.60f);
            labelColor  = Colors.White;
            accentColor = Colors.White;
        }
        else if (bg == Colors.White)
        {
            skelColor   = Colors.Black.WithAlpha(0.30f);
            nodeColor   = Colors.Black.WithAlpha(0.60f);
            labelColor  = Colors.Black;
            accentColor = Colors.Black;
        }
        else
        {
            skelColor   = GetColor(res, "TextGray",     Color.FromArgb("#B0B0B0")).WithAlpha(0.25f);
            nodeColor   = GetColor(res, "TextGray",     Color.FromArgb("#B0B0B0")).WithAlpha(0.60f);
            labelColor  = GetColor(res, "TextDim",      Color.FromArgb("#E5E5E5"));
            accentColor = GetColor(res, "JubileeAccent",Color.FromArgb("#FFD700"));
        }

        // Compute all 9 node positions (1-indexed; node 1 at top = -90 deg).
        // nodes[i] gives the canvas position for node number (i+1).
        var nodes = new PointF[9];
        for (int i = 0; i < 9; i++)
        {
            double angleDeg = -90.0 + i * 40.0;
            double rad      = angleDeg * DegToRad;
            nodes[i] = new PointF(
                cx + (float)(r * Math.Cos(rad)),
                cy + (float)(r * Math.Sin(rad)));
        }

        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        // 1a. Skeleton: outer circle.
        canvas.StrokeColor = skelColor;
        canvas.StrokeSize  = 1.0f;
        canvas.DrawCircle(cx, cy, r);

        // 1b. Skeleton: {9/4} nonagram - connect each node to node+4 (mod 9).
        //     Traversal: 0->4->8->3->7->2->6->1->5->0 (0-based), i.e. 1->5->9->4->8->3->7->2->6->1.
        canvas.StrokeSize = 1.0f;
        for (int i = 0; i < 9; i++)
        {
            int j = (i + 4) % 9;
            canvas.DrawLine(nodes[i], nodes[j]);
        }

        // 1c. Skeleton: {9/3} equilateral triangle - nodes 3, 6, 9 (0-based: 2, 5, 8).
        canvas.DrawLine(nodes[2], nodes[5]);
        canvas.DrawLine(nodes[5], nodes[8]);
        canvas.DrawLine(nodes[8], nodes[2]);

        // 2. Node circles (small, dim) at all 9 positions.
        float nodeR = r * 0.085f;
        canvas.StrokeColor = nodeColor;
        canvas.StrokeSize  = 1.0f;
        canvas.FillColor   = GetColor(res, "CardDark", Color.FromArgb("#121628"));
        for (int i = 0; i < 9; i++)
        {
            canvas.FillCircle(nodes[i].X, nodes[i].Y, nodeR);
            canvas.DrawCircle(nodes[i].X, nodes[i].Y, nodeR);
        }

        // 3. Number labels 1-9, placed slightly outside the node circles.
        float labelR    = r + nodeR * 2.2f + 2f;
        float labelSize = Math.Max(9f, r * 0.16f);
        canvas.FontSize   = labelSize;
        canvas.FontColor  = labelColor;
        for (int i = 0; i < 9; i++)
        {
            double angleDeg = -90.0 + i * 40.0;
            double rad      = angleDeg * DegToRad;
            float  lx       = cx + (float)(labelR * Math.Cos(rad));
            float  ly       = cy + (float)(labelR * Math.Sin(rad));
            string num      = (i + 1).ToString();
            canvas.DrawString(num, lx - labelSize, ly - labelSize,
                              labelSize * 2, labelSize * 2,
                              HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        // 4. Active highlight: filled accent circle at the active node.
        int    activeIdx    = _personalYear - 1;   // 0-based
        float  highlightR   = nodeR * 1.6f;
        canvas.FillColor    = accentColor;
        canvas.StrokeColor  = accentColor;
        canvas.StrokeSize   = 2.0f;
        canvas.FillCircle(nodes[activeIdx].X, nodes[activeIdx].Y, highlightR);

        // 5. Active label: redraw the number over the highlight in contrasting colour.
        //    Use CardDark for DefaultDark/HC-Light (gold/black bg), White for HC-Dark.
        Color activeNumColor = (bg == Colors.Black)
            ? Colors.Black
            : (bg == Colors.White ? Colors.White : GetColor(res, "CardDark", Color.FromArgb("#121628")));
        canvas.FontColor = activeNumColor;
        canvas.FontSize  = labelSize * 1.05f;
        string activeNum = _personalYear.ToString();
        canvas.DrawString(activeNum,
                          nodes[activeIdx].X - labelSize,
                          nodes[activeIdx].Y - labelSize,
                          labelSize * 2, labelSize * 2,
                          HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static Color GetColor(ResourceDictionary? res, string key, Color fallback)
    {
        if (res != null && res.TryGetValue(key, out var raw) && raw is Color c)
            return c;
        return fallback;
    }
}
