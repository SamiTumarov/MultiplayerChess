using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChessWPF
{
    /// <summary> Wraps square drawing functinality </summary>
    public class Squares
    {
        private Canvas canvas;
        private List<Square> SquaresList;

        public Squares(Canvas canvas, Action<object, RoutedEventArgs, int> clickCallback)
        {
            this.canvas = canvas;
            this.SquaresList = new List<Square>(Consts.BOARD_SIZE * Consts.BOARD_SIZE); // 8x8 Board
            this.Draw(canvas, clickCallback);
        }

        /// <summary> Draws board on canvas </summary>
        public void Draw(Canvas canvas, Action<object, RoutedEventArgs, int> clickCallback)
        {
            int index = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Square NewSquare = new Square(index % 2 == 0 ? Consts.FIRST_SQUARE_COLOR : Consts.SECOND_SQUARE_COLOR, i, j);
                    this.SquaresList.Add(NewSquare);
                    this.canvas.Children.Add(NewSquare.Draw(clickCallback));
                    index++;
                }
                index++;
            }
        }

        /// Clears selected square and possible moves
        public void ClearSelection()
        {
            foreach(Square Sq in this.SquaresList)
            {
                Sq.Is_selected = false;
                Ellipse ToRemove = Sq.RemoveEllipseOnSquare();
                if (ToRemove != null)
                    this.canvas.Children.Remove(ToRemove);
            }
        }

        /// <summary> Clears the red square if there is one </summary>
        public void ClearDanger()
        {
            foreach (Square Sq in this.SquaresList)
                Sq.Is_in_danger = false;
        }

        /// Marks one square as selected, and shows on canvas the possible moves
        public void ShowMove(int row, int col, PointsCollection Points)
        {
            int index = row * 8 + col;

            // Save information to restore state later
            this.SquaresList[index].Is_selected = true;

            foreach(_Point p in Points)
            {
                index = p.Row * 8 + p.Col;
                this.canvas.Children.Add(this.SquaresList[index].DrawEllipseOnSquare());
            }
        }

        public void SetInDanger(int row, int col)
        {
            SquaresList[row * 8 + col].Is_in_danger = true;
        }

    }

    class Square
    {
        private Brush color;
        private Rectangle square_rect;
        private Ellipse possible_elipse;

        private bool is_selected;
        private bool is_in_danger;

        private int row;
        private int col;
        
        public Square(Brush color, int row, int col)
        {
            this.color = color;
            this.is_selected = false;
            this.is_in_danger = false;

            this.row = row;
            this.col = col;
        }

        // Creates rectange object and returns it
        public Rectangle Draw(Action<object, RoutedEventArgs, int> clickCallback)
        {
            this.square_rect = new Rectangle()
            {
                Width = Consts.CUBE_SIZE,
                Height = Consts.CUBE_SIZE,
                Fill = this.color
            };

            // Bind click-listener
            this.square_rect.MouseLeftButtonDown += (sender, EventArgs) => { clickCallback(sender, EventArgs, this.row * 8 + this.col); };

            Canvas.SetLeft(this.square_rect, Consts.CUBE_SIZE * this.col);
            Canvas.SetTop(this.square_rect, Consts.CUBE_SIZE * this.row);

            return this.square_rect;
        }

        public bool Is_selected
        {
            get { return this.is_selected; }
            set
            {
                this.is_selected = value;
                this.evaluate_color();
            }
        }

        public bool Is_in_danger
        {
            get { return this.is_in_danger; }
            set
            {
                this.is_in_danger = value;
                this.evaluate_color();
            }
        }

        // Determines square color
        private void evaluate_color()
        {
            if (this.is_selected)
                this.square_rect.Fill = Consts.SELECTED_SQUARE_COLOR;
            else
            {
                if (this.is_in_danger)
                    this.square_rect.Fill = Consts.DANGEROUS_SQUARE_COLOR;
                else
                    this.square_rect.Fill = this.color;
            }
        }

        public Ellipse DrawEllipseOnSquare()
        {
            this.possible_elipse = new Ellipse()
            {
                Width = Consts.CUBE_SIZE / 4,
                Height = Consts.CUBE_SIZE / 4,
                Fill = Consts.POSSIBLE_MOVE_COLOR
            };

            this.possible_elipse.IsHitTestVisible = false;
            Canvas.SetLeft(this.possible_elipse, Consts.CUBE_SIZE * (this.col + 0.375));
            Canvas.SetTop(this.possible_elipse, Consts.CUBE_SIZE * (this.row + 0.375));

            return this.possible_elipse;
        }

        public Ellipse RemoveEllipseOnSquare()
        {
            Ellipse ToReturn = this.possible_elipse;
            this.possible_elipse = null;
            return ToReturn;
        }
    }
}
