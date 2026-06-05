using RememberFigureGame.Models;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace RememberFigureGame.Services;

/// <summary>
/// Управляет состоянием игры, таймерами, лампочками и статистикой.
/// </summary>
public sealed class GameEngine : IDisposable
{
    private const int ExpirationCheckIntervalMs = 25;

    private readonly WinFormsTimer flashTimer;
    private readonly WinFormsTimer expirationTimer;
    private readonly LampLayoutService lampLayoutService;
    private readonly RandomLampSelector randomLampSelector;
    private readonly StatisticsService statisticsService;
    private readonly List<Lamp> lamps = new List<Lamp>();
    private readonly GameStatistics statistics = new GameStatistics();

    private Rectangle fieldBounds;
    private GameSettings settings;
    private GameState state = GameState.NotStarted;

    /// <summary>
    /// Создаёт игровой движок.
    /// </summary>
    public GameEngine(
        GameSettings initialSettings,
        LampLayoutService? lampLayoutService = null,
        RandomLampSelector? randomLampSelector = null,
        StatisticsService? statisticsService = null)
    {
        settings = (initialSettings ?? throw new ArgumentNullException(nameof(initialSettings))).Clone();
        this.lampLayoutService = lampLayoutService ?? new LampLayoutService();
        this.randomLampSelector = randomLampSelector ?? new RandomLampSelector();
        this.statisticsService = statisticsService ?? new StatisticsService();

        flashTimer = new WinFormsTimer
        {
            Interval = settings.FlashIntervalMs
        };
        flashTimer.Tick += OnFlashTimerTick;

        expirationTimer = new WinFormsTimer
        {
            Interval = ExpirationCheckIntervalMs
        };
        expirationTimer.Tick += OnExpirationTimerTick;
    }

    /// <summary>
    /// Событие изменения набора лампочек.
    /// </summary>
    public event EventHandler<LampsChangedEventArgs>? LampsChanged;

    /// <summary>
    /// Событие изменения статистики.
    /// </summary>
    public event EventHandler<StatisticsChangedEventArgs>? StatisticsChanged;

    /// <summary>
    /// Событие изменения состояния игры.
    /// </summary>
    public event EventHandler<GameStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Текущие настройки игры.
    /// </summary>
    public GameSettings Settings
    {
        get
        {
            return settings.Clone();
        }
    }

    /// <summary>
    /// Текущая статистика.
    /// </summary>
    public GameStatistics Statistics
    {
        get
        {
            return statistics.Clone();
        }
    }

    /// <summary>
    /// Текущее состояние игры.
    /// </summary>
    public GameState State
    {
        get
        {
            return state;
        }
    }

    /// <summary>
    /// Инициализирует размеры игрового поля и строит лампочки.
    /// </summary>
    public void Initialize(Rectangle bounds)
    {
        fieldBounds = bounds;
        RebuildLamps(preserveActiveState: false);
        NotifyLampsChanged();
        NotifyStatisticsChanged();
        NotifyStateChanged();
    }

    /// <summary>
    /// Обновляет размер игрового поля и перестраивает расположение лампочек.
    /// </summary>
    public void UpdateFieldBounds(Rectangle bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        if (fieldBounds == bounds)
        {
            return;
        }

        fieldBounds = bounds;
        RebuildLamps(preserveActiveState: true);
        NotifyLampsChanged();
    }

    /// <summary>
    /// Запускает новую игру с текущими настройками.
    /// </summary>
    public void StartNewGame()
    {
        statisticsService.Reset(statistics);
        DeactivateAllLamps();

        state = GameState.Running;
        flashTimer.Interval = settings.FlashIntervalMs;
        flashTimer.Start();
        expirationTimer.Start();

        NotifyLampsChanged();
        NotifyStatisticsChanged();
        NotifyStateChanged();
    }

    /// <summary>
    /// Ставит игру на паузу.
    /// </summary>
    public void Pause()
    {
        if (state != GameState.Running)
        {
            return;
        }

        flashTimer.Stop();
        expirationTimer.Stop();
        state = GameState.Paused;
        NotifyStateChanged();
    }

    /// <summary>
    /// Продолжает игру после паузы.
    /// </summary>
    public void Resume()
    {
        if (state != GameState.Paused)
        {
            return;
        }

        flashTimer.Start();
        expirationTimer.Start();
        state = GameState.Running;
        NotifyStateChanged();
    }

    /// <summary>
    /// Переключает паузу/продолжение.
    /// </summary>
    public void TogglePauseResume()
    {
        if (state == GameState.Running)
        {
            Pause();
            return;
        }

        if (state == GameState.Paused)
        {
            Resume();
            return;
        }

        StartNewGame();
    }

    /// <summary>
    /// Применяет новые настройки.
    /// </summary>
    public void ApplySettings(GameSettings newSettings)
    {
        ArgumentNullException.ThrowIfNull(newSettings);

        bool needRebuildLayout = newSettings.LampCount != settings.LampCount;
        settings = newSettings.Clone();
        flashTimer.Interval = settings.FlashIntervalMs;

        if (needRebuildLayout)
        {
            RebuildLamps(preserveActiveState: false);
        }
        else
        {
            UpdateLampColors();
        }

        NotifyLampsChanged();
    }

    /// <summary>
    /// Обрабатывает клик пользователя по игровому полю.
    /// </summary>
    public void HandleFieldClick(Point clickPoint)
    {
        if (state != GameState.Running)
        {
            return;
        }

        Lamp? clickedLamp = lamps.FirstOrDefault(lamp => lamp.Contains(clickPoint));

        if (clickedLamp is not null && clickedLamp.IsActive)
        {
            clickedLamp.Deactivate();
            statisticsService.RegisterSuccessfulHit(statistics);
            NotifyLampsChanged();
            NotifyStatisticsChanged();
            return;
        }

        statisticsService.RegisterFailedAttempt(statistics);
        NotifyStatisticsChanged();
    }

    /// <summary>
    /// Освобождает ресурсы таймеров.
    /// </summary>
    public void Dispose()
    {
        flashTimer.Stop();
        expirationTimer.Stop();

        flashTimer.Tick -= OnFlashTimerTick;
        expirationTimer.Tick -= OnExpirationTimerTick;

        flashTimer.Dispose();
        expirationTimer.Dispose();
    }

    private void RebuildLamps(bool preserveActiveState)
    {
        IReadOnlyDictionary<int, DateTime> activeExpirationsById = preserveActiveState
            ? lamps
                .Where(lamp => lamp.IsActive)
                .ToDictionary(lamp => lamp.Id, lamp => lamp.ActiveUntilUtc)
            : new Dictionary<int, DateTime>();

        IReadOnlyList<Lamp> rebuiltLamps = lampLayoutService.CreateLayout(settings, fieldBounds);
        lamps.Clear();
        lamps.AddRange(rebuiltLamps);

        if (preserveActiveState)
        {
            DateTime now = DateTime.UtcNow;
            foreach (Lamp lamp in lamps)
            {
                if (activeExpirationsById.TryGetValue(lamp.Id, out DateTime activeUntilUtc) && activeUntilUtc > now)
                {
                    lamp.Activate(activeUntilUtc);
                }
            }
        }
    }

    private void UpdateLampColors()
    {
        foreach (Lamp lamp in lamps)
        {
            lamp.ActiveColor = settings.ActiveLampColor;
            lamp.InactiveColor = settings.InactiveLampColor;
        }
    }

    private void DeactivateAllLamps()
    {
        foreach (Lamp lamp in lamps)
        {
            lamp.Deactivate();
        }
    }

    private void OnFlashTimerTick(object? sender, EventArgs e)
    {
        if (state != GameState.Running)
        {
            return;
        }

        IReadOnlyList<Lamp> selectedLamps = randomLampSelector.SelectInactiveLamps(lamps, settings.SimultaneousFlashes);
        if (selectedLamps.Count == 0)
        {
            return;
        }

        DateTime activeUntilUtc = DateTime.UtcNow.AddMilliseconds(settings.FlashDurationMs);
        foreach (Lamp lamp in selectedLamps)
        {
            lamp.Activate(activeUntilUtc);
        }

        statisticsService.RegisterFlashes(statistics, selectedLamps.Count);
        NotifyLampsChanged();
        NotifyStatisticsChanged();
    }

    private void OnExpirationTimerTick(object? sender, EventArgs e)
    {
        if (state != GameState.Running)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        int missedFlashes = 0;

        foreach (Lamp lamp in lamps)
        {
            if (lamp.IsActive && lamp.ActiveUntilUtc <= now)
            {
                lamp.Deactivate();
                missedFlashes++;
            }
        }

        if (missedFlashes == 0)
        {
            return;
        }

        statisticsService.RegisterMissedFlashes(statistics, missedFlashes);
        NotifyLampsChanged();
        NotifyStatisticsChanged();
    }

    private void NotifyLampsChanged()
    {
        Lamp[] snapshot = lamps.Select(lamp => lamp.Clone()).ToArray();
        LampsChanged?.Invoke(this, new LampsChangedEventArgs(snapshot));
    }

    private void NotifyStatisticsChanged()
    {
        StatisticsChanged?.Invoke(this, new StatisticsChangedEventArgs(statistics.Clone()));
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, new GameStateChangedEventArgs(state));
    }
}

/// <summary>
/// Аргументы события изменения лампочек.
/// </summary>
public sealed class LampsChangedEventArgs : EventArgs
{
    /// <summary>
    /// Создаёт аргументы изменения лампочек.
    /// </summary>
    public LampsChangedEventArgs(IReadOnlyList<Lamp> lamps)
    {
        Lamps = lamps ?? throw new ArgumentNullException(nameof(lamps));
    }

    /// <summary>
    /// Снимок текущих лампочек.
    /// </summary>
    public IReadOnlyList<Lamp> Lamps { get; }
}

/// <summary>
/// Аргументы события изменения статистики.
/// </summary>
public sealed class StatisticsChangedEventArgs : EventArgs
{
    /// <summary>
    /// Создаёт аргументы изменения статистики.
    /// </summary>
    public StatisticsChangedEventArgs(GameStatistics statistics)
    {
        Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
    }

    /// <summary>
    /// Снимок текущей статистики.
    /// </summary>
    public GameStatistics Statistics { get; }
}

/// <summary>
/// Аргументы события изменения состояния игры.
/// </summary>
public sealed class GameStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Создаёт аргументы изменения состояния.
    /// </summary>
    public GameStateChangedEventArgs(GameState state)
    {
        State = state;
    }

    /// <summary>
    /// Текущее состояние игры.
    /// </summary>
    public GameState State { get; }
}
