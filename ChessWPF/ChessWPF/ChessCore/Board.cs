using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChessWPF
{
     /* Keeps a representation of the board. */
    public class Board : IEnumerable<Piece>
    {
        private Piece[,] A2D;

        private int my_color;

        private King WhiteKing;
        private King BlackKing;

        /// <summary> Keeps all moves. So can reverse and show them later </summary>
        private string current_move;
        public Stack<string> moves_stack;

        public Board(int my_color)
        {
            this.WhiteKing = new King(Consts.WHITE, true);
            this.BlackKing = new King(Consts.BLACK, true);
            this.moves_stack = new Stack<string>();
            this.current_move = "";

            this.my_color = my_color;

            this.A2D = Consts.StartingBoard(BlackKing, WhiteKing);
        }

        // <-------- Allows doing something like foreach(Piece p in board)... --------->

        public IEnumerator<Piece> GetEnumerator()
        {
            for (int i = 0; i < Consts.BOARD_SIZE; i++)
            {
                for (int j = 0; j < Consts.BOARD_SIZE; j++)
                {
                    if (this.A2D[i, j] != null)
                        yield return this.A2D[i, j];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        // <----------- Functions for playing with pieces on board ----------->

        // Length = 5
        public string MovePiece(int from_row, int from_col, int to_row, int to_col, bool animate)
        {
            Piece piece = this.A2D[from_row, from_col];

            this.A2D[to_row, to_col] = piece;

            // assuming the are no pieces in the "to" cell
            this.A2D[from_row, from_col] = null;
            piece.ChangePos(to_row, to_col, animate);

            // move description
            string des = (current_move.Length > 0 ? "," : "") + from_row + "" + from_col + "" + to_row + "" + to_col + Util.PieceToChar(piece);
            this.current_move += des;
            return des;
        }

        // Length = 6
        public string RemovePiece(int row, int col, Canvas canvas)
        {
            Piece piece = this.A2D[row, col];
            // find type of piece
            char PDes = Util.PieceToChar(piece);

            this.A2D[row, col] = null;

            if (canvas != null && piece.Img != null)
                canvas.Children.Remove(piece.Img);

            string num = piece.num_moves.ToString();
            if (num.Length == 1)
                num = '0' + num;
            string des = (current_move.Length > 0 ? "," : "") + num + piece.color + PDes + row + "" + col;
            this.current_move += des;
            return des;
        }

        // Length = 4
        public string AddPiece(int row, int col, Piece piece, Canvas canvas)
        {
            this.A2D[row, col] = piece;

            if (canvas != null)
            {
                canvas.Children.Add(piece.draw());
                if (my_color == Consts.BLACK)
                    piece.SetBlackPerpective();
            }

            piece.ChangePos(row, col, false);

            string des = (current_move.Length > 0 ? "," : "") + piece.color.ToString() + row + "" + col + Util.PieceToChar(piece);
            this.current_move += des;
            return des;
        }

        /// <summary> Pushes current_move to stack </summary>
        public void EndMove()
        {
            this.moves_stack.Push(this.current_move);
            this.current_move = "";
        }

        /// <summary> Returns board to it's state before last move. Used mainly for move validation </summary>
        public void ReverseMove(Canvas canvas)
        {
            string[] moves = this.moves_stack.Pop().Split(',');

            // Iterates over moves backwards. For each, does its opposite
            for (int i = moves.Length - 1; i >= 0; i--)
            {
                string move = moves[i];
                if (move.Length == 6) // Piece removal, to reverse, add
                {
                    int num = (move[0] - '0') * 10 + (move[1] - '0');
                    int row = move[4] - '0';
                    int col = move[5] - '0';
                    Piece pe = Util.CharToPiece(move[3], move[2] - '0');
                    this.AddPiece(row, col, pe, canvas);
                    pe.num_moves = num;
                }
                else if (move.Length == 5) // Piece moving, to reverse, move
                {
                    int from_row = move[0] - '0';
                    int from_col = move[1] - '0';
                    int to_row = move[2] - '0';
                    int to_col = move[3] - '0';
                    Piece p = this.A2D[to_row, to_col];
                    this.MovePiece(to_row, to_col, from_row, from_col, false);

                    // 1 for the current moving function, the second for the one we are reversing
                    p.num_moves -= 2;
                }
                else if (move.Length == 4) // Piece adding, to reverse, remove
                {
                    int row = move[1] - '0';
                    int col = move[2] - '0';
                    this.RemovePiece(row, col, canvas);
                }
            }

            this.current_move = "";
        }

        // <----------------------------------------------------------------->

        // <------------ Processing and executing moves --------------------->

        /// <summary> Validating all moves from board perspective. Also can add moves such as castling, en pessant... </summary>
        public void PostProcessMoves(Canvas canvas, int from_row, int from_col, PointsCollection Points, int for_color)
        {
            Piece[,] B = this.Board2D;
            Piece p = this.Board2D[from_row, from_col];

            // || <-- Check if can do en pessant --> ||
            if (p is Pawn)
            {
                int addition = p.color == Consts.BLACK ? 1 : -1;
                foreach (_Point pt in ((Pawn)p).EnPessant(B))
                {
                    string last = this.moves_stack.Peek();
                    this.ReverseMove(canvas);
                    if (this.Board2D[pt.Row - addition, pt.Col] == null) // last move was the pawn movment
                        Points.AddPoint(pt);
                    this.ExecuteMove(canvas, last, false);
                }
            }

            // || <-- Check if can do castling --> ||
            if (p is King && p.num_moves == 0)
            {
                int r = from_row;
                int c = from_col;
                Piece r1 = this.Board2D[p.Row, 0];
                Piece r2 = this.Board2D[p.Row, 7];

                if (r1 is Rook && r1.num_moves == 0 && !this.CheckIfInDanger(r, c - 1, for_color) && !this.CheckIfInDanger(r, c - 2, for_color) && !this.CheckIfInDanger(r, c, for_color))
                {
                    // queen side castling
                    if (B[r, 1] == null && B[r, 2] == null && B[r, 3] == null)
                        Points.AddPoint(r, c - 2);
                }

                if (r2 is Rook && r2.num_moves == 0 && !this.CheckIfInDanger(r, c + 1, for_color) && !this.CheckIfInDanger(r, c + 2, for_color) && !this.CheckIfInDanger(r, c, for_color))
                {
                    // right side castling
                    if (B[r, 6] == null && B[r, 5] == null)
                        Points.AddPoint(r, c + 2);
                }
            }

            // || <-- Check that none of the moves results in check --> ||
            for (int i = Points.Count - 1; i >= 0; i--)
            {
                int row = Points[i].Row;
                int col = Points[i].Col;

                this.ExecuteMove(canvas, from_row, from_col, row, col, false);

                if ((for_color == Consts.BLACK && this.BlackInCheck()) || (for_color == Consts.WHITE && this.WhiteInCheck()))
                {
                    // Move is not valid
                    Points.RemovePoint(Points[i]);
                }
                this.ReverseMove(canvas);
            }
        }

        /// <summary> Executes a move from one cell to the other. By default promotes to queen for move validation </summary>
        public string ExecuteMove(Canvas canvas, int from_row, int from_col, int to_row, int to_col, bool animate, char to_promote_if = 'Q')
        {
            if (this.Board2D[to_row, to_col] != null)
                this.RemovePiece(to_row, to_col, canvas);

            Piece piece = this.Board2D[from_row, from_col];

            // || <-- Check if can do castling -->
            if (piece is King && Math.Abs(to_col - piece.Col) == 2)
            {
                if (to_col > piece.Col) // king side castling
                    this.MovePiece(piece.Row, 7, piece.Row, 5, animate);
                else // queen side castling
                    this.MovePiece(piece.Row, 0, piece.Row, 3, animate);
            }

            // || <-- Check if can do en pessant --> ||
            if (piece is Pawn && ((Pawn)piece).EnPessant(this.Board2D).PointInCollection(to_row, to_col))
            {
                string last = this.moves_stack.Peek();
                if (last.Length == 5 && last[4] == 'P' && (last[2] - '0') == from_row && (last[3] - '0') == to_col)
                {
                    int addition = piece.color == Consts.BLACK ? 1 : -1;
                    this.RemovePiece(to_row - addition, to_col, canvas);
                }
            }

            this.MovePiece(piece.Row, piece.Col, to_row, to_col, animate);

            // || <-- Check if can do pawn promotion --> ||
            if (piece is Pawn && (piece.Row == 0 || piece.Row == 7))
            {
                Piece new_p = Util.CharToPiece(to_promote_if, piece.color);
                this.RemovePiece(to_row, to_col, canvas);
                this.AddPiece(to_row, to_col, new_p, canvas);
            }

            this.EndMove();
            return this.moves_stack.Peek();
        }

        /// <summary> Executes a move from one cell to the other, madeup from a list of actions, represented as strings </summary>
        public void ExecuteMove(Canvas canvas, string Moves, bool animate)
        {
            string[] Changes = Moves.Split(',');

            for (int i = 0; i < Changes.Length; i++)
            {
                string Change = Changes[i];
                if (Change.Length == 6)
                {
                    // Piece removal.
                    int row = Change[4] - '0';
                    int col = Change[5] - '0';
                    this.RemovePiece(row, col, canvas);
                }
                else if (Change.Length == 5)
                {
                    // Piece movment
                    int from_row = Change[0] - '0';
                    int from_col = Change[1] - '0';
                    int to_row = Change[2] - '0';
                    int to_col = Change[3] - '0';
                    this.MovePiece(from_row, from_col, to_row, to_col, animate);
                }
                else if (Change.Length == 4)
                {
                    // Piece addition, always queen
                    int row = Change[1] - '0';
                    int col = Change[2] - '0';
                    Piece q = Util.CharToPiece(Change[3], Change[0] - '0');
                    this.AddPiece(row, col, q, canvas);
                }
            }

            this.EndMove();
        }

        // <----------------------------------------------------------------->

        /// <summary> Sets up all pieces on board, and adds them to canvas if it's not null </summary>
        public void draw(Canvas canvas)
        {
            // Check to see if needs to flip the board because player is playing as black
            if (my_color == Consts.BLACK && canvas != null)
            {
                TransformGroup Group = new TransformGroup();
                Group.Children.Add(new RotateTransform(180, 380, 380));
                canvas.RenderTransform = Group;
            }

            for (int i = 0; i < Consts.BOARD_SIZE; i++)
            {
                for (int j = 0; j < Consts.BOARD_SIZE; j++)
                {
                    if (this.A2D[i, j] != null)
                    {
                        Piece p = this.A2D[i, j];

                        if (canvas != null)
                        {
                            Image PieceImg = p.draw();
                            PieceImg.IsHitTestVisible = false;
                            canvas.Children.Add(PieceImg);
                            if (my_color == Consts.BLACK)
                                p.SetBlackPerpective();
                        }
                        p.ChangePos(i, j, false);
                    }
                }
            }
        }

        /// <summary> Evaluates current state of game, draws kings in danger </summary>
        public void EvaluateGame(Squares SquaresUI)
        {
            SquaresUI.ClearSelection();
            SquaresUI.ClearDanger();

            if (this.WhiteInCheck())
                SquaresUI.SetInDanger(this.White_King.Row, this.White_King.Col);
            if (this.BlackInCheck())
                SquaresUI.SetInDanger(this.Black_King.Row, this.Black_King.Col);
        }

        // <--------- Used for check/checkmate checking ------------>
        public bool CheckIfInDanger(int row, int col, int of_color)
        {
            foreach (Piece p in this)
            {
                if (p != null && p.color != of_color)
                {
                    // piece of the opposite color
                    if (p.GetMoves(this.A2D).PointInCollection(row, col))
                        return true;
                }
            }
            return false;
        }

        public bool WhiteInCheck()
        {
            return this.CheckIfInDanger(this.WhiteKing.Row, this.WhiteKing.Col, Consts.WHITE);
        }

        public bool BlackInCheck()
        {
            return this.CheckIfInDanger(this.BlackKing.Row, this.BlackKing.Col, Consts.BLACK);
        }

        public King Black_King { get { return this.BlackKing; } }
        
        public King White_King { get { return this.WhiteKing; } }

        public Piece[,] Board2D { get { return this.A2D; } }
    }
}
