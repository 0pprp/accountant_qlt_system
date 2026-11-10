-- Phase 02 schema for isolated verify DB only. No migration file generated.
-- Matches SCHEMA_REQUIRED.md + EF mappings.

ALTER TABLE "Users"
  ADD COLUMN IF NOT EXISTS "PermissionsVersion" integer NOT NULL DEFAULT 0;

CREATE TABLE IF NOT EXISTS "UserPermissionDenials" (
  "UserId" integer NOT NULL,
  "PermissionId" integer NOT NULL,
  CONSTRAINT "PK_UserPermissionDenials" PRIMARY KEY ("UserId", "PermissionId"),
  CONSTRAINT "FK_UserPermissionDenials_Users_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_UserPermissionDenials_Permissions_PermissionId"
    FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE RESTRICT
);
