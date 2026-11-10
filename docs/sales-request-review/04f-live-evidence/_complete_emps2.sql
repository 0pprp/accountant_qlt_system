SELECT current_database();
UPDATE "Users"
SET "CreationStep" = 3
WHERE "Username" IN ('admin_decider_04f','emp_a_04f','emp_b_04f')
RETURNING "Id","Username","CreationStep";
