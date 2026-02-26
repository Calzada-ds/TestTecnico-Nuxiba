-- 3. Promedio de logueo por mes
SET LANGUAGE Spanish;

WITH SesionesDetalladas AS (
    SELECT 
        User_id, 
        fecha AS Inicio,
        LEAD(fecha) OVER (PARTITION BY User_id ORDER BY fecha) AS Fin,
        TipoMov, 
        LEAD(TipoMov) OVER (PARTITION BY User_id ORDER BY fecha) AS SigMov
    FROM ccloglogin
),
Promedios AS (
    SELECT 
        User_id, 
        MONTH(Inicio) AS MesNum, 
        YEAR(Inicio) AS Anio,
        DATENAME(MONTH, Inicio) AS MesNombre,
        AVG(CAST(DATEDIFF(SECOND, Inicio, Fin) AS BIGINT)) AS s
    FROM SesionesDetalladas 
    WHERE TipoMov = 1 AND SigMov = 0 -- TipoMov 1 (login) y 0 (logout)
    GROUP BY User_id, YEAR(Inicio), MONTH(Inicio), DATENAME(MONTH, Inicio)
),
Partes AS (
    SELECT 
        User_id, 
        MesNombre, 
        Anio, 
        MesNum,
        s / 86400 AS d, 
        (s % 86400) / 3600 AS h, 
        (s % 3600) / 60 AS m, 
        s % 60 AS sec
    FROM Promedios
)
SELECT 
    CONCAT('Usuario ', User_id, ' en ', MesNombre, ' ', Anio, ':') AS Detalle,
    COALESCE(NULLIF(CONCAT_WS(', ', 
        NULLIF(CAST(d AS VARCHAR) + ' días', '0 días'),
        NULLIF(CAST(h AS VARCHAR) + ' horas', '0 horas'),
        NULLIF(CAST(m AS VARCHAR) + ' minutos', '0 minutos'),
        NULLIF(CAST(sec AS VARCHAR) + ' segundos', '0 segundos')
    ), ''), '0 segundos') AS [Promedio de logueo]
FROM Partes 
ORDER BY Anio, MesNum, User_id; -- Ordenamiento cronologico
