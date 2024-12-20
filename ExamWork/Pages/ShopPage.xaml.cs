using ServiceLayer;
using ServiceLayer.DTOs;
using ServiceLayer.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExamWork.Pages
{
    /// <summary>
    /// Логика взаимодействия для ShopPage.xaml
    /// </summary>
    public partial class ShopPage : Page
    {
        public decimal MinCost { get; set; } = 0;
        public decimal? MaxCost { get; set; } = null;
        public string SortMethod { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string SearchedText { get; set; } = "";

        public static bool _isProgr = false;//флаг, проверяющий меняется ли текст программно
        public bool searchTextBoxIsFill = false;//свойство, показывающее заполнена ли чем либо строка поиска или нет
        public static List<ProductDTO> examProducts = new();
        public static readonly ExamProductService _productService = new();

        public ShopPage()
        {
            InitializeComponent();
            searchTextBox.TextChanged += SearchTextBox_TextChanged;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ManufacturerFilterComboBox.ItemsSource = await _productService.GetManufacturersAsync();
            CreateProductsList(SearchedText, SortMethod, MinCost, MaxCost, Manufacturer);//Вызов метода для вывода товаров на странцу
            searchTextBox.Foreground = new SolidColorBrush(Colors.Gray);

            //вывод информации о пользователе
            if (CurrentUser.IsGuest)
                CurrentUserLabel.Content = "Вы вошли как гость";
            else
                CurrentUserLabel.Content = $"{CurrentUser.UserName.Substring(0, 1)}.{CurrentUser.UserPatronymic.Substring(0, 1)}. {CurrentUser.UserSurname}";

            if (MakeOrderPage.ExamOrderList.Count == 0)
                orderButton.Visibility = Visibility.Hidden;

            if (CurrentUser.RoleID != 2)//если в приложение зашел менеджер или администратор, то им будет доступна кнопка перейти к просмотру всех заказов
                GoToAllOrdersButton.Visibility = Visibility.Visible;
            else
                GoToAllOrdersButton.Visibility = Visibility.Collapsed;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)//обновление списка товаров по поисковой строке
        {
            if (!_isProgr)
            {
                if (searchTextBox.Text != string.Empty)
                {
                    searchTextBoxIsFill = true;//если мы что-то написали в строку поиска, свойство searchTextBoxIsFill становится true
                }
                else
                {
                    SearchedText = searchTextBox.Text;
                    CreateProductsList(SearchedText, SortMethod, MinCost, MaxCost, Manufacturer);
                    searchTextBoxIsFill = false;//если мы стерли текст в строке поиска, свойство searchTextBoxIsFill становится false
                }
                if (searchedProductNameLabel != null)
                {
                    searchedProductNameLabel.Content = searchTextBox.Text;
                    searchedProductNameLabel.Content = searchTextBox.Text.Length > 7 ? searchTextBox.Text.Substring(0, 7) + "..." : searchTextBox.Text;
                }
                if (searchTextBoxIsFill)
                {
                    SearchedText = searchTextBox.Text;
                    CreateProductsList(SearchedText, SortMethod, MinCost, MaxCost, Manufacturer);//Вызов метода для вывода товаров на странцу
                }
            }
        }

        //вывод стандартной надписи Найти в Ароматном мире серым цветов в строке поиска, если там пусто
        private void Page_GotFocus(object sender, RoutedEventArgs e)//обработчик события, срабатывает, когда мы нажимаем на страницу, но не на строку поиска
        {
            if (!searchTextBox.IsFocused && searchTextBox.Text == string.Empty)//если строка поиска пустая и потеряла фокус, то ей устанавливаются новый свойства: Foreground и Text
            {
                searchTextBox.TextChanged -= SearchTextBox_TextChanged;
                searchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
                searchTextBox.Text = "Найти в Ароматном мире";
                searchTextBox.TextChanged += SearchTextBox_TextChanged;
                searchedProductNameLabel.Content = string.Empty;
                searchTextBoxIsFill = false;//строка поиска не заполнена текстом для поиска
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)//обработчик события, срабатывает, когда строка поиска получила фокус, то есть тогда, когда мы на нее нажали
        {
            if (searchTextBoxIsFill == false)//если строка поиска не была заполнена текстом для поиска, то
            {
                searchTextBox.Text = string.Empty;//строка поиска очищается от текста "Найти в Ароматном мире"
                searchTextBox.Foreground = new SolidColorBrush(Colors.Black);//цвет снова ставится на черный
            }
        }

        private async void CreateProductsList(string subs, string sortMethod, decimal min, decimal? max, string manufacturer)//метод для вывода товаров на странцу, согласно условиям фильтрации
        {
            examProducts.Clear();
            productsStackPanel.Children.Clear();
            examProducts = await _productService.GetProductsAsync(subs, sortMethod, min, max, manufacturer);
            int productsCount = examProducts.Count;
            for (int i = 0; i < productsCount; i++)//в цикле создаются отдельные элементы в которых хранятся данные о товарах
            {
                //для обводки панели одного товара
                Border productBorder = new();
                productBorder.Width = 600;
                productBorder.Margin = new Thickness(80, 5, 0, 5);
                productBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC6600"));
                if (examProducts[i].ProductQuantityInStock == 0)
                {
                    productBorder.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                }
                productBorder.BorderThickness = new(3);

                StackPanel productPanel = new();
                productPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCC99"));

                if (examProducts[i].ProductQuantityInStock == 0)
                {
                    productPanel.Background = new SolidColorBrush(Colors.Gray);
                }

                Label nameDataLabel = new();
                nameDataLabel.Content = examProducts[i].ProductName;
                if (examProducts[i].ProductQuantityInStock == 0)
                {
                    nameDataLabel.Content = examProducts[i].ProductName + " (Нет на складе)";
                }
                productPanel.Children.Add(nameDataLabel);

                Label desciptionDataLabel = new();
                desciptionDataLabel.Content = examProducts[i].ProductDescription;
                productPanel.Children.Add(desciptionDataLabel);

                StackPanel manufacturerPanel = new();
                manufacturerPanel.Orientation = Orientation.Horizontal;
                Label manufacturerLabel = new();
                manufacturerLabel.Content = "Производитель товара:";
                Label manufacturerDataLabel = new();
                manufacturerDataLabel.Content = examProducts[i].ProductManufacturer;
                manufacturerPanel.Children.Add(manufacturerLabel);
                manufacturerPanel.Children.Add(manufacturerDataLabel);
                productPanel.Children.Add(manufacturerPanel);

                DockPanel costDockPanel = new();//DockPanel чтобы в будущем в этой строке метки со скидкой не смещались, а были по правому краю
                Label costLabel = new();
                costLabel.Content = "Цена товара:";
                TextBlock costDataTextBlock = new();//TextBlock для возможности задания свойства зачеркнутости текста
                costDataTextBlock.Text = examProducts[i].ProductCost.ToString();
                Button addButton = new();
                addButton.Click += AddProduct_Click;
                addButton.Content = $"Заказать";
                addButton.FontSize = 12;
                costDockPanel.Children.Add(costLabel);
                costDockPanel.Children.Add(addButton);
                DockPanel.SetDock(addButton, Dock.Right);//размещение по правому краю, чтобы не было смещения при изменении ширины других меток
                costDockPanel.Children.Add(costDataTextBlock);
                if (examProducts[i].ProductDiscountAmount > 0)//если скидка на товар есть, цена зачеркивается и создается метка с новой ценой
                {
                    costDataTextBlock.TextDecorations = TextDecorations.Strikethrough;
                    costDataTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    Label costWithDiscountDataLabel = new();
                    decimal resultCost = (decimal)Convert.ToDouble(costDataTextBlock.Text) * (100 - Convert.ToInt32(examProducts[i].ProductDiscountAmount)) / 100;
                    costWithDiscountDataLabel.Content = resultCost;
                    costDockPanel.Children.Add(costWithDiscountDataLabel);
                }
                productPanel.Children.Add(costDockPanel);

                if (examProducts[i].ProductQuantityInStock == 0)
                    addButton.IsEnabled = false;
                productPanel.Tag = i.ToString();

                productBorder.Child = productPanel;
                productsStackPanel.Children.Add(productBorder);
                searchedProductsCount.Content = productsStackPanel.Children.Count + " из " + await _productService.GetProductsCountAsync();
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)//добавление товара в корзину
        {
            // Получение источника события (Button)
            Button addButton = sender as Button;

            // Проверка, что источник события не null
            if (addButton != null)
            {
                // Получение DockPanel, которому принадлежит Button
                DockPanel costDockPanel = addButton.Parent as DockPanel;

                // Проверка, что DockPanel не null
                if (costDockPanel != null)
                {
                    // Получение StackPanel, которому принадлежит DockPanel
                    StackPanel productPanel = costDockPanel.Parent as StackPanel;

                    // Проверка, что элемент управления не null и имеет свойство Tag
                    if (productPanel != null && productPanel.Tag != null)
                    {
                        var existingProduct = MakeOrderPage.ExamOrderList.FirstOrDefault(p => p.ProductName == examProducts[Convert.ToInt32(productPanel.Tag)].ProductName);
                        if (existingProduct != null)
                        {
                            existingProduct.ProductCountInOrder += 1;
                        }
                        else
                        {
                            examProducts[Convert.ToInt32(productPanel.Tag)].ProductCountInOrder += 1;
                            MakeOrderPage.ExamOrderList.Add(examProducts[Convert.ToInt32(productPanel.Tag)]);
                            orderButton.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        private void ManufactorerFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)//изменение условий вывода товаров с помощью фильтра
        {
            if (ManufacturerFilterComboBox.SelectedIndex != -1)
            {
                GetProductsWithFilter();
            }
        }

        private void GetProductsWithFilter()
        {
            if (SortComboBox.SelectedIndex == 0)
                SortMethod = "asc";
            else if (SortComboBox.SelectedIndex == 1)
                SortMethod = "desc";

            Manufacturer = ManufacturerFilterComboBox.SelectedIndex != 0 ? $"{ManufacturerFilterComboBox.SelectedItem}" : "";
            CreateProductsList(SearchedText, SortMethod, MinCost, MaxCost, Manufacturer);//обновление списка товаров с фильтрацией
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedIndex != -1)
            {
                GetProductsWithFilter();
            }
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new MakeOrderPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new AuthorizationPage());
            MakeOrderPage.ExamOrderList.Clear();
            YourOrdersPage.examCreatedOrdersList.Clear();
        }

        private void GoToYourOrders_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new YourOrdersPage());
        }

        private void GoToAllOrders_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new AllOrdersPage());
        }

        private void FilterOff_Click(object sender, RoutedEventArgs e)//сброс фильтрации и сортировки
        {
            _isProgr = true;
            searchTextBox.Text = string.Empty;
            SearchedText = string.Empty;
            searchedProductNameLabel.Content = string.Empty;
            SortComboBox.SelectedIndex = -1;
            ManufacturerFilterComboBox.SelectedIndex = -1;
            MinCostTextBox.Text = string.Empty;
            MaxCostTextBox.Text = string.Empty;
            MinCost = 0;
            MaxCost = null;
            SortMethod = "";
            Manufacturer = "";
            _isProgr = false;
            CreateProductsList(SearchedText, SortMethod, MinCost, MaxCost, Manufacturer);//обновление списка товаров без фильтрации
        }

        private void Cost_TextChanged(object sender, TextChangedEventArgs e)//Фильтрацие по ценовому диапазону
        {
            if (!_isProgr)
            {
                if (int.TryParse(MinCostTextBox.Text, out int minCost) || MinCostTextBox.Text == "")
                {
                    if (int.TryParse(MaxCostTextBox.Text, out int maxCost) || MaxCostTextBox.Text == "")
                    {
                        MinCost = MinCostTextBox.Text == "" ? 0 : minCost;
                        MaxCost = MaxCostTextBox.Text == "" ? null : maxCost;
                        CreateProductsList(SearchedText, SortMethod, MinCost, MaxCost, Manufacturer);
                    }
                }
            }
        }
    }
}