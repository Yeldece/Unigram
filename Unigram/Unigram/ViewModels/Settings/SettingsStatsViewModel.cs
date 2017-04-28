﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Template10.Mvvm;
using Unigram.Core.Common;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.Settings
{
    public class SettingsStatsViewModel : UnigramViewModelBase
    {
        private readonly IStatsService _statsService;

        public SettingsStatsViewModel(IMTProtoService protoService, ICacheService cacheService, ITelegramEventAggregator aggregator, IStatsService statsService) 
            : base(protoService, cacheService, aggregator)
        {
            _statsService = statsService;

            Items = new MvxObservableCollection<SettingsStatsNetwork>
            {
                new SettingsStatsNetwork(statsService, "Mobile", NetworkType.Mobile),
                new SettingsStatsNetwork(statsService, "Wi-Fi", NetworkType.WiFi),
                new SettingsStatsNetwork(statsService, "Roaming", NetworkType.Roaming)
            };
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            foreach (var item in Items)
            {
                item.ReloadData();
            }

            return Task.CompletedTask;
        }

        public MvxObservableCollection<SettingsStatsNetwork> Items { get; private set; }
    }

    public class SettingsStatsNetwork : BindableBase
    {
        private readonly IStatsService _statsService;

        public SettingsStatsNetwork(IStatsService statsService, string title, NetworkType type)
        {
            _statsService = statsService;

            Title = title;
            Type = type;

            Items = new MvxObservableCollection<SettingsStatsDataBase>
            {
                new SettingsStatsData(statsService, "PHOTOS", type, DataType.Photos),
                new SettingsStatsData(statsService, "VIDEOS", type, DataType.Videos),
                new SettingsStatsData(statsService, "VOICE/VIDEO MESSAGES", type, DataType.Audios),
                new SettingsStatsData(statsService, "FILES", type, DataType.Files),
                new SettingsStatsCallData(statsService, "CALLS", type, DataType.Calls),
                new SettingsStatsDataBase(statsService, "MESSAGES AND OTHER DATA", type, DataType.Messages),
                new SettingsStatsDataBase(statsService, "TOTAL", type, DataType.Total)
            };
        }

        public void ReloadData()
        {
            foreach (var item in Items)
            {
                item.ReloadData();
            }
        }

        public string Title { get; private set; }
        public NetworkType Type { get; private set; }

        public MvxObservableCollection<SettingsStatsDataBase> Items { get; private set; }
    }

    public class SettingsStatsCallData : SettingsStatsData
    {
        public SettingsStatsCallData(IStatsService statsService, string title, NetworkType networkType, DataType type)
            : base(statsService, title, networkType, type)
        {
        }

        public override void ReloadData()
        {
            base.ReloadData();
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                Set(ref _duration, value);
            }
        }
    }

    public class SettingsStatsData : SettingsStatsDataBase
    {
        public SettingsStatsData(IStatsService statsService, string title, NetworkType networkType, DataType type)
            : base(statsService, title, networkType, type)
        {
        }

        public override void ReloadData()
        {
            SentItems = _statsService.GetSentItemsCount(_networkType, Type);
            ReceivedItems = _statsService.GetReceivedItemsCount(_networkType, Type);
            base.ReloadData();
        }

        private int _sentItems;
        public int SentItems
        {
            get
            {
                return _sentItems;
            }
            set
            {
                Set(ref _sentItems, value);
            }
        }

        private int _receivedItems;
        public int ReceivedItems
        {
            get
            {
                return _receivedItems;
            }
            set
            {
                Set(ref _receivedItems, value);
            }
        }
    }

    public class SettingsStatsDataBase : BindableBase
    {
        protected readonly IStatsService _statsService;
        protected readonly NetworkType _networkType;

        public SettingsStatsDataBase(IStatsService statsService, string title, NetworkType networkType, DataType type)
        {
            _statsService = statsService;
            _networkType = networkType;

            Title = title;
            Type = type;
        }

        public virtual void ReloadData()
        {
            SentBytes = _statsService.GetSentBytesCount(_networkType, Type);
            ReceivedBytes = _statsService.GetReceivedBytesCount(_networkType, Type);
        }

        public string Title { get; private set; }
        public DataType Type { get; private set; }

        private long _sentBytes;
        public long SentBytes
        {
            get
            {
                return _sentBytes;
            }
            set
            {
                Set(ref _sentBytes, value);
            }
        }

        private long _receivedBytes;
        public long ReceivedBytes
        {
            get
            {
                return _receivedBytes;
            }
            set
            {
                Set(ref _receivedBytes, value);
            }
        }
    }
}