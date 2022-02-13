using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Editors;
using MovieMatrix.Helper;
using Newtonsoft.Json;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace MovieMatrix.Controls
{
    public partial class TokenComboBoxEdit : ComboBoxEdit
    {
        public TokenMode Mode
        {
            get { return (TokenMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(TokenMode), typeof(TokenComboBoxEdit), new PropertyMetadata(TokenMode.Invalid, ModeChanged));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(TokenComboBoxEdit));

        public TokenComboBoxEdit()
        {
            InitializeComponent();

            Loaded += (s, e) => { ExecuteCommand(); };
            EditValueChanged += (s, e) => { ExecuteCommand(); };
            IsVisibleChanged += (s, e) => { ExecuteCommand(); };

            MouseLeftButtonUp += (s, e) =>
            {
                Focus();
                ShowPopup();
            };

            PopupClosed += (s, e) =>
            {
                DoValidate();
            };
        }

        protected override object OnSelectedItemCoerce(object baseValue)
        {
            object coerceValue = null;

            switch (baseValue)
            {
                case SearchPerson person:
                    coerceValue = AddPerson(person);
                    break;
                case SearchKeyword keyword:
                    coerceValue = AddKeyword(keyword);
                    break;
                case SearchCompany company:
                    coerceValue = AddCompany(company);
                    break;
                case Network network:
                    coerceValue = AddNetwork(network);
                    break;
            }

            return base.OnSelectedItemCoerce(coerceValue);
        }

        private void ExecuteCommand()
        {
            List<int> editValue = new List<int>();
            if (EditValue != null && EditValue is List<object> list)
                editValue = new List<int>(list.OfType<int>());

            if (IsVisible)
                Command?.Execute(editValue);
        }

        private static void ModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TokenComboBoxEdit tokenComboBox = d as TokenComboBoxEdit;

            switch (tokenComboBox.Mode)
            {
                case TokenMode.MovieGenre:
                    tokenComboBox.DisplayMember = "DisplayName";
                    tokenComboBox.ItemsSource = MovieGenres;
                    break;

                case TokenMode.TvShowGenre:
                    tokenComboBox.DisplayMember = "DisplayName";
                    tokenComboBox.ItemsSource = TvShowGenres;
                    break;

                case TokenMode.Network:
                    tokenComboBox.DisplayMember = "Name";
                    tokenComboBox.ItemsSource = Networks;
                    break;

                case TokenMode.Person:
                    tokenComboBox.DisplayMember = "Name";
                    tokenComboBox.ItemsSource = People;
                    break;

                case TokenMode.Keyword:
                    tokenComboBox.DisplayMember = "Name";
                    tokenComboBox.ItemsSource = Keywords;
                    break;

                case TokenMode.Company:
                    tokenComboBox.DisplayMember = "Name";
                    tokenComboBox.ItemsSource = Companies;
                    break;
            }
        }

        private async void ProcessNewToken(DependencyObject d, ProcessNewValueEventArgs e)
        {
            string query = e.DisplayText;

            if (String.IsNullOrEmpty(query))
                return;

            switch (Mode)
            {
                case TokenMode.Person:
                    List<SearchPerson> people = await App.Repository.SearchPersonAsync(query);
                    SearchPeople(people, query);
                    break;

                case TokenMode.Keyword:
                    List<SearchKeyword> keywords = await App.Repository.SearchKeywordAsync(query);
                    SearchKeywords(keywords, query);
                    break;

                case TokenMode.Company:
                    List<SearchCompany> companies = await App.Repository.SearchCompanyAsync(query);
                    SearchCompanies(companies, query);
                    break;
            }
        }

        private void SearchPeople(List<SearchPerson> itemList, string query)
        {
            foreach (SearchPerson item in itemList)
                AddPerson(item);

            SearchPerson match = People.FirstOrDefault(x => x.Name.Equals(query, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                match = People.FirstOrDefault(x => x.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                match = People.FirstOrDefault(x => x.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) > 0);

            if (match != null)
                SelectedItems.Add(match);
        }

        private void SearchKeywords(List<SearchKeyword> itemList, string query)
        {
            foreach (SearchKeyword item in itemList)
                AddKeyword(item);

            SearchKeyword match = Keywords.FirstOrDefault(x => x.Name.Equals(query, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                match = Keywords.FirstOrDefault(x => x.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                match = Keywords.FirstOrDefault(x => x.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) > 0);

            if (match != null)
                SelectedItems.Add(match);
        }

        private void SearchCompanies(List<SearchCompany> itemList, string query)
        {
            foreach (SearchCompany item in itemList)
                AddCompany(item);

            SearchCompany match = Companies.FirstOrDefault(x => x.Name.Equals(query, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                match = Companies.FirstOrDefault(x => x.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                match = Companies.FirstOrDefault(x => x.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) > 0);

            if (match != null)
                SelectedItems.Add(match);
        }

        private static SearchPerson AddPerson(SearchPerson person)
        {
            SearchPerson existingItem = People.FirstOrDefault(x => x.Id == person.Id);
            if (existingItem != null)
                return existingItem;

            People.Add(person);
            return person;
        }

        private static SearchKeyword AddKeyword(SearchKeyword keyword)
        {
            SearchKeyword existingItem = Keywords.FirstOrDefault(x => x.Id == keyword.Id);
            if (existingItem != null)
                return existingItem;

            Keywords.Add(keyword);
            return keyword;
        }

        private static SearchCompany AddCompany(SearchCompany company)
        {
            SearchCompany existingItem = Companies.FirstOrDefault(x => x.Id == company.Id);
            if (existingItem != null)
                return existingItem;

            Companies.Add(company);
            return company;
        }

        private static Network AddNetwork(Network network)
        {
            Network existingItem = Networks.FirstOrDefault(x => x.Id == network.Id);
            if (existingItem != null)
                return existingItem;

            Networks.Add(network);
            return network;
        }

        private static ObservableCollection<SearchPerson> People = new ObservableCollection<SearchPerson>();

        private static ObservableCollection<SearchKeyword> Keywords = new ObservableCollection<SearchKeyword>();

        private static ObservableCollection<SearchCompany> Companies = new ObservableCollection<SearchCompany>();

        private static ObservableCollection<Network> Networks = new ObservableCollection<Network>();

        private static ObservableCollection<Genre> MovieGenres = new ObservableCollection<Genre>(GenreHelper.MovieGenres.Where(x => x.Id != 0));

        private static ObservableCollection<Genre> TvShowGenres = new ObservableCollection<Genre>(GenreHelper.TvShowGenres.Where(x => x.Id != 0));

        static TokenComboBoxEdit()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MovieMatrix.Assets.Data.AutoSuggestData.zip"))
            {
                using (ZipArchive archive = new ZipArchive(stream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name == "Companies.txt")
                        {
                            List<SearchCompany> companies = new List<SearchCompany>();
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                string line = String.Empty;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    SearchCompany company = JsonConvert.DeserializeObject<SearchCompany>(line);
                                    companies.Add(company);
                                }
                            }

                            Companies = new ObservableCollection<SearchCompany>(companies);
                        }
                        else if (entry.Name == "Keywords.txt")
                        {
                            List<SearchKeyword> keywords = new List<SearchKeyword>();
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                string line = String.Empty;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    SearchKeyword keyword = JsonConvert.DeserializeObject<SearchKeyword>(line);
                                    keywords.Add(keyword);
                                }
                            }

                            Keywords = new ObservableCollection<SearchKeyword>(keywords);
                        }
                        else if (entry.Name == "Networks.txt")
                        {
                            List<Network> networks = new List<Network>();
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                string line = String.Empty;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    Network network = JsonConvert.DeserializeObject<Network>(line);
                                    networks.Add(network);
                                }
                            }

                            Networks = new ObservableCollection<Network>(networks);
                        }
                        else if (entry.Name == "People.txt")
                        {
                            List<SearchPerson> people = new List<SearchPerson>();
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                string line = String.Empty;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    SearchPerson person = JsonConvert.DeserializeObject<SearchPerson>(line);
                                    people.Add(person);
                                }
                            }

                            People = new ObservableCollection<SearchPerson>(people);
                        }
                    }
                }
            }
        }
    }

    public enum TokenMode
    {
        Invalid,
        MovieGenre,
        TvShowGenre,
        Person,
        Keyword,
        Company,
        Network
    }
}
