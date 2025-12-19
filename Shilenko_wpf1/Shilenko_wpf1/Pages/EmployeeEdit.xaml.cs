using Microsoft.Win32;
using Shilenko_wpf1.Models;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using Shilenko_wpf1.Validators;


namespace Shilenko_wpf1.Pages
{
    public partial class EmployeeEdit : Page
    {
        private Employees _currentEmployee;
        private AutobaseEntities _db;
        private string _imagePath;

        public EmployeeEdit(Employees employee)
        {
            InitializeComponent();
            _currentEmployee = employee ?? new Employees();
            _db = new AutobaseEntities();
            LoadPositions();
            LoadEmployeeData();
        }

        private void LoadPositions()
        {
            try
            {
                var positions = _db.EmployeePositions.ToList();
                cmbPosition.ItemsSource = positions;
                cmbPosition.DisplayMemberPath = "PositionName";
                cmbPosition.SelectedValuePath = "PositionID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки должностей: {ex.Message}");
            }
        }

        private void LoadEmployeeData()
        {
            if (_currentEmployee.EmployeeID == 0) // Новый сотрудник
            {
                tbTitle.Text = "Добавление сотрудника";
                btnDelete.Visibility = Visibility.Collapsed;
                dpHireDate.SelectedDate = DateTime.Today;

                // Устанавливаем изображение по умолчанию
                SetDefaultImage();
                txtImagePath.Text = "Изображение не выбрано";
            }
            else // Редактирование существующего
            {
                tbTitle.Text = "Редактирование сотрудника";
                btnDelete.Visibility = Visibility.Visible;

                // Загружаем данные сотрудника
                txtLastName.Text = _currentEmployee.LastName;
                txtFirstName.Text = _currentEmployee.FirstName;
                cmbPosition.SelectedValue = _currentEmployee.PositionID;
                dpHireDate.SelectedDate = _currentEmployee.HireDate;
                txtPhone.Text = _currentEmployee.Phone;
                txtEmail.Text = _currentEmployee.Email;

                // TODO: Здесь можно добавить загрузку сохраненного изображения из базы
                // Пока просто показываем, что изображение не выбрано
                SetDefaultImage();
                txtImagePath.Text = "Изображение не сохранено в базе";
            }
            // Загружаем сохраненное изображение если есть
            if (!string.IsNullOrEmpty(_currentEmployee.PhotoPath))
            {
                try
                {
                    string imagesFolder = GetEmployeeImagesFolder();
                    string imageFullPath = Path.Combine(imagesFolder, _currentEmployee.PhotoPath);

                    if (File.Exists(imageFullPath))
                    {
                        // Загружаем изображение
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imageFullPath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        imgPhoto.Source = bitmap;
                        txtImagePath.Text = imageFullPath;
                    }
                    else
                    {
                        SetDefaultImage();
                        txtImagePath.Text = "Файл изображения не найден: " + _currentEmployee.PhotoPath;
                    }
                }
                catch (Exception ex)
                {
                    SetDefaultImage();
                    txtImagePath.Text = $"Ошибка загрузки: {ex.Message}";
                }
            }
            else
            {
                SetDefaultImage();
                txtImagePath.Text = "Изображение не сохранено";
            }
        }

        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            openFileDialog.Title = "Выберите фотографию сотрудника";

            if (openFileDialog.ShowDialog() == true)
            {
                _imagePath = openFileDialog.FileName;
                txtImagePath.Text = _imagePath; // Показываем путь

                try
                {
                    // Загружаем изображение
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Делаем изображение потокобезопасным

                    imgPhoto.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    SetDefaultImage();
                }
            }
        }

        private void SetDefaultImage()
        {
            try
            {
                var defaultImage = new BitmapImage();
                defaultImage.BeginInit();
                defaultImage.UriSource = new Uri("pack://application:,,,/Resources/picture.png");
                defaultImage.CacheOption = BitmapCacheOption.OnLoad;
                defaultImage.EndInit();
                defaultImage.Freeze();

                imgPhoto.Source = defaultImage;
            }
            catch
            {
                // Если файл по умолчанию не найден, используем пустой источник
                imgPhoto.Source = null;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var model = new EmployeeValidationModel
            {
                LastName = txtLastName.Text?.Trim(),
                FirstName = txtFirstName.Text?.Trim(),
                PositionID = cmbPosition.SelectedValue as int?,
                HireDate = dpHireDate.SelectedDate,
                Phone = txtPhone.Text?.Trim(),
                Email = txtEmail.Text?.Trim(),
                ImagePath = _imagePath
            };

            var validator = new EmployeeValidator();
            var result = validator.Validate(model);

            if (!result.IsValid)
            {
                MessageBox.Show($"Ошибки:\n{result.ErrorMessage}",
                               "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
                errors.AppendLine("Введите фамилию");
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
                errors.AppendLine("Введите имя");
            if (cmbPosition.SelectedItem == null)
                errors.AppendLine("Выберите должность");
            if (dpHireDate.SelectedDate == null)
                errors.AppendLine("Выберите дату приёма");
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
                errors.AppendLine("Введите email");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Заполняем данные сотрудника
                _currentEmployee.LastName = txtLastName.Text;
                _currentEmployee.FirstName = txtFirstName.Text;
                _currentEmployee.PositionID = (int)cmbPosition.SelectedValue;
                _currentEmployee.HireDate = dpHireDate.SelectedDate.Value;
                _currentEmployee.Phone = txtPhone.Text;
                _currentEmployee.Email = txtEmail.Text;

                // Сохраняем сотрудника в БД чтобы получить EmployeeID
                if (_currentEmployee.EmployeeID == 0) // Новый сотрудник
                {
                    _db.Employees.Add(_currentEmployee);
                }

                _db.SaveChanges(); // Сохраняем чтобы получить EmployeeID для новых

                // Если выбрано новое изображение, сохраняем его
                if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
                {
                    string imagesFolder = GetEmployeeImagesFolder();

                    // Создаем уникальное имя файла
                    string fileName = $"emp_{_currentEmployee.EmployeeID}_{Guid.NewGuid():N}{Path.GetExtension(_imagePath)}";
                    string destinationPath = Path.Combine(imagesFolder, fileName);

                    // Копируем файл
                    File.Copy(_imagePath, destinationPath, true);

                    // Сохраняем только имя файла в базу (без полного пути)
                    _currentEmployee.PhotoPath = fileName;

                    // Обновляем запись в базе
                    var employeeInDb = _db.Employees.Find(_currentEmployee.EmployeeID);
                    if (employeeInDb != null)
                    {
                        employeeInDb.PhotoPath = fileName;
                        _db.SaveChanges();
                    }

                    MessageBox.Show($"Изображение сохранено", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (_currentEmployee.EmployeeID > 0) // Редактирование существующего
                {
                    // Обновляем другие данные для существующего сотрудника
                    var employeeInDb = _db.Employees.Find(_currentEmployee.EmployeeID);
                    if (employeeInDb != null)
                    {
                        employeeInDb.LastName = _currentEmployee.LastName;
                        employeeInDb.FirstName = _currentEmployee.FirstName;
                        employeeInDb.PositionID = _currentEmployee.PositionID;
                        employeeInDb.HireDate = _currentEmployee.HireDate;
                        employeeInDb.Phone = _currentEmployee.Phone;
                        employeeInDb.Email = _currentEmployee.Email;
                        _db.SaveChanges();
                    }
                }

                MessageBox.Show("Данные сохранены успешно", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private string GetEmployeeImagesFolder()
        {
            // Путь к исполняемому файлу
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string imagesFolder = Path.Combine(appPath, "EmployeeImages");

            // Создаем папку если ее нет
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            return imagesFolder;
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить этого сотрудника?",
                "Подтверждение удаления", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var employeeToDelete = _db.Employees.Find(_currentEmployee.EmployeeID);
                    if (employeeToDelete != null)
                    {
                        _db.Employees.Remove(employeeToDelete);
                        _db.SaveChanges();
                        MessageBox.Show("Сотрудник удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService.GoBack();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}