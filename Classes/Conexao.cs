using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Classes
{
    public class Conexao
    {
        public OracleCommand OraComando { get; private set; }
        public OracleTransaction OraTransactiondll { get; set; }

        public OracleConnection OraConn { get; private set; }

        public bool Transaction { get; private set; }
        public bool MostraErro { get; set; }


        public Conexao(string strDsn, string strUsuario, string strSenha)
        {
            OraComando = new OracleCommand();

            OraConn = new OracleConnection();

            string strConexao = "data source=" + strDsn + ";user ID=RJ" + double.Parse(strUsuario).ToString().PadLeft(10, '0') +
                                ";password = " + strSenha + "; Pooling = false;";
            OraConn.ConnectionString = strConexao;

            //Indica se a Transaction foi feita para as operações de Insert / Update
            Transaction = false;
        }

        public OracleConnection Conectar()
        {
            try
            {
                if (OraConn.State.Equals(ConnectionState.Closed))
                {
                    OraConn.Open();
                }
            }
            catch (OracleException ex)
            {
                string errorMessage = "Código : " + ex.ErrorCode + "\n" + "Mensagem: " + ex.Message;
                ManipulaErro.MostraErro(errorMessage, ex.ErrorCode, ex.Message, MostraErro);
            }
            return OraConn;
        }

        public void Desconectar()
        {
            if (OraConn.State == System.Data.ConnectionState.Open)
            {
                OraConn.Close();
            }
        }

        public OracleTransaction IniciaTransacao()
        {
            if (OraConn.State.Equals(ConnectionState.Open))
            {
                if (!Transaction)
                {
                    Transaction = true;
                    return OraConn.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                else
                    return null;
            }
            else
            {
                return null;
            }
        }
    }

}
