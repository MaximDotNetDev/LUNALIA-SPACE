using System.Windows.Controls;

namespace SchoolJournal.Desktop.Features.Infrastructure.Outbox;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal")]
public sealed partial class OutboxView : UserControl
{
    public OutboxView()
    {
        InitializeComponent();
    }
}