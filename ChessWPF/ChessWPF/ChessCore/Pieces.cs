using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ChessWPF
{
    public abstract class Piece
    {
        const double ANIMATION_TIME = 0.18;

        public int color;
        public string ImgPath;
        public BitmapImage Bmp;
        public Image Img;

        protected int row, column;
        public int num_moves;

        /// <summary> Creates a new piece. </summary>
        public Piece(int color)
        {
            this.color = color;
            this.num_moves = -1; // moving the piece to it's initial location will set it to 0
        }

        /// <summary> Returns an image object that is used to draw onto canvas.
        /// The image then is stored. So through the piece, the image can be moved </summary>
        public Image draw()
        {
            this.Img = new Image()
            {
                Width = 79,
                Height = this.Bmp.Height,
                Source = this.Bmp,
                RenderTransformOrigin = new Point(0.5, 0.5),
                IsHitTestVisible = false
            };

            return this.Img;
        }

        /// <summary> Changes the position of the piece. And with it, changes the position of the image </summary>
        public void ChangePos(int row, int column, bool animate)
        {
            this.row = row;
            this.column = column;

            if (this.Img != null)
            {
                if (num_moves >= 0 && animate)
                {
                    // Animate movment
                    double x = Canvas.GetLeft(this.Img);
                    double y = Canvas.GetTop(this.Img);
                    double endx = this.column * Consts.CUBE_SIZE + 8;
                    double endy = this.row * Consts.CUBE_SIZE;

                    double distance = Math.Sqrt((endy - y) * (endy - y) + (endx - x) * (endx - x));

                    DoubleAnimation anim1 = new DoubleAnimation(x, endx, TimeSpan.FromSeconds(ANIMATION_TIME + distance / 13000));
                    DoubleAnimation anim2 = new DoubleAnimation(y, endy, TimeSpan.FromSeconds(ANIMATION_TIME + distance / 13000));

                    this.Img.BeginAnimation(Canvas.LeftProperty, anim1);
                    this.Img.BeginAnimation(Canvas.TopProperty, anim2);

                }
                else
                {
                    Canvas.SetLeft(this.Img, this.column * Consts.CUBE_SIZE + 8);
                    Canvas.SetTop(this.Img, this.row * Consts.CUBE_SIZE);
                }
            }
            this.num_moves += 1;
        }

        /// <summary> Rotates the position of the image. So it matches black player's perspective </summary>
        public void SetBlackPerpective()
        {
            RotateTransform rotate = new RotateTransform(180, 0.5, 0.5);
            this.Img.RenderTransform = rotate;
        }

        /// <summary> Returns a collection of valid moves the piece can do. Implemented by inheriting classes </summary>
        public abstract PointsCollection GetMoves(Piece[,] Pos);

        public int Row { get { return this.row; } }
        public int Col { get { return this.column; } }
    }


    public class Rook: Piece
    {
        public Rook(int color) : base(color)
        {
            if (this.color == Consts.WHITE)
                this.ImgPath = "pack://application:,,,/Images/white_rook.png";
            else
                this.ImgPath = "pack://application:,,,/Images/black_rook.png";
            this.Bmp = new BitmapImage(new Uri(this.ImgPath));
        }

        public override PointsCollection GetMoves(Piece[,] Pos)
        {
            PointsCollection PossibleMoves = new PointsCollection();
            int r = this.row; // Make name shorter
            int c = this.column;

            for (int i = r + 1; i < Consts.BOARD_SIZE; i++)
            {
                if (Util.ColorOnSquare(Pos, i, c) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, i, c) != this.color)
                        PossibleMoves.AddPoint(i, c);
                    break; // Stop when colliding
                }
                PossibleMoves.AddPoint(i, c);
            }

            for (int i = r - 1; i >= 0; i--)
            {
                if (Util.ColorOnSquare(Pos, i, c) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, i, c) != this.color)
                        PossibleMoves.AddPoint(i, c);
                    break; // Stop when colliding
                }
                PossibleMoves.AddPoint(i, c);
            }

            for (int i = c + 1; i < Consts.BOARD_SIZE; i++)
            {
                if (Util.ColorOnSquare(Pos, r, i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r, i) != this.color)
                        PossibleMoves.AddPoint(r, i);
                    break;
                }
                PossibleMoves.AddPoint(r, i);
            }

            for (int i = c - 1; i >= 0; i--)
            {
                if (Util.ColorOnSquare(Pos, r, i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r, i) != this.color)
                        PossibleMoves.AddPoint(r, i);
                    break;
                }
                PossibleMoves.AddPoint(r, i);
            }

            return PossibleMoves;
        }
    }


    public class Knight : Piece
    {
        public Knight(int color) : base(color)
        {
            if (this.color == Consts.WHITE)
                this.ImgPath = "pack://application:,,,/Images/white_knight.png";
            else
                this.ImgPath = "pack://application:,,,/Images/black_knight.png";
            this.Bmp = new BitmapImage(new Uri(this.ImgPath));
        }

        public override PointsCollection GetMoves(Piece[,] Pos)
        {
            PointsCollection PossibleMoves = new PointsCollection();
            int r = this.row; // Make name shorter
            int c = this.column;

            PointsCollection Temp = new PointsCollection();
            Temp.AddPoint(r + 2, c + 1);
            Temp.AddPoint(r + 2, c - 1);
            Temp.AddPoint(r - 2, c + 1);
            Temp.AddPoint(r - 2, c - 1);
            Temp.AddPoint(r + 1, c + 2);
            Temp.AddPoint(r + 1, c - 2);
            Temp.AddPoint(r - 1, c + 2);
            Temp.AddPoint(r - 1, c - 2);

            foreach (_Point p in Temp)
            {
                if (Util.InBoard(p.Row, p.Col) && Util.ColorOnSquare(Pos, p.Row, p.Col) != this.color)
                    PossibleMoves.AddPoint(p.Row, p.Col);
            } 

            return PossibleMoves;
        }
    }


    public class Bishop: Piece
    {
        public Bishop(int color) : base(color)
        {
            if (this.color == Consts.WHITE)
                this.ImgPath = "pack://application:,,,/Images/white_bishop.png";
            else
                this.ImgPath = "pack://application:,,,/Images/black_bishop.png";
            this.Bmp = new BitmapImage(new Uri(this.ImgPath));
        }

        public override PointsCollection GetMoves(Piece[,] Pos)
        {
            PointsCollection PossibleMoves = new PointsCollection();
            int r = this.row; // Make name shorter
            int c = this.column;

            for (int i = 1; c + i < Consts.BOARD_SIZE && r + i < Consts.BOARD_SIZE; i++)
            {
                if (Util.ColorOnSquare(Pos, r + i, c + i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r + i, c + i) != this.color)
                        PossibleMoves.AddPoint(r + i, c + i); // can eat
                    break; // Stop when colliding
                }
                PossibleMoves.AddPoint(r + i, c + i);
            }

            for (int i = 1; c - i >= 0 && r - i >= 0; i++)
            {
                if (Util.ColorOnSquare(Pos, r - i, c - i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r - i, c - i) != this.color)
                        PossibleMoves.AddPoint(r - i, c - i);
                    break;
                }
                PossibleMoves.AddPoint(r - i, c - i);
            }

            for(int i = 1; r + i < Consts.BOARD_SIZE && c - i >= 0; i++)
            {
                if (Util.ColorOnSquare(Pos, r + i, c - i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r + i, c - i) != this.color)
                        PossibleMoves.AddPoint(r + i, c - i);
                    break;
                }
                PossibleMoves.AddPoint(r + i, c - i);
            }

            for (int i = 1; r - i >= 0 && c + i < Consts.BOARD_SIZE; i++)
            {
                if (Util.ColorOnSquare(Pos, r - i, c + i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r - i, c + i) != this.color)
                        PossibleMoves.AddPoint(r - i, c + i);
                    break;
                }
                PossibleMoves.AddPoint(r - i, c + i);
            }

            return PossibleMoves;
        }
    }


    public class Queen: Piece
    {
        public Queen(int color) : base(color)
        {
            if (this.color == Consts.WHITE)
                this.ImgPath = "pack://application:,,,/Images/white_queen.png";
            else
                this.ImgPath = "pack://application:,,,/Images/black_queen.png";
            this.Bmp = new BitmapImage(new Uri(this.ImgPath));
        }


        public override PointsCollection GetMoves(Piece[,] Pos)
        {
            PointsCollection PossibleMoves = new PointsCollection();
            int r = this.row; // Make name shorter
            int c = this.column;

            // QUEEN Logic is logic + Bishop

            // ROOK LOGIC
            for (int i = r + 1; i < Consts.BOARD_SIZE; i++)
            {
                if (Util.ColorOnSquare(Pos, i, c) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, i, c) != this.color)
                        PossibleMoves.AddPoint(i, c);
                    break; // Stop when colliding
                }
                PossibleMoves.AddPoint(i, c);
            }

            for (int i = r - 1; i >= 0; i--)
            {
                if (Util.ColorOnSquare(Pos, i, c) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, i, c) != this.color)
                        PossibleMoves.AddPoint(i, c);
                    break; // Stop when colliding
                }
                PossibleMoves.AddPoint(i, c);
            }

            for (int i = c + 1; i < Consts.BOARD_SIZE; i++)
            {
                if (Util.ColorOnSquare(Pos, r, i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r, i) != this.color)
                        PossibleMoves.AddPoint(r, i);
                    break;
                }
                PossibleMoves.AddPoint(r, i);
            }

            for (int i = c - 1; i >= 0; i--)
            {
                if (Util.ColorOnSquare(Pos, r, i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r, i) != this.color)
                        PossibleMoves.AddPoint(r, i);
                    break;
                }
                PossibleMoves.AddPoint(r, i);
            }

            // BISHOP LOGIC

            for (int i = 1; c + i < Consts.BOARD_SIZE && r + i < Consts.BOARD_SIZE; i++)
            {
                if (Util.ColorOnSquare(Pos, r + i, c + i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r + i, c + i) != this.color)
                        PossibleMoves.AddPoint(r + i, c + i); // can eat
                    break; // Stop when colliding
                }
                PossibleMoves.AddPoint(r + i, c + i);
            }

            for (int i = 1; c - i >= 0 && r - i >= 0; i++)
            {
                if (Util.ColorOnSquare(Pos, r - i, c - i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r - i, c - i) != this.color)
                        PossibleMoves.AddPoint(r - i, c - i);
                    break;
                }
                PossibleMoves.AddPoint(r - i, c - i);
            }

            for (int i = 1; r + i < Consts.BOARD_SIZE && c - i >= 0; i++)
            {
                if (Util.ColorOnSquare(Pos, r + i, c - i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r + i, c - i) != this.color)
                        PossibleMoves.AddPoint(r + i, c - i);
                    break;
                }
                PossibleMoves.AddPoint(r + i, c - i);
            }

            for (int i = 1; r - i >= 0 && c + i < Consts.BOARD_SIZE; i++)
            {
                if (Util.ColorOnSquare(Pos, r - i, c + i) != Consts.NONE)
                {
                    if (Util.ColorOnSquare(Pos, r - i, c + i) != this.color)
                        PossibleMoves.AddPoint(r - i, c + i);
                    break;
                }
                PossibleMoves.AddPoint(r - i, c + i);
            }


            return PossibleMoves;
        }
    }


    public class King: Piece
    {
        public King(int color, bool work) : base(color)
        {
            if (this.color == Consts.WHITE)
                this.ImgPath = "pack://application:,,,/Images/white_king.png";
            else
                this.ImgPath = "pack://application:,,,/Images/black_king.png";
            this.Bmp = new BitmapImage(new Uri(this.ImgPath));
        }

        public override PointsCollection GetMoves(Piece[,] Pos)
        {
            PointsCollection PossibleMoves = new PointsCollection();

            int r = this.row; // Make name shorter
            int c = this.column;

            PointsCollection Temp = new PointsCollection();
            Temp.AddPoint(r + 1, c);
            Temp.AddPoint(r + 1, c + 1);
            Temp.AddPoint(r + 1, c - 1);
            Temp.AddPoint(r, c + 1);
            Temp.AddPoint(r, c - 1);
            Temp.AddPoint(r - 1, c + 1);
            Temp.AddPoint(r - 1, c);
            Temp.AddPoint(r - 1, c - 1);

            foreach(_Point point in Temp)
            {
                if (Util.InBoard(point.Row, point.Col) && Util.ColorOnSquare(Pos, point.Row, point.Col) != this.color)
                    PossibleMoves.AddPoint(point);
            }

            return PossibleMoves;
        }
    }


    public class Pawn: Piece
    {
        public Pawn(int color) : base(color)
        {
            if (this.color == Consts.WHITE)
                this.ImgPath = "pack://application:,,,/Images/white_pawn.png";
            else
                this.ImgPath = "pack://application:,,,/Images/black_pawn.png";
            this.Bmp = new BitmapImage(new Uri(this.ImgPath));
        }

        public override PointsCollection GetMoves(Piece[,] Pos)
        {
            PointsCollection PossibleMoves = new PointsCollection();

            int r = this.row;
            int c = this.column;

            // To move forward the black needs to add to the row, the white needs to subtruct
            int addition = this.color == Consts.BLACK ? 1 : -1;

            // Check if the square in front of the piece is clear
            if (Util.CleanSquare(Pos, r + addition, c))
            {
                PossibleMoves.AddPoint(r + addition, c);

                // check if can move two forward
                if (this.num_moves == 0 && Util.CleanSquare(Pos, r + (2 * addition), c))
                {
                    PossibleMoves.AddPoint(r + (2 * addition), c);
                }
            }

            // Check if can eat
            if (Util.InBoard(r + addition, c - 1))
            {
                int ColorOnSquare = Util.ColorOnSquare(Pos, r + addition, c - 1);
                if (ColorOnSquare != this.color && ColorOnSquare != Consts.NONE)
                    PossibleMoves.AddPoint(r + addition, c - 1);
            }

            if (Util.InBoard(r + addition, c + 1))
            {
                int ColorOnSquare = Util.ColorOnSquare(Pos, r + addition, c + 1);
                if (ColorOnSquare != this.color && ColorOnSquare != Consts.NONE)
                    PossibleMoves.AddPoint(r + addition, c + 1);
            }

            return PossibleMoves;
        }


        /// <summary> Part of enpessant rule, moves are validated later in board object </summary>
        public PointsCollection EnPessant(Piece[,] Pos)
        {
            PointsCollection PossibleMoves = new PointsCollection();
            int r = this.row;
            int c = this.column;
            int addition = this.color == Consts.BLACK ? 1 : -1;
            if ((this.color == Consts.WHITE && r == 3) || (this.color == Consts.BLACK && r == 4))
            {
                if (c > 0)
                {
                    // left side
                    Piece p = Pos[r, c - 1];
                    if (p != null && p is Pawn && p.color != this.color && p.num_moves == 1)
                        PossibleMoves.AddPoint(r + addition, c - 1);
                }
                if (c < 7)
                {
                    // right side
                    Piece p = Pos[r, c + 1];
                    if (p != null && p is Pawn && p.color != this.color && p.num_moves == 1)
                        PossibleMoves.AddPoint(r + addition, c + 1);
                }
            }
            return PossibleMoves;
        }
    }
}
