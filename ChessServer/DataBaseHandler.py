import random
import hashlib
import Utilities
from datetime import datetime
import pickle
import os
from collections import namedtuple

USERS_FOLDER = 'Users'
GAMES_FOLDER = 'Games'
SALT_SIZE = 5
ABC = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOQRSTUVWXYZ'
LIMIT_GAMES_TO = 5

UserTuple = namedtuple('UserTuple',
            ['username', 'hashed_pw', 'hash_salt', 'logged_in', 'games_list'])

GameTuple = namedtuple('GameTuple',
            ['date', 'white_name', 'black_name', 'winner_name'])


def register_user(username, password):
    if not Utilities.validate_credentials(username, password):
        return False

    filename = username + ".user"
    filepath = os.path.join(USERS_FOLDER, filename)
    # Check if user exists first
    if os.path.exists(filepath):
        return False

    # Generate salt and hashed password
    salt = ""
    for i in range(SALT_SIZE):
        salt += random.choice(ABC)
    hashed_pw = hashlib.sha256((password + salt).encode()).hexdigest()

    user = UserTuple(username, hashed_pw, salt, False, [])
    with open(filepath, 'wb') as file:
        pickle.dump(user, file)
        # Also create "games" file
        with open(os.path.join(GAMES_FOLDER, username + ".games"), 'wb') as file2:
            # Empty list because didn't play any games yet
            pickle.dump([], file2)
            return True


def login_user(username, password):
    if not Utilities.validate_credentials(username, password):
        return False

    filename = username + ".user"
    filepath = os.path.join(USERS_FOLDER, filename)

    # Check if user exists first
    if not os.path.exists(filepath):
        return False

    with open(filepath, 'rb') as file:
        user = pickle.load(file)
        # Check password
        hashed_pw = hashlib.sha256((password + user.hash_salt).encode()).hexdigest()
        if not user.username == username:
            return False
        if not hashed_pw == user.hashed_pw:
            return False
        if user.logged_in:
            return False

    with open(filepath, 'wb') as file:
        pickle.dump(user._replace(logged_in=True), file)
        return True


def logout_user(username):
    filename = username + ".user"
    filepath = os.path.join(USERS_FOLDER, filename)

    # Check if user exists first
    if not os.path.exists(filepath):
        return

    with open(filepath, 'rb') as file:
        user = pickle.load(file)

    with open(filepath, 'wb') as file:
        if user.logged_in:
            pickle.dump(user._replace(logged_in=False), file)


def logout_all_users():
    # In case server was shutdown in the middle
    # On boot - logout all users

    for user_file in os.listdir(USERS_FOLDER):
        with open(os.path.join(USERS_FOLDER, user_file), 'rb') as file:
            user = pickle.load(file)

        with open(os.path.join(USERS_FOLDER, user_file), 'wb') as file:
            pickle.dump(user._replace(logged_in=False), file)


def save_game(game_object, winner_name):
    whitename, blackname = game_object.white_player.name, game_object.black_player.name
    date = datetime.today().strftime('%Y-%m-%d')
    game = GameTuple(date, whitename, blackname, winner_name)

    filepath1 = os.path.join(GAMES_FOLDER, whitename + ".games")
    filepath2 = os.path.join(GAMES_FOLDER, blackname + ".games")

    with open(filepath1, 'rb') as file:
        games_list1 = pickle.load(file)

    with open(filepath2, 'rb') as file:
        games_list2 = pickle.load(file)

    # Add the new game
    games_list1.append(game)
    with open(filepath1, 'wb') as file:
        pickle.dump(games_list1, file)

    games_list2.append(game)
    with open(filepath2, 'wb') as file:
        pickle.dump(games_list2, file)


def get_games_of_player(username):
    # Find all games of player
    filepath = os.path.join(GAMES_FOLDER, username + ".games")
    with open(filepath, 'rb') as file:
        usergames = pickle.load(file)

    # Win count
    wins = 0
    for game in usergames:
        if game.winner_name == username:
            wins += 1

    # We want to see games starting from the last one
    usergames.reverse()
    if len(usergames) > LIMIT_GAMES_TO:
        # Limit sending games to a maximum amount
        usergames = usergames[0:LIMIT_GAMES_TO:]

    string_games = Utilities.saved_games_to_string(usergames, username)

    # Returns a string describing the games, and the win count
    return string_games, wins
