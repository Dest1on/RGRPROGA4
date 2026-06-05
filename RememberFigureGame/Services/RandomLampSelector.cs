using RememberFigureGame.Models;

namespace RememberFigureGame.Services;

/// <summary>
/// Выбирает случайные неактивные лампочки для новой вспышки.
/// </summary>
public sealed class RandomLampSelector
{
    private readonly Random random;

    /// <summary>
    /// Создаёт селектор случайных лампочек.
    /// </summary>
    public RandomLampSelector()
        : this(Random.Shared)
    {
    }

    /// <summary>
    /// Создаёт селектор случайных лампочек с заданным генератором.
    /// </summary>
    public RandomLampSelector(Random random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    /// <summary>
    /// Выбирает случайный набор неактивных лампочек.
    /// </summary>
    public IReadOnlyList<Lamp> SelectInactiveLamps(IReadOnlyList<Lamp> lamps, int requestedCount)
    {
        ArgumentNullException.ThrowIfNull(lamps);

        if (requestedCount <= 0)
        {
            return Array.Empty<Lamp>();
        }

        List<Lamp> inactiveLamps = lamps.Where(lamp => !lamp.IsActive).ToList();
        if (inactiveLamps.Count == 0)
        {
            return Array.Empty<Lamp>();
        }

        int resultCount = Math.Min(requestedCount, inactiveLamps.Count);

        for (int index = 0; index < resultCount; index++)
        {
            int swapIndex = random.Next(index, inactiveLamps.Count);
            (inactiveLamps[index], inactiveLamps[swapIndex]) = (inactiveLamps[swapIndex], inactiveLamps[index]);
        }

        return inactiveLamps.Take(resultCount).ToArray();
    }
}
