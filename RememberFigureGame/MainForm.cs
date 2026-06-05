using System.Globalization;
using RememberFigureGame.Controls;
using RememberFigureGame.Forms;
using RememberFigureGame.Models;
using RememberFigureGame.Services;

namespace RememberFigureGame;

/// <summary>
/// Главное окно приложения. Отвечает за интерфейс и связь с игровым движком.
/// </summary>
public sealed class MainForm : Form
{
    private readonly GameEngine gameEngine;
    private readonly InputValidator inputValidator;
    private readonly Stack<GameSettings> settingsHistory = new Stack<GameSettings>();

    private readonly GameFieldControl gameFieldControl = new GameFieldControl();
    private readonly Button newGameButton = new Button();
    private readonly Button pauseResumeButton = new Button();
    private readonly Button applySettingsButton = new Button();
    private readonly Button undoSettingsButton = new Button();
    private readonly Button helpButton = new Button();
    private readonly Button activeColorButton = new Button();
    private readonly Button inactiveColorButton = new Button();
    private readonly Button fieldColorButton = new Button();

    private readonly NumericUpDown lampCountNumericUpDown = new NumericUpDown();
    private readonly NumericUpDown flashIntervalNumericUpDown = new NumericUpDown();
    private readonly NumericUpDown flashDurationNumericUpDown = new NumericUpDown();
    private readonly NumericUpDown simultaneousFlashesNumericUpDown = new NumericUpDown();

    private readonly Label totalFlashesValueLabel = new Label();
    private readonly Label successfulHitsValueLabel = new Label();
    private readonly Label failedAttemptsValueLabel = new Label();
    private readonly Label missedFlashesValueLabel = new Label();
    private readonly Label successRateValueLabel = new Label();

    /// <summary>
    /// Создаёт главное окно.
    /// </summary>
    public MainForm()
    {
        gameEngine = new GameEngine(GameSettings.CreateDefault());
        inputValidator = new InputValidator();

        InitializeForm();
        InitializeLayout();
        SubscribeToEvents();

        GameSettings settings = gameEngine.Settings;
        ApplySettingsToControls(settings);
        ApplySettingsColors(settings);
        UpdateStatisticsDisplay(gameEngine.Statistics);
        UpdatePauseButtonText(gameEngine.State);
        UpdateUndoButtonState();
    }

    /// <inheritdoc />
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ExecuteWithErrorHandling(() =>
        {
            gameEngine.Initialize(gameFieldControl.ClientRectangle);
            gameEngine.StartNewGame();
        });
    }

    /// <inheritdoc />
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.N))
        {
            ExecuteWithErrorHandling(() => gameEngine.StartNewGame());
            return true;
        }

        if (keyData == Keys.Space)
        {
            ExecuteWithErrorHandling(() => gameEngine.TogglePauseResume());
            return true;
        }

        if (keyData == (Keys.Control | Keys.Z))
        {
            ExecuteWithErrorHandling(UndoSettings);
            return true;
        }

        if (keyData == Keys.F1)
        {
            ExecuteWithErrorHandling(ShowHelp);
            return true;
        }

        if (keyData == Keys.Escape)
        {
            ExecuteWithErrorHandling(() => gameEngine.Pause());
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            gameEngine.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeForm()
    {
        Text = "Запомни фигуру";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1220;
        Height = 760;
        MinimumSize = new Size(1040, 640);
        KeyPreview = true;
    }

    private void InitializeLayout()
    {
        TableLayoutPanel rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

        gameFieldControl.Dock = DockStyle.Fill;
        rootLayout.Controls.Add(gameFieldControl, 0, 0);

        Panel rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(8)
        };

        TableLayoutPanel rightLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 3
        };
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        rightLayout.Controls.Add(CreateActionsGroup(), 0, 0);
        rightLayout.Controls.Add(CreateSettingsGroup(), 0, 1);
        rightLayout.Controls.Add(CreateStatisticsGroup(), 0, 2);

        rightPanel.Controls.Add(rightLayout);
        rootLayout.Controls.Add(rightPanel, 1, 0);

        Controls.Add(rootLayout);
    }

    private GroupBox CreateActionsGroup()
    {
        GroupBox groupBox = new GroupBox
        {
            Text = "Управление",
            Dock = DockStyle.Top,
            AutoSize = true
        };

        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(8),
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        ConfigureButton(newGameButton, "Новая игра");
        ConfigureButton(pauseResumeButton, "Пауза/Продолжить");
        ConfigureButton(applySettingsButton, "Применить настройки");
        ConfigureButton(undoSettingsButton, "Отменить настройки");
        ConfigureButton(helpButton, "Справка");

        layout.Controls.Add(newGameButton, 0, 0);
        layout.Controls.Add(pauseResumeButton, 1, 0);
        layout.Controls.Add(applySettingsButton, 0, 1);
        layout.Controls.Add(undoSettingsButton, 1, 1);
        layout.Controls.Add(helpButton, 0, 2);
        layout.SetColumnSpan(helpButton, 2);

        groupBox.Controls.Add(layout);
        return groupBox;
    }

    private GroupBox CreateSettingsGroup()
    {
        GroupBox groupBox = new GroupBox
        {
            Text = "Настройки",
            Dock = DockStyle.Top,
            AutoSize = true
        };

        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(8),
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));

        ConfigureNumericUpDown(
            lampCountNumericUpDown,
            GameSettings.MinLampCount,
            GameSettings.MaxLampCount);

        ConfigureNumericUpDown(
            flashIntervalNumericUpDown,
            GameSettings.MinFlashIntervalMs,
            GameSettings.MaxFlashIntervalMs);

        ConfigureNumericUpDown(
            flashDurationNumericUpDown,
            GameSettings.MinFlashDurationMs,
            GameSettings.MaxFlashDurationMs);

        ConfigureNumericUpDown(
            simultaneousFlashesNumericUpDown,
            GameSettings.MinSimultaneousFlashes,
            GameSettings.MaxSimultaneousFlashes);

        ConfigureColorButton(activeColorButton);
        ConfigureColorButton(inactiveColorButton);
        ConfigureColorButton(fieldColorButton);

        AddLabeledControl(layout, 0, "Количество лампочек:", lampCountNumericUpDown);
        AddLabeledControl(layout, 1, "Интервал вспышек (мс):", flashIntervalNumericUpDown);
        AddLabeledControl(layout, 2, "Длительность вспышки (мс):", flashDurationNumericUpDown);
        AddLabeledControl(layout, 3, "Активных за такт:", simultaneousFlashesNumericUpDown);
        AddLabeledControl(layout, 4, "Цвет активной лампы:", activeColorButton);
        AddLabeledControl(layout, 5, "Цвет неактивной лампы:", inactiveColorButton);
        AddLabeledControl(layout, 6, "Цвет фона поля:", fieldColorButton);

        groupBox.Controls.Add(layout);
        return groupBox;
    }

    private GroupBox CreateStatisticsGroup()
    {
        GroupBox groupBox = new GroupBox
        {
            Text = "Статистика",
            Dock = DockStyle.Top,
            AutoSize = true
        };

        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(8),
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));

        ConfigureValueLabel(totalFlashesValueLabel);
        ConfigureValueLabel(successfulHitsValueLabel);
        ConfigureValueLabel(failedAttemptsValueLabel);
        ConfigureValueLabel(missedFlashesValueLabel);
        ConfigureValueLabel(successRateValueLabel);

        AddLabeledControl(layout, 0, "Общее количество вспышек:", totalFlashesValueLabel);
        AddLabeledControl(layout, 1, "Успешные попадания:", successfulHitsValueLabel);
        AddLabeledControl(layout, 2, "Неудачные попытки:", failedAttemptsValueLabel);
        AddLabeledControl(layout, 3, "Пропущенные вспышки:", missedFlashesValueLabel);
        AddLabeledControl(layout, 4, "Процент успешности:", successRateValueLabel);

        groupBox.Controls.Add(layout);
        return groupBox;
    }

    private void ConfigureButton(Button button, string text)
    {
        button.Text = text;
        button.Dock = DockStyle.Top;
        button.Height = 32;
        button.Margin = new Padding(3);
    }

    private static void ConfigureNumericUpDown(NumericUpDown numericUpDown, int min, int max)
    {
        numericUpDown.Minimum = min;
        numericUpDown.Maximum = max;
        numericUpDown.Dock = DockStyle.Fill;
        numericUpDown.TextAlign = HorizontalAlignment.Right;
    }

    private static void ConfigureColorButton(Button button)
    {
        button.Text = "Выбрать";
        button.Dock = DockStyle.Fill;
        button.Height = 28;
    }

    private static void ConfigureValueLabel(Label label)
    {
        label.Dock = DockStyle.Fill;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.AutoSize = false;
    }

    private static void AddLabeledControl(TableLayoutPanel layout, int rowIndex, string labelText, Control control)
    {
        Label label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft
        };

        layout.Controls.Add(label, 0, rowIndex);
        layout.Controls.Add(control, 1, rowIndex);
    }

    private void SubscribeToEvents()
    {
        newGameButton.Click += (_, _) => ExecuteWithErrorHandling(() => gameEngine.StartNewGame());
        pauseResumeButton.Click += (_, _) => ExecuteWithErrorHandling(() => gameEngine.TogglePauseResume());
        applySettingsButton.Click += (_, _) => ExecuteWithErrorHandling(ApplySettingsFromControls);
        undoSettingsButton.Click += (_, _) => ExecuteWithErrorHandling(UndoSettings);
        helpButton.Click += (_, _) => ExecuteWithErrorHandling(ShowHelp);

        activeColorButton.Click += (_, _) => SelectColor(activeColorButton);
        inactiveColorButton.Click += (_, _) => SelectColor(inactiveColorButton);
        fieldColorButton.Click += (_, _) => SelectColor(fieldColorButton);

        gameFieldControl.FieldClicked += (_, eventArgs) =>
            ExecuteWithErrorHandling(() => gameEngine.HandleFieldClick(eventArgs.Location));

        gameFieldControl.Resize += (_, _) =>
            ExecuteWithErrorHandling(() => gameEngine.UpdateFieldBounds(gameFieldControl.ClientRectangle));

        gameEngine.LampsChanged += OnLampsChanged;
        gameEngine.StatisticsChanged += OnStatisticsChanged;
        gameEngine.StateChanged += OnStateChanged;
    }

    private void OnLampsChanged(object? sender, LampsChangedEventArgs e)
    {
        gameFieldControl.SetLamps(e.Lamps);
    }

    private void OnStatisticsChanged(object? sender, StatisticsChangedEventArgs e)
    {
        UpdateStatisticsDisplay(e.Statistics);
    }

    private void OnStateChanged(object? sender, GameStateChangedEventArgs e)
    {
        UpdatePauseButtonText(e.State);
    }

    private void ApplySettingsFromControls()
    {
        GameSettings settings = ReadSettingsFromControls();
        if (!inputValidator.TryValidate(settings, out string errorMessage))
        {
            ShowValidationError(errorMessage);
            return;
        }

        settingsHistory.Push(gameEngine.Settings);
        gameEngine.ApplySettings(settings);
        ApplySettingsColors(settings);
        UpdateUndoButtonState();
    }

    private void UndoSettings()
    {
        if (settingsHistory.Count == 0)
        {
            MessageBox.Show(
                this,
                "История изменений настроек пуста.",
                "Отмена настроек",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        GameSettings previousSettings = settingsHistory.Pop();
        ApplySettingsToControls(previousSettings);
        gameEngine.ApplySettings(previousSettings);
        ApplySettingsColors(previousSettings);
        UpdateUndoButtonState();
    }

    private GameSettings ReadSettingsFromControls()
    {
        return new GameSettings
        {
            LampCount = decimal.ToInt32(lampCountNumericUpDown.Value),
            FlashIntervalMs = decimal.ToInt32(flashIntervalNumericUpDown.Value),
            FlashDurationMs = decimal.ToInt32(flashDurationNumericUpDown.Value),
            SimultaneousFlashes = decimal.ToInt32(simultaneousFlashesNumericUpDown.Value),
            ActiveLampColor = activeColorButton.BackColor,
            InactiveLampColor = inactiveColorButton.BackColor,
            FieldBackgroundColor = fieldColorButton.BackColor
        };
    }

    private void ApplySettingsToControls(GameSettings settings)
    {
        lampCountNumericUpDown.Value = settings.LampCount;
        flashIntervalNumericUpDown.Value = settings.FlashIntervalMs;
        flashDurationNumericUpDown.Value = settings.FlashDurationMs;
        simultaneousFlashesNumericUpDown.Value = settings.SimultaneousFlashes;

        activeColorButton.BackColor = settings.ActiveLampColor;
        inactiveColorButton.BackColor = settings.InactiveLampColor;
        fieldColorButton.BackColor = settings.FieldBackgroundColor;
    }

    private void ApplySettingsColors(GameSettings settings)
    {
        activeColorButton.BackColor = settings.ActiveLampColor;
        inactiveColorButton.BackColor = settings.InactiveLampColor;
        fieldColorButton.BackColor = settings.FieldBackgroundColor;
        gameFieldControl.BackColor = settings.FieldBackgroundColor;
    }

    private void SelectColor(Button button)
    {
        using ColorDialog dialog = new ColorDialog
        {
            Color = button.BackColor,
            FullOpen = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            button.BackColor = dialog.Color;
        }
    }

    private void ShowHelp()
    {
        using HelpForm helpForm = new HelpForm();
        helpForm.ShowDialog(this);
    }

    private void UpdatePauseButtonText(GameState state)
    {
        pauseResumeButton.Text = state switch
        {
            GameState.Running => "Пауза",
            GameState.Paused => "Продолжить",
            _ => "Пауза/Продолжить"
        };
    }

    private void UpdateUndoButtonState()
    {
        undoSettingsButton.Enabled = settingsHistory.Count > 0;
    }

    private void UpdateStatisticsDisplay(GameStatistics statistics)
    {
        totalFlashesValueLabel.Text = statistics.TotalFlashes.ToString(CultureInfo.InvariantCulture);
        successfulHitsValueLabel.Text = statistics.SuccessfulHits.ToString(CultureInfo.InvariantCulture);
        failedAttemptsValueLabel.Text = statistics.FailedAttempts.ToString(CultureInfo.InvariantCulture);
        missedFlashesValueLabel.Text = statistics.MissedFlashes.ToString(CultureInfo.InvariantCulture);
        successRateValueLabel.Text = $"{statistics.SuccessRate:0.00} %";
    }

    private void ShowValidationError(string message)
    {
        MessageBox.Show(
            this,
            message,
            "Некорректные настройки",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private void ExecuteWithErrorHandling(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                $"Ошибка: {exception.Message}",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
