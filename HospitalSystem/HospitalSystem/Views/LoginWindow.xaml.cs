using HospitalSystem.Helpers;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace HospitalSystem.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Без рамки (WindowStyle="None") окно нельзя двигать штатно —
            // разрешаем перетаскивание за любую точку
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(sender, e);
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = PasswordBox.Password;

            using (var db = new HospitalDBEntities())
            {
                // Ищем только по логину — сверку пароля EF не умеет превратить в SQL,
                // потому что PasswordHelper.Verify это обычный C#-метод, а не выражение
                var user = db.Users.FirstOrDefault(u => u.Login == login);

                if (user != null && PasswordHelper.Verify(password, user.PasswordHash))
                {
                    // Тихая миграция: если пароль ещё лежал в открытом виде,
                    // при первом же успешном входе заменяем его на bcrypt-хеш
                    if (!PasswordHelper.IsBcryptHash(user.PasswordHash))
                    {
                        user.PasswordHash = PasswordHelper.Hash(password);
                        db.SaveChanges();
                    }

                    SessionManager.CurrentUser = user;
                    AuditHelper.Log("Вход в систему", "Users");

                    MainWindow main = new MainWindow();
                    main.Show();

                    this.Close();
                }
                else
                {
                    if (user != null)
                    {
                        AuditHelper.Log("Неудачная попытка входа", "Users", user.UserID);
                    }

                    MessageBox.Show("Неверный логин или пароль");
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}