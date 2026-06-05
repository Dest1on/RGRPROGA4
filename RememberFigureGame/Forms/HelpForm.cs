namespace RememberFigureGame.Forms;

/// <summary>
/// Простая справочная форма по правилам и горячим клавишам.
/// </summary>
public sealed class HelpForm : Form
{
    private const string HelpText =
        "Правила игры \"Запомни фигуру\"\r\n" +
        "\r\n" +
        "1. На поле случайно загораются лампочки.\r\n" +
        "2. Нужно кликнуть по активной лампочке, пока она не погасла.\r\n" +
        "3. Клик мимо или по неактивной лампочке считается неудачной попыткой.\r\n" +
        "4. Если лампочка погасла сама, это пропущенная вспышка.\r\n" +
        "\r\n" +
        "Горячие клавиши\r\n" +
        "Ctrl+N — новая игра\r\n" +
        "Space — пауза/продолжить\r\n" +
        "Ctrl+Z — отменить последнее изменение настроек\r\n" +
        "F1 — открыть справку\r\n" +
        "Esc — пауза";

    /// <summary>
    /// Создаёт окно справки.
    /// </summary>
    public HelpForm()
    {
        InitializeForm();
    }

    private void InitializeForm()
    {
        Text = "Справка";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 560;
        Height = 420;

        TextBox helpTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Text = HelpText,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Window,
            Font = new Font(Font.FontFamily, 10F, FontStyle.Regular),
            TabStop = false
        };

        Panel container = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14)
        };

        container.Controls.Add(helpTextBox);
        Controls.Add(container);
    }
}
