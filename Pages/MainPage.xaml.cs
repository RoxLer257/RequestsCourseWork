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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            DtgRequest.ItemsSource = WebAgencyRequestsEntities.GetContext().Requests
                .Include(r => r.Clients)
                .Include(r => r.Statuses)
                .Include(r => r.Employees)
                .ToList();
            CheckUserRole();
        }

        private void CheckUserRole()
        {
            if (ClassFrame.RoleId == 2)
            {
                BtnStaticReq.Visibility = Visibility.Visible;
            }
            else
            {
                BtnStaticReq.Visibility = Visibility.Collapsed;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = TxtSearch.Text;
            var riskSearch = WebAgencyRequestsEntities.GetContext().Requests.
                Where(r => r.Clients.LastName.Contains(search)).ToList();

            DtgRequest.ItemsSource = riskSearch;
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            StartDate.SelectedDate = null;
            EndDate.SelectedDate = null;
            DtgRequest.ItemsSource = WebAgencyRequestsEntities.GetContext().Requests.ToList();
            TxtSearch.Text = "";
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? startDate = StartDate.SelectedDate;
                DateTime? endDate = EndDate.SelectedDate;

                if (startDate > endDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты окончания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var query = WebAgencyRequestsEntities.GetContext().Requests
                    .Include(ra => ra.Clients)
                    .Include(r => r.Statuses)
                    .AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(ra => ra.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(ra => ra.CreatedAt <= endDate.Value);
                }

                DtgRequest.ItemsSource = query.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации по дате: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddRequest_Click(object sender, RoutedEventArgs e)
        {
            ClassFrame.frmObj.Navigate(new AddEditRequests(this, null));
        }

        private void DelRequest_Click(object sender, RoutedEventArgs e)
        {
            var reqForRemoving = DtgRequest.SelectedItems.Cast<Requests>().ToList();

            if(reqForRemoving.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите заявки для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить {reqForRemoving.Count()} заявку(и)?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new WebAgencyRequestsEntities())
                    {
                        var reqCheck = reqForRemoving.Select(r => r.RequestId).Distinct().ToList();

                        foreach( var request in reqForRemoving)
                        {
                            var req = context.Requests.FirstOrDefault(r => r.RequestId == request.RequestId);
                            if(req != null)
                            {
                                context.Requests.Remove(req);
                            }
                        }

                        context.SaveChanges();

                        context.ChangeTracker.Entries().ToList().ForEach(entry => entry.Reload());

                        DtgRequest.ItemsSource = WebAgencyRequestsEntities.GetContext().Requests
                            .Include(r => r.Clients)
                            .Include(r => r.Statuses)
                            .ToList();

                        MessageBox.Show("Данные удалены");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void EditRequest_Click(object sender, RoutedEventArgs e)
        {
            if (DtgRequest.SelectedItem is Requests selectedRequest)
            {
                NavigationService.Navigate(new AddEditRequests(this, selectedRequest));
            }
            else
            {
                MessageBox.Show("Выберите заявку для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnListClient_Click(object sender, RoutedEventArgs e)
        {
            ClassFrame.frmObj.Navigate(new ListClientsPage());
        }

        private void BtnStaticReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var requests = DtgRequest.ItemsSource as IEnumerable<Requests>;

                if (requests == null || !requests.Any())
                {
                    MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var groupedRequests = requests.GroupBy(r => r.Statuses.StatusName);

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    foreach (var group in groupedRequests)
                    {
                        string statusName = group.Key;
                        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(statusName);

                        string[] headers = { "Название заявки", "Описание заявки", "Дата создания", "Фамилия клиента", "Имя клиента" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headers[i];
                        }

                        using (ExcelRange range = worksheet.Cells["A1:E1"])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(173, 216, 230));
                        }

                        int row = 2;
                        foreach (var request in group)
                        {
                            worksheet.Cells[row, 1].Value = request.Title;
                            worksheet.Cells[row, 2].Value = request.Description;
                            worksheet.Cells[row, 3].Value = request.CreatedAt?.ToString("dd.MM.yyyy");
                            worksheet.Cells[row, 4].Value = request.Clients.FirstName;
                            worksheet.Cells[row, 5].Value = request.Clients.LastName;
                            row++;
                        }

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    }

                    string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Excel отчеты");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    string filePath = Path.Combine(directoryPath, "Заявки_Отчет.xlsx");
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
