﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using CenaPlus.Server.Bll;

namespace CenaPlus.Server.ServerMode.Contest
{
    /// <summary>
    /// Interaction logic for Questions.xaml
    /// </summary>
    public partial class Questions : UserControl, IContent
    {
        private List<QuestionListItem> QuestionListItems = new List<QuestionListItem>();
        private int contestID;
        public Questions()
        {
            InitializeComponent();
            LocalCenaServer.QuestionUpdated += this.QuestionUpdated;
            App.RemoteCallback.OnQuestionUpdated += this.QuestionUpdated;
            LocalCenaServer.NewQuestion += this.NewQuestion;
            App.RemoteCallback.OnNewQuestion += this.NewQuestion;
        }

        private void lstQuestion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstQuestion.SelectedItem == null)
            {
                spDetails.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                spDetails.Visibility = System.Windows.Visibility.Visible;
                QuestionTextBlock.Text = (lstQuestion.SelectedItem as QuestionListItem).Description;
                txtAnswer.Text = (lstQuestion.SelectedItem as QuestionListItem).Answer;
                if ((lstQuestion.SelectedItem as QuestionListItem).Status != Entity.QuestionStatus.Pending)
                {
                    spDetails.IsEnabled = false;
                }
                else
                {
                    spDetails.IsEnabled = true;
                }
            }
        }

        class QuestionListItem : Entity.Question
        {
            public string Details
            {
                get
                {
                    return String.Format("Asker: {0} / {1} @{2}", AskerNickName, Status, Time.ToShortTimeString());
                }
            }
        }
        public void QuestionUpdated(int question_id)
        {
            var questionindex = QuestionListItems.FindIndex(x => x.ID == question_id);
            if (questionindex == -1) return;
            var q = App.Server.GetQuestion(question_id);
            Dispatcher.Invoke(new Action(() =>
            {
                QuestionListItems[questionindex] = new QuestionListItem()
                {
                    ID = q.ID,
                    AskerID = q.AskerID,
                    Answer = q.Answer,
                    Asker = q.Asker,
                    Contest = q.Contest,
                    ContestID = q.ContestID,
                    ContestName = q.ContestName,
                    Description = q.Description,
                    Status = q.Status,
                    StatusAsInt = q.StatusAsInt,
                    Time = q.Time,
                    AskerNickName = q.AskerNickName
                };
                lstQuestion.Items.Refresh();
            }));
        }
        public void NewQuestion(int question_id)
        {
            var q = App.Server.GetQuestion(question_id);
            if (q == null) return;
            if (q.ContestID != contestID) return;
            var item = new QuestionListItem()
            {
                ID = q.ID,
                AskerID = q.AskerID,
                Answer = q.Answer,
                Asker = q.Asker,
                Contest = q.Contest,
                ContestID = q.ContestID,
                ContestName = q.ContestName,
                Description = q.Description,
                Status = q.Status,
                StatusAsInt = q.StatusAsInt,
                Time = q.Time,
                AskerNickName = q.AskerNickName
            };
            Dispatcher.Invoke(new Action(() =>
            {
                QuestionListItems.Add(item);
                lstQuestion.Items.Refresh();
            }));
        }
        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            QuestionListItems.Clear();
            contestID = int.Parse(e.Fragment);
            var list = from id in App.Server.GetQuestionList(contestID)
                       let q = App.Server.GetQuestion(id)
                       select new QuestionListItem
                       {
                           ID = q.ID,
                           AskerID = q.AskerID,
                           Answer = q.Answer,
                           Asker =q.Asker,
                           Contest = q.Contest,
                           ContestID = q.ContestID,
                           ContestName = q.ContestName,
                           Description = q.Description,
                           Status = q.Status,
                           StatusAsInt = q.StatusAsInt,
                           Time = q.Time,
                           AskerNickName = q.AskerNickName
                       };
            foreach (var item in list) QuestionListItems.Add(item);
            lstQuestion.ItemsSource = QuestionListItems;
            lstQuestion.Items.Refresh();
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {

        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {

        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {

        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            var q = lstQuestion.SelectedItem as QuestionListItem;
            if (rdbtnPrivate.IsChecked == true)
                q.Status = Entity.QuestionStatus.Private;
            else if (rdbtnPublic.IsChecked == true)
                q.Status = Entity.QuestionStatus.Public;
            else
                q.Status = Entity.QuestionStatus.Rejected;
            q.Answer = txtAnswer.Text;
            App.Server.UpdateQuestion(q.ID, null, q.Answer, q.Status);
            lstQuestion.Items.Refresh();
        }
    }
}
