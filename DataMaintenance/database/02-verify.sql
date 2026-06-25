-- 校验脚本: 查看各表记录数与样例
USE `SmartLabOS-Presales-AI`;

SELECT 'SmartLabOS-Module'       AS table_name, COUNT(*) AS rows_count FROM `SmartLabOS-Module`
UNION ALL
SELECT 'SmartLabOS-PlatformBase', COUNT(*) FROM `SmartLabOS-PlatformBase`
UNION ALL
SELECT 'SmartLabOS-WorkStation',  COUNT(*) FROM `SmartLabOS-WorkStation`
UNION ALL
SELECT 'SmartLabOS-Solution',     COUNT(*) FROM `SmartLabOS-Solution`
UNION ALL
SELECT 'SmartLabOS-Project',      COUNT(*) FROM `SmartLabOS-Project`;

-- 样例: 模块前 5 条关键字段
SELECT id, name, category, status, owner, last_updated
FROM `SmartLabOS-Module`
ORDER BY id
LIMIT 5;
