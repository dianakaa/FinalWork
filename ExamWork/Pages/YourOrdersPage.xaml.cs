using Microsoft.Win32;
using ServiceLayer;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;

namespace ExamWork.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class YourOrdersPage : Page
    {
        public static List<ExamOrder> createdByGuestOrdersList = new();
        public static List<ExamOrder> examCreatedOrdersList = new();
        public static readonly ExamOrderService _orderService = new();
        public static readonly ExamOrderProductService _orderProductService = new();
        public static readonly ExamProductService _productService = new();

        public YourOrdersPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            examCreatedOrdersList = CurrentUser.IsGuest ? createdByGuestOrdersList : await _orderService.GetOrdersByUserIdAsync(CurrentUser.UserID);
            //если пользователь зашел как гость, то список заказов заполняется из createdByGuestOrdersList, до момента,
            //пока кто-либо не зайдет в авторизованный аккаунт или приложение не будет перезапущено, гостю следует сохранить талоны заказов в txt, чтобы не потерять свои заказы

            GetOrderList();

            //вывод данных о текущем пользователе
            if (CurrentUser.IsGuest)
                CurrentUserLabel.Content = "Вы вошли как гость";
            else
                CurrentUserLabel.Content = $"{CurrentUser.UserName.Substring(0, 1)}.{CurrentUser.UserPatronymic.Substring(0, 1)}. {CurrentUser.UserSurname}";

            if (CurrentUser.RoleID != 2)//если в приложение зашел менеджер или администратор, то им будет доступна кнопка перейти к просмотру всех заказов
                GoToAllOrdersButton.Visibility = Visibility.Visible;
            else
                GoToAllOrdersButton.Visibility = Visibility.Collapsed;
        }

        private async void GetOrderList()//метод для вывода заказов текущего пользователя на странцу
        {
            YourOrdersStackPanel.Children.Clear();
            int productsCount = examCreatedOrdersList.Count;
            for (int i = 0; i < productsCount; i++)//в цикле создаются отдельные элементы в которых хранятся данные о товарах
            {
                List<ExamOrderProduct> examOrderProducts = await _orderProductService.GetProductsInOrder(examCreatedOrdersList[i].OrderId);

                Border orderBorder = new();
                orderBorder.Width = 600;
                orderBorder.Margin = new Thickness(80, 5, 0, 5);
                orderBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC6600"));
                orderBorder.BorderThickness = new(3);

                StackPanel orderPanel = new();
                orderPanel.Tag = i;
                orderPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCC99"));

                Label orderIdLabel = new();
                orderIdLabel.Content = $"Заказ №{examCreatedOrdersList[i].OrderId}";
                orderPanel.Children.Add(orderIdLabel);

                Label orderDateLabel = new Label();
                orderDateLabel.Content = $"Дата: {examCreatedOrdersList[i].OrderDate}";
                orderPanel.Children.Add(orderDateLabel);

                StackPanel orderCompositionPanel = new();
                orderCompositionPanel.Orientation = Orientation.Horizontal;
                Label orderCompositionLabel = new();
                string orderComposition = "";
                for (int j = 0; j < examOrderProducts.Count; j++)
                {
                    orderComposition += $"\n{await _productService.GetProductNameByArticleAsync(examOrderProducts[j].ProductArticleNumber)}" +
                        $"({await _orderProductService.GetProductAmountInOrderWithArticle(examCreatedOrdersList[i].OrderId, examOrderProducts[j].ProductArticleNumber)})";
                }
                orderCompositionLabel.Content = $"Состав заказа:{orderComposition}";
                orderCompositionPanel.Children.Add(orderCompositionLabel);
                orderPanel.Children.Add(orderCompositionPanel);

                Label orderSumLabel = new();
                decimal? orderSum = await _orderProductService.GetSumOrder(examCreatedOrdersList[i].OrderId);
                string orderSumStr = orderSum.HasValue ? orderSum.Value.ToString("F2") : "0.00";
                orderSumLabel.Content = $"Сумма заказа: " + orderSumStr;
                orderPanel.Children.Add(orderSumLabel);

                Label orderDiscountLabel = new();
                decimal? orderDiscount = await _orderProductService.GetDiscountOrder(examCreatedOrdersList[i].OrderId);
                string orderDiscontStr = orderDiscount.HasValue ? orderDiscount.Value.ToString("F2") : "0.00";
                orderDiscountLabel.Content = $"Сумма скидки в заказе: " + orderDiscontStr;
                orderPanel.Children.Add(orderDiscountLabel);

                Label orderPickupPointLabel = new();
                orderPickupPointLabel.Content = $"Пункт выдачи: {examCreatedOrdersList[i].OrderPickupPoint}";
                orderPanel.Children.Add(orderPickupPointLabel);

                DockPanel dockPanel = new();
                Label orderPickupCodeLabel = new();
                orderPickupCodeLabel.Content = $"Код получения: {examCreatedOrdersList[i].OrderPickupCode}";
                Button printOrderButton = new();
                printOrderButton.Content = "Распечатать талон заказа";
                printOrderButton.Margin = new(5);
                printOrderButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCC99"));
                printOrderButton.BorderThickness = new Thickness(4);
                printOrderButton.Click += PrintOrderButton_Click;
                printOrderButton.Tag = $"Заказ №{examCreatedOrdersList[i].OrderId}\nДата: {examCreatedOrdersList[i].OrderDate}\n" +
                    $"Состав заказа:{orderComposition}\nСумма заказа: {(orderSum.HasValue ? orderSum.Value.ToString("F2") : "0.00")}\nСумма скидки в заказе: {(orderDiscount.HasValue ? orderDiscount.Value.ToString("F2") : "0.00")}\n" +
                    $"Пункт выдачи: {examCreatedOrdersList[i].OrderPickupPoint}\nКод получения: {examCreatedOrdersList[i].OrderPickupCode}";
                DockPanel.SetDock(printOrderButton, Dock.Right);
                dockPanel.Children.Add(printOrderButton);
                dockPanel.Children.Add(orderPickupCodeLabel);
                orderPanel.Children.Add(dockPanel);

                orderBorder.Child = orderPanel;
                YourOrdersStackPanel.Children.Add(orderBorder);
            }
        }

        private void PrintOrderButton_Click(object sender, RoutedEventArgs e)//печать талончика заказа в txt-файл
        {
            Button printOrderButton = (Button)sender;
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "Текстовый файл (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == true)
            {
                // Текст для сохранения
                string textToSave = printOrderButton.Tag.ToString();

                // Запись текста в файл
                File.WriteAllText(saveFileDialog.FileName, textToSave);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new ShopPage());
        }

        private void GoToAllOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new AllOrdersPage());
        }
    }
}
