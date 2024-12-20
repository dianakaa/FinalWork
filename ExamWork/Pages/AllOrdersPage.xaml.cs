using ServiceLayer;
using ServiceLayer.DTOs;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExamWork.Pages
{
    /// <summary>
    /// Логика взаимодействия для AllOrdersPage.xaml
    /// </summary>
    public partial class AllOrdersPage : Page
    {
        public static List<OrderSummaryDTO> examOrders = new();
        public static readonly ExamOrderService _orderService = new();
        public static readonly ExamOrderProductService _orderProductService = new();
        public static readonly ExamProductService _productService = new();
        public static readonly ExamUserService _userService = new();

        public AllOrdersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GetOrderList();
            CurrentUserLabel.Content = $"{CurrentUser.UserName.Substring(0, 1)}.{CurrentUser.UserPatronymic.Substring(0, 1)}. {CurrentUser.UserSurname}";
        }

        private async void GetOrderList()//метод для вывода всех существующих заказов на странцу
        {
            AllOrdersStackPanel.Children.Clear();
            examOrders = await _orderService.GetOrdersWithTotalCost();
            int productsCount = examOrders.Count;
            for (int i = 0; i < productsCount; i++)//в цикле создаются отдельные элементы в которых хранятся данные о заказах
            {
                List<ExamOrderProduct> examOrderProducts = await _orderProductService.GetProductsInOrder(examOrders[i].OrderID);//список товаров в заказе

                Border orderBorder = new();
                orderBorder.Tag = examOrders[i].OrderID.ToString();
                orderBorder.Width = 600;
                orderBorder.Margin = new Thickness(80, 5, 0, 5);
                orderBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC6600"));
                orderBorder.BorderThickness = new(3);

                StackPanel orderPanel = new();
                orderPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCC99"));

                DockPanel statusPanel = new();
                Label orderIdLabel = new();
                orderIdLabel.Content = $"Заказ №{examOrders[i].OrderID}";
                Label orderStatus = new();
                DockPanel.SetDock(orderStatus, Dock.Right);
                orderStatus.Content = examOrders[i].OrderStatus;
                statusPanel.Children.Add(orderStatus);
                statusPanel.Children.Add(orderIdLabel);
                orderPanel.Children.Add(statusPanel);

                StackPanel orderCompositionPanel = new();
                orderCompositionPanel.Orientation = Orientation.Horizontal;
                Label orderCompositionLabel = new();
                string orderComposition = "";
                for (int j = 0; j < examOrderProducts.Count; j++)
                {
                    var product = await _productService.GetProductNameByArticleAsync(examOrderProducts[j].ProductArticleNumber);
                    orderComposition += $"\n{product}({await _orderProductService.GetProductAmountInOrderWithArticle(examOrders[i].OrderID, examOrderProducts[j].ProductArticleNumber)})";
                }
                orderCompositionLabel.Content = $"Состав заказа:{orderComposition}";
                orderCompositionPanel.Children.Add(orderCompositionLabel);
                orderPanel.Children.Add(orderCompositionPanel);

                Label orderSumLabel = new();
                decimal? orderSum = await _orderProductService.GetSumOrder(examOrders[i].OrderID);
                string orderSumStr = orderSum.HasValue ? orderSum.Value.ToString("F2") : "0.00";
                orderSumLabel.Content = $"Сумма заказа: " + orderSumStr;
                orderPanel.Children.Add(orderSumLabel);

                Label orderDiscountLabel = new();
                decimal? orderDiscount = await _orderProductService.GetDiscountOrder(examOrders[i].OrderID);
                string orderDiscontStr = orderDiscount.HasValue ? orderDiscount.Value.ToString("F2") : "0.00";
                orderDiscountLabel.Content = $"Сумма скидки в заказе: " + orderDiscontStr;
                orderPanel.Children.Add(orderDiscountLabel);

                if (examOrders[i].UserID != 0)
                {
                    Label orderPickupPointLabel = new();
                    string? fullName = await _userService.GetUserFullNameWithOrderIdAsync(examOrders[i].OrderID);
                    orderPickupPointLabel.Content = !string.IsNullOrWhiteSpace(fullName) ? $"ФИО клиента: {fullName}" : "Заказчик неавторизован";
                    orderPanel.Children.Add(orderPickupPointLabel);
                }

                Label orderDateLabel = new();
                orderDateLabel.Content = $"Дата: {examOrders[i].OrderDate}";
                orderPanel.Children.Add(orderDateLabel);

                StackPanel DeliveryStackPanel = new();
                DeliveryStackPanel.Orientation = Orientation.Horizontal;
                Label orderDeliveryDateLabel = new();
                orderDeliveryDateLabel.Content = $"Дата доставки:\n{examOrders[i].OrderDeliveryDate:yyyy-MM-dd}";
                StackPanel ChangeDeliveryDateStackPanel = new();
                Label changeDeliveryDateLabel = new();
                changeDeliveryDateLabel.Content = "Изменить дату доставки:";
                TextBox changeDeliveryDateTextBox = new();
                changeDeliveryDateTextBox.Name = "changeTextBox";
                Button changeButton = new();
                changeButton.Tag = examOrders[i].OrderID;
                changeButton.Content = "Изменить";
                changeButton.Click += ChangeButton_Click;
                StackPanel StatusStackPanel = new();
                Label changeStatusLabel = new();
                changeStatusLabel.Content = "Статус:";
                ComboBox changeStatusComboBox = new();
                changeStatusComboBox.Tag = examOrders[i].OrderID;
                changeStatusComboBox.Items.Add("Новый");
                changeStatusComboBox.Items.Add("Завершен");
                changeStatusComboBox.SelectionChanged += ChangeStatusComboBox_SelectionChanged;
                StatusStackPanel.Children.Add(changeStatusLabel);
                StatusStackPanel.Children.Add(changeStatusComboBox);
                ChangeDeliveryDateStackPanel.Children.Add(changeDeliveryDateLabel);
                ChangeDeliveryDateStackPanel.Children.Add(changeDeliveryDateTextBox);
                ChangeDeliveryDateStackPanel.Children.Add(changeButton);
                DeliveryStackPanel.Children.Add(orderDeliveryDateLabel);
                DeliveryStackPanel.Children.Add(ChangeDeliveryDateStackPanel);
                DeliveryStackPanel.Children.Add(StatusStackPanel);
                orderPanel.Children.Add(DeliveryStackPanel);

                orderBorder.Child = orderPanel;
                AllOrdersStackPanel.Children.Add(orderBorder);
            }
        }

        private async void ChangeStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)//изменение статуса заказа
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedIndex == 0)
                await _orderService.UpdateExamOrderStatus("Новый", Convert.ToInt32(comboBox.Tag));
            else
                await _orderService.UpdateExamOrderStatus("Завершен", Convert.ToInt32(comboBox.Tag));
            GetOrderList();
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)//изменение даты доставки заказа
        {
            Button changeButton = sender as Button;
            StackPanel stackPanel = changeButton.Parent as StackPanel;
            TextBox newDeliveryDateTextBox = stackPanel.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "changeTextBox");
            try
            {
                _orderService.UpdateExamOrderDeliveryDate(Convert.ToDateTime(newDeliveryDateTextBox.Text), Convert.ToInt32(changeButton.Tag));
                MessageBox.Show("Дата доставки изменена");
            }
            catch
            {
                MessageBox.Show("Введен некорректный формат даты");
            }
            GetOrderList();
        }

        private void SearchByIdButton_Click(object sender, RoutedEventArgs e)
        {
            var targetBorder = AllOrdersStackPanel.Children
                                    .OfType<Border>()
                                    .FirstOrDefault(b => b.Tag.ToString() == IdTextBox.Text);

            if (targetBorder != null)
            {
                // Получаем координаты элемента относительно ScrollViewer
                var scrollViewer = FindVisualParent<ScrollViewer>(AllOrdersStackPanel);
                if (scrollViewer != null)
                {
                    // Получаем позицию элемента
                    var position = targetBorder.TransformToAncestor(AllOrdersStackPanel)
                                                .Transform(new Point(0, 0));
                    // Прокручиваем ScrollViewer до нужной позиции
                    scrollViewer.ScrollToVerticalOffset(position.Y);
                }
            }
            else
            {
                MessageBox.Show("Заказа с данным id не найдено");
            }
        }

        // Метод для поиска родительского элемента определенного типа
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            return parent != null ? parent : FindVisualParent<T>(parentObject);
        }

        private void ToStartButton_Click(object sender, RoutedEventArgs e)//сброс фильтрации и сортировки
        {
            var scrollViewer = FindVisualParent<ScrollViewer>(AllOrdersStackPanel);
            scrollViewer.ScrollToVerticalOffset(0);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.GoBack();
        }
    }
}
