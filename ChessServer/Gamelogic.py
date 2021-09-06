import time
from Chess import Board

WHITE = 0
BLACK = 1


class Player:
    def __init__(self, name, color, time_left):
        self.time_left = time_left
        self.name = name
        self.color = color
        self.started_on = None

    def start_turn(self):
        # Calculates time
        self.started_on = time.time()

    def end_turn(self):
        # Calculates time
        self.time_left -= (time.time() - self.started_on)
        self.started_on = None


class Game:
    def __init__(self, game_time):
        self.white_player = None
        self.black_player = None
        self.turn = 0  # white
        # Not changing, representing the time that was chosen for the game
        self.game_time = game_time
        self.game_over = False
        self.board = Board()

    def add_player(self, player):
        if player.color == WHITE:
            self.white_player = player
        if player.color == BLACK:
            self.black_player = player

    def __getitem__(self, key):
        # Allows doing something like Game[WHITE] -> returns white player
        if key == WHITE:
            return self.white_player
        return self.black_player

    def toggle_turn(self, move):
        if self.turn == 1:
            self.turn = 0
            self.black_player.end_turn()
            self.white_player.start_turn()
        else:
            self.turn = 1
            self.white_player.end_turn()
            self.black_player.start_turn()

        start = time.perf_counter()
        self.board.execute_string(move)
        self.board.moves.append(move)
        self.board.evaluate_game(self.turn)

        if len(self.board.possible_moves) == 0:
            # Game is over
            self.game_over = True

    def is_draw(self):
        # If game is over, checks if it is a draw.
        # Assumes game is indeed over
        loser = self.turn
        if loser == WHITE:
            if not self.board.white_check:
                # White has turn and has no moves, but is not in check.
                # Therefore lost
                return True
        else:
            if not self.board.black_check:
                return True
        return False

    def get_player_with_turn(self):
        if self.turn == 1:
            return self.black_player
        return self.white_player

    def waiting_for_player(self) -> bool:
        return not self.white_player or not self.black_player
