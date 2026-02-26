using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using TestNuxibaAPI.Data;
using TestNuxibaAPI.Models;

namespace TestNuxibaAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.Migrate();

            string excelPath = Path.Combine(AppContext.BaseDirectory, "CCenterRIA.xlsx");

            if (!File.Exists(excelPath))
            {
                throw new FileNotFoundException($"No se encontró el archivo Excel en: {excelPath}");
            }

            using var transaction = context.Database.BeginTransaction();
            try
            {
                // 1. Poblar Áreas
                if (!context.Areas.Any())
                {
                    var areas = MiniExcel.Query<Area>(excelPath, sheetName: "ccRIACat_Areas").ToList();
                    context.Areas.AddRange(areas);
                    context.SaveChanges();
                }

                // 2. Poblar Usuarios
                if (!context.Users.Any())
                {
                    var users = MiniExcel.Query<User>(excelPath, sheetName: "ccUsers").ToList();
                    context.Users.AddRange(users);
                    context.SaveChanges();
                }

                // 3. Poblar Logins
                if (!context.Logins.Any())
                {
                    var logins = MiniExcel.Query<Login>(excelPath, sheetName: "ccloglogin").ToList();
                    context.Logins.AddRange(logins);
                    context.SaveChanges();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error al inicializar la base de datos: {ex.Message}", ex);
            }
        }
    }
}