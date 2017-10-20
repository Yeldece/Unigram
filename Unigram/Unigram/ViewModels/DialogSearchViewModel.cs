﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Api.TL.Messages;
using Unigram.Common;
using Unigram.Core.Common;

namespace Unigram.ViewModels
{
    public class DialogSearchViewModel : UnigramViewModelBase
    {
        private readonly DialogViewModel _dialog;

        public DialogSearchViewModel(IMTProtoService protoService, ICacheService cacheService, ITelegramEventAggregator aggregator, DialogViewModel viewModel)
            : base(protoService, cacheService, aggregator)
        {
            _dialog = viewModel;
        }

        public DialogViewModel Dialog
        {
            get
            {
                return _dialog;
            }
        }

        private bool _isFiltering;
        public bool IsFiltering
        {
            get
            {
                return _isFiltering;
            }
            set
            {
                Set(ref _isFiltering, value);
            }
        }

        private ICollection _autocomplete;
        public ICollection Autocomplete
        {
            get
            {
                return _autocomplete;
            }
            set
            {
                Set(ref _autocomplete, value);
            }
        }

        #region Filters

        private string _query;
        public string Query
        {
            get
            {
                return _query;
            }
            set
            {
                Set(ref _query, value);
            }
        }

        private DateTimeOffset? _date;
        public DateTimeOffset? Date
        {
            get
            {
                return _date;
            }
            set
            {
                Set(ref _date, value);
            }
        }

        private TLUser _from;
        public TLUser From
        {
            get
            {
                return _from;
            }
            set
            {
                Set(ref _from, value);
            }
        }

        public bool IsFromEnabled
        {
            get
            {
                if (Dialog.With is TLChat || (Dialog.With is TLChannel channel && channel.IsMegaGroup))
                {
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region Results

        public int SelectedIndex
        {
            get
            {
                if (Items == null || SelectedItem == null)
                {
                    return 0;
                }

                var index = Items.IndexOf(SelectedItem);
                //if (index == Items.Count - 1)
                //{
                //    LoadNext();
                //}
                //if (index == 0)
                //{
                //    LoadPrevious();
                //}

                return index + 1;
            }
        }

        protected int _totalItems;
        public int TotalItems
        {
            get
            {
                return _totalItems;
            }
            set
            {
                Set(ref _totalItems, value);
            }
        }

        protected TLMessageBase _selectedItem;
        public TLMessageBase SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                Set(ref _selectedItem, value);
                NextCommand.RaiseCanExecuteChanged();
                PreviousCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(() => SelectedIndex);
            }
        }

        public MvxObservableCollection<TLMessageBase> Items { get; protected set; }

        #endregion

        public RelayCommand FilterCommand { get; } = new RelayCommand(FilterExecute);
        private void FilterExecute()
        {
            IsFiltering = true;
        }

        public RelayCommand<string> SearchCommand { get; } = new RelayCommand<string>(SearchExecute);
        private async void SearchExecute(string query)
        {
            Query = query;

            var response = await ProtoService.SearchAsync(_dialog.Peer, _query, _from?.ToInputUser(), null, 0, 0, 0, 0, 100);
            if (response.IsSucceeded)
            {
                if (response.Result is TLMessagesMessagesSlice slice)
                {
                    TotalItems = slice.Count;
                }
                else if (response.Result is TLMessagesChannelMessages channelMessages)
                {
                    TotalItems = channelMessages.Count;
                }
                else
                {
                    TotalItems = response.Result.Messages.Count;
                }

                Items = new MvxObservableCollection<TLMessageBase>(response.Result.Messages);
                SelectedItem = Items.FirstOrDefault();

                if (_selectedItem != null)
                {
                    await Dialog.LoadMessageSliceAsync(null, _selectedItem.Id);
                }
            }
            else
            {
                // TODO
            }
        }

        public RelayCommand NextCommand { get; } = new RelayCommand(NextExecute, NextCanExecute);
        private async void NextExecute()
        {
            if (Items == null || SelectedIndex <= 1)
            {
                return;
            }

            SelectedItem = Items[SelectedIndex - 2];

            if (_selectedItem != null)
            {
                await Dialog.LoadMessageSliceAsync(null, _selectedItem.Id);
            }
        }

        private bool NextCanExecute()
        {
            if (Items == null)
            {
                return false;
            }

            return SelectedIndex > 1;
        }

        public RelayCommand PreviousCommand { get; } = new RelayCommand(PreviousExecute, PreviousCanExecute);
        private async void PreviousExecute()
        {
            if (Items == null || SelectedIndex >= TotalItems)
            {
                return;
            }

            SelectedItem = Items[SelectedIndex];

            if (_selectedItem != null)
            {
                await Dialog.LoadMessageSliceAsync(null, _selectedItem.Id);
            }
        }

        private bool PreviousCanExecute()
        {
            if (Items == null)
            {
                return false;
            }

            return SelectedIndex < TotalItems;
        }
    }
}
