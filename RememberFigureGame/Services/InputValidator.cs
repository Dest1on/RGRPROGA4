using RememberFigureGame.Models;

namespace RememberFigureGame.Services;

/// <summary>
/// Проверяет корректность пользовательских настроек.
/// </summary>
public sealed class InputValidator
{
    /// <summary>
    /// Проверяет корректность настроек.
    /// </summary>
    public bool TryValidate(GameSettings settings, out string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.LampCount < GameSettings.MinLampCount || settings.LampCount > GameSettings.MaxLampCount)
        {
            errorMessage = $"Количество лампочек должно быть от {GameSettings.MinLampCount} до {GameSettings.MaxLampCount}.";
            return false;
        }

        if (settings.FlashIntervalMs < GameSettings.MinFlashIntervalMs || settings.FlashIntervalMs > GameSettings.MaxFlashIntervalMs)
        {
            errorMessage = $"Интервал вспышек должен быть от {GameSettings.MinFlashIntervalMs} до {GameSettings.MaxFlashIntervalMs} мс.";
            return false;
        }

        if (settings.FlashDurationMs < GameSettings.MinFlashDurationMs || settings.FlashDurationMs > GameSettings.MaxFlashDurationMs)
        {
            errorMessage = $"Длительность вспышки должна быть от {GameSettings.MinFlashDurationMs} до {GameSettings.MaxFlashDurationMs} мс.";
            return false;
        }

        if (settings.SimultaneousFlashes < GameSettings.MinSimultaneousFlashes
            || settings.SimultaneousFlashes > GameSettings.MaxSimultaneousFlashes)
        {
            errorMessage = $"Одновременных вспышек должно быть от {GameSettings.MinSimultaneousFlashes} до {GameSettings.MaxSimultaneousFlashes}.";
            return false;
        }

        if (settings.SimultaneousFlashes > settings.LampCount)
        {
            errorMessage = "Количество одновременно вспыхивающих лампочек не может быть больше общего количества лампочек.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
