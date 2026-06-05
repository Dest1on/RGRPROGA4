using System.Drawing;
using RememberFigureGame.Models;

namespace RememberFigureGame.Services;

/// <summary>
/// Строит равномерное расположение лампочек в игровом поле.
/// </summary>
public sealed class LampLayoutService
{
    private const float RadiusFactor = 0.30F;
    private const float MinRadius = 4F;
    private const float BorderPadding = 2F;

    /// <summary>
    /// Создаёт набор лампочек, равномерно размещённых внутри прямоугольника поля.
    /// </summary>
    public IReadOnlyList<Lamp> CreateLayout(GameSettings settings, Rectangle fieldBounds)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (fieldBounds.Width <= 0 || fieldBounds.Height <= 0)
        {
            return Array.Empty<Lamp>();
        }

        double aspectRatio = fieldBounds.Width / (double)fieldBounds.Height;
        int columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(settings.LampCount * aspectRatio)));
        int rows = Math.Max(1, (int)Math.Ceiling(settings.LampCount / (double)columns));

        float spacingX = fieldBounds.Width / (columns + 1F);
        float spacingY = fieldBounds.Height / (rows + 1F);

        float radius = MathF.Min(spacingX, spacingY) * RadiusFactor;
        float maxRadius = MathF.Max(MinRadius, MathF.Min(spacingX, spacingY) / 2F - BorderPadding);
        radius = MathF.Max(MinRadius, MathF.Min(radius, maxRadius));

        List<Lamp> lamps = new List<Lamp>(settings.LampCount);

        int currentId = 0;
        for (int row = 0; row < rows && currentId < settings.LampCount; row++)
        {
            for (int column = 0; column < columns && currentId < settings.LampCount; column++)
            {
                float centerX = fieldBounds.Left + spacingX * (column + 1);
                float centerY = fieldBounds.Top + spacingY * (row + 1);

                Lamp lamp = new Lamp(
                    currentId,
                    new PointF(centerX, centerY),
                    radius,
                    settings.ActiveLampColor,
                    settings.InactiveLampColor);

                lamps.Add(lamp);
                currentId++;
            }
        }

        return lamps;
    }
}
