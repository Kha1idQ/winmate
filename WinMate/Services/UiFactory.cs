using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using WinMate.Data;
using Wpf.Ui.Controls;

namespace WinMate.Services;

// Small builders for the repeating pieces of the design system: status chips
// and the rounded, tinted icon tiles. Every brush comes from a DynamicResource
// so both themes work without rebuilding anything.
public static class UiFactory
{
    // Builds an icon from the WinMate icon pack. Colour comes from a theme brush
    // (brushKey) or, when null, is inherited from the parent's Foreground — which
    // is what lets navigation icons follow their selected/hover state.
    public static FrameworkElement Icon(string name, double size = 22, string? brushKey = "AccentBrush")
    {
        // The 1.8px stroke straddles the path, so half of it falls outside the
        // 24x24 box. Draw on a 26x26 surface offset by 1 so nothing gets clipped.
        const double pad = 1;
        var canvas = new Canvas { Width = 24 + pad * 2, Height = 24 + pad * 2 };

        foreach (var shape in IconData.All[name])
        {
            var path = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(shape.Data),
                StrokeThickness = 1.8,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
            };
            Canvas.SetLeft(path, pad);
            Canvas.SetTop(path, pad);

            var property = shape.Filled
                ? System.Windows.Shapes.Shape.FillProperty
                : System.Windows.Shapes.Shape.StrokeProperty;

            if (brushKey is not null)
                path.SetResourceReference(property, brushKey);
            else
                path.SetBinding(property, new Binding(nameof(Control.Foreground))
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Control), 1),
                });

            canvas.Children.Add(path);
        }

        return new Viewbox
        {
            Width = size,
            Height = size,
            Child = canvas,
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            // Icons are drawn artwork, not text — never mirror them in Arabic.
            FlowDirection = FlowDirection.LeftToRight,
        };
    }

    // Compact chip: "8 tweaks", "Installed", "Restart required", "Advanced".
    // accent = "Accent" | "AccentAlt" | "Success"
    public static Border Chip(string textResourceKey, string accent = "Accent", string? literalText = null)
    {
        var text = new System.Windows.Controls.TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (literalText is not null)
            text.Text = literalText;
        else
            text.SetResourceReference(System.Windows.Controls.TextBlock.TextProperty, textResourceKey);
        text.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, $"{accent}Brush");

        var chip = new Border
        {
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 3, 8, 3),
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
            Child = text,
        };
        chip.SetResourceReference(Border.BackgroundProperty, $"{accent}TintBrush");
        chip.SetResourceReference(Border.BorderBrushProperty, $"{accent}BorderBrush");
        return chip;
    }

    // The pack's ready-made gradient tiles (96x96), used for the profile cards.
    public static FrameworkElement ProfileTile(string tileName, double size = 64)
    {
        var tile = TileData.All[tileName];
        const double pad = 2; // room for the 2px outer stroke and 4px glyph strokes
        var canvas = new Canvas { Width = 96 + pad * 2, Height = 96 + pad * 2 };

        var gradient = new LinearGradientBrush(
            [new GradientStop(ParseColor(tile.From), 0), new GradientStop(ParseColor(tile.To), 1)],
            new Point(8.0 / 96.0, 6.0 / 96.0),
            new Point(86.0 / 96.0, 92.0 / 96.0));

        foreach (var shape in tile.Shapes)
        {
            var path = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(shape.Data),
                StrokeThickness = shape.StrokeWidth,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Fill = shape.Fill switch
                {
                    null => null,
                    "GRAD" => gradient,
                    var hex => new SolidColorBrush(ParseColor(hex)),
                },
                Stroke = shape.Stroke is null ? null : new SolidColorBrush(ParseColor(shape.Stroke)),
            };
            Canvas.SetLeft(path, pad);
            Canvas.SetTop(path, pad);
            canvas.Children.Add(path);
        }

        return new Viewbox
        {
            Width = size,
            Height = size,
            Child = canvas,
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            FlowDirection = FlowDirection.LeftToRight,
        };
    }

    // Pack icon wrapped as a WPF-UI IconElement, so it can be handed to controls
    // that only accept one (Button.Icon, TextBox.Icon, NavigationViewItem.Icon).
    // It draws with the live theme brush, so it recolours on a theme switch.
    public static IconElement IconElement(string name, string brushKey = "AccentBrush", double size = 20)
    {
        var brush = (Brush)Application.Current.Resources[brushKey];
        var group = new DrawingGroup();

        // A DrawingImage sizes itself to the tight bounds of what it contains, so
        // an icon that doesn't fill its 24x24 box (the play triangle spans only
        // 8..19) would be blown up and clipped. This invisible frame pins every
        // icon to the same 26x26 space: consistent scale, nothing cut off.
        group.Children.Add(new GeometryDrawing
        {
            Geometry = new RectangleGeometry(new Rect(-1, -1, 26, 26)),
            Brush = Brushes.Transparent,
        });

        foreach (var shape in IconData.All[name])
        {
            var geometry = Geometry.Parse(shape.Data);
            group.Children.Add(new GeometryDrawing
            {
                Geometry = geometry,
                Brush = shape.Filled ? brush : null,
                Pen = shape.Filled
                    ? null
                    : new Pen(brush, 1.8)
                    {
                        StartLineCap = PenLineCap.Round,
                        EndLineCap = PenLineCap.Round,
                        LineJoin = PenLineJoin.Round,
                    },
            });
        }

        return new ImageIcon
        {
            Source = new DrawingImage(group),
            Width = size,
            Height = size,
            FlowDirection = FlowDirection.LeftToRight,
        };
    }

    private static Color ParseColor(string hex) => (Color)ColorConverter.ConvertFromString(hex);

    // Rounded tinted square holding an icon from the pack.
    public static Border IconTile(string iconName, string accent = "Accent", double size = 56, double iconSize = 26)
    {
        var icon = Icon(iconName, iconSize, $"{accent}Brush");
        icon.HorizontalAlignment = HorizontalAlignment.Center;
        icon.VerticalAlignment = VerticalAlignment.Center;

        var tile = new Border
        {
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
            Child = icon,
        };
        tile.SetResourceReference(Border.BackgroundProperty, $"{accent}TintBrush");
        tile.SetResourceReference(Border.BorderBrushProperty, $"{accent}BorderBrush");
        return tile;
    }

    // Standard content card: surface fill, 1px cool border, 12px radius.
    public static Border Card(UIElement content, double minHeight = 72)
    {
        var card = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(20, 16, 20, 16),
            Margin = new Thickness(0, 0, 0, 12),
            MinHeight = minHeight,
            Child = content,
        };
        card.SetResourceReference(Border.BackgroundProperty, "SurfaceBrush");
        card.SetResourceReference(Border.BorderBrushProperty, "CardBorderBrush");
        return card;
    }

    public static System.Windows.Controls.TextBlock Title(string text, double size = 18)
    {
        var t = new System.Windows.Controls.TextBlock
        {
            Text = text,
            FontSize = size,
            FontWeight = FontWeights.SemiBold,
        };
        t.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "TextPrimaryBrush");
        return t;
    }

    public static System.Windows.Controls.TextBlock Description(string text, double size = 14)
    {
        var t = new System.Windows.Controls.TextBlock
        {
            Text = text,
            FontSize = size,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 3, 0, 0),
        };
        t.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "TextSecondaryBrush");
        return t;
    }

    // Filled primary action button (gradient fill, high-contrast label).
    public static Wpf.Ui.Controls.Button PrimaryButton(string iconName, string? accentBrushKey = null)
    {
        var button = new Wpf.Ui.Controls.Button
        {
            Icon = IconElement(iconName, "OnPrimaryBrush"),
            Height = 44,
            Padding = new Thickness(18, 0, 18, 0),
            VerticalAlignment = VerticalAlignment.Center,
            BorderThickness = new Thickness(0),
        };
        button.SetResourceReference(Control.BackgroundProperty, accentBrushKey ?? "PrimaryButtonBrush");
        button.SetResourceReference(Control.ForegroundProperty, "OnPrimaryBrush");
        return button;
    }
}
