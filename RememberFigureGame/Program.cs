using System.Threading;

namespace RememberFigureGame;

/// <summary>
/// Точка входа приложения.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Запускает окно игры.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    private static void OnThreadException(object? sender, ThreadExceptionEventArgs e)
    {
        ShowError(e.Exception);
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Exception exception = e.ExceptionObject as Exception ?? new Exception("Произошла неизвестная ошибка.");
        ShowError(exception);
    }

    private static void ShowError(Exception exception)
    {
        MessageBox.Show(
            $"Ошибка: {exception.Message}",
            "Критическая ошибка",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
