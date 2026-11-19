using Application.Archive.Common;

namespace Application.Archive.Commands.Reconcile;

/// <summary>مصالحة يدوية — نفس شفرة الخدمة الدورية بلا أرضية تاريخ.</summary>
public record ReconcileArchiveCommand : IRequest<Result<ArchiveReconcileReport>>;
