import re

WHITE = 0
BLACK = 1

def game_to_string(game, room_tag):
    """ Used to send to client the available games """
    play_as = BLACK
    available_player = game[WHITE]
    if available_player is None:
        available_player = game[BLACK]
        play_as = WHITE

    game_time = game.game_time
    game_time = {0: 'Normal', 180: 'Blitz', 300: 'Fast'
                , 600: 'Classic'}[game_time]

    host_name = available_player.name

    return "{0};{1};{2};{3}".format(host_name, room_tag, game_time, play_as)


def saved_games_to_string(gameslist, username):
    """ Gets a list of saved games, and the username that requested them,
        encodes the games to a string so it can be sent to the user """
    description = ""
    for game in gameslist:
        if username == game.white_name:
            played_as = WHITE
            opponent_name = game.black_name
        else:
            played_as = BLACK
            opponent_name = game.white_name
        result = "LOSS"
        if username == game.winner_name:
            result = "WIN"
        if game.winner_name == "":
            result = "DRAW"
        des = "{0};{1};{2};{3}".format(game.date, opponent_name,
                            result, played_as)
        description += des + "."
    return description[:-1:]


def validate_credentials(username, password):
    """ Checks that username and password are valid """
    if ' ' in password or ' ' in username:
        return False
    if len(username) > 10 or len(password) > 15:
        return False
    if not re.search("^[a-zA-Z0-9]+$", username):
        return False
    return True
