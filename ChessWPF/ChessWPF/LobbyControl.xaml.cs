using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ChessWPF
{
    /// <summary>
    /// Interaction logic for LobbyControl.xaml
    /// </summary>
    public partial class LobbyControl : UserControl
    {
        public delegate void Logout();
        public event Logout LogoutEvent;

        private int color_selected;

        private NetworkClient Client;
        private ObservableCollection<Room> GamesList;
        private ObservableCollection<PastGame> PastGamesList;

        DispatcherTimer GamesListTimer;

        /// <summary> Needs client instance for communications, and username to show it on the screen </summary>
        public LobbyControl(NetworkClient Client, string username)
        {
            InitializeComponent();
            Username.Content = username;
            this.Client = Client;
            
            // Start with random color
            Random rand = new Random();
            color_selected = rand.Next(0, 2);

            // Request update
            Client.SendMsg("AVAILABLEGAMES");
            Client.SendMsg("GETPASTGAMES");

            // Some data binding
            GamesList = new ObservableCollection<Room>();
            RoomsList.ItemsSource = GamesList;
            PastGamesList = new ObservableCollection<PastGame>();
            PastGamesListXAML.ItemsSource = PastGamesList;

            // First show the "Join game" panel
            JoinPanel_Click(null, null);

            // Ask the server every second for updated list of games
            GamesListTimer = new DispatcherTimer();
            GamesListTimer.Interval = new TimeSpan(0, 0, 0, 0, Consts.REQUEST_GAMES_EVERY);
            GamesListTimer.Tick += OnGamesListTimerTicked;
            GamesListTimer.Start();
        }

        public void OnGamesListTimerTicked(object sender, EventArgs e)
        {
            // Request update from server on available games every second or so
            Client.SendMsg("AVAILABLEGAMES");
        }

        public void DestroyGamesListTimer()
        {
            GamesListTimer.Stop();
            GamesListTimer.Tick -= OnGamesListTimerTicked;
        }

        /// <summary> Gets string with a list of available games. shows it </summary>
        public void SetAvailableGames(string games)
        {
            GamesList.Clear();
            string[] games_list = games.Split('.');
            foreach (string game in games_list)
                if (game.Length > 0)
                {
                    string[] args = game.Split(';');
                    GamesList.Add(new Room(args[0], args[1], args[2], int.Parse(args[3])));
                }
        }

        /// <summary> Gets a list of past games, and shows it as a list </summary>
        public void SetPastGames(string games, int wins)
        {
            PastGamesList.Clear();
            string[] games_str = games.Split('.');

            if (wins > 0)
                LabelForWins.Content = "Total Wins:   " + wins + " 🏆";
            else
                LabelForWins.Content = "";

            if (games_str.Length == 1 && games_str[0] == "")
                return; // User didn't play any games
      
            for (int i = 0; i < games_str.Length; i++)
            {
                string[] args = games_str[i].Split(';');
                PastGame game = new PastGame(args[0], args[1], args[2], args[3]);
                PastGamesList.Add(game);
            }

            PastGamesTitle.Content = "Your Last Five Games";
        }

        /// <summary> Clicked on a room in the available-games list, try to join </summary>
        private void Join_specific_room_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TryJoin(((Button)sender).Tag.ToString());
        }

        private void TryJoin(string room_tag)
        {
            Client.SendMsg("JOIN\n" + room_tag);
        }

        /// <summary> Send to the server the host command with the settings of the game </summary>
        private void HostBtn_Click(object sender, RoutedEventArgs e)
        {
            string Time = ((ComboBoxItem)TimeCheckBox.SelectedItem).Tag.ToString();
            Client.SendMsg("HOST\n" + Time.ToString() + "\n" + color_selected);
        }

        /// <summary> Send to the server the game that the client wants to join </summary>
        private void JoinBtn_Click(object sender, RoutedEventArgs e)
        {
            TryJoin(RoomTag.Text);
        }

        // <summary> Trigger logout event, resulting in switching to login window, and
        // sending the approprite message to the server </summary>
        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            LogoutEvent?.Invoke();
        }

        // <summary> Switching between the host, join and my-games panels </summary>
        private void JoinPanel_Click(object sender, RoutedEventArgs e)
        {
            JoinCanvas.Visibility = Visibility.Visible;
            HostCanvas.Visibility = Visibility.Collapsed;
            HistoryCanvas.Visibility = Visibility.Collapsed;

            JoinPanel.Background = Consts.PRESSEDBTNCOLOR;
            HostPanel.Background = Consts.RELEASEDBTNCOLOR;
            GamesPanel.Background = Consts.RELEASEDBTNCOLOR;
        }

        private void HostPanel_Click(object sender, RoutedEventArgs e)
        {
            JoinCanvas.Visibility = Visibility.Collapsed;
            HostCanvas.Visibility = Visibility.Visible;
            HistoryCanvas.Visibility = Visibility.Collapsed;

            JoinPanel.Background = Consts.RELEASEDBTNCOLOR;
            HostPanel.Background = Consts.PRESSEDBTNCOLOR;
            GamesPanel.Background = Consts.RELEASEDBTNCOLOR;
        }

        private void GamesPanel_Click(object sender, RoutedEventArgs e)
        {
            JoinCanvas.Visibility = Visibility.Collapsed;
            HostCanvas.Visibility = Visibility.Collapsed;
            HistoryCanvas.Visibility = Visibility.Visible;

            JoinPanel.Background = Consts.RELEASEDBTNCOLOR;
            HostPanel.Background = Consts.RELEASEDBTNCOLOR;
            GamesPanel.Background = Consts.PRESSEDBTNCOLOR;
        }

        // <summary> color selection for the 'HOST' panel </summary>
        private void BlackColor_Click(object sender, RoutedEventArgs e)
        {
            color_selected = Consts.BLACK;
            BlackColor.BorderThickness = new Thickness(4);
            WhiteColor.BorderThickness = new Thickness(0);
            RandomColor.BorderThickness = new Thickness(0);
        }

        private void RandomColor_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            color_selected = rand.Next(0, 2);
            BlackColor.BorderThickness = new Thickness(0);
            WhiteColor.BorderThickness = new Thickness(0);
            RandomColor.BorderThickness = new Thickness(4);
        }

        private void WhiteColor_Click(object sender, RoutedEventArgs e)
        {
            color_selected = Consts.WHITE;
            BlackColor.BorderThickness = new Thickness(0);
            WhiteColor.BorderThickness = new Thickness(4);
            RandomColor.BorderThickness = new Thickness(0);
        }
    }


    // Info bounded to lists

    /// <summary> Used to show available rooms </summary>
    class Room
    {
        private string room_tag;
        private string host_name;
        private string match_type;
        private int play_as;

        public Room(string host_name, string room_tag, string match_type, int play_as)
        {
            this.host_name = host_name;
            this.room_tag = room_tag;
            this.match_type = match_type;
            this.play_as = play_as;
        }

        public string Room_tag { get { return room_tag; } }
        public string Host_name { get { return host_name; } }
        public string Play_as { get { return play_as == Consts.WHITE ? "white" : "black"; } }
        public string Match_type { get { return match_type; } }
    }

    class PastGame
    {
        public string Date { get; set; }
        public string Opponent { get; set; }
        public string Result { get; set; }
        public string PlayedAs { get; set; }

        public PastGame(string date, string opponent, string result, string my_color)
        {
            Date = date;
            Opponent = opponent;
            Result = result;

            int my_color_int = int.Parse(my_color);
            if (my_color_int == Consts.WHITE)
                PlayedAs = "White";
            else
                PlayedAs = "Black";
        }
    }
}
