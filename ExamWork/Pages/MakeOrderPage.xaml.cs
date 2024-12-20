using ServiceLayer;
using ServiceLayer.DTOs;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ExamWork.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public partial class MakeOrderPage : Page
    {
        public static List<ProductDTO> ExamOrderList = new();
        public static int orderInProductsCount;
        public static decimal? totalCost;
        public static decimal? totalDiscount;
        public static List<ExamPickupPoint> examPickupPoints = new();
        public static List<int> existingPickupCodes = new();
        public static readonly ExamPickupPointService _pickupPointService = new();
        public static readonly ExamOrderService _orderService = new();
        public static readonly ExamOrderProductService _orderProductService = new();

        public MakeOrderPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CreateOrderList();

            examPickupPoints = await _pickupPointService.GetPickupPointsAsync();
            existingPickupCodes = await _orderService.GetExistingPickupCodesAsync();

            //вывод информации о пользователе
            if (CurrentUser.IsGuest)
                CurrentUserLabel.Content = "Вы вошли как гость";
            else
                CurrentUserLabel.Content = $"{CurrentUser.UserName.Substring(0, 1)}.{CurrentUser.UserPatronymic.Substring(0, 1)}. {CurrentUser.UserSurname}";

            PickupPointsComboBox.ItemsSource = examPickupPoints;

            if (CurrentUser.RoleID != 2)//если в приложение зашел менеджер или администратор, то им будет доступна кнопка перейти к просмотру всех заказов
                GoToAllOrdersButton.Visibility = Visibility.Visible;
            else
                GoToAllOrdersButton.Visibility = Visibility.Collapsed;
        }

        private void CreateOrderList()//метод для вывода товаров в корзине на странцу
        {
            productsInOrderStackPanel.Children.Clear();
            int productsCount = ExamOrderList.Count;
            for (int i = 0; i < productsCount; i++)//в цикле создаются отдельные элементы в которых хранятся данные о товарах в корзине
            {
                Border productBorder = new();
                productBorder.Width = 600;
                productBorder.Margin = new Thickness(80, 5, 0, 5);
                productBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC6600"));
                productBorder.BorderThickness = new(3);

                StackPanel productPanel = new();
                productPanel.Tag = i;
                productPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCC99"));

                Image productImage = new();
                productImage.Source = new BitmapImage(new Uri(ExamOrderList[i].ProductPhoto));
                productImage.Width = 200;
                productImage.Height = 200;
                productPanel.Children.Add(productImage);

                StackPanel articleNumberPanel = new();
                articleNumberPanel.Orientation = Orientation.Horizontal;
                Label articleNumberLabel = new();
                articleNumberLabel.Content = "Артикул:";
                Label articleDataLabel = new();
                articleDataLabel.Content = ExamOrderList[i].ProductArticleNumber;
                articleNumberPanel.Children.Add(articleNumberLabel);
                articleNumberPanel.Children.Add(articleDataLabel);
                productPanel.Children.Add(articleNumberPanel);

                Label nameDataLabel = new();
                nameDataLabel.Content = ExamOrderList[i].ProductName;
                productPanel.Children.Add(nameDataLabel);

                Label desciptionDataLabel = new();
                desciptionDataLabel.Content = ExamOrderList[i].ProductDescription;
                productPanel.Children.Add(desciptionDataLabel);

                StackPanel categoryPanel = new();
                categoryPanel.Orientation = Orientation.Horizontal;
                Label categoryLabel = new();
                categoryLabel.Content = "Категория товара:";
                Label categoryDataLabel = new();
                categoryDataLabel.Content = ExamOrderList[i].ProductCategory;
                categoryPanel.Children.Add(categoryLabel);
                categoryPanel.Children.Add(categoryDataLabel);
                productPanel.Children.Add(categoryPanel);

                StackPanel manufacturerPanel = new StackPanel();
                manufacturerPanel.Orientation = Orientation.Horizontal;
                Label manufacturerLabel = new();
                manufacturerLabel.Content = "Производитель товара:";
                Label manufacturerDataLabel = new();
                manufacturerDataLabel.Content = ExamOrderList[i].ProductManufacturer;
                manufacturerPanel.Children.Add(manufacturerLabel);
                manufacturerPanel.Children.Add(manufacturerDataLabel);
                productPanel.Children.Add(manufacturerPanel);

                DockPanel costDockPanel = new DockPanel();//DockPanel чтобы в будущем в этой строке метки со скидкой не смещались, а были по правому краю
                Label costLabel = new();
                costLabel.Content = "Цена товара:";
                TextBlock costDataTextBlock = new TextBlock();//TextBlock для возможности задания свойства зачеркнутости текста
                costDataTextBlock.Text = ExamOrderList[i].ProductCost.ToString();
                Label discountLabel = new();
                discountLabel.Content = $"Скидка:";
                discountLabel.FontSize = 12;
                Label discountDataLabel = new();
                discountDataLabel.FontSize = 12;
                discountDataLabel.Content = ExamOrderList[i].ProductDiscountAmount;
                costDockPanel.Children.Add(costLabel);
                costDockPanel.Children.Add(discountDataLabel);
                DockPanel.SetDock(discountDataLabel, Dock.Right);//размещение по правому краю, чтобы не было смещения при изменении ширины других меток
                costDockPanel.Children.Add(discountLabel);
                DockPanel.SetDock(discountLabel, Dock.Right);//размещение по правому краю, чтобы не было смещения при изменении ширины других меток
                if (ExamOrderList[i].ProductDiscountAmount > 0)//если скидка на товар есть, цена зачеркивается и создается метка с новой ценой
                {
                    costDataTextBlock.TextDecorations = TextDecorations.Strikethrough;
                    costDataTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    Label costWithDiscountDataLabel = new();
                    decimal resultCost = (decimal)Convert.ToDouble(costDataTextBlock.Text) * (100 - Convert.ToInt32(discountDataLabel.Content)) / 100;
                    costWithDiscountDataLabel.Content = resultCost;
                    costDockPanel.Children.Add(costWithDiscountDataLabel);
                }
                costDockPanel.Children.Add(costDataTextBlock);
                productPanel.Children.Add(costDockPanel);

                StackPanel productStatusPanel = new();
                productStatusPanel.Orientation = Orientation.Horizontal;
                Label productStatusLabel = new();
                productStatusLabel.Content = "Статус:";
                Label productStatusDataLabel = new();
                productStatusDataLabel.Content = ExamOrderList[i].ProductStatus;
                productStatusPanel.Children.Add(productStatusLabel);
                productStatusPanel.Children.Add(productStatusDataLabel);
                productPanel.Children.Add(productStatusPanel);

                StackPanel productQuantityInStockPanel = new();
                productQuantityInStockPanel.Orientation = Orientation.Horizontal;
                Label productQuantityInStockLabel = new();
                productQuantityInStockLabel.Content = "Количество на складе:";
                Label productQuantityInStockDataLabel = new();
                productQuantityInStockDataLabel.Content = ExamOrderList[i].ProductQuantityInStock;
                productQuantityInStockPanel.Children.Add(productQuantityInStockLabel);
                productQuantityInStockPanel.Children.Add(productQuantityInStockDataLabel);
                productPanel.Children.Add(productQuantityInStockPanel);

                DockPanel countPanel = new();
                CountControl countControl = new();
                countControl.Tag = i;
                countControl.HorizontalAlignment = HorizontalAlignment.Left;
                countControl.Value = ExamOrderList[i].ProductCountInOrder;
                countControl.MaxValue = ExamOrderList[i].ProductQuantityInStock;
                countControl.countTextBox.Text = ExamOrderList[i].ProductCountInOrder.ToString();
                countControl.ValueChanged += CountControl_ValueChanged;
                Button deleteButton = new();
                deleteButton.Click += DeleteButton_Click;
                Image deleteImage = new();
                deleteButton.Width = 50;
                deleteButton.HorizontalAlignment = HorizontalAlignment.Right;
                deleteImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/delete.png"));
                deleteButton.Content = deleteImage;
                DockPanel.SetDock(deleteButton, Dock.Right);
                countPanel.Children.Add(deleteButton);
                countPanel.Children.Add(countControl);
                productPanel.Children.Add(countPanel);

                productBorder.Child = productPanel;
                productsInOrderStackPanel.Children.Add(productBorder);
            }
            UpdateProductsCount();
            UpdateDiscount();
            UpdateCost();
        }

        private void UpdateProductsCount()//Обновление количества товаров в корзине
        {
            int productsCount = ExamOrderList.Count;
            orderInProductsCount = 0;
            for (int i = 0; i < productsCount; i++)
            {
                orderInProductsCount += ExamOrderList[i].ProductCountInOrder;
                CountProductsInOrderLabel.Content = orderInProductsCount.ToString();
            }
            if (ExamOrderList.Count == 0)
                CountProductsInOrderLabel.Content = 0;
        }

        private void UpdateDiscount()//обновление общей скидки товаров в корзине
        {
            int productsCount = ExamOrderList.Count;
            totalDiscount = 0;
            for (int i = 0; i < productsCount; i++)
            {
                totalDiscount += (ExamOrderList[i].ProductCost - ExamOrderList[i].TotalCost) * ExamOrderList[i].ProductCountInOrder;
                string totalDiscountStr = totalDiscount.HasValue ? totalDiscount.Value.ToString("F2") : "0.00";
                OrderDiscountLabel.Content = totalDiscountStr + " руб.";
            }
            if (ExamOrderList.Count == 0)
                OrderDiscountLabel.Content = 0;
        }

        private void UpdateCost()//обновление общей стоимости товаров в корзине
        {
            int productsCount = ExamOrderList.Count;
            totalCost = 0;
            for (int i = 0; i < productsCount; i++)
            {
                totalCost += ExamOrderList[i].TotalCost * ExamOrderList[i].ProductCountInOrder;
                string totalCostStr = totalCost.HasValue ? totalCost.Value.ToString("F2") : "0.00";
                OrderCostLabel.Content = totalCostStr + " руб.";
            }
            if (ExamOrderList.Count == 0)
                OrderCostLabel.Content = 0;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)//удаление товара из корзины
        {
            Button deleteButton = sender as Button;
            DockPanel countPanel = deleteButton.Parent as DockPanel;
            StackPanel productPanel = countPanel.Parent as StackPanel;
            StackPanel productsInOrderStackPanel = productPanel.Parent as StackPanel;
            ExamOrderList.RemoveAt((int)productPanel.Tag);
            if (productsInOrderStackPanel != null)
                productsInOrderStackPanel.Children.Remove(productPanel);
            CreateOrderList();
        }

        private void CountControl_ValueChanged(object sender, RoutedEventArgs e)//изменение количества штук определенного товара в корзине
        {
            CountControl countControl = sender as CountControl;
            ExamOrderList[Convert.ToInt32(countControl.Tag)].ProductCountInOrder = countControl.Value;
            UpdateProductsCount();
            UpdateCost();
            UpdateDiscount();
            if (countControl.Value == 0)//если ставим количество 0, то товар удаляется
            {
                DockPanel countPanel = countControl.Parent as DockPanel;
                StackPanel productPanel = countPanel.Parent as StackPanel;
                StackPanel productsInOrderStackPanel = productPanel.Parent as StackPanel;
                ExamOrderList.RemoveAt((int)productPanel.Tag);
                productsInOrderStackPanel?.Children.Remove(productPanel);
                CreateOrderList();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)//возврат на страницу магазинна
        {
            App.CurrentFrame.GoBack();
        }

        private async void MakeOrderButton_Click(object sender, RoutedEventArgs e)//создание записи о товаре в БД
        {
            if (ExamOrderList.Count != 0)
            {
                if (PickupPointsComboBox.SelectedItem != null)
                {
                    //получение даты доставки
                    Random rnd = new Random();
                    int travelDays = rnd.Next(3, 8); // Генерирует число от 3 до 7
                    DateTime currentDate = DateTime.Now;
                    DateTime deliveryDate = currentDate.AddDays(travelDays);

                    //получение кода выдачи
                    int pickupCode;
                    do
                    {
                        pickupCode = rnd.Next(100, 1000); // Генерирует число от 1 до 999
                    }
                    while (existingPickupCodes.Contains(pickupCode));

                    await _orderService.AddExamOrderAsync(CurrentUser.UserID, "Новый", currentDate, deliveryDate, examPickupPoints[PickupPointsComboBox.SelectedIndex].OrderPickupPoint, pickupCode);
                    var newOrder = await _orderService.GetLastOrderAsync();

                    //добавление записей о товарах в новом заказе в БД
                    for (int i = 0; i < ExamOrderList.Count; i++)
                    {
                        await _orderProductService.AddOrderProductAsync(newOrder.OrderId, ExamOrderList[i].ProductArticleNumber, ExamOrderList[i].ProductCountInOrder);
                    }

                    //если пользователь вошел как гость, то создается отдельная запись о его заказе, которую он может получить пока не вышел из приложения,
                    //тк у него нет никаких данных чтобы этот заказ подтвердить, пусть печатает талончик
                    if (CurrentUser.IsGuest)
                    {
                        YourOrdersPage.createdByGuestOrdersList.Add(newOrder);
                    }

                    ExamOrderList.Clear();
                    productsInOrderStackPanel.Children.Clear();
                    App.CurrentFrame.Navigate(new YourOrdersPage());

                }
                else
                {
                    MessageBox.Show("Укажите пункт выдачи");
                    WarnLabel.Content = "*Ошибка";
                }
            }
            else
            {
                MessageBox.Show("Заказ не может быть пустым");
                WarnLabel.Content = "*Ошибка";
            }
        }

        private void PickupPointsComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            WarnLabel.Content = string.Empty;
        }

        private void GoToYourOrders_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new YourOrdersPage());
        }

        private void GoToAllOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new AllOrdersPage());
        }
    }
}
