using System.Windows;
using System.Windows.Media;
using WinMate.Services;

namespace WinMate.Views.Controls;

// A cheap, elegant "floating gold dust" layer: a few dozen soft gold specks
// drifting slowly upward with a gentle sway. Rendered behind all content and
// non-interactive, so it adds depth without costing responsiveness.
public class ParticleBackground : FrameworkElement
{
    private sealed class Particle
    {
        public double X, Y, Radius, SpeedY, SwayAmp, SwayFreq, Phase;
        public Brush Brush = Brushes.Transparent;
    }

    private const int Count = 46;

    private readonly List<Particle> _particles = [];
    private readonly Random _rng = new();
    private TimeSpan _lastTime;
    private double _elapsed;
    private bool _ready;

    public ParticleBackground()
    {
        IsHitTestVisible = false;
        Loaded += (_, _) =>
        {
            CompositionTarget.Rendering += OnFrame;
            ThemeService.ThemeChanged += Seed;
        };
        Unloaded += (_, _) =>
        {
            CompositionTarget.Rendering -= OnFrame;
            ThemeService.ThemeChanged -= Seed;
        };
        SizeChanged += (_, _) => Seed();
    }

    private void Seed()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
            return;

        _particles.Clear();
        for (var i = 0; i < Count; i++)
            _particles.Add(NewParticle(randomY: true));
        _ready = true;
    }

    private Particle NewParticle(bool randomY)
    {
        var opacity = 0.05 + _rng.NextDouble() * 0.30;
        var c = ThemeService.ParticleColor;
        var brush = new SolidColorBrush(Color.FromArgb((byte)(opacity * 255), c.R, c.G, c.B));
        brush.Freeze();
        return new Particle
        {
            X = _rng.NextDouble() * ActualWidth,
            Y = randomY ? _rng.NextDouble() * ActualHeight : ActualHeight + 6,
            Radius = 0.8 + _rng.NextDouble() * 1.9,
            SpeedY = 6 + _rng.NextDouble() * 16,           // px per second, upward
            SwayAmp = 4 + _rng.NextDouble() * 14,
            SwayFreq = 0.2 + _rng.NextDouble() * 0.5,
            Phase = _rng.NextDouble() * Math.PI * 2,
            Brush = brush,
        };
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!_ready)
            return;

        var args = (RenderingEventArgs)e;
        var dt = _lastTime == TimeSpan.Zero ? 0.016 : (args.RenderingTime - _lastTime).TotalSeconds;
        _lastTime = args.RenderingTime;
        if (dt <= 0 || dt > 0.2)
            dt = 0.016; // clamp after stalls so particles don't teleport
        _elapsed += dt;

        foreach (var p in _particles)
        {
            p.Y -= p.SpeedY * dt;
            if (p.Y < -p.Radius)
            {
                // Respawn at the bottom in a new column.
                p.Y = ActualHeight + p.Radius;
                p.X = _rng.NextDouble() * ActualWidth;
            }
        }

        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (!_ready)
            return;

        foreach (var p in _particles)
        {
            var x = p.X + Math.Sin(_elapsed * p.SwayFreq + p.Phase) * p.SwayAmp;
            dc.DrawEllipse(p.Brush, null, new Point(x, p.Y), p.Radius, p.Radius);
        }
    }
}
