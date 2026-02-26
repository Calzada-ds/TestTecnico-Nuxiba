using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestNuxibaAPI.Data;
using TestNuxibaAPI.DTOs;
using TestNuxibaAPI.Models;
using System;

namespace TestNuxibaAPI.Controllers
{
    [Route("api/logins")]
    [ApiController]
    public class LoginsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LoginsController(AppDbContext context)
        {
            _context = context;
        }

        // Método de mapeo privado para evitar duplicación
        private static LoginResponseDTO MapToDto(Login l) => new()
        {
            Id = l.Id,
            User_id = l.User_id,
            Extension = l.Extension,
            TipoMov = l.TipoMov,
            fecha = l.fecha
        };

        /// Devuelve todos los registros de logins y logouts mapeados a DTO.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoginResponseDTO>>> GetLogins()
        {
            var logins = await _context.Logins
                .AsNoTracking() 
                .OrderByDescending(l => l.fecha)
                .Select(l => new LoginResponseDTO
                {
                    Id = l.Id,
                    User_id = l.User_id,
                    Extension = l.Extension,
                    TipoMov = l.TipoMov,
                    fecha = l.fecha
                })
                .ToListAsync();

            return Ok(logins);
        }

        /// Registra un nuevo movimiento con validaciones de negocio y seguridad.
        [HttpPost]
        public async Task<ActionResult<LoginResponseDTO>> PostLogin(LoginCreateDTO loginDto)
        {
            if (loginDto.fecha == default || loginDto.fecha > DateTime.UtcNow)
            {
                return BadRequest("Debe proporcionar una fecha y hora válida.");
            }

            if (loginDto.TipoMov != 0 && loginDto.TipoMov != 1)
            {
                return BadRequest("El TipoMov debe ser 0 (Logout) o 1 (Login).");
            }

            var (userExists, ultimoMovimiento) = await GetUserAndLastMovAsync(loginDto.User_id);

            if (!userExists)
            {
                return BadRequest($"El User_id {loginDto.User_id} no existe en la tabla de usuarios.");
            }

            if (ultimoMovimiento != null && ultimoMovimiento.TipoMov == loginDto.TipoMov)
            {
                string tipo = loginDto.TipoMov == 1 ? "Login" : "Logout";
                return BadRequest($"Error de secuencia: El usuario ya tiene un registro de {tipo} como último movimiento.");
            }

            var nuevoLogin = new Login
            {
                User_id = loginDto.User_id,
                Extension = loginDto.Extension,
                TipoMov = loginDto.TipoMov,
                fecha = loginDto.fecha
            };

            _context.Logins.Add(nuevoLogin);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLogins), new { id = nuevoLogin.Id }, MapToDto(nuevoLogin));
        }

        /// Actualiza un registro existente usando DTOs.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogin(int id, LoginCreateDTO loginDto)
        {
            if (loginDto.TipoMov != 0 && loginDto.TipoMov != 1)
            {
                return BadRequest("El TipoMov debe ser 0 (Logout) o 1 (Login).");
            }

            var existing = await _context.Logins.FindAsync(id);
            if (existing == null)
            {
                return NotFound("El registro a actualizar no existe.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.User_id == loginDto.User_id);
            if (!userExists)
            {
                return BadRequest($"El User_id {loginDto.User_id} no existe en la tabla de usuarios.");
            }

            var ultimoMovimiento = await _context.Logins
                .AsNoTracking()
                .Where(l => l.User_id == loginDto.User_id && l.Id != id)
                .OrderByDescending(l => l.fecha)
                .FirstOrDefaultAsync();

            if (ultimoMovimiento != null && ultimoMovimiento.TipoMov == loginDto.TipoMov)
            {
                string tipo = loginDto.TipoMov == 1 ? "Login" : "Logout";
                return BadRequest($"Error de secuencia: El usuario ya tiene un registro de {tipo} como último movimiento.");
            }

            existing.User_id = loginDto.User_id;
            existing.Extension = loginDto.Extension;
            existing.TipoMov = loginDto.TipoMov;
            existing.fecha = loginDto.fecha;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await LoginExistsAsync(id)) return NotFound();
                throw;
            }

            return Ok(new
            {
                mensaje = $"El registro con ID {id} se actualizó correctamente.",
                datosActualizados = loginDto
            });
        }

        /// Elimina un registro del sistema.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogin(int id)
        {
            var login = await _context.Logins.FindAsync(id);
            if (login == null)
            {
                return NotFound("El registro no existe.");
            }

            _context.Logins.Remove(login);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"El registro con ID {id} fue eliminado exitosamente de la base de datos." });
        }

        //Helpers
        private async Task<bool> LoginExistsAsync(int id)
        {
            return await _context.Logins.AnyAsync(e => e.Id == id);
        }

        private async Task<(bool userExists, Login? ultimoMovimiento)> GetUserAndLastMovAsync(int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.User_id == userId);

            var ultimoMovimiento = await _context.Logins
                .AsNoTracking()
                .Where(l => l.User_id == userId)
                .OrderByDescending(l => l.fecha)
                .FirstOrDefaultAsync();

            return (userExists, ultimoMovimiento);
        }
    }
}