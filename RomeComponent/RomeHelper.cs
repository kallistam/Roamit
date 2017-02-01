﻿using QuickShare.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;
using Windows.UI.Core;

namespace QuickShare.Rome
{
    public class RomeHelper : IDisposable
    {
        private RemoteSystemWatcher _remoteSystemWatcher;

        private ObservableCollection<RemoteSystem> _remoteSystems = new ObservableCollection<RemoteSystem>();

        public ObservableCollection<RemoteSystem> RemoteSystems
        {
            get { return _remoteSystems; }
        }

        public async Task Initialize()
        {
            if (_remoteSystemWatcher != null)
                return;

            RemoteSystemAccessStatus accessStatus = await RemoteSystem.RequestAccessAsync();
            if (accessStatus == RemoteSystemAccessStatus.Allowed)
            {
                _remoteSystemWatcher = RemoteSystem.CreateWatcher();
                _remoteSystemWatcher.RemoteSystemAdded += RemoteSystemWatcher_RemoteSystemAdded;
                _remoteSystemWatcher.RemoteSystemRemoved += RemoteSystemWatcher_RemoteSystemRemoved;
                _remoteSystemWatcher.RemoteSystemUpdated += RemoteSystemWatcher_RemoteSystemUpdated;
                _remoteSystemWatcher.Start();
            }
        }

        private async void RemoteSystemWatcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
        {
            var remoteSystem = args.RemoteSystem;
            await DispatcherEx.RunOnCoreDispatcherIfPossible(() =>
            {
                AddToRemoteSystemsList(args.RemoteSystem);
            });
        }

        private async void RemoteSystemWatcher_RemoteSystemRemoved(RemoteSystemWatcher sender, RemoteSystemRemovedEventArgs args)
        {
            await DispatcherEx.RunOnCoreDispatcherIfPossible(() =>
            {
                var remoteSystem = _remoteSystems.Where(s => s.Id == args.RemoteSystemId).FirstOrDefault();
                if (remoteSystem != null)
                {
                    _remoteSystems.Remove(remoteSystem);
                }
            });
        }

        private async void RemoteSystemWatcher_RemoteSystemUpdated(RemoteSystemWatcher sender, RemoteSystemUpdatedEventArgs args)
        {
            RemoteSystem remoteSystem = null;

            await DispatcherEx.RunOnCoreDispatcherIfPossible(() =>
            {
                remoteSystem = _remoteSystems.Where(s => s.Id == args.RemoteSystem.Id).FirstOrDefault();
            });

            await DispatcherEx.RunOnCoreDispatcherIfPossible(() =>
            {
                if (remoteSystem != null)
                    _remoteSystems.Remove(remoteSystem);
                AddToRemoteSystemsList(args.RemoteSystem);
            });
        }

        public void Dispose()
        {
            if (_remoteSystemWatcher != null)
            {
                _remoteSystemWatcher.RemoteSystemAdded -= RemoteSystemWatcher_RemoteSystemAdded;
                _remoteSystemWatcher.RemoteSystemRemoved -= RemoteSystemWatcher_RemoteSystemRemoved;
                _remoteSystemWatcher.RemoteSystemUpdated -= RemoteSystemWatcher_RemoteSystemUpdated;
                _remoteSystemWatcher.Stop();
                _remoteSystemWatcher = null;
            }
        }

        public void AddToRemoteSystemsList(RemoteSystem r)
        {
            int min = 0, max = _remoteSystems.Count;

            if (r.IsAvailableByProximity)
            {
                while (((max - 1) >= 0) && (!_remoteSystems[max - 1].IsAvailableByProximity))
                {
                    max--;
                }
            }
            else
            {
                while ((min < _remoteSystems.Count) && (_remoteSystems[min].IsAvailableByProximity))
                {
                    min++;
                }
            }

            int i;
            for (i = min; i < max; i++)
            {
                if (string.Compare(_remoteSystems[i].DisplayName, r.DisplayName) >= 0)
                    break;
            }
            _remoteSystems.Insert(i, r);
        }
    }
}
