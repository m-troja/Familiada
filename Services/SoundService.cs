using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Familiada.Services;

public class SoundService
{
    private MediaPlayer _player = new MediaPlayer();

    public async Task PlaySequenceAsync(params string[] sounds)
    {
        foreach (var sound in sounds)
        {
            var tcs = new TaskCompletionSource<bool>();

            _player.Open(new Uri(sound, UriKind.Relative));
            _player.MediaEnded += (s, e) => tcs.TrySetResult(true);
            _player.Play();

            await tcs.Task;
        }
    }
}
