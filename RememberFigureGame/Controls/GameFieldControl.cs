using System.Drawing.Drawing2D;
using RememberFigureGame.Models;

namespace RememberFigureGame.Controls;

/// <summary>
/// Отрисовывает игровое поле и лампочки, а также передаёт клики по полю.
/// </summary>
public sealed class GameFieldControl : Control
{
    private IReadOnlyList<Lamp> lamps = Array.Empty<Lamp>();

    /// <summary>
    /// Создаёт контрол игрового поля.
    /// </summary>
    public GameFieldControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.FromArgb(30, 30, 35);
        Margin = new Padding(10);
    }

    /// <summary>
    /// Событие клика по игровому полю.
    /// </summary>
    public event EventHandler<GameFieldClickEventArgs>? FieldClicked;

    /// <summary>
    /// Устанавливает снимок лампочек для отрисовки.
    /// </summary>
    public void SetLamps(IReadOnlyList<Lamp> lamps)
    {
        this.lamps = lamps ?? Array.Empty<Lamp>();
        Invalidate();
    }

    /// <inheritdoc />
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using Pen borderPen = new Pen(Color.DimGray, 2F);
        e.Graphics.DrawRectangle(
            borderPen,
            1,
            1,
            Math.Max(0, ClientSize.Width - 2),
            Math.Max(0, ClientSize.Height - 2));

        using Pen outlinePen = new Pen(Color.Black, 1.2F);
        foreach (Lamp lamp in lamps)
        {
            using SolidBrush fillBrush = new SolidBrush(lamp.CurrentColor);
            RectangleF bounds = lamp.Bounds;
            e.Graphics.FillEllipse(fillBrush, bounds);
            e.Graphics.DrawEllipse(outlinePen, bounds);
        }
    }

    /// <inheritdoc />
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        FieldClicked?.Invoke(this, new GameFieldClickEventArgs(e.Location));
    }
}

/// <summary>
/// Аргументы клика по игровому полю.
/// </summary>
public sealed class GameFieldClickEventArgs : EventArgs
{
    /// <summary>
    /// Создаёт аргументы клика.
    /// </summary>
    public GameFieldClickEventArgs(Point location)
    {
        Location = location;
    }

    /// <summary>
    /// Координата клика внутри игрового поля.
    /// </summary>
    public Point Location { get; }
}
