namespace Application.Common.AdminPanel;

/// <summary>تعريف تبويب وظيفي جاهز في لوحة الويب — مفتاح ثابت مستقل عن التسمية والـURL.</summary>
public sealed record AdminPanelTabDefinition(
    string TabKey,
    string? ParentKey,
    string DisplayNameAr,
    bool Ready,
    IReadOnlyList<string> GrantedActions,
    IReadOnlyList<AdminPanelEndpointBinding> Endpoints);

public sealed record AdminPanelEndpointBinding(
    string HttpMethod,
    string PathTemplate,
    string LegacyPermission,
    string ResourceScope,
    string TestHint);
