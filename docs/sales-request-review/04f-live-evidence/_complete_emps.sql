-- Test fixture only on qalaat_04f_sales_verify: mark Mandob users creation-complete for assign eligibility
SELECT current_database();
UPDATE "Users"
SET "CreationStep" = 3
WHERE "Username" IN ('emp_a_04f','emp_b_04f')
RETURNING "Id","Username","CreationStep";
