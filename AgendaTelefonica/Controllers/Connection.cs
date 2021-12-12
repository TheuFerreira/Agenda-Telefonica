using MySqlConnector;

namespace AgendaTelefonica.Controllers
{
    public static class Connection
    {
        private const string CONNECTION_STRING = "server=localhost;port=3306;database=agenda;username=root;";
        public static MySqlConnection? MyConnection { get; set; }

        public static async Task Connect()
        {
            try
            {
                if (MyConnection == null)
                    MyConnection = new MySqlConnection(CONNECTION_STRING);

                await MyConnection.OpenAsync();
            }
            catch (MySqlException e)
            {
                throw e;
            }
        }

        public static async Task Close()
        {
            try
            {
                await MyConnection!.CloseAsync();
            }
            catch (MySqlException e)
            {
                throw e;
            }
        }
    }
}
