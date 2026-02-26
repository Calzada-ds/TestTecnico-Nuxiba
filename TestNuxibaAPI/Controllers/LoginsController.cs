using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuxibaPracticeAPI.Data;
using NuxibaPracticeAPI.Models;

namespace NuxibaPracticeAPI.Controllers
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

        /// Devuelve todos los registros de logins y logouts.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Login>>> GetLogins()
        {
            return await _context.Logins.OrderByDescending(l => l.fecha).ToListAsync();
        }

        /// Registra un nuevo movimiento con validaciones de negocio.
        [HttpPost]
        public async Task<ActionResult<Login>> PostLogin(Login login)
        {
            if (login.fecha == default)
            {
                return BadRequest("Debe proporcionar una fecha y hora válida.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.User_id == login.User_id);
            if (!userExists)
            {
                return BadRequest($"El User_id {login.User_id} no existe en la tabla de usuarios.");
            }

            var ultimoMovimiento = await _context.Logins
                .Where(l => l.User_id == login.User_id)
                .OrderByDescending(l => l.fecha)
                .FirstOrDefaultAsync();

            if (ultimoMovimiento != null && ultimoMovimiento.TipoMov == login.TipoMov)
            {
                string tipo = login.TipoMov == 1 ? "Login" : "Logout";
                return BadRequest($"Error de secuencia: El usuario ya tiene un registro de {tipo} como último movimiento.");
            }

            _context.Logins.Add(login);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLogins), new { id = login.Id }, login);
        }

        /// Actualiza un registro existente.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogin(int id, Login login)
        {
            if (id != login.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la petición."); 
            }

            _context.Entry(login).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoginExists(id)) return NotFound();
                throw;
            }

            return NoContent(); 
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

            return NoContent();
        }

        private bool LoginExists(int id)
        {
            return _context.Logins.Any(e => e.Id == id);
        }
    }
}