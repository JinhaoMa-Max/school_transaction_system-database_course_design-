-- Older database instances had a unique constraint that prevented the
-- supported second (follow-up) review for the same order and reviewer.
DECLARE
    constraint_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO constraint_count
    FROM user_constraints
    WHERE constraint_name = 'UQ_REVIEW_ORDER_REVIEWER'
      AND table_name = 'REVIEW';

    IF constraint_count > 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE review DROP CONSTRAINT uq_review_order_reviewer';
    END IF;
END;
/

EXIT;
