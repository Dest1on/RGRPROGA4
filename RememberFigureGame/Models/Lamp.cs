using System.Drawing;

namespace RememberFigureGame.Models;

/// <summary>
/// Описание лампочки на игровом поле.
/// </summary>
public sealed class Lamp
{
    /// <summary>
    /// Создаёт лампочку.
    /// </summary>
    public Lamp(
        int id,
        PointF center,
        float radius,
        Color activeColor,
        Color inactiveColor)
    {
        Id = id;
        Center = center;
        Radius = radius;
        ActiveColor = activeColor;
        InactiveColor = inactiveColor;
    }

    /// <summary>
    /// Уникальный идентификатор лампочки.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Центр лампочки.
    /// </summary>
    public PointF Center { get; set; }

    /// <summary>
    /// Радиус лампочки.
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// Цвет активного состояния.
    /// </summary>
    public Color ActiveColor { get; set; }

    /// <summary>
    /// Цвет неактивного состояния.
    /// </summary>
    public Color InactiveColor { get; set; }

    /// <summary>
    /// Признак активной вспышки.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Время (UTC), когда активная вспышка должна закончиться.
    /// </summary>
    public DateTime ActiveUntilUtc { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Текущий цвет для отрисовки.
    /// </summary>
    public Color CurrentColor
    {
        get
        {
            return IsActive ? ActiveColor : InactiveColor;
        }
    }

    /// <summary>
    /// Границы лампочки.
    /// </summary>
    public RectangleF Bounds
    {
        get
        {
            float diameter = Radius * 2F;
            return new RectangleF(Center.X - Radius, Center.Y - Radius, diameter, diameter);
        }
    }

    /// <summary>
    /// Активирует лампочку.
    /// </summary>
    public void Activate(DateTime activeUntilUtc)
    {
        IsActive = true;
        ActiveUntilUtc = activeUntilUtc;
    }

    /// <summary>
    /// Деактивирует лампочку.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ActiveUntilUtc = DateTime.MinValue;
    }

    /// <summary>
    /// Проверяет, попадает ли точка в лампочку.
    /// </summary>
    public bool Contains(Point point)
    {
        float dx = point.X - Center.X;
        float dy = point.Y - Center.Y;
        return (dx * dx) + (dy * dy) <= Radius * Radius;
    }

    /// <summary>
    /// Создаёт копию лампочки.
    /// </summary>
    public Lamp Clone()
    {
        Lamp clone = new Lamp(Id, Center, Radius, ActiveColor, InactiveColor);
        if (IsActive)
        {
            clone.Activate(ActiveUntilUtc);
        }

        return clone;
    }
}
