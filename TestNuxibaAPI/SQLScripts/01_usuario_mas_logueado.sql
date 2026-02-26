-- 1 Consulta del usuario que más tiempo ha estado logueado
WITH Sesiones AS (
    SELECT 
        User_id,
        fecha AS Inicio,
        LEAD(fecha) OVER (PARTITION BY User_id ORDER BY fecha) AS Fin,
        TipoMov,
        LEAD(TipoMov) OVER (PARTITION BY User_id ORDER BY fecha) AS SigMov
    FROM ccloglogin
),
CalculoSegundos AS (
    SELECT 
        User_id,
        SUM(DATEDIFF(SECOND, Inicio, Fin)) AS TotalSegundos
    FROM Sesiones
    WHERE TipoMov = 1 AND SigMov = 0 -- Solo pares Login -> Logout
    GROUP BY User_id
)
SELECT TOP 1
    User_id,
    CONCAT(
        TotalSegundos / 86400, ' días, ',
        (TotalSegundos % 86400) / 3600, ' horas, ',
        (TotalSegundos % 3600) / 60, ' minutos, ',
        TotalSegundos % 60, ' segundos'
    ) AS [Tiempo total]
FROM CalculoSegundos
ORDER BY TotalSegundos DESC;