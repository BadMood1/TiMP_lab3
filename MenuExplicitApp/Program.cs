using System;
using System.Windows.Forms;

namespace MenuExplicitApp;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using var login = new LoginForm();
        if (login.ShowDialog() != DialogResult.OK || login.ResultStatuses == null)
        {
            return;
        }

        Application.Run(new MainForm(login.ResultStatuses));
    }
}
