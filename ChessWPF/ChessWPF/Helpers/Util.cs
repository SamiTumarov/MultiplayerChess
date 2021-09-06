using System;
using System.Collections.Generic;

namespace ChessWPF
{
    public static class Util // Short for Utilities - Helper functions
    { 
        public static readonly Dictionary<char, string> PieceAbbr = new Dictionary<char, string> {{'R',"Rook"}, {'P', "Pawn"}, {'K', "King"},
                                                { 'Q', "Queen"}, { 'B', "Bishop"}, { 'H', "Knight"} };

        /// <summary> Checks if a point is on board </summary>
        public static bool InBoard(int row, int col)
        {
            if (row < 0 || row >= Consts.BOARD_SIZE) // row indexes are 1,2,3...7, BOARD_SIZE is 8
                return false;
            if (col < 0 || col >= Consts.BOARD_SIZE)
                return false;
            return true;
        }

        /// <summary> Returns color on square </summary>
        public static int ColorOnSquare(Piece[,] Pos, int row, int col)
        {
            if (Pos[row, col] == null)
                return Consts.NONE;
            return Pos[row, col].color;
        }

        /// <summary> true or false whether or not a point is on board, and contains no pieces on it </summary>
        public static bool CleanSquare(Piece[,] Pos, int row, int col)
        {
            if (!InBoard(row, col))
            {
                return false;
            }
            return Pos[row, col] == null;
        }

        /// <summary> Translates piece type to one representing char </summary>
        public static char PieceToChar(Piece p)
        {
            switch (p.GetType().ToString())
            {
                case "ChessWPF.Rook": return 'R';
                case "ChessWPF.Pawn": return 'P';
                case "ChessWPF.Bishop": return 'B';
                case "ChessWPF.Queen": return 'Q';
                case "ChessWPF.King": return 'K';
                case "ChessWPF.Knight": return 'H';
            }

            return 'N'; // N for null, A glitch in the matrix
        }

        /// <summary> Translates char to piece </summary>
        public static Piece CharToPiece(char c, int color)
        {
            switch (c)
            {
                case 'R': return new Rook(color);
                case 'Q': return new Queen(color);
                case 'K': return new King(color, false);
                case 'P': return new Pawn(color);
                case 'B': return new Bishop(color);
                case 'H': return new Knight(color);
            }

            return null; // A glitch in the matrix
        }

        /// <summary> Converts a char representing a chess piece to a symbol that can be shown on screen (also char) </summary>
        public static char CharToSymbol(char c)
        {
            switch (c)
            {
                case 'R': return '♜';
                case 'Q': return '♛';
                case 'K': return '♚';
                case 'P': return '♟';
                case 'B': return '♝';
                case 'H': return '♞';
            }
            return 'N'; // not supposed to happen
        }

        /// <summary> Converts column to char </summary>
        public static char ColumnToChar(int i)
        {
            switch (i)
            {
                case 0: return 'a';
                case 1: return 'b';
                case 2: return 'c';
                case 3: return 'd';
                case 4: return 'e';
                case 5: return 'f';
                case 6: return 'g';
                case 7: return 'h';
            }
            return 'n'; // not supposed to happen
        }

        public static char RowToOnScreenNumber(char i)
        {
            switch (i)
            {
                case '0': return '8';
                case '1': return '7';
                case '2': return '6';
                case '3': return '5';
                case '5': return '3';
                case '6': return '2';
                case '7': return '1';
            }

            return '4'; // Not supposed to happen
        }


        /// <summary> Parses a string that represents a move, to algebric notation that can be shows on board </summary>
        public static string ParseMove(string move)
        {
            string first, second;

            if (move.Length == 5)
            {
                first = new string(new char[] { ColumnToChar(move[1] - '0'), RowToOnScreenNumber(move[0]) });
                second = new string(new char[] { ColumnToChar(move[3] - '0'), RowToOnScreenNumber(move[2])});
                return string.Format("{1}{2}", PieceAbbr[move[4]], first, second);
            }

            string[] moves = move.Split(',');
            if (moves.Length == 2 && moves[0].Length == 6 && moves[1].Length == 5)
            {
                // Captures
                second = new string(new char[] { RowToOnScreenNumber(moves[1][2]), ColumnToChar(moves[1][3] - '0') });
                return string.Format("{0}x{1}", CharToSymbol(moves[0][3]), second);
            }

            if(moves.Length == 2 && moves[0].Length == 5 && moves[1].Length == 5)
            {
                // castling, two moves only in move string
                if (Math.Abs(moves[0][1] - moves[1][1]) == 3)
                    return "O-O"; // king side
                return "O-O-O"; // queen side
            }

            // Pawn promotion, check if there is addition of piece
            for (int i = 0; i < moves.Length; i++)
            {
                if (moves[i].Length == 4)
                {
                    // it is pawn promotion, return right string
                    string tile = new string(new char[] { ColumnToChar(moves[i][2]), RowToOnScreenNumber(moves[i][1]) });
                    return string.Format("{0}{1}", tile, CharToSymbol(moves[i][3]));
                }
            }

            return "";
        }

        /// <summary> Converts time left in seconds to a string with minutes and seconds </summary>
        public static string SecondsToString(double seconds)
        {
            double total_seconds = seconds;
            int minutes = (int)seconds / 60;
            seconds = (int)seconds % 60;
            double milliseconds = total_seconds % 1;
            milliseconds = Math.Round(milliseconds, 2) * 100;

            if (total_seconds >= 10)
                return minutes.ToString() + ":"+ (seconds < 10 ? "0" : "") + seconds.ToString();
            else
                return minutes.ToString() + ":" + (seconds < 10 ? "0" : "") + seconds.ToString() + ":" + (milliseconds < 10 ? "0" : "") + milliseconds.ToString();
        }
    }
}
