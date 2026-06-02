using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace AuthApp
{
    public partial class AdminDesktopWindow : Window
    {
        private string connectionString;

        public AdminDesktopWindow(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT ID, Login, Role, IsBlocked, ErrorCount, LastLoginDate, NeedChangePassword FROM Users";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgUsers.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message, "Ошибка", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow addWin = new AddUserWindow(connectionString);
            addWin.ShowDialog();
            LoadUsers();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для редактирования", "Предупреждение", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            DataRowView row = (DataRowView)dgUsers.SelectedItem;
            int userId = Convert.ToInt32(row["ID"]);
            
            EditUserWindow editWin = new EditUserWindow(connectionString, userId);
            editWin.ShowDialog();
            LoadUsers();
        }

        private void btnUnblock_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для разблокировки", "Предупреждение", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            DataRowView row = (DataRowView)dgUsers.SelectedItem;
            int userId = Convert.ToInt32(row["ID"]);
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Users SET IsBlocked = 0, ErrorCount = 0 WHERE ID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", userId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Пользователь успешно разблокирован", "Успех", 
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show("Пользователь не найден", "Ошибка", 
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }
    }
}
