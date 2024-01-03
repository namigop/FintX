#region

using System.Linq;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;

using File = Tefin.Core.File;

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

    public override string GenerateNewTitle() {
        //name format {Title}(count)
        var methodPath =
            Project.getMethodPath(this.Client.Path)
                .Then(p => Path.Combine(p, this.ClientMethod.MethodInfo.Name));
        var name = Core.Utils.getFileName(methodPath, this.Title, Ext.requestFileExt);
        return Path.GetFileNameWithoutExtension(name);

        //throw new Exception("Unable to generate a tab name");
    }

    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileNameWithoutExtension(this.Id);
    }

    protected override string GetTabId() {

        var methodName = this.ClientMethod.MethodInfo.Name;
        var localMethodPath = Project.getMethodPath(this.Client.Path).Then(p => Path.Combine(p, methodName));

        var fileName = Core.Utils.getFileName(localMethodPath, methodName, Ext.requestFileExt);
        var id = Path.Combine(localMethodPath, fileName);
        Io.File.WriteAllText(id, "");
        return id;
    }

    private void OnIsBusyChanged(ViewModelBase obj) {
        this.IsBusy = obj.IsBusy;
    }
}