namespace RememberFigureGame.Models;

/// <summary>
/// Статистика текущей партии.
/// </summary>
public sealed class GameStatistics
{
    /// <summary>
    /// Общее количество вспышек.
    /// </summary>
    public int TotalFlashes { get; internal set; }

    /// <summary>
    /// Успешные нажатия по активным лампочкам.
    /// </summary>
    public int SuccessfulHits { get; internal set; }

    /// <summary>
    /// Неудачные попытки (мимо или по неактивной лампочке).
    /// </summary>
    public int FailedAttempts { get; internal set; }

    /// <summary>
    /// Пропущенные вспышки.
    /// </summary>
    public int MissedFlashes { get; internal set; }

    /// <summary>
    /// Процент успешных попаданий от общего числа вспышек.
    /// </summary>
    public double SuccessRate
    {
        get
        {
            if (TotalFlashes == 0)
            {
                return 0D;
            }

            return SuccessfulHits * 100D / TotalFlashes;
        }
    }

    /// <summary>
    /// Создаёт копию статистики.
    /// </summary>
    public GameStatistics Clone()
    {
        return new GameStatistics
        {
            TotalFlashes = TotalFlashes,
            SuccessfulHits = SuccessfulHits,
            FailedAttempts = FailedAttempts,
            MissedFlashes = MissedFlashes
        };
    }
}
