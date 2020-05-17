using Oracle.ManagedDataAccess.Client;
using System.Data;
using Enum;


namespace Classes
{
    public static class FunctionSql
    {
        /*  public static OracleDataReader Consulta(string strSql)
          {
              CnConexaoDll conexao = new CnConexaoDll();
              OracleCommand cmd = new OracleCommand();
              OracleDataReader readerResult = null;

              try
              {
                  cmd.CommandText = strSql;
                  cmd.Connection = ;
                  readerResult = cmd.ExecuteReader();
                  return readerResult;
              }
              catch (OracleException ex)
              {                string errorMessage = "Código : " + ex.ErrorCode + "\n" + "Mensagem: " + ex.Message;
                  return readerResult;
              }
          } */

        public static RetSqlFunction SqlError(OracleException Erro, bool blHideMsg = false)
        {
            string strMsg;
            RetSqlFunction SqlError = RetSqlFunction.retSQLEmpty;

            switch (Erro.Errors.GetHashCode())
            {
                case 12154:
                case 12224:
                    strMsg = "Falha na Conexão !";
                    SqlError = RetSqlFunction.retSQLErrorReconnect;
                    break;
                case 1017:
                    strMsg = "Usuário/Senha inválida !";
                    SqlError = RetSqlFunction.retInvalidUserPwd;
                    break;
                case 1031:
                    strMsg = "Privilégios insuficientes !";
                    SqlError = RetSqlFunction.retSQLError;
                    break;
                case 942:
                    strMsg = "Tabela inexistente ou privilégios insuficientes !";
                    SqlError = RetSqlFunction.retSQLError;
                    break;

                case 2627:
                case 1:
                    strMsg = "Registro já cadastrado !";
                    SqlError = RetSqlFunction.retSQLDuplicatePK;
                    break;
                case 547:
                case 2292:
                    strMsg = "Registro não pode ser excluído! Existe alguma referência para a informação.";
                    SqlError = RetSqlFunction.retSQLError;
                    break;
                case 54:
                    strMsg = "Registro já selecionado por outro usuário !";
                    SqlError = RetSqlFunction.retSQLErrorLocked;
                    break;
                case 3113:
                case 3114:
                case 3146:
                case 12571:
                case 12570:
                case 3151:
                    strMsg = "Não foi possível completar sua solicitação. Tente novamente !";
                    /*If TransactionInProgress Then
                        HidestrMsg = False
                    Else
                        HidestrMsg = True
                    End If*/
                    SqlError = RetSqlFunction.retSQLErrorReconnect;
                    break;
                case 1013:  //'TimeOut
                    strMsg = "Não foi possível completar sua solicitação. Tente novamente !";
                    SqlError = RetSqlFunction.retSQLErrorTimeOut;
                    break;
                case 1631:
                    strMsg = "Número máximo de registro na Tabela excedido!";
                    SqlError = RetSqlFunction.retSQLError;
                    break;
                case 2396:
                    //exceeded maximum idle time, please connect again
                    strMsg = "Não foi possível completar sua solicitação. Tente novamente !";
                    /*If TransactionInProgress Then
                        HidestrMsg = False
                    Else
                        HidestrMsg = True
                    End If*/
                    SqlError = RetSqlFunction.retSQLErrorReconnect;
                    break;
                case 28:  //'Sessão cancelada
                    strMsg = "Não foi possível completar sua solicitação. Tente novamente !";
                    /*If TransactionInProgress Then
                        HidestrMsg = False
                    Else
                        HidestrMsg = True
                    End If*/
                    SqlError = RetSqlFunction.retSQLErrorReconnect;
                    break;
                default:
                    strMsg = Erro.Message;
                    SqlError = RetSqlFunction.retSQLError;
                    break;
            }

            if (!blHideMsg && !strMsg.Equals(""))
            {
                ManipulaErro.MostraErro(strMsg, Erro.ErrorCode, "", true);
            }
            else if (!blHideMsg)
            {
                if (Erro.Errors.GetHashCode().Equals(40071))
                {
                    SqlError = RetSqlFunction.retSQLError;
                    strMsg = "Não foi possível completar sua conexão com a base do Detran. \n" +
                        "As operações remotas não serão realizadas!";
                    SqlError = RetSqlFunction.retSQLError;
                    ManipulaErro.MostraErro(strMsg, Erro.ErrorCode, "", true);
                    return SqlError;
                }

                ManipulaErro.MostraErro(Erro.Message, Erro.ErrorCode, "", true);
                SqlError = RetSqlFunction.retVBError;
            }

            return SqlError;

        }

        /*2020.03.26 - Rogerio Itoh : Sinplificando a função AbreResult*/
        public static (Enum.RetSqlFunction, OracleDataReader rdrResultado) AbreResult(string strSql, OracleConnection cnConexao, bool HideMsgErro = true)
        {
            Enum.RetSqlFunction iResultado = Enum.RetSqlFunction.retSQLEmpty;
            OracleCommand cmd = new OracleCommand();
            OracleDataReader rdrResultado = null;

            try
            {
                cmd.CommandText = strSql;
                cmd.Connection = cnConexao;
                rdrResultado = cmd.ExecuteReader();
                if (rdrResultado.HasRows)
                {
                    iResultado = RetSqlFunction.retSQLOk;
                }
            }
            catch (OracleException ex)
            {
                ManipulaErro.MostraErro("AbreResult(): ", ex.GetHashCode(), ex.Message, HideMsgErro);
                ManipulaErro.LogarMensagem("AbreResult(): ", ex.GetHashCode(), ex.Message);
                iResultado = RetSqlFunction.retSQLError;

            }
            return (iResultado, rdrResultado);
        }

        public static (Enum.RetSqlFunction, DataSet ResultDataSet) AbreResultAdapter(string strSql, OracleConnection cnConexao, bool HideMsgErro = true)
        {
            RetSqlFunction iResultado = RetSqlFunction.retSQLEmpty;
            OracleDataAdapter ResultAdapter = new OracleDataAdapter();
            DataSet ResultDataSet = new DataSet("ResultadoAdapter");

            try
            {
                ResultAdapter.SelectCommand = new OracleCommand(strSql, cnConexao);
                ResultAdapter.Fill(ResultDataSet, "ResultadoAdapter");
                if (ResultDataSet.Tables["ResultadoAdapter"].Rows.Count > 0)
                {
                    iResultado = Enum.RetSqlFunction.retSQLOk;
                }
                ResultAdapter.Dispose();
            }
            catch (OracleException ex)
            {
                ManipulaErro.MostraErro("AbreResultAdapter(): ", ex.GetHashCode(), ex.Message, HideMsgErro);
                ManipulaErro.LogarMensagem("AbreResultAdapter(): ", ex.GetHashCode(), ex.Message);
                iResultado = RetSqlFunction.retSQLError;

            }
            return (iResultado, ResultDataSet);
        }



        public static int ExecutaCount(string strSqlCount, OracleConnection cnConexao)
        {
            OracleCommand cmd = new OracleCommand();
            OracleDataReader rdrCount;
            int iTotal = 0;
            try
            {
                cmd.CommandText = strSqlCount;
                //cmd.Connection = cnConexao.Conectar();                
                cmd.Connection = cnConexao;
                rdrCount = cmd.ExecuteReader();
                if (rdrCount.HasRows)
                {
                    if (rdrCount.Read())
                        iTotal = rdrCount.GetInt16(0);
                }

                rdrCount.Close();
            }
            catch (OracleException ex)
            {
                ManipulaErro.MostraErro("ExecutaCount(): ", ex.GetHashCode(), ex.Message, false);
                iTotal = -1;
            }
            return iTotal;
        }

    }
}
