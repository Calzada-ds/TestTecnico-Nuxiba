-- 2 Consulta del usuario que menos tiempo ha estado logueado
WITH Sesiones AS (
    SELECT User_id, fecha AS Inicio,
           LEAD(fecha) OVER (PARTITION BY User_id ORDER BY fecha) AS Fin,
           TipoMov, LEAD(TipoMov) OVER (PARTITION BY User_id ORDER BY fecha) AS SigMov
    FROM ccloglogin
),
Calculo AS (
    SELECT User_id, SUM(DATEDIFF(SECOND, Inicio, Fin)) AS s
    FROM Sesiones WHERE TipoMov = 1 AND SigMov = 0 GROUP BY User_id
),
Partes AS (
    SELECT User_id, s / 86400 AS d, (s % 86400) / 3600 AS h, (s % 3600) / 60 AS m, s % 60 AS sec
    FROM Calculo
)
SELECT TOP 1 User_id,
       COALESCE(CONCAT_WS(', ', 
           NULLIF(CAST(d AS VARCHAR) + ' días', '0 días'),
           NULLIF(CAST(h AS VARCHAR) + ' horas', '0 horas'),
           NULLIF(CAST(m AS VARCHAR) + ' minutos', '0 minutos'),
           NULLIF(CAST(sec AS VARCHAR) + ' segundos', '0 segundos')
       ), '0 segundos') AS [Tiempo total]
FROM Partes ORDER BY (d*86400 + h*3600 + m*60 + sec) ASC;