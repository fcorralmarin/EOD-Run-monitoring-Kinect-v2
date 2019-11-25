using Common.Utils.Core;
using Common.Utils.Core.Commands;
using Server.DB;
using Server.Models;
using Server.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Server.ViewModels
{
    class DataHistoryViewModel : IBase
    {
        public DataHistoryViewModel()
        {
            DataHistoryModel = new DataHistoryModel();
        }

        private SessionCollection sessionsCollection;
        public SessionCollection SessionsCollection
        {
            get { return sessionsCollection; }
            set
            {
                sessionsCollection = value;
                RaisePropertyChanged("SessionsCollection");
            }
        }

        private SessionCollection selectedSessions;
        public SessionCollection SelectedSessions
        {
            get { return selectedSessions; }
            set
            {
                selectedSessions = value;
                RaisePropertyChanged("SelectedSessions");
                RaisePropertyChanged("CanDelete");
                RaisePropertyChanged("CanPlay");
            }
        }

        private DataHistoryModel dataHistoryModel;
        public DataHistoryModel DataHistoryModel
        {
            get { return dataHistoryModel; }
            set
            {
                dataHistoryModel = value;
                RaisePropertyChanged("DataHistoryModel");
            }
        }

        private ICommand showSessionsCollectionCommand;
        public ICommand ShowSessionsCollectionCommand
        {
            get
            {
                if (showSessionsCollectionCommand == null)
                    showSessionsCollectionCommand = new RelayCommand(new Action(ShowSessionsCollection));
                return showSessionsCollectionCommand;
            }
        }

        private void ShowSessionsCollection()
        {
            SessionsCollection = DBAccessor.GetAllDBEntries();
        }


        private ICommand updateSelectionCommand;
        public ICommand UpdateSelectionCommand
        {
            get
            {
                if (updateSelectionCommand == null)
                    updateSelectionCommand = new ParamCommand(new Action<object>(UptateSelection));
                return updateSelectionCommand;
            }
        }

        private void UptateSelection(object datasets)
        {
            SelectedSessions = new SessionCollection(((System.Collections.IList)datasets).Cast<Models.SessionModel>());
        }

        private ICommand filterAppliedCommand;
        public ICommand FilterAppliedCommand
        {
            get
            {
                if (filterAppliedCommand == null)
                    filterAppliedCommand = new RelayCommand(new Action(Filter));
                return filterAppliedCommand;
            }
        }

        private void Filter()
        {
            SessionsCollection = DBAccessor.GetFilteredDBEntries(DataHistoryModel.Search, (string)(DataHistoryModel.Gender).Content, DataHistoryModel.From, DataHistoryModel.To);
        }

        private bool CanDelete
        {
            get
            {
                return SelectedSessions != null;
            }
        }

        private ICommand deleteSelectionCommand;
        public ICommand DeleteSelectionCommand
        {
            get
            {
                if (deleteSelectionCommand == null)
                    deleteSelectionCommand = new RelayCommand(new Action(DeleteSelection), () => CanDelete);
                return deleteSelectionCommand;
            }
        }

        private void DeleteSelection()
        {
            DBUpdater.DeleteFromSelection(SelectedSessions);
            SessionsCollection = new SessionCollection(SessionsCollection.Where(x => !SelectedSessions.Contains(x)));
        }

        private bool CanPlay
        {
            get
            {
                return SelectedSessions != null && SelectedSessions.Count == 1;
            }
        }

        private ICommand playSessionCommand;
        public ICommand PlaySessionCommand
        {
            get
            {
                if (playSessionCommand == null)
                    playSessionCommand = new RelayCommand(new Action(PlaySession), () => CanPlay);
                return playSessionCommand;
            }
        }

        private PlaySession PlaySessionWindow;

        private void PlaySession()
        {
            PlaySessionWindow = new PlaySession(SelectedSessions.First());
            PlaySessionWindow.Show();
        }
    }
}
