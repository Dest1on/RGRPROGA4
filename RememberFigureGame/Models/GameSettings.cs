using System.Drawing;

namespace RememberFigureGame.Models;

/// <summary>
/// Настройки игры.
/// </summary>
public sealed class GameSettings
{
    public const int MinLampCount = 4;
    public const int MaxLampCount = 100;
    public const int MinFlashIntervalMs = 300;
    public const int MaxFlashIntervalMs = 5000;
    public const int MinFlashDurationMs = 200;
    public const int MaxFlashDurationMs = 3000;
    public const int MinSimultaneousFlashes = 1;
    public const int MaxSimultaneousFlashes = 5;

    /// <summary>
    /// Общее количество лампочек.
    /// </summary>
    public int LampCount { get; set; } = 16;

    /// <summary>
    /// Интервал появления вспышек в миллисекундах.
    /// </summary>
    public int FlashIntervalMs { get; set; } = 1000;

    /// <summary>
    /// Длительность активности лампочки в миллисекундах.
    /// </summary>
    public int FlashDurationMs { get; set; } = 800;

    /// <summary>
    /// Количество одновременно активных лампочек.
    /// </summary>
    public int SimultaneousFlashes { get; set; } = 2;

    /// <summary>
    /// Цвет активной лампочки.
    /// </summary>
    public Color ActiveLampColor { get; set; } = Color.OrangeRed;

    /// <summary>
    /// Цвет неактивной лампочки.
    /// </summary>
    public Color InactiveLampColor { get; set; } = Color.LightGray;

    /// <summary>
    /// Цвет фона игрового поля.
    /// </summary>
    public Color FieldBackgroundColor { get; set; } = Color.FromArgb(30, 30, 35);

    /// <summary>
    /// Создаёт копию настроек.
    /// </summary>
    public GameSettings Clone()
    {
        return new GameSettings
        {
            LampCount = LampCount,
            FlashIntervalMs = FlashIntervalMs,
            FlashDurationMs = FlashDurationMs,
            SimultaneousFlashes = SimultaneousFlashes,
            ActiveLampColor = ActiveLampColor,
            InactiveLampColor = InactiveLampColor,
            FieldBackgroundColor = FieldBackgroundColor
        };
    }

    /// <summary>
    /// Возвращает настройки по умолчанию.
    /// </summary>
    public static GameSettings CreateDefault()
    {
        return new GameSettings();
    }
}
