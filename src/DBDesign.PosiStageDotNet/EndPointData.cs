using DBDesign.PosiStageDotNet;
using DBDesign.PosiStageDotNet.Chunks;
using JetBrains.Annotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SCPosiStageDotNet
{
    public class EndPointData
    {
        /// <summary>
        /// The list of trackers
        /// </summary>
        public ReadOnlyDictionary<int, PsnTracker> Trackers { get; private set; }

        /// <summary>
        ///     System name of the remote PosiStageNet server, or null if no info packets have been received
        /// </summary>
        [CanBeNull]
        public string RemoteSystemName { get; private set; }

        /// <summary>
        /// The actual IPEndPoint
        /// </summary>
        public IPEndPoint IPEndpoint { get; private set; }

        private readonly ConcurrentDictionary<int, PsnTracker> _trackers = new ConcurrentDictionary<int, PsnTracker>();

        public EndPointData(IPEndPoint ipEndPoint)
        {
            Trackers = new ReadOnlyDictionary<int, PsnTracker>(_trackers);
            IPEndpoint = ipEndPoint;
        }

        public void updateTracker(PsnInfoTrackerChunk chunk, PsnInfoTrackerNameChunk trackerNameChunk, PsnInfoHeaderChunk header)
        {
            if (!_trackers.TryGetValue(chunk.TrackerId, out var tracker))
            {
                tracker = new PsnTracker(chunk.TrackerId, trackerNameChunk.TrackerName, null, header.TimeStamp);
                _trackers.TryAdd(chunk.TrackerId, tracker);
            }
            else
            {
                tracker = tracker.WithTrackerName(trackerNameChunk.TrackerName);
                tracker = tracker.WithInfoTimeStamp(header.TimeStamp);
            }

            _trackers[chunk.TrackerId] = tracker;
        }

        public void updateTracker(PsnTracker tracker)
        {
            _trackers[tracker.TrackerId] = tracker;
        }

        public void UpdateRemoteSystemName(String remoteSystemName)
        {
            RemoteSystemName = remoteSystemName;
        }
    }

    public class TrackerUpdateEventArgs
    {
        public int TrackerId;
        public EndPointData Data;
    }
}
