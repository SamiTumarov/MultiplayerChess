using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ChessWPF
{
    /// <summary>
    /// Interaction logic for GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        public delegate void ExitGame();
        public event ExitGame ExitEvent;

        // A board object that has the game's logic
        private Board board;


        private Squares SquaresOnBoard;

        private NetworkClient Client;
        private int my_color;

        private ObservableCollection<string> AlgebricNotatedMoves;

        private GameTimer game_timer;
        private DispatcherTimer timer_tick;

        public bool my_turn { get; set; }
        public bool is_frozen { get; set; } = false;
        public string my_color_str { get; set; }

        private PointsCollection LastSelectedMoves;
        private int last_selected_row = -1;
        private int last_selected_col = -1;

        private int waiting_for_selection_of = -1;

        /// <summary> Initiated when a client hosts or joins a game
        ///  Needs to know:
        ///  NetworkClient - to send info to the server regarding moves
        ///  room_tag - to show it when hosting the game. my_color - to know ho to show the board
        ///  my_name - to show it. Opponent name is showed in the start_game method
        /// </summary>
        public GameControl(NetworkClient Client, string room_tag, int my_color, string my_name)
        {
            my_color_str = my_color == Consts.WHITE ? "white" : "black";
            DataContext = this;
            
            InitializeComponent();

            AlgebricNotatedMoves = new ObservableCollection<string>();
            MovesPanel.ItemsSource = AlgebricNotatedMoves;

            this.my_color = my_color;
            my_turn = my_color == Consts.WHITE ? true : false;
            this.Client = Client;

            RoomTag.Content = room_tag;
            MeName.Content = my_name;
        }

        /// <summary> Called when starting the game, if the player joins a game it's immediatly. if he 
        /// hosts one, he needs to wait for another player </summary>
        public void start_game(string opponent_name, int time_for_game)
        {
            BoardCanvas.Children.Clear();
            SquaresOnBoard = new Squares(BoardCanvas, SquareClick);
            board = new Board(my_color);
            board.draw(BoardCanvas);

            PlayerNameTitle.Visibility = Visibility.Visible;
            PlayerName.Visibility = Visibility.Visible;
            OpName.Content = opponent_name;

            // Game started - allow resigning, disable exiting
            ResignBtn.IsEnabled = true;
            ExitBtn.IsEnabled = false;

            // Show letters near board
            if (my_color == Consts.WHITE)
            {
                SideWhiteLetters.Visibility = Visibility.Visible;
                FrontWhiteLetters.Visibility = Visibility.Visible;
            }
            else
            {
                SideBlackLetters.Visibility = Visibility.Visible;
                FrontBlackLetters.Visibility = Visibility.Visible;
            }

            if (time_for_game == 0)
            {
                // Unlimited time
                OpTime.Content = "Time: Unlimited";
                MeTime.Content = "Time: Unlimited";
                OpTime.Visibility = Visibility.Visible;
                MeTime.Visibility = Visibility.Visible;
            }
            else
            {
                OpTime.Content = "Time: " + Util.SecondsToString(time_for_game) + " Minutes";
                OpTime.Visibility = Visibility.Visible;
                MeTime.Content = "Time: " + Util.SecondsToString(time_for_game) + " Minutes";
                MeTime.Visibility = Visibility.Visible;

                timer_tick = new DispatcherTimer();
                timer_tick.Tick += TimeTicked;
                timer_tick.Interval = new TimeSpan(0, 0, 0, 0, 80);
                timer_tick.Start();
                game_timer = new GameTimer(time_for_game, my_color == Consts.WHITE);
            }
        }

        /// <summary> Following time on each client </summary>
        public void TimeTicked(object sender, EventArgs e)
        {
            // A second passed
            game_timer.Ping();
            OpTime.Content = "Time: " + Util.SecondsToString(game_timer.Oponent_time) + " Minutes";
            MeTime.Content = "Time: " + Util.SecondsToString(game_timer.My_time) + " Minutes";

            System.Diagnostics.Trace.WriteLine("Time tick");
        }

        public void DestroyGameTimer()
        {
            if (timer_tick != null)
            {
                timer_tick.Stop();
                timer_tick.Tick -= TimeTicked;
            }
        }

        /// <summary> Opponent finished his move, now is mine </summary>
        public void opponent_moved(string move, double my_time, double opponent_time)
        {   
            if (my_time >= 0 && opponent_time >= 0)
            {
                game_timer.My_time = my_time;
                game_timer.Oponent_time = opponent_time;
            }
            board.ExecuteMove(BoardCanvas, move, true);
            my_turn = true;
            AlgebricNotatedMoves.Add(Util.ParseMove(move));
            // Scroll to the end
            MovesPanel.SelectedIndex = MovesPanel.Items.Count - 1;
            MovesPanel.ScrollIntoView(MovesPanel.SelectedItem);

            // Move timer to me if game is with time
            if (game_timer != null)
                game_timer.ChangeTimer();

            board.EvaluateGame(SquaresOnBoard);
            AudioManager.PlaySoundForMove(move);
        }

        /// <summary> I finished my move, now is opponent's turn </summary>
        public void me_moved(string move)
        {
            my_turn = false;
            Client.SendMsg("MOVED\n" + move);
            AlgebricNotatedMoves.Add(Util.ParseMove(move));
            // Scroll to the end
            MovesPanel.SelectedIndex = MovesPanel.Items.Count - 1;
            MovesPanel.ScrollIntoView(MovesPanel.SelectedItem);

            // Move timer to me if game is with time
            if (game_timer != null)
                game_timer.ChangeTimer();

            board.EvaluateGame(SquaresOnBoard);
            AudioManager.PlaySoundForMove(move);
        }
        /// <summary> Exited game, send to server "EXIT" so he can update the user about new games  </summary>
        private void BackToLobby_Click(object sender, RoutedEventArgs e)
        {
            if (ExitEvent != null)
                ExitEvent();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Client.SendMsg("RESIGN");
        }

        /// <summary> Outcomes of the game, once got outcome, freeze game and show result </summary>
        public void GameIsLost()
        {
            AudioManager.PlayGenericSound();
            is_frozen = true;
            Afterwards.Visibility = Visibility.Visible;
            Afterwards.IsEnabled = true;
            Statement.Content = "You Lost!";
            PremadeLoserImage.Visibility = Visibility.Visible;

            if (timer_tick != null)
                timer_tick.Stop();

            ResignBtn.IsEnabled = false;
            PawnPieceSelection.Visibility = Visibility.Collapsed;
        }

        public void GameIsWon()
        {
            AudioManager.PlayGenericSound();
            is_frozen = true;
            Afterwards.Visibility = Visibility.Visible;
            Afterwards.IsEnabled = true;
            Statement.Content = "You Won!";
            PremadeWinnerImg.Visibility = Visibility.Visible;

            if (timer_tick != null)
                timer_tick.Stop();

            ResignBtn.IsEnabled = false;
            PawnPieceSelection.Visibility = Visibility.Collapsed;
        }

        public void GameIsDrawen()
        {
            AudioManager.PlayGenericSound();
            is_frozen = true;
            Afterwards.Visibility = Visibility.Visible;
            Afterwards.IsEnabled = true;
            Statement.Content = "It's A Draw!";
            PremadeDrawImage.Visibility = Visibility.Visible;

            if (timer_tick != null)
                timer_tick.Stop();

            ResignBtn.IsEnabled = false;
            PawnPieceSelection.Visibility = Visibility.Collapsed;
        }

        /// <summary> Calls when a square was clicked, checks what was clicked last, and if a move
        /// is possible, delegates it to the game object </summary>
        private void SquareClick(object sender, RoutedEventArgs e, int index)
        {
            if (!my_turn || is_frozen)
                return;

            int row = index / Consts.BOARD_SIZE;
            int col = index % Consts.BOARD_SIZE;
            Piece piece = board.Board2D[row, col];

            if (LastSelectedMoves != null && LastSelectedMoves.PointInCollection(row, col))
            {
                // Can move selected piece to desired cell
                CanMovePiece(row, col);
                return;
            }

            if (piece != null)
            {
                // Select piece if it's my color
                if (piece.color == my_color)
                {
                    SquaresOnBoard.ClearSelection(); // either unselected piece, or selected a new one

                    if (row == last_selected_row && col == last_selected_col)
                    {
                        // unselect piece
                        last_selected_col = -1;
                        last_selected_row = -1;
                        LastSelectedMoves = null;
                    }
                    else
                    {
                        last_selected_col = col;
                        last_selected_row = row;
                        LastSelectedMoves = piece.GetMoves(board.Board2D);
                        board.PostProcessMoves(BoardCanvas, row, col, LastSelectedMoves, my_color);
                        SquaresOnBoard.ShowMove(row, col, LastSelectedMoves);
                    }
                }
            }
            else
            {
                last_selected_col = -1;
                last_selected_row = -1;
                LastSelectedMoves = null;
                SquaresOnBoard.ClearSelection();
            }
        }
        
        /// <summary> Logic to move piece once one was selected. Might pause for pawn promotion dialog </summary>
        public void CanMovePiece(int row, int col)
        {
            if (board.Board2D[last_selected_row, last_selected_col] is Pawn && (row == 7 || row == 0))
            {
                // Pause for pawn promotion
                is_frozen = true;
                waiting_for_selection_of = row * 10 + col;
                PawnPieceSelection.Visibility = Visibility.Visible;
            }
            else
            {
                me_moved(board.ExecuteMove(BoardCanvas, last_selected_row, last_selected_col, row, col, true));
                LastSelectedMoves = null;
                last_selected_col = -1;
                last_selected_row = -1;
                SquaresOnBoard.ClearSelection();
            }
        }

        /// <summary> Logic for piece selection when promoting a pawn </summary>
        public void PieceSelection(object sender, RoutedEventArgs e)
        {
            char piece = ((Button)sender).Tag.ToString()[0];
            if (waiting_for_selection_of != -1)
            {
                int row = waiting_for_selection_of / 10;
                int col = waiting_for_selection_of % 10;
                me_moved(board.ExecuteMove(BoardCanvas, last_selected_row, last_selected_col, row, col, true, piece));
                LastSelectedMoves = null;
                last_selected_col = -1;
                last_selected_row = -1;
                SquaresOnBoard.ClearSelection();

                PawnPieceSelection.Visibility = Visibility.Collapsed;
                waiting_for_selection_of = -1;
                is_frozen = false;
            }
        }

    }

    /// <summary>
    /// Holding time in game for both player, if it's not unlimited
    /// </summary>
    public class GameTimer
    {

        private double my_time;
        private double opponent_time;

        public double Oponent_time { get { return opponent_time; } set { opponent_time = value; } }
        public double My_time { get { return my_time; } set { my_time = value; } }

        private bool counting_for_me;

        private DateTime LastOpponent_time;
        private DateTime LastMy_time;

        public GameTimer(int time_for_game, bool start_with_me)
        {
            opponent_time = time_for_game;
            my_time = time_for_game;

            counting_for_me = start_with_me;

            if (counting_for_me)
            {
                LastMy_time = DateTime.Now;
            }
            else
                LastOpponent_time = DateTime.Now;
        }

        /// <summary> Update time </summary>
        public void Ping()
        {
            if (counting_for_me)
            {
                TimeSpan Diff = DateTime.Now - LastMy_time;
                my_time -= Diff.TotalSeconds;
                LastMy_time = DateTime.Now;
            }
            else
            {
                TimeSpan Diff = DateTime.Now - LastOpponent_time;
                opponent_time -= Diff.TotalSeconds;
                LastOpponent_time = DateTime.Now;
            }

            if (my_time < 0)
                my_time = 0;
            if (opponent_time < 0)
                opponent_time = 0;
        }

        /// <summary> Changing timer running from one player to the other </summary>
        public void ChangeTimer()
        {
            Ping();
            if (counting_for_me)
            {
                LastOpponent_time = DateTime.Now;
            }
            else
            {
                LastMy_time = DateTime.Now;
            }

            counting_for_me = !counting_for_me;
        }

        /// <summary> Stops timer </summary>
        public void Stop()
        {
            Ping();
        }
    }


    /// <summary>
    /// Converting a path string to bitmapimage
    /// </summary>
    public class PathToImageConverter : IValueConverter
    {

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string color = value as string;
            string full_path = parameter as string;

            if (color != null && full_path != null)
            {
                string path = string.Format("pack://application:,,,/" + full_path, color);
                System.Diagnostics.Trace.WriteLine(path);
                return new BitmapImage(new Uri(path));
            }

            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
