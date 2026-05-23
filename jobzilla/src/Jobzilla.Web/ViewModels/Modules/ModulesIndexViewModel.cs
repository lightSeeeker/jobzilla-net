namespace Jobzilla.Web.ViewModels.Modules;

public class ModulesIndexViewModel
{
    public IReadOnlyList<TemplateModuleViewModel> Modules { get; set; } = Array.Empty<TemplateModuleViewModel>();
}
