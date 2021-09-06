import socket
from ClientThread import ThreadedClient, SuperVisor, Connection
import threading
from DataBaseHandler import logout_all_users


class Server:
    def __init__(self, port):
        logout_all_users()

        games_lock = threading.Lock()

        SuperVisor(games_lock).start()

        self.server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server.bind(('0.0.0.0', port))
        self.server.listen(10)

        print("Server is Up and Running")

        # Accept clients forever
        while True:
            client, addr = self.server.accept()
            print("[SERVER] {0} connected".format(addr))
            ThreadedClient(client, games_lock).start()


if __name__ == "__main__":
    Server(5000)
