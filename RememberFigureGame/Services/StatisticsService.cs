using RememberFigureGame.Models;

namespace RememberFigureGame.Services;

/// <summary>
/// Обновляет статистику партии.
/// </summary>
public sealed class StatisticsService
{
    /// <summary>
    /// Сбрасывает статистику до начальных значений.
    /// </summary>
    public void Reset(GameStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        statistics.TotalFlashes = 0;
        statistics.SuccessfulHits = 0;
        statistics.FailedAttempts = 0;
        statistics.MissedFlashes = 0;
    }

    /// <summary>
    /// Добавляет количество вспышек.
    /// </summary>
    public void RegisterFlashes(GameStatistics statistics, int count)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        if (count <= 0)
        {
            return;
        }

        statistics.TotalFlashes += count;
    }

    /// <summary>
    /// Регистрирует успешное попадание.
    /// </summary>
    public void RegisterSuccessfulHit(GameStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);
        statistics.SuccessfulHits++;
    }

    /// <summary>
    /// Регистрирует неудачную попытку.
    /// </summary>
    public void RegisterFailedAttempt(GameStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);
        statistics.FailedAttempts++;
    }

    /// <summary>
    /// Регистрирует пропущенные вспышки.
    /// </summary>
    public void RegisterMissedFlashes(GameStatistics statistics, int count)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        if (count <= 0)
        {
            return;
        }

        statistics.MissedFlashes += count;
    }
}
