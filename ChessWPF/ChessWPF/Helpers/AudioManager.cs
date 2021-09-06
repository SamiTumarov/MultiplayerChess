using System;
using System.Windows.Media;

namespace ChessWPF
{
    static class AudioManager
    {
        private static readonly MediaPlayer CaptureSound;
        private static readonly MediaPlayer MoveSound;
        private static readonly MediaPlayer GenericNotify;

        static AudioManager()
        {
            CaptureSound = new MediaPlayer();
            CaptureSound.Open(new Uri("Sounds/Capture.mp3", UriKind.Relative));
            CaptureSound.Stop();

            MoveSound = new MediaPlayer();
            MoveSound.Open(new Uri("Sounds/Move.mp3", UriKind.Relative));
            MoveSound.Stop();

            GenericNotify = new MediaPlayer();
            GenericNotify.Open(new Uri("Sounds/GenericNotify.mp3", UriKind.Relative));
            GenericNotify.Stop();
        }

        public static void Init()
        {
            // First call to audio. CLR Invokes the Constructor
        }

        static void PlayCaptureSound()
        {
            CaptureSound.Position = TimeSpan.Zero;
            CaptureSound.Play();
        }

        static void PlayMovingSound()
        {
            MoveSound.Position = TimeSpan.Zero;
            MoveSound.Play();
        }

        public static void PlayGenericSound()
        {
            GenericNotify.Position = TimeSpan.Zero;
            GenericNotify.Play();
        }


        public static void PlaySoundForMove(string move)
        {
            if (move.Length == 5 || (move.Length == 11 && move[5] == ','))
                PlayMovingSound();
            else
                PlayCaptureSound();
        }
    }
}
