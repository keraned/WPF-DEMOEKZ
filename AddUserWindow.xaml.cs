using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace AuthApp
{
    public partial class AddUserWindow : Window
    {
        private string connectionString;

        public AddUserWindow(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            cmbRole.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;
            
            lblError.Text = "";

            if (string.IsNullOrEmpty(login))
            {
                lblError.Text = "Логин обязателен для заполнения";
                return;
            }
            
            if (string.IsNullOrEmpty(password))
            {
                lblError.Text = "Пароль обязателен для заполнения";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Login = @login";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@login", login);
                    
                    int count = (int)checkCmd.ExecuteScalar();
                    
                    if (count > 0)
                    {
                        lblError.Text = "Пользователь с таким логином уже существует";
                        return;
                    }
                    
                    ComboBoxItem selectedItem = (ComboBoxItem)cmbRole.SelectedItem;
                    string role = selectedItem.Content.ToString();
                    
                    // Для новых пользователей (кроме администратора) устанавливаем NeedChangePassword = 1
                    int needChangePassword = (role == "User") ? 1 : 0;
                    
                    string insertQuery = @"INSERT INTO Users (Login, Password, Role, NeedChangePassword, IsBlocked, ErrorCount)
                                          VALUES (@login, @password, @role, @needChangePassword, 0, 0)";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@login", login);
                    insertCmd.Parameters.AddWithValue("@password", password);
                    insertCmd.Parameters.AddWithValue("@role", role);
                    insertCmd.Parameters.AddWithValue("@needChangePassword", needChangePassword);
                    insertCmd.ExecuteNonQuery();
                    
                    MessageBox.Show($"Пользователь '{login}' успешно добавлен с ролью '{role}'", "Успех", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
