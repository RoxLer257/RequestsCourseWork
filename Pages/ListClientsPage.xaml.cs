using RequestsCourse.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Diagnostics;
using System.IO;

namespace RequestsCourse.Pages
{
    /// <summary>
    /// Логика взаимодействия для ListClientsPage.xaml
    /// </summary>
    public partial class ListClientsPage : Page
    {
        public ListClientsPage()
        {
            InitializeComponent();
            DtgClients.ItemsSource = WebAgencyRequestsEntities.GetContext().Requests.ToList();
            CheckUserRole();
        }

        private void CheckUserRole()
        {
            if (ClassFrame.RoleId == 2)
            {
                BtnStaticClient.Visibility = Visibility.Visible;
            }
            else
            {
                BtnStaticClient.Visibility= Visibility.Collapsed;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = TxtSearch.Text;
            var riskSearch = WebAgencyRequestsEntities.GetContext().Requests.
                Where(r => r.Clients.LastName.Contains(search)).ToList();

            DtgClients.ItemsSource = riskSearch;
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            //StartDate.SelectedDate = null;
            //EndDate.SelectedDate = null;
            DtgClients.ItemsSource = WebAgencyRequestsEntities.GetContext().Clients.ToList();
            TxtSearch.Text = "";
        }

        //private void Apply_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        DateTime? startDate = StartDate.SelectedDate;
        //        DateTime? endDate = EndDate.SelectedDate;

        //        if (startDate > endDate)
        //        {
        //            MessageBox.Show("Дата начала не может быть позже даты окончания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //            return;
        //        }

        //        var query = WebAgencyRequestsEntities.GetContext().Requests
        //            .Include(ra => ra.Clients)
        //            .Include(r => r.Statuses)
        //            .AsQueryable();

        //        if (startDate.HasValue)
        //        {
        //            query = query.Where(ra => ra.CreatedAt >= startDate.Value);
        //        }

        //        if (endDate.HasValue)
        //        {
        //            query = query.Where(ra => ra.CreatedAt <= endDate.Value);
        //        }

        //        DtgClients.ItemsSource = query.ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при фильтрации по дате: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void BtnListRequests_Click(object sender, RoutedEventArgs e)
        {
            ClassFrame.frmObj.Navigate(new MainPage());
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            ClassFrame.frmObj.Navigate(new AddEditClient(this, null));
        }

        private void DelClient_Click(object sender, RoutedEventArgs e)
        {
            var clientForRemoving = DtgClients.SelectedItems.Cast<Clients>().ToList();
            if (clientForRemoving.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите клиентов для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"Удалить {clientForRemoving.Count()} клиента(ов)?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new WebAgencyRequestsEntities())
                    {
                        var clientCheck = clientForRemoving.Select(r => r.ClientId).Distinct().ToList();

                        foreach (var client in clientForRemoving)
                        {
                            var clients = context.Clients.FirstOrDefault(r => r.ClientId == client.ClientId);
                            if (clients != null)
                            {
                                context.Clients.Remove(clients);
                            }
                        }

                        context.SaveChanges();

                        context.ChangeTracker.Entries().ToList().ForEach(entry => entry.Reload());

                        DtgClients.ItemsSource = WebAgencyRequestsEntities.GetContext().Clients.ToList();

                        MessageBox.Show("Данные удалены");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            if (DtgClients.SelectedItem is Clients selectedClient)
            {
                NavigationService.Navigate(new AddEditClient(this, selectedClient));
            }
            else
            {
                MessageBox.Show("Выберите клиента для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnStaticClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var clients = DtgClients.ItemsSource as IEnumerable<Clients>;

                if (clients == null || !clients.Any())
                {
                    MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Клиенты");

                    string[] headers = { "Фамилия клиента", "Имя клиента", "Почта клиента" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                    }

                    using (ExcelRange range = worksheet.Cells["A1:C1"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(173, 216, 230));
                    }

                    int row = 2;
                    foreach (var client in clients)
                    {
                        worksheet.Cells[row, 1].Value = client.FirstName;
                        worksheet.Cells[row, 2].Value = client.LastName;
                        worksheet.Cells[row, 3].Value = client.Email;
                        row++;
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Excel отчеты");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    string filePath = Path.Combine(directoryPath, "Клиенты_Отчет.xlsx");
                    excelPackage.SaveAs(new FileInfo(filePath));

                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
