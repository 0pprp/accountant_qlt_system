class PermissionActions {
  const PermissionActions({
    this.canCreate = false,
    this.canRead = false,
    this.canUpdate = false,
    this.canDelete = false,
  });
  final bool canCreate;
  final bool canRead;
  final bool canUpdate;
  final bool canDelete;
}
