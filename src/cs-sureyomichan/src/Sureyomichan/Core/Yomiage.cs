using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan.Core;

class Yomiage(BouyomiChan bouyomi, IConfigProxy config) {
	private bool stateStart = false;
	private bool stateOld = false;
	private bool stateDie = false;

	public void SpeakStarted() {
		if (!stateStart) {
			this.DoYomiage(config.Get().YomiageStarted);
		}
		stateStart = true;
	}

	public void SpeakOld() {
		if (!stateOld) {
			this.DoYomiage(config.Get().YomiageOld);
		}
		stateOld = true;
	}

	public void SpeakDead() {
		if (!stateDie) {
			this.DoYomiage(config.Get().YomiageDie);
		}
		stateDie = true;
	}

	public void SaveImage() {
		this.DoYomiage(config.Get().YomiageSaveTegaki);
	}

	public void DoYomiage(Models.YomiageConfig c) {
		static void file(Models.YomiageConfig c) {
			var file = Path.IsPathFullyQualified(c.File) switch {
				true => c.File,
				_ => Path.Combine(AppContext.BaseDirectory, "assets", "sound", c.File)
			};
			Task.Run(() => {
				var reader = new AudioFileReader(file);

				WaveOut waveOut = new WaveOut();
				waveOut.Init(reader);
				waveOut.Play();

				while(waveOut.PlaybackState == PlaybackState.Playing) ;
			});
		}

		switch(c.Method) {
		case Models.YomiageConfig.YomiageMethodOff:
			break;
		case Models.YomiageConfig.YomiageMethodFile:
			file(c);
			break;
		case Models.YomiageConfig.YomiageMethodText:
			this.EnqueueSpeak(c.Text);
			break;
		}
	}

	public void EnqueueSpeak(string text) {
		bouyomi.EnqueueSpeak(text);
	}

	public void EnqueueSpeak(Models.SureyomiChanModel reply) {
		bouyomi.EnqueueSpeak(this.ParseResBody(reply.Body));
	}

	public void EnqueueSpeak(IEnumerable<Models.SureyomiChanModel> replies) {
		bouyomi.EnqueueSpeak(replies.Select(x => this.ParseResBody(x.Body)).ToArray());
	}

	private string ParseResBody(string body) {
		var speakLines = body.Split("<br>")
			.Select(line => {
				var t1 = Regex.Replace(line, @"<(\""[^\""]*\""|'[^']*'|[^'\"">])*>", "");
				var t2 = Regex.Replace(t1, @"&([^;]+);", m => {
					return m.Groups[1].Value.ToLower() switch {
						"gt" => ">",
						"lt" => "<",
						"amp" => "&",
						"quot" => "\"",
						string v when v[0] == '#' => this.DecodeFromCodePoint(v),
						_ => "",
					};
				});

				string tegakiStorage_resTag = "___";
				return t2.FirstOrDefault() switch {
					'>' => $"{tegakiStorage_resTag}{t2}",
					_ => t2
				};
			});
		return string.Join("\n", speakLines);
	}

	private string DecodeFromCodePoint(string p1) {
		if (p1[0] == '#') {
			var span = p1.AsSpan().Slice(1);
			if (span[0] switch {
				'x' => true,
				'X' => true,
				_ => false,
			}) {
				span = span.Slice(1);
				try {
					return char.ConvertFromUtf32(
						BitConverter.ToInt32(
							Convert.FromHexString(span)));
				}
				catch (FormatException) { }
			} else {
				try {
					return char.ConvertFromUtf32(
						int.Parse(span));
				}
				catch (FormatException) { }
			}
		}
		return "";
	}
}

