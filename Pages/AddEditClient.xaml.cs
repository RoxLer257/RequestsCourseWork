using RequestsCourse.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Логика взаимодействия для AddEditClient.xaml
    /// </summary>
    public partial class AddEditClient : Page
    {

        private Clients _clients;
        private ListClientsPage _listClients;
        public AddEditClient(ListClientsPage listClients, Clients clients)
        {
            InitializeComponent();
            DataContext = this;
            this._listClients = listClients;
            this._clients = clients;

            if (_clients != null)
            {
                Clients client = WebAgencyRequestsEntities.GetContext().Clients.FirstOrDefault(r => r.ClientId == _clients.ClientId);
                if (client != null)
                {
                    TxtFirstName.Text = client.FirstName;
                    TxtLastName.Text = client.LastName;
                    TxtEmail.Text = client.Email;
                }

                listClients.DtgClients.ItemsSource = WebAgencyRequestsEntities.GetContext().Clients.ToList();
            }
        }
        private void BntSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var context = WebAgencyRequestsEntities.GetContext();

                if (string.IsNullOrWhiteSpace(TxtFirstName.Text) || string.IsNullOrWhiteSpace(TxtLastName.Text) || string.IsNullOrWhiteSpace(TxtEmail.Text))
                {
                    MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_clients == null)
                {
                    _clients = new Clients
                    {
                        FirstName = TxtFirstName.Text,
                        LastName = TxtLastName.Text,
                        Email = TxtEmail.Text
                    };

                    context.Clients.Add(_clients);
                }
                else
                {
                    _clients.FirstName = TxtFirstName.Text;
                    _clients.LastName = TxtLastName.Text;
                    _clients.Email = TxtEmail.Text;
                }

                context.SaveChanges();

                _listClients.DtgClients.ItemsSource = null;
                WebAgencyRequestsEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(entry => entry.Reload());
                _listClients.DtgClients.ItemsSource = WebAgencyRequestsEntities.GetContext().Clients.ToList();

                ClassFrame.frmObj.Navigate(new ListClientsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
