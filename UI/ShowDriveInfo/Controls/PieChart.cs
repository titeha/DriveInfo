using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using ShowDriveInfo.Models;

namespace ShowDriveInfo.Controls
{
  /// <summary>
  /// Круговая диаграмма разбивки текущего уровня по занимаемому месту.
  /// Двойной клик по сектору-каталогу выполняет <see cref="NavigateCommand"/> с этим узлом.
  /// </summary>
  public sealed class PieChart : FrameworkElement
  {
    private readonly record struct Slice(IFileSystemNode Node, double Start, double End);

    private static readonly Brush[] _palette = CreatePalette();
    private static readonly Pen _border = CreatePen();

    private readonly List<Slice> _slices = new();

    public static readonly DependencyProperty ItemsSourceProperty =
      DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<IFileSystemNode>), typeof(PieChart),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnItemsSourceChanged));

    public static readonly DependencyProperty NavigateCommandProperty =
      DependencyProperty.Register(nameof(NavigateCommand), typeof(ICommand), typeof(PieChart));

    public IEnumerable<IFileSystemNode>? ItemsSource
    {
      get => (IEnumerable<IFileSystemNode>?)GetValue(ItemsSourceProperty);
      set => SetValue(ItemsSourceProperty, value);
    }

    public ICommand? NavigateCommand
    {
      get => (ICommand?)GetValue(NavigateCommandProperty);
      set => SetValue(NavigateCommandProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((PieChart)d).InvalidateVisual();

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      _slices.Clear();

      List<IFileSystemNode>? _items = ItemsSource?.Where(_i => _i.OccupiedSize > 0).ToList();
      if (_items is null || _items.Count == 0)
        return;

      double _radius = Math.Min(ActualWidth, ActualHeight) / 2 - 4;
      if (_radius <= 0)
        return;

      var _center = new Point(ActualWidth / 2, ActualHeight / 2);

      ulong _total = 0;
      foreach (IFileSystemNode _item in _items)
        _total += _item.OccupiedSize;

      if (_total == 0)
        return;

      double _start = 0;
      int _color = 0;

      foreach (IFileSystemNode _item in _items)
      {
        double _sweep = 360.0 * _item.OccupiedSize / _total;
        double _end = _start + _sweep;

        drawingContext.DrawGeometry(_palette[_color % _palette.Length], _border, CreateWedge(_center, _radius, _start, _end));
        _slices.Add(new Slice(_item, _start, _end));

        if (_sweep >= 20)
          DrawLabel(drawingContext, _item.Name, _center, _radius, (_start + _end) / 2);

        _start = _end;
        _color++;
      }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      base.OnMouseLeftButtonDown(e);

      if (e.ClickCount != 2)
        return;

      var _center = new Point(ActualWidth / 2, ActualHeight / 2);
      double _radius = Math.Min(ActualWidth, ActualHeight) / 2 - 4;

      Point _p = e.GetPosition(this);
      double _dx = _p.X - _center.X;
      double _dy = _p.Y - _center.Y;

      if (_dx * _dx + _dy * _dy > _radius * _radius)
        return; // клик вне круга

      double _deg = Math.Atan2(_dy, _dx) * 180.0 / Math.PI;
      if (_deg < 0)
        _deg += 360;

      foreach (Slice _slice in _slices)
        if (_deg >= _slice.Start && _deg < _slice.End)
        {
          if (NavigateCommand?.CanExecute(_slice.Node) == true)
            NavigateCommand.Execute(_slice.Node);
          break;
        }
    }

    private static Geometry CreateWedge(Point center, double radius, double startDeg, double endDeg)
    {
      if (endDeg - startDeg >= 360)
        return new EllipseGeometry(center, radius, radius);

      Point _p1 = PointOnCircle(center, radius, startDeg);
      Point _p2 = PointOnCircle(center, radius, endDeg);
      bool _isLarge = endDeg - startDeg > 180;

      var _geometry = new StreamGeometry();
      using (StreamGeometryContext _ctx = _geometry.Open())
      {
        _ctx.BeginFigure(center, true, true);
        _ctx.LineTo(_p1, true, false);
        _ctx.ArcTo(_p2, new Size(radius, radius), 0, _isLarge, SweepDirection.Clockwise, true, false);
      }
      _geometry.Freeze();
      return _geometry;
    }

    private void DrawLabel(DrawingContext drawingContext, string text, Point center, double radius, double midDeg)
    {
      var _formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
        new Typeface("Segoe UI"), 11, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)
      {
        MaxTextWidth = radius,
        MaxLineCount = 1,
        Trimming = TextTrimming.CharacterEllipsis
      };

      Point _at = PointOnCircle(center, radius * 0.6, midDeg);
      drawingContext.DrawText(_formatted, new Point(_at.X - _formatted.Width / 2, _at.Y - _formatted.Height / 2));
    }

    private static Point PointOnCircle(Point center, double radius, double degrees)
    {
      double _rad = degrees * Math.PI / 180.0;
      return new Point(center.X + radius * Math.Cos(_rad), center.Y + radius * Math.Sin(_rad));
    }

    private static Brush[] CreatePalette()
    {
      var _colors = new[]
      {
        "#4E79A7", "#F28E2B", "#E15759", "#76B7B2", "#59A14F", "#EDC948",
        "#B07AA1", "#FF9DA7", "#9C755F", "#BAB0AC", "#86BCB6", "#D37295"
      };

      return _colors.Select(_c =>
      {
        var _brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_c));
        _brush.Freeze();
        return (Brush)_brush;
      }).ToArray();
    }

    private static Pen CreatePen()
    {
      var _pen = new Pen(Brushes.White, 1);
      _pen.Freeze();
      return _pen;
    }
  }
}
