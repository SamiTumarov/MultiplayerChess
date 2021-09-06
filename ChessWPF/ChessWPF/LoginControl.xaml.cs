using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Text.RegularExpressions;

namespace ChessWPF
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        public delegate void Login(string username, string password);
        public event Login LoginEvent;

        private NetworkClient Client;

        public LoginControl(NetworkClient Client)
        {
            InitializeComponent();
            this.Client = Client;
        }

        public void OnRegisterSuccessfull()
        {
            RegisterSuccessfullLabel.Visibility = Visibility.Visible;
            Register.IsEnabled = false;

            // Clean register info
            UsernameR.Text = "";
            PasswordR.Password = "";

            DispatcherTimer _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(Consts.POPUP_FOR);
            _timer.Tick += new EventHandler(delegate (object s, EventArgs a)
            {
                // Hide after some time
                RegisterSuccessfullLabel.Visibility = Visibility.Hidden;
                _timer.Stop();
                Register.IsEnabled = true;
            });

            _timer.Start();
        }

        public void OnRegisterDenied()
        {
            RegisterFailedLabel.Visibility = Visibility.Visible;
            Register.IsEnabled = false;

            // Clean register info
            UsernameR.Text = "";
            PasswordR.Password = "";

            DispatcherTimer _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(Consts.POPUP_FOR);
            _timer.Tick += new EventHandler(delegate (object s, EventArgs a)
            {
                // Hide after some time
                RegisterFailedLabel.Visibility = Visibility.Hidden;
                _timer.Stop();
                Register.IsEnabled = true;
            });

            _timer.Start();
        }

        public void OnLoginDenied()
        {
            LoginFailedLabel.Visibility = Visibility.Visible;
            SignIn.IsEnabled = false;

            // Clean login info
            Username.Text = "";
            Password.Password = "";

            DispatcherTimer _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(Consts.POPUP_FOR);
            _timer.Tick += new EventHandler(delegate (object s, EventArgs a)
            {
                // Hide after some time
                LoginFailedLabel.Visibility = Visibility.Hidden;
                _timer.Stop();
                SignIn.IsEnabled = true;
            });

            _timer.Start();
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (LoginEvent != null)
            {
                string Name = Username.Text;
                string Pswd = Password.Password;
                LoginEvent(Name, Pswd);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string Name = UsernameR.Text;
            string Pswd = PasswordR.Password;

            Client.SendMsg("REGISTER\n" + Name + "\n" + Pswd);
        }

        private void GoToLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginForm.Visibility = Visibility.Visible;
            RegisterForm.Visibility = Visibility.Collapsed;

            UsernameR.Text = "";
            PasswordR.Password = "";
        }

        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            LoginForm.Visibility = Visibility.Collapsed;
            RegisterForm.Visibility = Visibility.Visible;

            Username.Text = "";
            Password.Password = "";
        }


        /// Bindings for buttons 
        void OnLoginTextInfoChanged(object sender, TextChangedEventArgs e)
        {
            OnLoginInfoChanged();
        }
        void OnLoginPasswordInfoChanged(object sender, RoutedEventArgs e)
        {
            OnLoginInfoChanged();
        }
        void OnRegisterTextInfoChanged(object sender, TextChangedEventArgs e)
        {
            OnRegisterInfoChanged();
        }
        void OnRegisterPasswordInfoChanged(object sender, RoutedEventArgs e)
        {
            OnRegisterInfoChanged();
        }
        
        // Check register and login info
        void OnLoginInfoChanged()
        {
            LUsernameInvalid.Visibility = Visibility.Collapsed;
            LPasswordInvalid.Visibility = Visibility.Collapsed;

            // Username
            bool FoundError = false;
            if (!new Regex("^[a-zA-Z0-9]+$").Match(Username.Text.ToString()).Success)
            {
                FoundError = true;
                if (Username.Text.Length == 0)
                    LUsernameInvalid.Visibility = Visibility.Collapsed;
                else
                {
                    LUsernameInvalid.Visibility = Visibility.Visible;
                    LUsernameInvalid.Content = "Use 0-9, a-z Or A-Z!";
                }
            }

            if (Username.Text.Length > 10)
            {
                FoundError = true;
                LUsernameInvalid.Visibility = Visibility.Visible;
                LUsernameInvalid.Content = "Max Name Length Is 10!";
            }

            // Password
            if (Password.Password.Contains(" "))
            {
                FoundError = true;
                LPasswordInvalid.Visibility = Visibility.Visible;
                LPasswordInvalid.Content = "Password Can't Contain Spaces!";
            }
            if (Password.Password.Length > 15)
            {
                FoundError = true;
                LPasswordInvalid.Visibility = Visibility.Visible;
                LPasswordInvalid.Content = "Max Password Length Is 15!";
            }

            if (FoundError)
                SignIn.IsEnabled = false;
            else
                SignIn.IsEnabled = true;
        }


        void OnRegisterInfoChanged()
        {
            RUsernameInvalid.Visibility = Visibility.Collapsed;
            RPasswordInvalid.Visibility = Visibility.Collapsed;

            // Username
            bool FoundError = false;
            if (!new Regex("^[a-zA-Z0-9]+$").Match(UsernameR.Text.ToString()).Success)
            {
                FoundError = true;
                if (UsernameR.Text.Length == 0)
                    RUsernameInvalid.Visibility = Visibility.Collapsed;
                else
                {
                    RUsernameInvalid.Visibility = Visibility.Visible;
                    RUsernameInvalid.Content = "Use 0-9, a-z Or A-Z!";
                }
            }

            if (UsernameR.Text.Length > 10)
            {
                FoundError = true;
                RUsernameInvalid.Visibility = Visibility.Visible;
                RUsernameInvalid.Content = "Max Name Length Is 10!";
            }

            // Password
            if (PasswordR.Password.Contains(" "))
            {
                FoundError = true;
                RPasswordInvalid.Visibility = Visibility.Visible;
                RPasswordInvalid.Content = "Password Can't Contain Spaces!";
            }
            if (PasswordR.Password.Length > 15)
            {
                FoundError = true;
                RPasswordInvalid.Visibility = Visibility.Visible;
                RPasswordInvalid.Content = "Max Password Length Is 15!";
            }

            if (FoundError)
                Register.IsEnabled = false;
            else
                Register.IsEnabled = true;
        }

        private void Security_Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SecurityAlert.Visibility = Visibility.Visible;
        }

        private void Security_Image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SecurityAlert.Visibility = Visibility.Collapsed;
        }
    }
}
