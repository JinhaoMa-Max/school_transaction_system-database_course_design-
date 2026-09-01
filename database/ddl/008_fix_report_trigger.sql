-- Fix ORA-04091 raised by the original row-level report alert trigger.
CREATE OR REPLACE TRIGGER trg_report_threshold_alert
AFTER INSERT ON report
BEGIN
    FOR item IN (
        SELECT target_user_id, COUNT(*) AS report_count
        FROM report
        WHERE target_user_id IS NOT NULL
          AND report_status IN ('pending', 'processing')
        GROUP BY target_user_id
        HAVING COUNT(*) >= 5
    ) LOOP
        DBMS_OUTPUT.PUT_LINE(
            'WARNING: user ' || item.target_user_id ||
            ' has ' || item.report_count || ' unresolved reports.'
        );
    END LOOP;
END;
/

EXIT;
