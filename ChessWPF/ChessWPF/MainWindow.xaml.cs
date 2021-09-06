using System;
using System.Windows;
using System.Windows.Threading;

namespace ChessWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // The flow goes Login -> Lobby -> Game
        LoginControl Login;
        GameControl Game;
        LobbyControl Lobby;

        NetworkClient Client;

        // Some properties of the player
        string name;


        // To Show Reconnecting... Window
        int num_dots = 3;
        private DispatcherTimer ReconnectingAnimationTimer;

        public MainWindow()
        {

            Client = new NetworkClient(this);

            InitializeComponent();
            // Initialize login page 
            Login = new LoginControl(Client);
            Login.LoginEvent += LoggingIn;
            MainCanvas.Children.Add(Login);

            AudioManager.Init();
        }


        /// <summary> Called from main because it needs to save the selected username </summary>
        public void LoggingIn(string username, string password)
        {
            name = username;
            // try to login
            string Output = "LOGIN\n" + username + "\n" + password;
            Client.SendMsg(Output);
        }

        public void AppLogout()
        {
            Client.SendMsg("LOGOUT");
            // Initialize login page 
            MainCanvas.Children.Remove(Lobby);
            Login = new LoginControl(Client);
            Login.LoginEvent += LoggingIn;
            MainCanvas.Children.Add(Login);

            // Destroy Lobby reference
            Lobby.DestroyGamesListTimer();
            Lobby = null;
        }

        // <summary> Gets messages from the socket thread, sends them to the correct 
        // place to be processed/shown on the screen </summary>
        public void OnRecievedMessage(string msg)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                string[] Splitted = msg.Split((char)32); // 32 is WHITESPACE
                string Type = Splitted[0]; // message type is like the id of the message - used to identify it
                switch (Type)
                {
                    case "LOGINACCEPTED":
                        LoginAccepted();
                        break;
                    case "LOGINDENIED":
                        Login.OnLoginDenied();
                        break;
                    case "REGISTERACCEPTED":
                        Login.OnRegisterSuccessfull();
                        break;
                    case "REGISTERDENIED":
                        Login.OnRegisterDenied();
                        break;
                    case "HOSTED": // HOSTED {room_tag} {color}
                        HostedGame(Splitted[1], int.Parse(Splitted[2]));
                        break;
                    case "JOINED": // JOINED {color} {opponent_name} {time}
                        JoinedGame(int.Parse(Splitted[1]), Splitted[2], int.Parse(Splitted[3]));
                        break;
                    case "STARTED": // STARTED {opponent_name} {time}
                        Game.start_game(Splitted[1], int.Parse(Splitted[2]));
                        break;
                    case "MOVED": // MOVED {move decription}
                        Game.opponent_moved(Splitted[1], double.Parse(Splitted[2]), double.Parse(Splitted[3]));
                        break;
                    case "LOST":
                        Game.GameIsLost();
                        break;
                    case "WON":
                        Game.GameIsWon();
                        break;
                    case "DRAW":
                        Game.GameIsDrawen();
                        break;
                    case "AVAILABLEGAMES": // AVAILABLEGAMES {list_of_games}
                        Lobby.SetAvailableGames(Splitted[1]);
                        break;
                    case "PASTGAMES": // PASTGAMES {list of past games}
                        Lobby.SetPastGames(Splitted[1], int.Parse(Splitted[2]));
                        break;
                }
            }));
        }


        // Update UI According to connection state

        public void OnConnectionFailed()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MainCanvas.IsEnabled = false;
                OfflineCanvas.Visibility = Visibility.Visible;

                ReconnectingAnimationTimer = new DispatcherTimer();
                ReconnectingAnimationTimer.Interval = new TimeSpan(0, 0, 0, 0, 350);
                ReconnectingAnimationTimer.Tick += OnReconnectingAnimationTimerTicked;
                ReconnectingAnimationTimer.Start();
            }));
        }

        public void OnReconnection()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MainCanvas.IsEnabled = true;
                OfflineCanvas.Visibility = Visibility.Collapsed;
                ReconnectingAnimationTimer.Stop();
            }));
        }

        public void OnReconnectionAfterConnected()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MainCanvas.IsEnabled = true;
                OfflineCanvas.Visibility = Visibility.Collapsed;
                ReconnectingAnimationTimer.Stop();

                // Destroy references to all windows
                
                Login = null;
                Game?.DestroyGameTimer();
                Game = null;
                Lobby?.DestroyGamesListTimer();
                Lobby = null;

                Login = new LoginControl(Client);
                Login.LoginEvent += LoggingIn;
                MainCanvas.Children.Add(Login);
            }));
        }

        public void OnReconnectingAnimationTimerTicked(object sender, EventArgs e)
        {
            num_dots += 1;
            if (num_dots >= 4)
                num_dots = 0;
            ReconnectingAnimation.Content = "Trying To Reconnect" + new String('.', num_dots);
        }




        // Switch windows according to server input

        private void LoginAccepted()
        {
            MainCanvas.Children.Remove(Login);
            Lobby = new LobbyControl(Client, name);
            Lobby.LogoutEvent += AppLogout;
            MainCanvas.Children.Add(Lobby);

            // Destroy login reference
            Login = null;
        }
        
        private void HostedGame(string RoomTag, int my_color)
        {
            MainCanvas.Children.Remove(Lobby);
            Game = new GameControl(Client, RoomTag, my_color, name);
            Game.ExitEvent += ExitedGame;
            MainCanvas.Children.Add(Game);

            // Remove Lobby reference
            Lobby.DestroyGamesListTimer();
            Lobby = null;
        }

        private void JoinedGame(int color, string opponent_name, int time_for_game)
        {
            MainCanvas.Children.Remove(Lobby);
            Game = new GameControl(Client, "0000", color, name);
            Game.start_game(opponent_name, time_for_game);
            Game.ExitEvent += ExitedGame;
            MainCanvas.Children.Add(Game);

            // Remove Lobby reference
            Lobby.DestroyGamesListTimer();
            Lobby = null;
        }

        private void ExitedGame()
        {
            MainCanvas.Children.Remove(Game);
            Client.SendMsg("EXIT");
            Lobby = new LobbyControl(Client, name);
            Lobby.LogoutEvent += AppLogout;
            MainCanvas.Children.Add(Lobby);

            // Destroy game reference
            Game = null;
        }

    }
}
