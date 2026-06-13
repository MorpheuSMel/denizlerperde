namespace DenizlerPerdeDesktop;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Denizler Perde masaüstü uygulaması açılırken bir hata oluştu:\n\n" + ex.Message,
                "Denizler Perde",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
