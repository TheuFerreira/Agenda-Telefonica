using AgendaTelefonica.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;

namespace AgendaTelefonica.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index(string name = "")
        {
            List<Contact> contacts = await GetAll(name);
            return View(contacts);
        }

        private async Task<List<Contact>> GetAll(string name = "")
        {
            await Connection.Connect();

            string sql = "SELECT id_contato, nome FROM contato WHERE nome LIKE '%" + name + "%'";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection);

            List<Contact> contatos = new List<Contact>();
            MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            while (await reader.ReadAsync())
            {
                int idContact = await reader.GetFieldValueAsync<int>(0);
                string nameContact = await reader.GetFieldValueAsync<string>(1);

                Contact contato = new Contact(idContact, nameContact);
                contatos.Add(contato);
            }

            await Connection.Close();

            return contatos;
        }

        public async Task<IActionResult> Info(int id)
        {
            Contact contact = await GetContact(id);
            List<Phone> phones = await GetPhonesOfContact(id);

            contact.Phones = phones;

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

        private async Task<List<Phone>> GetPhonesOfContact(int id)
        {
            await Connection.Connect();

            string sql = "" +
                "SELECT t.id_telefone, t.numero " +
                "FROM contato_tem_telefone AS ct " +
                "INNER JOIN telefone AS t ON t.id_telefone = ct.id_telefone " +
                "WHERE ct.id_contato = ?; ";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection);
            command.Parameters.Add("?", DbType.Int32).Value = id;

            List<Phone> phones = new List<Phone>();
            MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            while (await reader.ReadAsync())
            {
                int idPhone = await reader.GetFieldValueAsync<int>(0);
                long number = await reader.GetFieldValueAsync<long>(1);

                Phone phone = new Phone(idPhone, number);
                phones.Add(phone);
            }

            await Connection.Close();
            return phones;
        }
   
        public IActionResult EditContact(Contact contact)
        {
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContact(Contact contact)
        {
            if (string.IsNullOrEmpty(contact.Name)) {
                return BadRequest();
            }

            await Connection.Connect();

            MySqlTransaction transaction = await Connection.MyConnection!.BeginTransactionAsync();

            string sql = "UPDATE contato SET nome = ? WHERE id_contato = ?;";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection, transaction);
            command.Parameters.Add("?", DbType.String).Value = contact.Name;
            command.Parameters.Add("?", DbType.Int32).Value = contact.Id;

            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();

            await Connection.Close();

            return RedirectToAction("Index", "Home", null);
        }

        public async Task<string> DeletePhone(int id)
        {
            await DeletePhoneById(id);

            return "Telefone excluido";
        }

        private async Task DeletePhoneById(int id)
        {
            await Connection.Connect();

            MySqlTransaction transaction = await Connection.MyConnection!.BeginTransactionAsync();
            await DeletePhoneFromContact(id, transaction);
            await DeletePhone(id, transaction);

            await transaction.CommitAsync();

            await Connection.Close();
        }

        private async Task DeletePhoneFromContact(int id, MySqlTransaction transaction)
        {
            string sql = "DELETE FROM contato_tem_telefone WHERE id_telefone = ?";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection, transaction);
            command.Parameters.Add("?", DbType.Int32).Value = id;

            await command.ExecuteNonQueryAsync();
        }

        private async Task DeletePhone(int id, MySqlTransaction transaction)
        {
            string sql = "DELETE FROM telefone WHERE id_telefone = ?;";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection, transaction);
            command.Parameters.Add("?", DbType.Int32).Value = id;

            await command.ExecuteNonQueryAsync();
        }

        public async Task<string> Delete(int id)
        {
            string answer = await DeleteContactFromDb(id);

            return answer;
        }

        private static async Task<string> DeleteContactFromDb(int id)
        {
            string answer;

            await Connection.Connect();

            MySqlTransaction transaction = await Connection.MyConnection!.BeginTransactionAsync();
            int count = await GetNumberPhonesOfContact(id, transaction);

            if (count > 0)
            {
                answer = "Este contato não pode ser excluído, pois, contém números associados a ele!";
            }
            else
            {
                await DeleteContact(id, transaction);
                answer = "Contato excluído!";
            }

            await transaction.CommitAsync();

            await Connection.Close();
            return answer;
        }

        private static async Task<int> GetNumberPhonesOfContact(int id, MySqlTransaction transaction)
        {
            string sql = "" +
                            "SELECT COUNT(id_telefone) " +
                            "FROM contato_tem_telefone " +
                            "WHERE id_contato = ?;";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection, transaction);
            command.Parameters.Add("?", DbType.Int32).Value = id;

            object? result = await command.ExecuteScalarAsync();
            int count = int.Parse(result!.ToString()!);
            return count;
        }

        private static async Task DeleteContact(int id, MySqlTransaction transaction)
        {
            string sql = "DELETE FROM contato WHERE id_contato = ?;";
            MySqlCommand command = new MySqlCommand(sql, Connection.MyConnection, transaction);
            command.Parameters.Add("?", DbType.Int32).Value = id;

            await command.ExecuteNonQueryAsync();
        }

    }
}