# AES
from Crypto.Cipher import AES
from Crypto import Random
# RSA
from Crypto.PublicKey import RSA
from Crypto.PublicKey.RSA import construct
from Crypto.Cipher import PKCS1_OAEP

from base64 import b64decode, b64encode

BLOCK_SIZE = 16
IV_SIZE = 24
PAD_WITH = "\x00"
ENCRYPTION_MODE = AES.MODE_CBC
AES_KEY_SIZE = 32
MESSAGE_SEPERATOR = '\n\n'


""" Some utilities """


def pad_str(data):
    to_add = BLOCK_SIZE - (len(data) % BLOCK_SIZE)
    return data + (to_add * PAD_WITH)


def un_pad(data):
    return data.replace('\x00', "")


""" AES Encryption """


def create_aes_key():
    key = Random.get_random_bytes(AES_KEY_SIZE)
    return key


def encrypt_to_send(data, key):
    """ Takes an aes key, and data. Encrypts the data and prepends to it
        the iv so it can be transported. The data is a string, the method also
        returns string where the ciphertext and iv are encoded with base64 """

    cipher = AES.new(key, ENCRYPTION_MODE)
    data = pad_str(data)
    data_bytes = cipher.encrypt(data.encode())

    # Encode both to a string
    iv = b64encode(cipher.iv).decode()
    ct = b64encode(data_bytes).decode()

    # Format of result:
    # {24 bytes iv} {ciphertext}
    msg = (iv + ct + MESSAGE_SEPERATOR)
    return msg


def decrypt_to_store(data, key):
    """ Takes an aes key, and data. Decrypts the data and removes padding.
    The data is a string with ciphertext and iv encoded in base64 """
    data = un_pad(data)

    iv = b64decode(data[:IV_SIZE:].encode())
    ciphertext = b64decode(data[IV_SIZE::].encode())

    cipher = AES.new(key, ENCRYPTION_MODE, iv=iv)
    data = un_pad(cipher.decrypt(ciphertext).decode())
    return data


# Takes public key, and string or bytes. Returns bytes
def rsa_encrypt_data(data, key):
    if isinstance(data, str):
        data = data.encode()
    encryptor = PKCS1_OAEP.new(key)
    encrypted = encryptor.encrypt(data)
    return encrypted


# Takes RSA Public key in form of xml, and turns it to a cipher object
def xml_to_rsa_key(xml):
    xml = xml.replace('<RSAKeyValue>', '')
    xml = xml.replace('</RSAKeyValue>', '')
    # </Modulus><Exponent> Is in the middle
    mod, expo = xml.split('</Modulus><Exponent>')
    mod = mod.replace('<Modulus>', '')
    expo = expo.replace('</Exponent>', '')
    m = int.from_bytes(b64decode(mod), byteorder='big')
    e = int.from_bytes(b64decode(expo), byteorder='big')
    return construct((m, e))
