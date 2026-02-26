using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TestNuxibaAPI.Controllers;
using TestNuxibaAPI.Data;
using TestNuxibaAPI.Models;
using TestNuxibaAPI.DTOs; 
using Xunit;

namespace TestNuxibaAPI.Tests
{
    public class LoginsControllerTests
    {
        private AppDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var databaseContext = new AppDbContext(options);
            databaseContext.Database.EnsureCreated();

            databaseContext.Users.Add(new User
            {
                User_id = 1,
                Login = "test_user",
                Nombres = "Test",
                ApellidoPaterno = "User",
                Password = "123",
                IDArea = 1,
                fCreate = DateTime.Now
            });
            databaseContext.SaveChanges();

            return databaseContext;
        }

        [Fact]
        public async Task PostLogin_CuandoUsuarioYaTieneLoginActivo_RetornaBadRequest()
        {
            // Arrange
            var context = GetDatabaseContext();
            var controller = new LoginsController(context);
            var userId = 1;

            context.Logins.Add(new Login { User_id = userId, Extension = 100, TipoMov = 1, fecha = DateTime.Now.AddHours(-1) });
            await context.SaveChangesAsync();

            var duplicateLogin = new LoginCreateDTO { User_id = userId, Extension = 100, TipoMov = 1, fecha = DateTime.Now };

            // Act
            var result = await controller.PostLogin(duplicateLogin);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Error de secuencia", actionResult.Value?.ToString());
        }

        [Fact]
        public async Task PostLogin_CuandoUsuarioNoExiste_RetornaBadRequest()
        {
            // Arrange
            var context = GetDatabaseContext();
            var controller = new LoginsController(context);

            var loginInexistente = new LoginCreateDTO { User_id = 999, Extension = 100, TipoMov = 1, fecha = DateTime.Now };

            // Act
            var result = await controller.PostLogin(loginInexistente);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("no existe", actionResult.Value?.ToString());
        }
    }
}