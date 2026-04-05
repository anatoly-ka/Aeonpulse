using Aeonpulse.Attributes;
using Microsoft.Maui.Graphics;

namespace Aeonpulse.Views;

/// <summary>
/// <see cref="IDrawable"/> for the Volumetric Cube visualizer in the Your Breath
/// expanded card view. Rendered by <c>GraphicsView x:Name="VolumeCubeView"</c>
/// (HeightRequest=250), layered inside a <c>Grid</c> together with
/// <c>Image x:Name="LandmarkImage"</c>.
///
/// <para>
/// <b>Visual concept:</b>
/// An isometric 2-D projection of a cube whose volume equals the user total
/// exhaled air. A separate XAML Image (tinted via helpers:ImageTint.Color) shows a
/// landmark at the same pixel-per-metre scale so the cube size is viscerally comparable.
/// </para>
/// <para>
/// <b>Isometric geometry (standard 30-degree projection):</b>
/// Axis vectors in screen coordinates (Y-down):
///   isoRight = (+s*sqrt3/2, -s/2),  isoLeft = (-s*sqrt3/2, -s/2),  isoUp = (0, -s).
/// Bottom-front vertex F anchored at canvas base (h - 4 px).
/// Total iso height = 2s, width = s*sqrt3.
/// Three visible faces: Right (45% alpha), Left (28% alpha), Top (60% alpha).
/// </para>
/// <para>
/// <b>Output properties:</b>
/// LastPpm and LastAnchorY are written during Draw so ApplyVolumeCube in
/// MainPage.xaml.cs can compute the landmark image height and bottom alignment.
/// </para>
/// <para>
/// <b>Colour strategy:</b> resolved at draw time via SpaceDarker discriminator.
/// DefaultDark: CyberCyan tinted faces. HC-Dark: white. HC-Light: dark slate.
/// </para>
/// </summary>
[AIContext("UIPresentation")]
internal sealed class VolumeCubeDrawable : IDrawable
{
    private const float Sqrt3 = 1.7320508f;

    internal double CubeEdgeMeters { get; set; }

    /// <summary>Pixels-per-metre used in the last Draw call.</summary>
    internal float LastPpm { get; private set; }

    /// <summary>Canvas Y of the cube base anchor used in the last Draw call.</summary>
    internal float LastAnchorY { get; private set; }

    /// <inheritdoc/>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        if (w <= 0 || h <= 0 || CubeEdgeMeters <= 0) return;

        // Dynamic ppm: fit cube (iso height = 2s) in 80% of canvas height.
        float maxCubeH = h * 0.80f;
        float ppm      = (float)(maxCubeH / (CubeEdgeMeters * 2.0));
        ppm = Math.Clamp(ppm, 3f, 280f);
        LastPpm = ppm;

        float s     = (float)(CubeEdgeMeters * ppm);
        float cubeW = s * Sqrt3;

        // Cube base anchor: bottom centre of canvas, shifted right so the
        // landmark image has room on the left side.
        float anchorY = h - 4f;
        LastAnchorY   = anchorY;
        float cubeX   = Math.Max(w * 0.55f, (w + cubeW * 0.3f) / 2f);
        float cubeY   = anchorY;

        // Iso axis vectors.
        float rxX =  s * Sqrt3 / 2f;   float rxY = -s / 2f;
        float lxX = -s * Sqrt3 / 2f;   float lxY = -s / 2f;

        // Resolve colours via SpaceDarker discriminator.
        var res = Application.Current?.Resources;
        Color bg = GetColor(res, "SpaceDarker", Color.FromArgb("#060812"));

        Color faceRight, faceLeft, faceTop, stroke;
        if (bg == Colors.Black)
        {
            faceRight = Colors.White.WithAlpha(0.50f);
            faceLeft  = Colors.White.WithAlpha(0.30f);
            faceTop   = Colors.White.WithAlpha(0.65f);
            stroke    = Colors.White.WithAlpha(0.80f);
        }
        else if (bg == Colors.White)
        {
            faceRight = Color.FromArgb("#1A2A5E").WithAlpha(0.50f);
            faceLeft  = Color.FromArgb("#1A2A5E").WithAlpha(0.30f);
            faceTop   = Color.FromArgb("#1A2A5E").WithAlpha(0.65f);
            stroke    = Color.FromArgb("#1A2A5E").WithAlpha(0.90f);
        }
        else
        {
            Color base_ = GetColor(res, "CyberCyan", Color.FromArgb("#00E5FF"));
            faceRight = base_.WithAlpha(0.45f);
            faceLeft  = base_.WithAlpha(0.28f);
            faceTop   = base_.WithAlpha(0.60f);
            stroke    = base_.WithAlpha(0.90f);
        }

        // 7 visible cube vertices.
        float fX   = cubeX;        float fY   = cubeY;
        float frX  = fX + rxX;     float frY  = fY + rxY;
        float flX  = fX + lxX;     float flY  = fY + lxY;
        float frtX = frX;          float frtY = frY - s;
        float fltX = flX;          float fltY = flY - s;
        float bX   = fX;           float bY   = fY - s;
        float tX   = fX;           float tY   = fY - 2 * s;

        // Right face: F, FR, FR_top, B
        canvas.FillColor   = faceRight;
        canvas.StrokeColor = stroke;
        canvas.StrokeSize  = 1.0f;
        var rightFace = new PathF();
        rightFace.MoveTo(fX, fY); rightFace.LineTo(frX, frY);
        rightFace.LineTo(frtX, frtY); rightFace.LineTo(bX, bY);
        rightFace.Close();
        canvas.FillPath(rightFace);
        canvas.DrawPath(rightFace);

        // Left face: F, FL, FL_top, B
        canvas.FillColor = faceLeft;
        var leftFace = new PathF();
        leftFace.MoveTo(fX, fY); leftFace.LineTo(flX, flY);
        leftFace.LineTo(fltX, fltY); leftFace.LineTo(bX, bY);
        leftFace.Close();
        canvas.FillPath(leftFace);
        canvas.DrawPath(leftFace);

        // Top face: B, FR_top, T, FL_top
        canvas.FillColor = faceTop;
        var topFace = new PathF();
        topFace.MoveTo(bX, bY); topFace.LineTo(frtX, frtY);
        topFace.LineTo(tX, tY); topFace.LineTo(fltX, fltY);
        topFace.Close();
        canvas.FillPath(topFace);
        canvas.DrawPath(topFace);

        // Front-bottom vertical edge.
        canvas.StrokeColor = stroke;
        canvas.StrokeSize  = 1.2f;
        canvas.DrawLine(fX, fY, bX, bY);
    }

    private static Color GetColor(ResourceDictionary? res, string key, Color fallback)
    {
        if (res != null && res.TryGetValue(key, out var raw) && raw is Color c) return c;
        return fallback;
    }
}