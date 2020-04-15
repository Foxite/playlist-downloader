using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace DownloadPlaylist {
	public class Program {
		private static readonly string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", Regex.Escape(new string(Path.GetInvalidFileNameChars())));

		public static YoutubeClient Client { get; set; }

		private static void Main(string[] args) {
			Client = new YoutubeClient();
			DownloadPlaylist(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Playlist"), args[0]).GetAwaiter().GetResult();
			Console.WriteLine("Done");
			Console.ReadKey();
		}

		private static async Task DownloadPlaylist(string folder, string id) {
			//Playlist playlist = await Client.Playlists.GetAsync(id);
			await foreach (Video video in Client.Playlists.GetVideosAsync(new PlaylistId(id))) {
				Console.WriteLine(video.Title);
				StreamManifest streamManifest = await Client.Videos.Streams.GetManifestAsync(video.Id);
				IStreamInfo streamInfo = streamManifest.GetAudioOnly().Where(stream => stream.Container == Container.Mp4).WithHighestBitrate();
				await Client.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(folder, SanitizeFilename(video.Title) + ".mp3"));
			}
		}

		// https://stackoverflow.com/a/847251
		private static string SanitizeFilename(string input) {
			return Regex.Replace(input, invalidRegStr, "");
		}
	}
}
