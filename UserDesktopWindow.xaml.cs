using System.Windows;

namespace AuthApp
{
    public partial class UserDesktopWindow : Window
    {
        public UserDesktopWindow(string login)
        {
            InitializeComponent();
            this.Title = $"Рабочий стол пользователя - {login}";
        }
    }
}
