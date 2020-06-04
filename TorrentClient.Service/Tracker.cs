using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using MiscUtil.Conversion;

namespace TorrentClient.Service
{
    public enum TrackerEvent
    {
        Started,
        Paused,
        Stopped
    }

    public class Tracker
    {
        public event EventHandler<List<IPEndPoint>> PeerListUpdated;

        public string Address { get; private set; }

        public DateTime LastPeerRequest { get; private set; } = DateTime.MinValue;
        public TimeSpan PeerRequestInterval { get; private set; } = TimeSpan.FromMinutes(30);

        private HttpWebRequest httpWebRequest;

        public Tracker(string address)
        {
            Address = address;
        }

        #region Announcing

        public void Update(Torrent torrent, TrackerEvent ev, string id, int port)
        {
            // wait until after request interval has elapsed before asking for new peers
            if (ev == TrackerEvent.Started && DateTime.UtcNow < LastPeerRequest.Add(PeerRequestInterval))
                return;

            LastPeerRequest = DateTime.UtcNow;

            string url = $"{Address}?info_hash={torrent.UrlSafeStringInfohash}&peer_id={id}&port={port}&uploaded={torrent.Uploaded}&downloaded={torrent.Downloaded}&left={torrent.Left}&event={Enum.GetName(typeof(TrackerEvent), ev).ToLower()}&compact=1";

            Request(url);
        }

        private void Request(string url)
        {
            httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.BeginGetResponse(HandleResponse, null);
        }

        private void HandleResponse(IAsyncResult result)
        {
            byte[] data;

            using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.EndGetResponse(result))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Log.WriteLine($"Error reaching tracker {this}: {response.StatusCode} {response.StatusDescription}");
                    return;
                }

                using Stream stream = response.GetResponseStream();
                data = new byte[response.ContentLength];
                stream.Read(data, 0, Convert.ToInt32(response.ContentLength));
            }

            if (!(BEncode.Decode(data) is Dictionary<string, object> info))
            {
                Log.WriteLine("Unable to decode tracker announce response");
                return;
            }

            PeerRequestInterval = TimeSpan.FromSeconds((long)info["interval"]);
            byte[] peerInfo = (byte[])info["peers"];

            List<IPEndPoint> peers = new List<IPEndPoint>();
            for (int i = 0; i < peerInfo.Length / 6; i++)
            {
                int offset = i * 6;
                string address = $"{peerInfo[offset]}.{peerInfo[offset + 1]}.{peerInfo[offset + 2]}.{peerInfo[offset + 3]}";
                int port = EndianBitConverter.Big.ToChar(peerInfo, offset + 4);

                peers.Add(new IPEndPoint(IPAddress.Parse(address), port));
            }

            PeerListUpdated?.Invoke(this, peers);
        }

        public void ResetLastRequest()
        {
            LastPeerRequest = DateTime.MinValue;
        }

        #endregion

        #region Helper

        public override string ToString()
        {
            return $"[Tracker: {Address}]";
        }

        #endregion
    }
}