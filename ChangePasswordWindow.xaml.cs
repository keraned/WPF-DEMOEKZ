using System;
using System.Data.SqlClient;
using System.Windows;

namespace AuthApp
{
    public partial class ChangePasswordWindow : Window
    {
        private string login;
        private int userId;
        private string connectionString;

        public ChangePasswordWindow(string login, int userId, string connectionString)
        {
            InitializeComponent();
            this.login = login;
            this.userId = userId;
            this.connectionString = connectionString;
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            string currentPass = txtCurrentPassword.Password;
            string newPass = txtNewPassword.Password;
            string confirmPass = txtConfirmPassword.Password;

            lblError.Text = "";

            if (string.IsNullOrEmpty(currentPass))
            {
                lblError.Text = "Текущий пароль обязателен для заполнения";
                return;
            }
            
            if (string.IsNullOrEmpty(newPass))
            {
                lblError.Text = "Новый пароль обязателен для заполнения";
                return;
            }
            
            if (string.IsNullOrEmpty(confirmPass))
            {
                lblError.Text = "Подтверждение пароля обязательно для заполнения";
                return;
            }

            if (newPass != confirmPass)
            {
                lblError.Text = "Новый пароль и подтверждение не совпадают";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Проверяем текущий пароль
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE ID = @id AND Password = @pass";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@id", userId);
                    checkCmd.Parameters.AddWithValue("@pass", currentPass);
                    
                    int count = (int)checkCmd.ExecuteScalar();
                    
                    if (count == 0)
                    {
                        lblError.Text = "Неверный текущий пароль";
                        return;
                    }
                    
                    // Меняем пароль
                    string updateQuery = @"UPDATE Users 
                                          SET Password = @newPass, NeedChangePassword = 0 
                                          WHERE ID = @id";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@newPass", newPass);
                    updateCmd.Parameters.AddWithValue("@id", userId);
                    updateCmd.ExecuteNonQuery();
                    
                    MessageBox.Show("Пароль успешно изменен! Пожалуйста, войдите снова.", "Успех", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    this.Close();
                    Application.Current.MainWindow.Close();
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Ошибка: " + ex.Message;
            }
        }
    }
}
