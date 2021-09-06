import threading
import random
import time
from Gamelogic import *
from Utilities import *
from Encryption import *
from DataBaseHandler import *


connections = {}  # Key: user name, Value: Connection object
games = {}  # Key: Game tag, Value: Game object

WHITE = 0
BLACK = 1


class User:
    def __init__(self, username):
        """ Info pulled from db to be stored in RAM """
        self.username = username
        self.in_game = False
        self.game_room = ''


class Connection:
    BUFFER_SIZE = 4096
    MESSAGE_SEPERATOR = '\n\n'

    def __init__(self, socket):
        # Will be also able to handle encryption and decryption
        self.socket = socket
        self.aes_key = None
        self.buffer = ''
        self.messages = []

    def establish_secret(self):
        # Expect rsa public key
        rsa_key = self.socket.recv(1024)
        rsa_key = xml_to_rsa_key(rsa_key.decode())
        # Create aes key
        key = create_aes_key()
        self.aes_key = key
        self.socket.send(rsa_encrypt_data(key, rsa_key))

    def send(self, msg):
        msg = encrypt_to_send(msg, self.aes_key)
        try:
            self.socket.send(msg.encode())
        except:
            print("Error on sending - IGNORING")

    def recv(self):
        while True:
            # Stay in function untill returning some message
            if len(self.messages) > 0:
                return self.messages.pop(0)
            msg = self.socket.recv(Connection.BUFFER_SIZE).decode()
            # Split messages
            if not msg:
                self.messages.append(None)
            else:
                self.buffer += msg
            seperated = self.buffer.split(Connection.MESSAGE_SEPERATOR)
            self.buffer = seperated.pop()
            for i in range(len(seperated)):
                seperated[i] = decrypt_to_store(seperated[i], self.aes_key)
            self.messages += seperated

    def close(self):
        self.socket.close()


class ThreadedClient(threading.Thread):
    SEPERATOR = '\n'

    def __init__(self, client, lock):
        """ Handles session with client. Queries db for user, and saves it
            in ram for fast access """
        threading.Thread.__init__(self)
        self.lock = lock
        self.user = None
        self.client = Connection(client)

    def run(self):
        try:
            self.client.establish_secret()
        except:
            print("Encryption Failed")
            self.client.close()
            return

        while True:
            try:
                msg = self.client.recv()
                if len(msg) == 0:
                    self.disconnected()
                    print("[Disconnection]")
                    break
            except:
                # Client disconnected
                self.disconnected()
                print("[Disconnection]")
                break

            arguments = msg.split(ThreadedClient.SEPERATOR)
            msg_type = arguments[0]
            print(arguments)
            arguments.pop(0)
            # If a request is invalid - Send "ERROR"
            handled = False

            if self.user is None:
                # User is not logged in
                if msg_type == "LOGIN":
                    handled = self.login(arguments)
                elif msg_type == "REGISTER":
                    handled = self.register(arguments)
            elif not self.user.in_game:
                if msg_type == "HOST":
                    handled = self.host_game(arguments)
                elif msg_type == "JOIN":
                    handled = self.join_game(arguments)
                elif msg_type == "AVAILABLEGAMES":
                    handled = self.send_all_games()
                elif msg_type == "GETPASTGAMES":
                    handled = self.get_past_games()
                elif msg_type == "LOGOUT":
                    handled = self.logout()
            else:
                if msg_type == "MOVED":
                    handled = self.make_move(arguments)
                elif msg_type == "RESIGN":
                    handled = self.resign()
                elif msg_type == "EXIT":
                    handled = self.exited_game()

            if handled is False:
                print("ERROR HANDLING {}".format(arguments))
                self.client.send("ERROR")

    def host_game(self, args):
        """ A user sent "HOST" Command, Create a new game """
        if not (len(args) == 2 and args[0].isdigit() and args[1].isdigit()):
            return False
        if not (int(args[1]) == 0 or int(args[1]) == 1):
            return False
        if not (int(args[0]) < 4 and int(args[0]) >= 0):
            return False

        time, color = args
        time, color = int(time), int(color)

        global games

        with self.lock:
            room_tag = None
            while not room_tag or room_tag in games:
                room_tag = str(random.randint(0, 10000))

            self.client.send("HOSTED " + str(room_tag) + " " + str(color))

            # Create game and player instances
            time = [0, 180, 300, 600][time]
            game = Game(time)
            game.add_player(Player(self.user.username, color, time))

            # update user's data that he is now in game
            games[room_tag] = game
            self.user.in_game = True
            self.user.game_room = room_tag

    def join_game(self, args):
        """ A user sent "JOIN" Command, find the game he requested """
        if not len(args) == 1:
            return False

        room = args[0]

        global games
        with self.lock:
            if room in games and games[room].waiting_for_player():
                self.user.in_game = True
                self.user.game_room = room

                if not games[room][BLACK]:
                    my_color = BLACK
                else:
                    my_color = WHITE

                # Because WHITE and BLACK are 0 and 1 we can flip them with some bool casting
                op_name = games[room][int(not bool(my_color))].name
                player = Player(self.user.username, my_color, games[room].game_time)

                # Alert client that he succesfully joined, and host that game has started
                self.client.send("JOINED {} {} {}".format(my_color, op_name, games[room].game_time))
                connections[op_name].send("STARTED {} {}".format(self.user.username, games[room].game_time))
                games[room].add_player(player)

                games[room][WHITE].start_turn()

    def make_move(self, args):
        """ A user sent "MOVED", send move to opponent """
        if not len(args) == 1:
            return False
        move = args[0]

        global games
        global connections

        with self.lock:
            game = games[self.user.game_room]
            if game[WHITE].name == self.user.username:  # the user is white
                opponent = game[BLACK]
                player = game[WHITE]
            else:
                opponent = game[WHITE]
                player = game[BLACK]

            if opponent and opponent.color != game.turn and not game.game_over:
                # The player has the turn. First check if move is legal
                if move in game.board.possible_moves:
                    game.toggle_turn(move)
                    my_time = round(opponent.time_left, 3)
                    opponent_time = round(player.time_left, 3)
                    connections[opponent.name].send("MOVED " + move + " " + str(my_time) + " " + str(opponent_time))
                    # Check if someone lost
                    if game.game_over:
                        if game.is_draw():
                            connections[opponent.name].send("DRAW")
                            self.client.send("DRAW")
                            del games[self.user.game_room]
                            check_if_to_save_game(game, "")  # Empty winner name because no one won
                        else:
                            loser = game.turn
                            connections[game[loser].name].send("LOST")
                            connections[game[not bool(loser)].name].send("WON")
                            del games[self.user.game_room]
                            check_if_to_save_game(game, game[not bool(loser)].name)

    def resign(self):
        global games
        with self.lock:
            if self.user.in_game and self.user.game_room in games:
                # game is not over
                game = games[self.user.game_room]
                if not game.waiting_for_player():
                    # Game is still going on, can resign
                    self.client.send("LOST")
                    # Find opponent
                    opponent = game[WHITE]
                    if game[WHITE].name == self.user.username:
                        opponent = game[BLACK]
                    connections[opponent.name].send("WON")
                    check_if_to_save_game(game, opponent.name)
                    del games[self.user.game_room]

    def exited_game(self):
        global games
        global connections

        with self.lock:
            if self.user.game_room in games:
                # Game is still going on, user resigned
                game = games[self.user.game_room]
                if not game.waiting_for_player():
                    if game[WHITE].name == self.user.username:  # the user is white
                        op = game[BLACK]
                    else:
                        op = game[WHITE]
                    connections[op.name].send("WON")
                    check_if_to_save_game(game, op.name)
                del games[self.user.game_room]
            self.user.in_game = False


    # LOGGIN IN AND OUT FUNCTIONS :::::::::::::

    def login(self, args):
        global connections
        if not len(args) == 2:
            return False
        username, password = args
        result = login_user(username, password)
        if result:
            self.user = User(username)
            self.client.send("LOGINACCEPTED")
            with self.lock:
                connections[self.user.username] = self.client
        else:
            self.client.send("LOGINDENIED")

    def register(self, args):
        if not len(args) == 2:
            return False
        username, password = args
        result = register_user(username, password)
        if result:
            self.client.send("REGISTERACCEPTED")
        else:
            self.client.send("REGISTERDENIED")

    def logout(self):
        global connections
        username = self.user.username
        with self.lock:
            del connections[self.user.username]
            self.user = None
        logout_user(username)

    def disconnected(self):
        global games
        global connections

        with self.lock:
            if self.user:
                # User was logged in
                if self.user.in_game and self.user.game_room in games:
                    # Game lobby still exists, either waiting for player or in game
                    if not games[self.user.game_room].waiting_for_player():
                        # Game in going on, second player wins automaticly
                        game = games[self.user.game_room]
                        opponent = game.white_player
                        if opponent.name == self.user.username:
                            opponent = game.black_player
                        connections[opponent.name].send("WON")
                        check_if_to_save_game(game, opponent.name)

                    del games[self.user.game_room]
                del connections[self.user.username]
                logout_user(self.user.username)
                self.user = None

        self.client.close()


    def get_past_games(self):
        games, wins = get_games_of_player(self.user.username)
        self.client.send("PASTGAMES " + games + " " + str(wins))

    def send_all_games(self):
        global games
        """ Sends to user all available games """
        with self.lock:
            descriptions = []
            for room in games:
                if games[room].waiting_for_player():
                    descriptions.append(game_to_string(games[room], room))
            to_send = '.'.join(descriptions)
            self.client.send("AVAILABLEGAMES " + to_send)


class SuperVisor(threading.Thread):
    RUN_EVERY = 1  # 1 second

    def __init__(self, lock):
        threading.Thread.__init__(self)
        self.lock = lock
        self.last_time = 0

    def run(self):
        """ Loop each second over all games to check if any of them
            Have no time left and therefore need to end """

        global games

        while True:
            if time.time() - self.last_time > SuperVisor.RUN_EVERY:
                self.last_time = time.time()
                # Logic of scanning
                self.lock.acquire()
                for game_tag, game in list(games.items()):
                    # Check time
                    if not game.waiting_for_player() and game.game_time != 0:
                        # Game time is not unlimited
                        turn_player = game.get_player_with_turn()
                        if turn_player.time_left < (time.time() - turn_player.started_on):
                            self.end_game(game_tag, turn_player.color)
                            print("[SUPERVISOR] Time for game {} ended".format(game_tag))
                self.lock.release()
            else:
                time.sleep(SuperVisor.RUN_EVERY - (time.time() - self.last_time))

    def end_game(self, game_room, player_lost_color):
        """ Time for one of the players passed, destroy game
        and say who lost """
        global games
        global connections
        gm = games[game_room]
        connections[gm[player_lost_color].name].send("LOST")
        connections[gm[int(not bool(player_lost_color))].name].send("WON")

        check_if_to_save_game(gm, gm[int(not bool(player_lost_color))].name)

        del games[game_room]


def check_if_to_save_game(game, winner):
    # A very complex algorithm that checks if need to save
    # The game to the "database"
    if game.game_over:
        # Game ended by checkmate or stalemate
        save_game(game, winner)
    else:
        if len(game.board.moves) > 5:
            # Game ended either because time ended
            # Or Because one disconnected/resigned
            save_game(game, winner)
