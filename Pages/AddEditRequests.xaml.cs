using RequestsCourse.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace RequestsCourse.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditRequests.xaml
    /// </summary>
    public partial class AddEditRequests : Page
    {

        private Requests _requests;
        private MainPage _mainPage;
        public AddEditRequests(MainPage mainPage, Requests requests)
        {
            InitializeComponent();
            DataContext = this;
            this._mainPage = mainPage;
            this._requests = requests;

            CmbClient.ItemsSource = WebAgencyRequestsEntities.GetContext().Clients
                .Select(c => new
                {
                    ClientId = c.ClientId,
                    FullName = c.LastName + " " + c.FirstName
                })
                .ToList();
            CmbClient.SelectedValuePath = "ClientId";
            CmbClient.DisplayMemberPath = "FullName";

            CmbStatusReq.ItemsSource = WebAgencyRequestsEntities.GetContext().Statuses.ToList();
            CmbStatusReq.SelectedValuePath = "StatusId";
            CmbStatusReq.DisplayMemberPath = "StatusName";

            if (_requests != null)
            {
                Requests request = WebAgencyRequestsEntities.GetContext().Requests.FirstOrDefault(r => r.RequestId == _requests.RequestId);
                if (request != null)
                {
                    TitleName.Text = request.Title;
                    RequestDesk.Text = request.Description;
                    DateCreated.SelectedDate = request.CreatedAt;
                }

                CmbClient.SelectedValue = _requests.ClientId;
                CmbStatusReq.SelectedValue = _requests.StatusId;
                mainPage.DtgRequest.ItemsSource = WebAgencyRequestsEntities.GetContext().Requests.ToList();
            }
        }

        private void BntSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var context = WebAgencyRequestsEntities.GetContext();

                if (string.IsNullOrWhiteSpace(TitleName.Text) || string.IsNullOrWhiteSpace(RequestDesk.Text) ||
                    DateCreated.SelectedDate == null || CmbClient.SelectedValue == null || CmbStatusReq.SelectedValue == null)
                {
                    MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int clientId = (int)CmbClient.SelectedValue;
                int statusId = (int)CmbStatusReq.SelectedValue;

                if (_requests == null)
                {
                    _requests = new Requests
                    {
                        Title = TitleName.Text,
                        Description = RequestDesk.Text,
                        CreatedAt = DateCreated.SelectedDate.Value,
                        ClientId = clientId,
                        StatusId = statusId
                    };

                    context.Requests.Add(_requests);
                }
                else
                {
                    _requests.Title = TitleName.Text;
                    _requests.Description = RequestDesk.Text;
                    _requests.CreatedAt = DateCreated.SelectedDate.Value;
                    _requests.ClientId = clientId;
                    _requests.StatusId = statusId;
                }

                context.SaveChanges();

                _mainPage.DtgRequest.ItemsSource = null;
                WebAgencyRequestsEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(entry => entry.Reload());
                _mainPage.DtgRequest.ItemsSource = WebAgencyRequestsEntities.GetContext().Requests
                    .Include(r => r.Clients)
                    .Include(r => r.Statuses)
                    .ToList();

                ClassFrame.frmObj.Navigate(new MainPage());
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
