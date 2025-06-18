using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LabOS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(NumberTextBox.Text, out int number) || number < 0)
            {
                ResultTextBlock.Text = "Пожалуйста, введите целое неотрицательное число!";
                return;
            }

            try
            {
                BigInteger result = await CalculateFactorialAsync(number);
                ResultTextBlock.Text = $"Факториал числа {number} равен:\n{result}";
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = $"Ошибка: {ex.Message}";
            }
        }
        private async Task<BigInteger> CalculateFactorialAsync(int n)
        {
            return await Task.Run(() =>
            {
                if (n == 0 || n == 1)
                    return 1;

                BigInteger result = 1;
                for (int i = 2; i <= n; i++)
                {
                    result *= i;
                }

                return result;
            });
        }
    }
}