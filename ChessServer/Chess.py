import math

WHITE = 0
BLACK = 1
PieceCharsDict = {'Pawn': 'P', 'Rook': 'R', 'Bishop': 'B', 'King': 'K', 'Knight': 'H', 'Queen': 'Q'}

# <------------------>
#   Pieces Classes
# <------------------>


class Piece:
    def __init__(self, color):
        self.color = color
        self.num_moves = -1
        self.row = -1
        self.col = -1

    def set_pos(self, row, col):
        self.row = row
        self.col = col
        self.num_moves += 1


class Rook(Piece):
    def __init__(self, color):
        Piece.__init__(self, color)

    def get_valid_moves(self, board):
        """ Gets 8x8 list, returns
            list of pseudo legal moves of Rook """
        valid = []  # List of tuples (to_row, to_col)
        r, c = self.row, self.col

        for i in range(r + 1, 8):
            if board[i][c] is None:
                valid.append((i, c))
            else:
                if board[i][c].color != self.color:
                    valid.append((i, c))
                break

        for i in range(r - 1, -1, -1):
            if board[i][c] is None:
                valid.append((i, c))
            else:
                if board[i][c].color != self.color:
                    valid.append((i, c))
                break

        for i in range(c + 1, 8):
            if board[r][i] is None:
                valid.append((r, i))
            else:
                if board[r][i].color != self.color:
                    valid.append((r, i))
                break

        for i in range(c - 1, -1, -1):
            if board[r][i] is None:
                valid.append((r, i))
            else:
                if board[r][i].color != self.color:
                    valid.append((r, i))
                break

        return valid


class Bishop(Piece):
    def __init__(self, color):
        Piece.__init__(self, color)

    def get_valid_moves(self, board):
        """ Gets 8x8 list and returns a list of valid
            moves for bishop """
        valid = []
        # up and right
        r, c = self.row + 1, self.col + 1
        while r < 8 and c < 8:
            if board[r][c] is None:
                valid.append((r, c))
            else:
                if board[r][c].color != self.color:
                    valid.append((r, c))
                break
            r, c = r + 1, c + 1

        # left and down
        r, c = self.row - 1, self.col - 1
        while r >= 0 and c >= 0:
            if board[r][c] is None:
                valid.append((r, c))
            else:
                if board[r][c].color != self.color:
                    valid.append((r, c))
                break
            r, c = r - 1, c - 1

        # up and left
        r, c = self.row + 1, self.col - 1
        while r < 8 and c >= 0:
            if board[r][c] is None:
                valid.append((r, c))
            else:
                if board[r][c].color != self.color:
                    valid.append((r, c))
                break
            r, c = r + 1, c - 1

        # right and down
        r, c = self.row - 1, self.col + 1
        while r >= 0 and c < 8:
            if board[r][c] is None:
                valid.append((r, c))
            else:
                if board[r][c].color != self.color:
                    valid.append((r, c))
                break
            r, c = r - 1, c + 1

        return valid


class Knight(Piece):
    def __init__(self, color):
        Piece.__init__(self, color)

    def get_valid_moves(self, board):
        """ Gets 8x8 list and returns valid
            moves of Knight """

        valid = []
        r, c = self.row, self.col
        pos = [(1 , 2), (-1 , 2), (1 , -2), (-1 , -2),
                (2, 1), (-2, 1), (2, -1), (-2, -1)]

        for p in pos:
            _r , _c = r + p[0], c + p[1]
            if _r < 8 and _r >= 0 and _c < 8 and _c >= 0:
                if board[_r][_c] is None or board[_r][_c].color != self.color:
                    valid.append((_r, _c))

        return valid


class Queen(Piece):
    def __init__(self, color):
        Piece.__init__(self, color)

    def get_valid_moves(self, board):
        """ Gets 8x8 list and returns
            a list of valid moves of Queen """

        r, c = self.row, self.col

        # Queen combines the logic
        # of bishop and rook
        rook = Rook(self.color)
        rook.set_pos(r, c)
        bishop = Bishop(self.color)
        bishop.set_pos(r, c)

        return (rook.get_valid_moves(board) + bishop.get_valid_moves(board))


class King(Piece):
    def __init__(self, color):
        Piece.__init__(self, color)

    def get_valid_moves(self, board):
        """ Gets 8x8 list and returns a list of
            valid moves for King """

        valid = []
        r, c = self.row, self.col
        pos = [(1, 0), (1, -1), (1, 1), (-1, 0),
                (-1, 1), (-1, -1), (0, 1), (0, -1)]
        for p in pos:
            _r, _c = r + p[0], c + p[1]
            if _r < 8 and _r >= 0 and _c < 8 and _c >= 0:
                if board[_r][_c] is None or board[_r][_c].color != self.color:
                    valid.append((_r, _c))

        return valid


class Pawn(Piece):
    def __init__(self, color):
        Piece.__init__(self, color)

    def get_valid_moves(self, board):
        """ Gets 8x8 list and returns a list of
            valid moves for Pawn """

        valid = []
        r, c = self.row, self.col
        add = 1 if self.color == BLACK else -1

        if board[r + add][c] is None:
            valid.append((r + add, c))
            if self.num_moves == 0 and board[r + add * 2][c] is None:
                valid.append((r + add * 2, c))

        if c < 7 and board[r + add][c + 1] != None:
            # Maybe can eat right
            if board[r + add][c + 1].color != self.color:
                valid.append((r + add, c + 1))

        if c > 0 and board[r + add][c - 1] != None:
            # Maybe can eat left
            if board[r + add][c - 1].color != self.color:
                valid.append((r + add, c - 1))

        return valid

    def en_pessant(self, board):
        """ Returns a list of pseduo-legal en-pessant moves """
        valid = []
        r, c = self.row, self.col
        add = 1 if self.color == BLACK else -1
        if (self.color == WHITE and r == 3) or (self.color == BLACK and r == 4):
            if c > 0:
                # Left side
                p = board[r][c - 1]
                if p != None and isinstance(p, Pawn) and p.color != self.color and p.num_moves == 1:
                    valid.append((r + add, c - 1))
            if c < 7:
                # right side
                p = board[r][c + 1]
                if p != None and isinstance(p, Pawn) and p.color != self.color and p.num_moves == 1:
                    valid.append((r + add, c + 1))
        return valid

# <------------>
#  functions
# <------------>

def get_starting_board(white_king, black_king):
    """ Creates and setups the board, gets the kings as parameters
        So caller (Usually board can keep references) """
    Board = [
    [Rook(BLACK), Knight(BLACK), Bishop(BLACK), Queen(BLACK), black_king, Bishop(BLACK), Knight(BLACK), Rook(BLACK)],
    [Pawn(BLACK), Pawn(BLACK), Pawn(BLACK), Pawn(BLACK), Pawn(BLACK), Pawn(BLACK), Pawn(BLACK), Pawn(BLACK)],
    [None, None, None, None, None, None, None, None],
    [None, None, None, None, None, None, None, None],
    [None, None, None, None, None, None, None, None],
    [None, None, None, None, None, None, None, None],
    [Pawn(WHITE), Pawn(WHITE), Pawn(WHITE), Pawn(WHITE), Pawn(WHITE), Pawn(WHITE), Pawn(WHITE), Pawn(WHITE)],
    [Rook(WHITE), Knight(WHITE), Bishop(WHITE), Queen(WHITE), white_king, Bishop(WHITE), Knight(WHITE), Rook(WHITE)]
    ]

    for i in range(8):
        for j in range(8):
            if Board[i][j] != None:
                Board[i][j].set_pos(i, j)

    return Board


def get_testing_board(white_king, black_king):
    Board = [
    [Rook(BLACK), Knight(BLACK), Bishop(BLACK), Queen(BLACK), None, black_king, None, Rook(BLACK)],
    [Pawn(BLACK), Pawn(BLACK), None, Pawn(WHITE), Bishop(BLACK), Pawn(BLACK), Pawn(BLACK), Pawn(BLACK)],
    [None, None, Pawn(BLACK), None, None, None, None, None],
    [None, None, None, None, None, None, None, None],
    [None, None, Bishop(WHITE), None, None, None, None, None],
    [None, None, None, None, None, None, None, None],
    [Pawn(WHITE), Pawn(WHITE), Pawn(WHITE), None, Knight(WHITE), Knight(BLACK), Pawn(WHITE), Pawn(WHITE)],
    [Rook(WHITE), Knight(WHITE), Bishop(WHITE), Queen(WHITE), white_king, None, None, Rook(WHITE)]
    ]

    Board[7][4].num_moves = -1
    Board[0][5].num_moves = 0
    Board[2][2].num_moves = 0
    Board[1][3].num_moves = 3

    for i in range(8):
        for j in range(8):
            if Board[i][j] != None:
                Board[i][j].set_pos(i, j)

    return Board


def piece_to_char(p):
    return PieceCharsDict[p.__class__.__name__]

def char_to_piece(char, color):
    if char == 'Q':
        return Queen(color)
    elif char == 'H':
        return Knight(color)
    elif char == 'K':
        return King(color)
    elif char == 'B':
        return Bishop(color)
    elif char == 'R':
        return Rook(color)
    elif char == 'P':
        return Pawn(color)

# <------------>
#  Board
# <------------>

class Board:
    def __init__(self):
        self.moves = []
        self.white_king = King(WHITE)
        self.black_king = King(BLACK)
        self.board = get_starting_board(self.white_king, self.black_king)
        self.evaluate_game(WHITE) # white starts

    def evaluate_game(self, as_color):
        self.possible_moves = set()
        self.white_check = self.white_in_check()
        self.black_check = self.black_in_check()

        for row in range(8):
            for col in range(8):
                piece = self.board[row][col]
                if piece and piece.color == as_color:
                    moves = piece.get_valid_moves(self.board)
                    moves = self.post_process_moves(row, col, moves, as_color)
                    if isinstance(piece, Pawn) and ((row == 1 and piece.color == WHITE) or (row == 6 and piece.color == BLACK)):
                        for move in moves:
                            # Every promotion can be of type Queen, Rook, Knight and Bishop
                            # The default is Queen
                            self.possible_moves.add(move[:-1:] + "H")
                            self.possible_moves.add(move[:-1:] + "R")
                            self.possible_moves.add(move[:-1:] + "B")
                    self.possible_moves.update(moves)
    # <--------->
    # Internal API: Move, Remove, Add:
    # <--------->

    def move(self, from_row, from_col, to_row, to_col):
        """ Moves a piece from one row and column to the other
            Assumming the destination is not occupied. If it is,
            Remove method should be called first """
        p = self.board[from_row][from_col]
        self.board[from_row][from_col] = None
        self.board[to_row][to_col] = p
        p.set_pos(to_row, to_col)
        return str(from_row) + str(from_col) + str(to_row) + str(to_col) +  piece_to_char(p)

    def remove(self, row, col):
        """ Removes a piece from a cell """
        p = self.board[row][col]
        self.board[row][col] = None
        num_moves = str(p.num_moves)
        if len(num_moves) == 1:
            num_moves = '0' + num_moves
        return num_moves + str(p.color) + piece_to_char(p) + str(row) + str(col)

    def add(self, row, col, piece):
        """ Adds a piece to a cell and sets it's position """
        self.board[row][col] = piece
        piece.set_pos(row, col)
        return str(piece.color) + str(row) + str(col) + piece_to_char(piece)

    # <---------->
    # Validating and executing moves
    # <---------->

    def post_process_moves(self, r, c, moves_list, for_player_color):
        piece = self.board[r][c]
        # <----- Check if can do en-pessant -------->
        if isinstance(piece, Pawn):
            add = 1 if piece.color == BLACK else -1
            for cell in piece.en_pessant(self.board):
                last = self.moves[-1]
                del self.moves[-1]
                self.reverse_move(last)
                if self.board[cell[0] - add][cell[1]] is None:
                    # Last move was pawn movment
                    moves_list.append(cell)
                self.execute_string(last)

        # <----- Check if can do castling ---------->
        if isinstance(piece, King) and piece.num_moves == 0:
            rook1 = self.board[r][0]
            rook2 = self.board[r][7]

            if isinstance(rook1, Rook) and not self.square_in_danger(r, c - 1, for_player_color) and not self.square_in_danger(r, c, for_player_color):
                # Queen Side castling
                if self.board[r][c - 1] is None and self.board[r][c - 2] is None and self.board[r][c - 3] is None:
                    moves_list.append((r, c - 2))
            if isinstance(rook2, Rook) and not self.square_in_danger(r, c + 1, for_player_color) and not self.square_in_danger(r, c, for_player_color):
                if self.board[r][c + 1] is None and self.board[r][c + 2] is None:
                    moves_list.append((r, c + 2))

        # <----- Check that none of the moves will result in check ----->
        new_list = []

        for move in moves_list:
            des = self.execute_move(r, c, move[0], move[1], 'Q')
            if for_player_color == WHITE:
                if not self.square_in_danger(self.white_king.row, self.white_king.col, for_player_color):
                    new_list.append(des)
            else:
                if not self.square_in_danger(self.black_king.row, self.black_king.col, for_player_color):
                    new_list.append(des)
            self.reverse_move(des)
            # Remove also last move from "moves" stack
            del self.moves[-1]
        return new_list

    def execute_move(self, from_row, from_col, to_row, to_col, to_promote_if = 'Q'):
        description = ""
        if self.board[to_row][to_col] != None:
            description += self.remove(to_row, to_col) + ','

        p = self.board[from_row][from_col]

        # <----- Check if can do castling --------->
        if isinstance(p, King) and abs(to_col - from_col) == 2:
            if to_col > from_col:
                # King side castling
                description += self.move(from_row, 7, from_row, 5) + ','
            else:
                description += self.move(from_row, 0, from_row, 3) + ','

        # <----- Check if can do en-pessant --------->
        if isinstance(p, Pawn) and (to_row, to_col) in p.en_pessant(self.board):
            last = self.moves[-1]
            if len(last) == 5 and last[4] == 'P' and int(last[2]) == from_row and int(last[3]) == to_col:
                add = 1 if p.color == BLACK else -1
                description += self.remove(to_row - add, to_col) + ','

        description += self.move(from_row, from_col, to_row, to_col) + ','
        # <------ Check if can do pawn promotion ------->
        if isinstance(p, Pawn) and (p.row == 0 or p.row == 7):
            new_p = char_to_piece(to_promote_if, p.color)
            description += self.remove(to_row, to_col) + ','
            description += self.add(to_row, to_col, new_p) + ','

        self.moves.append(description[:-1:])
        return description[:-1:]

    def execute_string(self, moves):
        changes = moves.split(',')
        des = ''
        for change in changes:
            if len(change) == 6:
                row, col = int(change[4]), int(change[5])
                des += self.remove(row, col) + ','
            if len(change) == 5:
                from_row, from_col, to_row, to_col = int(change[0]), int(change[1]), int(change[2]), int(change[3])
                des += self.move(from_row, from_col, to_row, to_col) + ','
            if len(change) == 4:
                row, col = int(change[1]), int(change[2])
                p = char_to_piece(change[3], int(change[0]))
                des += self.add(row, col, p) + ','
        self.moves.append(des[:-1:])
        return des[:-1:]

    def reverse_move(self, moves):
        for move in reversed(moves.split(',')):
            if len(move) == 5:
                from_row, from_col, to_row, to_col = int(move[0]), int(move[1]), int(move[2]), int(move[3])
                self.move(to_row, to_col, from_row, from_col)
                self.board[from_row][from_col].num_moves -= 2
            elif len(move) == 6:
                num_moves = int(move[0] + move[1])
                color, char, row, col = int(move[2]), move[3], int(move[4]), int(move[5])
                p = char_to_piece(char, color)
                self.add(row, col, p)
                p.num_moves = num_moves
            elif len(move) == 4:
                row, col = int(move[1]), int(move[2])
                self.remove(row, col)

    # <------>
    # Evaluating board state
    # <------->
    def square_in_danger(self, row, col, for_color):
        bishop = [(1 ,1), (-1, -1), (1, -1), (-1, 1)]
        for add_r, add_c in bishop:
            r, c = row, col
            r, c = r + add_r, c + add_c
            while r < 8 and r >= 0 and c < 8 and c >= 0:
                if self.board[r][c] != None:
                    p = self.board[r][c]
                    if p.color != for_color and (isinstance(p, Bishop) or isinstance(p, Queen)):
                        return True
                    break
                r, c = r + add_r, c + add_c

        rook = [(1, 0), (-1, 0), (0, 1), (0, -1)]
        for add_r, add_c in rook:
            r, c = row, col
            r, c = r + add_r, c + add_c
            while r < 8 and r >= 0 and c < 8 and c >= 0:
                if self.board[r][c] != None:
                    p = self.board[r][c]
                    if p.color != for_color and (isinstance(p, Rook) or isinstance(p, Queen)):
                        return True
                    break
                r, c = r + add_r, c + add_c

        knight = [(1 , 2), (-1 , 2), (1 , -2), (-1 , -2),
                (2, 1), (-2, 1), (2, -1), (-2, -1)]
        r, c = row, col
        for add_r, add_c in knight:
            _r, _c = r + add_r, c + add_c
            if _r >= 0 and _r < 8 and _c >= 0 and _c < 8:
                p = self.board[_r][_c]
                if isinstance(p, Knight) and p.color != for_color:
                    return True

        king = [(1, 0), (1, -1), (1, 1), (-1, 0),
                (-1, 1), (-1, -1), (0, 1), (0, -1)]
        r, c = row, col
        for add_r, add_c in king:
            _r, _c = r + add_r, c + add_c
            if _r >= 0 and _r < 8 and _c >= 0 and _c < 8:
                p = self.board[_r][_c]
                if isinstance(p, King) and p.color != for_color:
                    return True

        add = 1 if for_color == BLACK else -1
        r, c = row + add, col + 1
        if r < 8 and r >= 0 and c < 8 and c >= 0:
            p = self.board[r][c]
            if isinstance(p, Pawn) and p.color != for_color:
                return True

        r, c = row + add, col - 1
        if r < 8 and r >= 0 and c < 8 and c >= 0:
            p = self.board[r][c]
            if isinstance(p, Pawn) and p.color != for_color:
                return True

        return False


    def white_in_check(self):
        return self.square_in_danger(self.white_king.row, self.white_king.col, WHITE)

    def black_in_check(self):
        return self.square_in_danger(self.black_king.row, self.black_king.col, BLACK)

    def print_board(self):
        b_print = ""
        for i in self.board:
            for j in i:
                if j != None: b_print += piece_to_char(j)
                else: b_print += "#"
            b_print += "\n"
        return b_print



# <-------->
# Testing
# <-------->

def num_in_depth(b, depth, is_w = True):
    if depth == 0:
        return 1

    num_moves = 0
    b.evaluate_game(WHITE if is_w else BLACK)
    moves = b.possible_moves
    for move_str in moves:
        des = b.execute_string(move_str)
        num_moves += num_in_depth(b, depth - 1, not is_w)
        b.reverse_move(des)
        del b.moves[-1]

    return num_moves


if __name__ == "__main__":
    import time
    starting = [1, 20, 400, 8902]
    board = Board()
    for i in range(len(starting)):
        start = time.time()
        result = num_in_depth(board, i)
        print("{0} moves in {1} seconds".format(result, time.time() - start))

    print()
    print("<-------TESTING-BOARD--------->")
    print()

    testing = [1, 44, 1486, 62379]
    board = Board()
    board.board = get_testing_board(board.white_king, board.black_king)
    for i in range(len(testing)):
        start = time.time()
        result = num_in_depth(board, i)
        print("{0} moves in {1} seconds".format(result, time.time() - start))
