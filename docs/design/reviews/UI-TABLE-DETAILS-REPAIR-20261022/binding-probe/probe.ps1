$ErrorActionPreference = 'Stop'
$taskBin = 'C:\Users\lopmatu\.codex\worktrees\ui-layout-build\GameSaveCenter\artifacts\gsc-b\8802760e\bin\GameSaveCenter.Playnite\Release\net462'
Add-Type -AssemblyName PresentationFramework,PresentationCore,WindowsBase
Add-Type -TypeDefinition @"
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
public sealed class InboxBindingFixture : INotifyPropertyChanged {
    public ObservableCollection<object> MediaClassificationHistoryItems { get; private set; }
    private object selected;
    public int NullWrites { get; private set; }
    public object SelectedMediaClassificationBatch { get { return selected; } set { if (value == null) NullWrites++; selected = value; var h=PropertyChanged; if(h!=null) h(this,new PropertyChangedEventArgs("SelectedMediaClassificationBatch")); } }
    public object SelectedInboxMedia { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;
    public InboxBindingFixture() { MediaClassificationHistoryItems = new ObservableCollection<object>(); }
}
public static class InboxProbeResolver {
    private static string root;
    public static void Install(string path) { root=path; AppDomain.CurrentDomain.AssemblyResolve += Resolve; }
    private static Assembly Resolve(object sender, ResolveEventArgs args) { var path=Path.Combine(root,new AssemblyName(args.Name).Name+".dll"); return File.Exists(path) ? Assembly.LoadFrom(path) : null; }
}
"@
[InboxProbeResolver]::Install($taskBin)
[void][Reflection.Assembly]::LoadFrom((Join-Path $taskBin 'GameSaveCenter.Contracts.dll'))
[void][Reflection.Assembly]::LoadFrom((Join-Path $taskBin 'GameSaveCenter.Playnite.dll'))
$taskWindow=$null; $taskFailure=$null; $taskSteps=New-Object Collections.Generic.List[object]
try {
    $taskFixture=New-Object InboxBindingFixture
    $taskBatchA=New-Object GameSaveCenter.Contracts.MediaClassificationBatchSummaryDto; $taskBatchA.BatchId='synthetic-a'
    $taskBatchB=New-Object GameSaveCenter.Contracts.MediaClassificationBatchSummaryDto; $taskBatchB.BatchId='synthetic-b'
    $taskFixture.MediaClassificationHistoryItems.Add($taskBatchA); $taskFixture.MediaClassificationHistoryItems.Add($taskBatchB); $taskFixture.SelectedMediaClassificationBatch=$taskBatchB
    $taskItem=New-Object GameSaveCenter.Contracts.MediaItemDto; $taskItem.MediaId='synthetic-media'; $taskItem.Kind=[GameSaveCenter.Contracts.MediaKind]::Screenshot; $taskItem.ArchivePath='C:\GscSyntheticMissing\image.png'; $taskFixture.SelectedInboxMedia=$taskItem
    $taskView=New-Object GameSaveCenter.Playnite.Views.MediaCenterView; $taskView.DataContext=$taskFixture
    $taskTabs=$taskView.FindName('MediaTabControl'); [Windows.Data.BindingOperations]::ClearBinding($taskTabs,[Windows.Controls.Primitives.Selector]::SelectedIndexProperty); $taskTabs.SelectedIndex=0
    $taskGrid=$taskView.FindName('MediaInboxGrid'); [Windows.Data.BindingOperations]::ClearBinding($taskGrid,[Windows.Controls.ItemsControl]::ItemsSourceProperty); $taskGrid.ItemsSource=@($taskItem)
    $taskWindow=New-Object Windows.Window; $taskWindow.Width=1400; $taskWindow.Height=760; $taskWindow.Content=$taskView; $taskWindow.ShowInTaskbar=$false; $taskWindow.ShowActivated=$false; $taskWindow.Opacity=0.01; $taskWindow.WindowStyle='None'; $taskWindow.Show()
    $taskHistory=$taskView.FindName('MediaClassificationHistoryList'); $taskFrame=$taskView.FindName('MediaInboxInspectorFrame')
    $taskGrid.SelectedItem=$taskItem; $taskView.ApplyResponsiveLayout(1400,760); $taskWindow.UpdateLayout(); $taskHistory.SelectedItem=$taskBatchB; $taskWindow.UpdateLayout()
    foreach($taskWidth in @(1400,860,1400,860,1400)) {
        $taskWindow.Width=$taskWidth; $taskWindow.UpdateLayout(); $taskView.ApplyResponsiveLayout($taskWidth,760); $taskWindow.UpdateLayout(); $taskWindow.Dispatcher.Invoke([Windows.Threading.DispatcherPriority]::ApplicationIdle,[Action]{})
        $taskSelected=$taskFixture.SelectedMediaClassificationBatch
        $taskSteps.Add([ordered]@{width=$taskWidth;parent=$taskFrame.Parent.Name;sourceBatch=if($taskSelected){$taskSelected.BatchId}else{$null};targetBatch=if($taskHistory.SelectedItem){$taskHistory.SelectedItem.BatchId}else{$null};nullWrites=$taskFixture.NullWrites})
        if(-not [object]::ReferenceEquals($taskSelected,$taskBatchB) -or -not [object]::ReferenceEquals($taskHistory.SelectedItem,$taskBatchB)) { throw "Selected history batch was lost at width $taskWidth" }
    }
}
catch { $taskFailure=$_.Exception.ToString() }
finally { if($taskWindow){$taskWindow.Close()}; if(-not [Windows.Threading.Dispatcher]::CurrentDispatcher.HasShutdownStarted){[Windows.Threading.Dispatcher]::CurrentDispatcher.InvokeShutdown()} }
[ordered]@{sourceCommit='5ced8abb087a2e6bc29fbd0526b157167e106eda';ok=($null -eq $taskFailure);steps=$taskSteps;error=$taskFailure} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath 'D:\workplace\github\GameSaveCenter\artifacts\ui-layout-binding-probe\result.json' -Encoding utf8
if($taskFailure){throw $taskFailure}
