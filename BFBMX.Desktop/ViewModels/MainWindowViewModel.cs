using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFBMX.Desktop.ViewModels
{
    public partial class MainWindowViewModel
    {

        [RelayCommand(CanExecute = nameof(CanInitMonitor))]
        public void InitAlphaMonitor()
        {

        }

        public bool CanInitMonitor()
        {
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStartAlphaMonitor))]
        public void StartAlphaMonitor()
        {

        }

        public bool CanStartAlphaMonitor()
        {
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStopAlphaMonitor))]
        public void StopAlphaMonitor()
        {

        }

        public bool CanStopAlphaMonitor()
        {
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyAlphaMonitor))]
        public void DestroyAlphaMonitor()
        {

        }

        public bool CanDestroyAlphaMonitor()
        {
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanInitMonitor))]
        public void InitBravoMonitor()
        {

        }

        [RelayCommand(CanExecute = nameof(CanStartBravoMonitor))]
        public void StartBravoMonitor()
        {

        }

        public bool CanStartBravoMonitor()
        {
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStopBravoMonitor))]
        public void StopBravoMonitor()
        {

        }

        public bool CanStopBravoMonitor()
        {
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyBravoMonitor))]
        public void DestroyBravoMonitor()
        {

        }

        public bool CanDestroyBravoMonitor()
        {
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanInitMonitor))]
        public void InitCharlieMonitor()
        {

        }

        [RelayCommand(CanExecute = nameof(CanStartCharlieMonitor))]
        public void StartCharlieMonitor()
        {

        }

        public bool CanStartCharlieMonitor()
        {
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStopCharlieMonitor))]
        public void StopCharlieMonitor()
        {

        }

        public bool CanStopCharlieMonitor()
        {
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyCharlieMonitor))]
        public void DestroyCharlieMonitor()
        {

        }

        public bool CanDestroyCharlieMonitor()
        {
            return true;
        }
    }
}
