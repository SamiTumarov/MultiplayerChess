using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ChessWPF
{
    public class Consts
    {
        // Global Constants that are used in the application
        public const int WHITE = 0;
        public const int BLACK = 1;
        public const int NONE = 2;
        public const int CUBE_SIZE = 95;
        public const int BOARD_SIZE = 8;
        public const int POPUP_FOR = 1500;
        public const int REQUEST_GAMES_EVERY = 1000;
        //public static readonly Brush FIRST_SQUARE_COLOR = (SolidColorBrush)(new BrushConverter().ConvertFrom("#5A3300"));
        //public static readonly Brush SECOND_SQUARE_COLOR = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F0C98D"));
        public static readonly Brush FIRST_SQUARE_COLOR = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F0C98D"));
        public static readonly Brush SECOND_SQUARE_COLOR = (SolidColorBrush)(new BrushConverter().ConvertFrom("#5A3300"));

        public static readonly Brush RELEASEDBTNCOLOR = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD1A46C"));
        public static readonly Brush PRESSEDBTNCOLOR = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFE2C36D"));
        public static readonly Brush DANGEROUS_SQUARE_COLOR = Brushes.Red;
        public static readonly Brush SELECTED_SQUARE_COLOR = Brushes.Green;
        public static readonly Brush POSSIBLE_MOVE_COLOR = Brushes.Coral;

        /// <summary> Returns the starting board, which is always the same - Constant
        /// Gets reference for both kings </summary>
        public static Piece[,] StartingBoard(Piece BlackKing, Piece WhiteKing)
        {
            return new Piece[8, 8]
            {
                // Board order is always the same, BLACK on top, white on bottom. If a client is playing as black, he flips his canvas
                {new Rook(Consts.BLACK), new Knight(Consts.BLACK), new Bishop(Consts.BLACK), new Queen(Consts.BLACK), BlackKing, new Bishop(Consts.BLACK), new Knight(Consts.BLACK), new Rook(Consts.BLACK)},
                {new Pawn(Consts.BLACK), new Pawn(Consts.BLACK), new Pawn(Consts.BLACK), new Pawn(Consts.BLACK), new Pawn(Consts.BLACK), new Pawn(Consts.BLACK), new Pawn(Consts.BLACK), new Pawn(Consts.BLACK)},
                {null, null, null, null, null, null, null, null},
                {null, null, null, null, null, null, null, null},
                {null, null, null, null, null, null, null, null},
                {null, null, null, null, null, null, null, null},
                {new Pawn(Consts.WHITE), new Pawn(Consts.WHITE), new Pawn(Consts.WHITE), new Pawn(Consts.WHITE), new Pawn(Consts.WHITE), new Pawn(Consts.WHITE), new Pawn(Consts.WHITE), new Pawn(Consts.WHITE)},
                {new Rook(Consts.WHITE), new Knight(Consts.WHITE), new Bishop(Consts.WHITE), new Queen(Consts.WHITE), WhiteKing, new Bishop(Consts.WHITE), new Knight(Consts.WHITE), new Rook(Consts.WHITE)}
            };
        }

    }
}
