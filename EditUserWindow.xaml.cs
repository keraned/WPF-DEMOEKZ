using System.Data.SqlClient;
using System.Windows;

namespace AuthApp
{
    public partial class EditUserWindow : Window
    {
        private string connectionString;
        private int userId;

        public EditUserWindow(string connectionString, int userId)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            this.userId = userId;
            LoadUserData();
        }

        private void LoadUserData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Login, Role, NeedChangePassword FROM Users WHERE ID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", userId);
                    
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtLogin.Text = reader.GetString(0);
                        string role = reader.GetString(1);
                        bool needChangePassword = reader.GetBoolean(2);
                        
                        cmbRole.Items.Clear();
                        cmbRole.Items.Add("User");
                        cmbRole.Items.Add("Admin");
                        cmbRole.SelectedItem = role;
                        
                        chkResetPassword.IsChecked = needChangePassword;
                        
                        // Если роль Admin, отключаем чекбокс
                        if (role == "Admin")
                        {
                            chkResetPassword.IsEnabled = false;
                            chkResetPassword.IsChecked = false;
                        }
                    }
                    reader.Close();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных пользователя: " + ex.Message, "Ошибка", 
                              MessageBox
