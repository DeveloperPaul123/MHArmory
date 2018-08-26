﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MHArmory.Configurations;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.ViewModels;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly RootViewModel rootViewModel = new RootViewModel();

        private SkillSelectorWindow skillSelectorWindow;

        public MainWindow()
        {
            InitializeComponent();

            LoadConfiguration();
            rootViewModel.NotifyConfigurationLoaded();

            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenSkillsSelector, OpenSkillSelector));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenAdvancedSearch, OpenAdvancedSearch));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenDecorationsOverride, OpenDecorationsOverride));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenEquipmentExplorer, OpenEquipmentExplorer));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenSearchResultProcessing, OpenSearchResultProcessing));

            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.NewLoadout, OnNewLoadout));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenLoadout, OnOpenLoadout));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.SaveLoadout, OnSaveLoadout));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.SaveLoadoutAs, OnSaveLoadoutAs));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.ManageLoadouts, OnManageLoadouts));

            AssemblyName asmName = Assembly.GetEntryAssembly().GetName();
            Title = $"{asmName.Name} {asmName.Version.Major}.{asmName.Version.Minor}.{asmName.Version.Build}";

            DataContext = rootViewModel;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Dispatcher.Yield(DispatcherPriority.Render);

            skillSelectorWindow = new SkillSelectorWindow
            {
                Owner = this
            };

            await LoadData();

            loadoutManager = new LoadoutManager(rootViewModel);
            rootViewModel.SetLoadoutManager(loadoutManager);

            string lastOpenedLoadout = GlobalData.Instance.Configuration?.LastOpenedLoadout;
            Dictionary<string, int[]> loadout = GlobalData.Instance.Configuration?.Loadout;

            if (loadout != null && lastOpenedLoadout != null && loadout.TryGetValue(lastOpenedLoadout, out int[] abilities))
                loadoutManager.Open(lastOpenedLoadout, abilities);

            rootViewModel.IsDataLoading = false;
            rootViewModel.IsDataLoaded = true;

            rootViewModel.CreateSolverData();
        }

        private void LoadConfiguration()
        {
            Configuration configuration = Configuration.Load();

            // Possibly process stuffs.

            GlobalData.Instance.Configuration = configuration;
        }

        private async Task LoadData()
        {
            //var source = new MhwDbDataSource.DataSource(null);
            var source = new AthenaAssDataSource.DataSource();

            ISkill[] skills = await source.GetSkills();
            IArmorPiece[] armors = await source.GetArmorPieces();
            ICharm[] charms = await source.GetCharms();
            IJewel[] jewels = await source.GetJewels();

            if (skills == null)
            {
                CloseApplicationBecauseOfDataSource(((ISkillDataSource)source).Description);
                return;
            }
            else if (armors == null)
            {
                CloseApplicationBecauseOfDataSource(((IArmorDataSource)source).Description);
                return;
            }
            else if (charms == null)
            {
                CloseApplicationBecauseOfDataSource(((ICharmDataSource)source).Description);
                return;
            }
            else if (jewels == null)
            {
                CloseApplicationBecauseOfDataSource(((IJewelDataSource)source).Description);
                return;
            }

            IList<SkillViewModel> allSkills = skills
                .OrderBy(x => x.Name)
                .Select(x => new SkillViewModel(x, jewels.Where(j => j.Abilities.Any(a => a.Skill.Id == x.Id)).ToList(), rootViewModel, skillSelectorWindow.SkillSelector))
                .ToList();

            skillSelectorWindow.SkillSelector.Skills = allSkills;

            IList<AbilityViewModel> allAbilities = allSkills
                .SelectMany(x => x.Abilities)
                .ToList();

            rootViewModel.SelectedAbilities = allAbilities;

            GlobalData.Instance.SetSkills(skills);
            GlobalData.Instance.SetArmors(armors);
            GlobalData.Instance.Charms = charms.SelectMany(x => x.Levels).ToList();
            GlobalData.Instance.Jewels = jewels;

            rootViewModel.FoundArmorSets = new ArmorSetViewModel[]
            {
                new ArmorSetViewModel(armors.Skip(armors.Length - 10).Take(5).ToList(), charms[0].Levels[0], null, null),
                new ArmorSetViewModel(armors.Skip(armors.Length - 5).ToList(), charms[1].Levels[0], null, null)
            };
        }

        private void OpenSkillSelector(object parameter)
        {
            skillSelectorWindow.Show();
        }

        private AdvancedSearchWindow advancedSearchWindow;

        private void OpenAdvancedSearch(object parameter)
        {
            if (advancedSearchWindow == null)
            {
                advancedSearchWindow = new AdvancedSearchWindow(rootViewModel)
                {
                    Owner = this
                };
            }

            advancedSearchWindow.Update();
            advancedSearchWindow.Show();
        }

        private void OpenDecorationsOverride(object obj)
        {
            var window = new DecorationsOverrideWindow(rootViewModel)
            {
                Owner = this
            };
            window.ShowDialog();
        }

        private void OpenEquipmentExplorer(object obj)
        {
            EquipmentExplorerWindow.Show(this);
        }

        private void OpenSearchResultProcessing(object parameter)
        {
            if (SearchResultProcessingWindow.IsOpened)
                return;

            var window = new SearchResultProcessingWindow(rootViewModel)
            {
                Owner = this
            };
            window.Show();
        }

        private LoadoutManager loadoutManager;

        private void OnNewLoadout(object parameter)
        {
            loadoutManager.Close();
        }

        private void OnOpenLoadout(object parameter)
        {
            loadoutManager.Open();
        }

        private void OnSaveLoadout(object parameter)
        {
            loadoutManager.Save();
        }

        private void OnSaveLoadoutAs(object parameter)
        {
            loadoutManager.SaveAs();
        }

        private void OnManageLoadouts(object parameter)
        {
            loadoutManager.ManageLoadouts();
        }

        private void CloseApplicationBecauseOfDataSource(string description)
        {
            string message = $"Could not load required data from '{description}'\nContact the data source owner for more information.";
            MessageBox.Show(this, message, "Data source error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (loadoutManager.ApplicationClose() == false)
            {
                e.Cancel = true;
                return;
            }

            loadoutManager.Dispose();

            skillSelectorWindow.ApplicationClose();
        }
    }
}
