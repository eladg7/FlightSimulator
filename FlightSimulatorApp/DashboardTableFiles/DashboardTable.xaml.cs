using System.Windows.Controls;

namespace FlightSimulatorApp.DashboardTableFiles
{
    public partial class DashboardTable : UserControl
    {
        private DashboardTableViewModel _vm;

        public DashboardTable()
        {
            InitializeComponent();
        }

        public void SetViewModel(DashboardTableViewModel vm)
        {
            _vm = vm;
            DataContext = _vm;
        }
    }
}