using AgendaTelefonica.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;

namespace AgendaTelefonica.Controllers
{
    public class AddContact : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Insert(Contact contact)
        {
            if (string.IsNullOrEmpty(contact.Name))
                return BadRequest();

            await Connection.Connect();

            MySqlTransaction transaction = await Connection.MyConnection!.BeginTransactionAsync();

            string sql = "INSERT INTO contato (nome) VALUES (?)";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection, transaction);
            command.Parameters.Add("?", DbType.String).Value = contact.Name;

            await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            await Connection.Close();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Cancel()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
