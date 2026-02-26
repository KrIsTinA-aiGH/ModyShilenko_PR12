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
    /// <summary>
    /// Страница добавления и редактирования сотрудника.
    /// Позволяет вводить данные сотрудника, выбирать должность,
    /// загружать фотографию и сохранять изменения в базе данных.
    /// </summary>
    public partial class EmployeeEdit : Page
    {
        /// <summary>Текущий редактируемый сотрудник (или новый экземпляр)</summary>
        private Employees _currentEmployee;

        /// <summary>Контекст базы данных для операций с сущностями</summary>
        private AutobaseEntities _db;

        /// <summary>Путь к выбранному изображению на диске</summary>
        private string _imagePath;

        /// <summary>
        /// Конструктор страницы редактирования сотрудника.
        /// </summary>
        /// <param name="employee">Сотрудник для редактирования или null для создания нового</param>
        public EmployeeEdit(Employees employee)
        {
            InitializeComponent();
            _currentEmployee = employee ?? new Employees();
            _db = new AutobaseEntities();
            LoadPositions();
            LoadEmployeeData();
        }

        /// <summary>
        /// Загружает список должностей из базы данных в ComboBox.
        /// </summary>
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

        /// <summary>
        /// Загружает данные сотрудника в поля формы.
        /// Различает режимы «новый сотрудник» и «редактирование».
        /// </summary>
        private void LoadEmployeeData()
        {
            // Режим добавления нового сотрудника
            if (_currentEmployee.EmployeeID == 0)
            {
                tbTitle.Text = "Добавление сотрудника";
                btnDelete.Visibility = Visibility.Collapsed;
                dpHireDate.SelectedDate = DateTime.Today;

                // Устанавливаем изображение по умолчанию
                SetDefaultImage();
                txtImagePath.Text = "Изображение не выбрано";
            }
            else // Режим редактирования существующего сотрудника
            {
                tbTitle.Text = "Редактирование сотрудника";
                btnDelete.Visibility = Visibility.Visible;

                // Заполняем поля данными из объекта сотрудника
                txtLastName.Text = _currentEmployee.LastName;
                txtFirstName.Text = _currentEmployee.FirstName;
                cmbPosition.SelectedValue = _currentEmployee.PositionID;
                dpHireDate.SelectedDate = _currentEmployee.HireDate;
                txtPhone.Text = _currentEmployee.Phone;
                txtEmail.Text = _currentEmployee.Email;

                // Пока просто показываем, что изображение не выбрано
                SetDefaultImage();
                txtImagePath.Text = "Изображение не сохранено в базе";
            }

            /* 
             * Попытка загрузить сохранённое изображение сотрудника,
             * если указан путь в базе данных.
             */
            if (!string.IsNullOrEmpty(_currentEmployee.PhotoPath))
            {
                try
                {
                    string imagesFolder = GetEmployeeImagesFolder();
                    string imageFullPath = Path.Combine(imagesFolder, _currentEmployee.PhotoPath);

                    if (File.Exists(imageFullPath))
                    {
                        // Загружаем изображение с кэшированием и заморозкой для потокобезопасности
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

        /// <summary>
        /// Обработчик нажатия кнопки «Выбрать изображение».
        /// Открывает диалог выбора файла и загружает изображение в интерфейс.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
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
                    // Загружаем изображение с кэшированием и заморозкой
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

        /// <summary>
        /// Устанавливает изображение по умолчанию из ресурсов приложения.
        /// </summary>
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

        /// <summary>
        /// Обработчик нажатия кнопки «Сохранить».
        /// Выполняет валидацию, сохранение данных сотрудника и изображения в БД.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Создаём модель валидации с данными из формы
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

            // Выполняем валидацию через EmployeeValidator
            var validator = new EmployeeValidator();
            var result = validator.Validate(model);

            if (!result.IsValid)
            {
                MessageBox.Show($"Ошибки:\n{result.ErrorMessage}",
                                "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Дополнительная проверка обязательных полей
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
                // Заполняем объект сотрудника данными из формы
                _currentEmployee.LastName = txtLastName.Text;
                _currentEmployee.FirstName = txtFirstName.Text;
                _currentEmployee.PositionID = (int)cmbPosition.SelectedValue;
                _currentEmployee.HireDate = dpHireDate.SelectedDate.Value;
                _currentEmployee.Phone = txtPhone.Text;
                _currentEmployee.Email = txtEmail.Text;

                // Сохраняем сотрудника в БД (для новых записей получаем EmployeeID)
                if (_currentEmployee.EmployeeID == 0)
                {
                    _db.Employees.Add(_currentEmployee);
                }

                _db.SaveChanges(); // Сохраняем, чтобы получить EmployeeID для новых записей

                /* 
                 * Если выбрано новое изображение — копируем его в папку приложения
                 * и сохраняем имя файла в базе данных.
                 */
                if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
                {
                    string imagesFolder = GetEmployeeImagesFolder();

                    // Создаем уникальное имя файла: emp_{ID}_{GUID}.ext
                    string fileName = $"emp_{_currentEmployee.EmployeeID}_{Guid.NewGuid():N}{Path.GetExtension(_imagePath)}";
                    string destinationPath = Path.Combine(imagesFolder, fileName);

                    // Копируем файл с перезаписью
                    File.Copy(_imagePath, destinationPath, true);

                    // Сохраняем только имя файла в базу (без полного пути)
                    _currentEmployee.PhotoPath = fileName;

                    // Обновляем запись в базе с путём к изображению
                    var employeeInDb = _db.Employees.Find(_currentEmployee.EmployeeID);
                    if (employeeInDb != null)
                    {
                        employeeInDb.PhotoPath = fileName;
                        _db.SaveChanges();
                    }

                    MessageBox.Show("Изображение сохранено", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (_currentEmployee.EmployeeID > 0) // Редактирование без нового изображения
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

        /// <summary>
        /// Возвращает путь к папке для хранения изображений сотрудников.
        /// Создаёт папку, если она не существует.
        /// </summary>
        /// <returns>Полный путь к папке EmployeeImages</returns>
        private string GetEmployeeImagesFolder()
        {
            // Путь к исполняемому файлу приложения
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string imagesFolder = Path.Combine(appPath, "EmployeeImages");

            // Создаем папку, если её нет
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            return imagesFolder;
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Удалить».
        /// Удаляет сотрудника из базы данных после подтверждения.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Подтверждение удаления
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

        /// <summary>
        /// Обработчик нажатия кнопки «Отмена».
        /// Возвращает пользователя на предыдущую страницу.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}