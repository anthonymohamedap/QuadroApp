// Views/PlanningCalendarWindow.axaml.cs
using Avalonia.Controls;
using System;

namespace QuadroApp.Views
{
    public partial class PlanningCalendarWindow : Window
    {
        public PlanningCalendarWindow()
        {
            InitializeComponent();
        }

        // Raised wanneer gebruiker "Plan taak op selectie" kiest of dubbelklikt op dag
        public event EventHandler<PlanningChosenEventArgs>? PlanningChosen;

        public void RaiseChosen(DateTime startLocal, int durationMin)
            => PlanningChosen?.Invoke(this, new PlanningChosenEventArgs(startLocal, durationMin));
    }


    public sealed class PlanningChosenEventArgs : EventArgs
    {
        public PlanningChosenEventArgs(DateTime startLocal, int durationMin)
        {
            StartLocal = startLocal; DurationMin = durationMin;
        }
        public DateTime StartLocal { get; }
        public int DurationMin { get; }
    }
}
