using System;
using System.Data.SqlClient;
using System.Windows;

namespace AuthApp
{
    public partial class MainWindow : Window
    {
        private string connectionString = @"Server=localhost;Database=AuthAppDB;Integrated Security=True;";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
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
                    
                    string query = @"SELECT ID, Login, Password, Role, IsBlocked, ErrorCount, LastLoginDate, NeedChangePassword 
                                    FROM Users 
                                    WHERE Login = @login";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@login", login);
                    
                    SqlDataReader reader = cmd.ExecuteReader();
                    
                    if (!reader.Read())
                    {
                        reader.Close();
                        lblError.Text = "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные";
                        return;
                    }
                    
                    int userId = reader.GetInt32(0);
                    string dbLogin = reader.GetString(1);
                    string dbPassword = reader.GetString(2);
                    string role = reader.GetString(3);
                    bool isBlocked = reader.GetBoolean(4);
                    int errorCount = reader.GetInt32(5);
                    DateTime? lastLoginDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6);
                    bool needChangePassword = reader.GetBoolean(7);
                    
                    reader.Close();
                    
                    // Блокировка применяется только для обычных пользователей, НЕ для администратора
                    if (role == "User" && isBlocked)
                    {
                        lblError.Text = "Вы заблокированы. Обратитесь к администратору";
                        return;
                    }
                    
                    // Проверка на неактивность более 30 дней (только для обычных пользователей)
                    if (role == "User" && lastLoginDate.HasValue)
                    {
                        TimeSpan inactiveDays = DateTime.Now - lastLoginDate.Value;
                        if (inactiveDays.TotalDays > 30)
                        {
                            BlockUser(login);
                            lblError.Text = "Вы заблокированы (неактивность более 30 дней). Обратитесь к администратору";
                            return;
                        }
                    }
                    
                    // Проверка пароля
                    if (dbPassword != password)
                    {
                        // Увеличиваем счетчик ошибок ТОЛЬКО для обычных пользователей
                        if (role == "User")
                        {
                            errorCount++;
                            UpdateErrorCount(login, errorCount);
                            
                            if (errorCount >= 3)
                            {
                                BlockUser(login);
                                lblError.Text = "Вы заблокированы (3 неудачных попытки). Обратитесь к администратору";
                            }
                            else
                            {
                                lblError.Text = $"Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные. Осталось попыток: {3 - errorCount}";
                            }
                        }
                        else
                        {
                            // Администратор не блокируется, просто показывает ошибку
                            lblError.Text = "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные";
                        }
                        return;
                    }
                    
                    // УСПЕШНАЯ АВТОРИЗАЦИЯ
                    // Сбрасываем счетчик ошибок (только для пользователей)
                    if (role == "User")
                    {
                        ResetErrorCount(login);
                    }
                    
                    // Обновляем дату последнего входа
                    UpdateLastLogin(login);
                    
                    MessageBox.Show("Вы успешно авторизовались", "Успех", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Проверяем, нужно ли сменить пароль (ТОЛЬКО ДЛЯ ОБЫЧНОГО ПОЛЬЗОВАТЕЛЯ)
                    if (needChangePassword && role == "User")
                    {
                        ChangePasswordWindow changePassWin = new ChangePasswordWindow(login, userId, connectionString);
                        changePassWin.ShowDialog();
                    }
                    
                    // Открываем соответствующее окно в зависимости от роли
                    if (role == "Admin")
                    {
                        AdminDesktopWindow adminWin = new AdminDesktopWindow(connectionString);
                        adminWin.Show();
                    }
                    else
                    {
                        UserDesktopWindow userWin = new UserDesktopWindow(login);
                        userWin.Show();
                    }
                    
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Ошибка базы данных: " + ex.Message;
            }
        }

        private void UpdateErrorCount(string login, int count)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Users SET ErrorCount = @count WHERE Login = @login";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@count", count);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.ExecuteNonQuery();
            }
        }

        private void ResetErrorCount(string login)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Users SET ErrorCount = 0 WHERE Login = @login";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.ExecuteNonQuery();
            }
        }

        private void BlockUser(string login)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Users SET IsBlocked = 1 WHERE Login = @login";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateLastLogin(string login)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Users SET LastLoginDate = @date WHERE Login = @login";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
