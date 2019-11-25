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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Server.ViewModels
{
    public class SaveSessionViewModel : IBase
    {
        private SaveSession Window;

        public SaveSessionViewModel(SaveSession window, Models.SessionModel session)
        {
            this.sessionModel = session;
            Window = window;
            Genders = new ObservableCollection<string>() { "Man", "Woman" };
        }
        
        public ObservableCollection<string> Genders
        {
            get;
        }

        private SessionModel sessionModel = new Models.SessionModel();
        public SessionModel SessionModel
        {
            get { return sessionModel; }
            set { sessionModel = value; }
        }

        private Models.AthleteModel athleteModel = new Models.AthleteModel();
        public Models.AthleteModel AthleteModel
        {
            get
            {
                return athleteModel;
            }
            set
            {
                athleteModel = value;
                RaisePropertyChanged("AthleteModel");
                RaisePropertyChanged("CanSave");
            }
        }

        private string id;
        public string ID
        {
            get { return id; }
            set
            {
                id = value;
                RaisePropertyChanged("CanCheck");
            }
        }

        private bool CanCheck
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ID);
            }
        }

        private string athleteFormularyEnabled = "False";
        public string AthleteFormularyEnabled
        {
            get
            {
                return athleteFormularyEnabled;
            }
            set
            {
                athleteFormularyEnabled = value;
                RaisePropertyChanged("AthleteFormularyEnabled");
            }
        }

        private ICommand checkAthleteCommand;
        public ICommand CheckAthleteCommand
        {
            get
            {
                if (checkAthleteCommand == null)
                    checkAthleteCommand = new RelayCommand(new Action(CheckAthlete), () => CanCheck);
                return checkAthleteCommand;
            }
        }

        private void CheckAthlete()
        {
            Models.AthleteModel foundAthlete = DBAccessor.GetAthleteIfExists(ID);
            if(foundAthlete != null)
            {
                AthleteModel = foundAthlete;
                AthleteFormularyEnabled = "False";
            }
            else
            {
                MessageBox.Show("No records found for \"" + ID + "\" Id number." + Environment.NewLine + "Please, add it manually.");
                AthleteModel.IdNumber = ID;
                AthleteModel.Gender = "";
                AthleteModel.Name = "";
                AthleteFormularyEnabled = "True";
                RaisePropertyChanged("AthleteModel");
            }
        }

        private ICommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                    cancelCommand = new RelayCommand(new Action(Cancel));
                return cancelCommand;
            }
        }

        private void Cancel()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure that you want to discard this session?", "Cancel" ,MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Window.CloseAndNotify(false);
                    break;
                case MessageBoxResult.No:                   
                    break;
                default:
                    break;
            }
        }

        private bool FilledAthlete
        {
            get
            {
                return AthleteModel != null && !string.IsNullOrWhiteSpace(AthleteModel.Gender) && (AthleteModel.Gender == "Man" || AthleteModel.Gender == "Woman") && !string.IsNullOrWhiteSpace(AthleteModel.Name) && !string.IsNullOrWhiteSpace(AthleteModel.IdNumber);
            }
        }

        private bool FilledSession
        {
            get
            {
                return SessionModel.Weight != 0 && SessionModel.Height != 0 && SessionModel.TreadmillSpeed != 0;
            }
        }

        private bool CanSave
        {
            get
            {
                return FilledAthlete && FilledSession;
            }
        }

        private ICommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                    saveCommand = new RelayCommand(new Action(Save), () => CanSave);
                return saveCommand;
            }
        }

        private void Save()
        {
            string athleteHash = "";
            Models.AthleteModel validationAthlete = DBAccessor.GetAthleteIfExists(AthleteModel.IdNumber);
            if (AthleteFormularyEnabled == "True")
            {
                if(validationAthlete != null)
                {
                    MessageBoxResult res =  MessageBox.Show("Already exists an athlete with id number \"" + AthleteModel.IdNumber + "\" named \"" + validationAthlete.Name + "\"" + Environment.NewLine +
                        "Do you want to update his name?", "Athlete already exists", MessageBoxButton.YesNo);
                    switch (res)
                    {
                        case MessageBoxResult.Yes:
                            break;
                        case MessageBoxResult.No:
                            AthleteModel.Name = validationAthlete.Name;
                            AthleteModel.Gender = validationAthlete.Gender;
                            break;
                        default:
                            break;
                    }
                }
                athleteHash = DBUpdater.AddOrUpdateAthlete(AthleteModel);
            }
            else
            {
                athleteHash = DBAccessor.GetExistingAthleteHash(AthleteModel.IdNumber);
            }
            try
            {
                DBUpdater.SaveSession(SessionModel, athleteHash);
            }
            catch
            {

            }
            Window.CloseAndNotify(true);
        }
    }
}
