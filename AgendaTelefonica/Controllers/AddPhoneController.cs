using AgendaTelefonica.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;

namespace AgendaTelefonica.Controllers
{
    public class AddPhoneController : Controller
    {
        public async Task<IActionResult> Index(int id)
        {
            Contact contact = await GetContact(id);
            return View(contact);
        }

        private async Task<Contact> GetContact(int id)
        {
            await Connection.Connect();

            string sql = "SELECT nome FROM contato WHERE id_contato = ? LIMIT 1;";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection);
            command.Parameters.Add("?", DbType.Int32).Value = id;

            object? result = await command.ExecuteScalarAsync();
            string name = result!.ToString()!;

            Contact contact = new Contact(id, name);

            await Connection.Close();

            return contact;
        }

        [HttpPost]
        public async Task<IActionResult> Insert(Contact contact)
        {
            string name = contact.Name;
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            if (!long.TryParse(name, out long res))
                return BadRequest();

            long value = res;
            await InsertIntoDB(value, contact.Id);

            return RedirectToAction("Info", "Home", contact);
        }

        private async Task InsertIntoDB(long value, int idContact)
        {
            await Connection.Connect();

            MySqlTransaction transaction = await Connection.MyConnection!.BeginTransactionAsync();
            int idPhone = await InsertPhone(value, transaction);

            await InsertPhoneIntoContact(transaction, idPhone, idContact);

            await transaction.CommitAsync();

            await Connection.Close();
        }

        private static async Task<int> InsertPhone(long value, MySqlTransaction transaction)
        {
            string sql = "" +
                            "INSERT INTO telefone(numero) VALUES (?);" +
                            "SELECT LAST_INSERT_ID();";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection, transaction);
            command.Parameters.Add("?", DbType.Int64).Value = value;

            object? result = await command.ExecuteScalarAsync();
            return int.Parse(result!.ToString()!);
        }

        private async Task InsertPhoneIntoContact(MySqlTransaction transaction, int idPhone, int idContact)
        {
            string sql = "INSERT INTO contato_tem_telefone VALUES (?,?);";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection, transaction);
            command.Parameters.Add("?", DbType.Int32).Value = idContact;
            command.Parameters.Add("?", DbType.Int32).Value = idPhone;

            await command.ExecuteNonQueryAsync();
        }

    }
}
