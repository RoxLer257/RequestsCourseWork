using RequestsCourse.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RequestsCourse.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Page
    {
        public Authorization()
        {
            InitializeComponent();
        }

        private void BtnEnter_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text;
            string password = TxtPassword.Password;

            try
            {
                var user = WebAgencyRequestsEntities.GetContext().Employees.FirstOrDefault(u => u.Username == login);

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Необходимо заполнить все поля", "Ошибка при авторизации",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (user == null)
                {
                    TxtLogin.ToolTip = "Неверный логин";
                    TxtLogin.Background = Brushes.Red;
                    MessageBox.Show("Неверный логин", "Ошибка при авторизации",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (user.Password != password)
                {
                    TxtPassword.Clear();
                    TxtPassword.ToolTip = "Неправильный пароль";
                    TxtPassword.Background = Brushes.Red;
                    MessageBox.Show("Неправильный пароль", "Ошибка при авторизации",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ClassFrame.RoleId = user.RoleId;
                CurrentUser.Username = user.Username;
                CurrentUser.PasswordHash = user.Password;
                CurrentUser.Role = WebAgencyRequestsEntities.GetContext().Roles.FirstOrDefault(r => r.RoleId == user.RoleId)?.RoleName;
                ClassFrame.frmObj.Navigate(new MainPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка при авторизации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
