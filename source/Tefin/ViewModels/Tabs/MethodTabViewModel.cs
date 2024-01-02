#region

using System.Linq;

using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class MethodTabViewModel : TabViewModelBase {
    public MethodTabViewModel(MethodNode item) : base(item) {
        this.ClientMethod = item.CreateViewModel();
        this.Client = item.Client;
        
        this.ClientMethod.SubscribeTo(x => x.IsBusy, this.OnIsBusyChanged);
        this.AllowDuplicates = true;
    }

    public ProjectTypes.ClientGroup Client {
        get;
    }

    public ClientMethodViewModelBase ClientMethod { get; }

    public override void Dispose() {
        base.Dispose();
        this.ClientMethod.Dispose();
    }

    public string GetRequestContent() {
        return this.ClientMethod.GetRequestContent();
    }

    public override string GenerateNewTitle(string[] existingNames) {
        //name format {Title}(count)
        var maxTabs = 1000;
        for (var i = 1; i < maxTabs; i++) {
            var suggestedName = $"{this.Title}({i})";
            if (!existingNames.Contains(suggestedName))
                return suggestedName;
        }

        throw new Exception("Unable to generate a tab name");
    }

    protected override string GetTabId() {
        return $"Todo/[url.config]/{this.Title}"; //TODO:
    }

    private void OnIsBusyChanged(ViewModelBase obj) {
        this.IsBusy = obj.IsBusy;
    }
}