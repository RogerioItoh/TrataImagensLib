using Classes;
using Enum;
using Constantes;
using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Reflection;
using System.IO;
using System.Data;
using System.Drawing;

namespace TrataImagensLib
{
    
    public class TrataImagens
    {
        public string DiretorioImagens { get; set; }
        public bool HideMsgBox { get; set; }
        public List<string> NomeDoArquivo { get; private set; }
        public List<double> TamanhoDoArquivo { get; private set; }
        public int AnoAtual { get; private set; }
        public bool ConectadoGravacao { get; private set; }
        public bool ConectadoLeitura { get; private set; }
        public double IdCorpo { get; private set; }
        public bool DeletaArquivo { get; set; }
        private int iTotalImagens;
        public Conexao cnConexaoDll;

        public TrataImagens( string strConexao )
        {
            IniFile ini = new IniFile("TrataImagens.ini");
            DiretorioImagens = ini.IniReadString("Arquivos de Imagens", "Diretorio", "Default");

            if (DiretorioImagens.Equals("Default"))
            {
                Imagens.GeraArquivoIni();
                DiretorioImagens = ini.IniReadString("Arquivos de Imagens", "Diretorio", "Default");
            }

            Imagens.CriaPath(DiretorioImagens);
            NomeDoArquivo = new List<String>();
            TamanhoDoArquivo = new List<double>();

            ConectadoLeitura = false;
            ConectadoGravacao = false;
            HideMsgBox = true;
            DeletaArquivo = false;

            /* Objeto de conexão para ser utilizada nas funções internas da dll (privadas) */
            cnConexaoDll = new Conexao(strConexao);            
            cnConexaoDll.MostraErro = HideMsgBox;
        }

       /* public Enum.RetornoTrataImagem EliminaArquivo(string sArquivo)
        {
            try
            {
                if (File.Exists(sArquivo))
                {
                    File.Delete(sArquivo);
                    return Enum.RetornoTrataImagem.Rok;
                }
            }
            catch (IOException e)
            {

                return Enum.RetornoTrataImagem.RArquivoNaoApagado;
            }

            return Enum.RetornoTrataImagem.RArquivoNaoApagado;
        }*/


        public RetornoTrataImagem ConsultaPedido(double dblPedido, TipoDeImagem eTipoDeImagem, ref OracleConnection cnConexaoBiografica, bool blApenasConsulta = false)
        {
            RetornoTrataImagem resultadoConsultaPedido = Enum.RetornoTrataImagem.RFalhaConexaoBiografica;
            try
            {
                RetSqlFunction rAbreImagem;
                double dblTamanhoGravado;
                string strExtensaoDefault, strSql,
                       strSqlCount, strWhere;
                string strNomeDoCampo, strTipoDeArquivo,
                       strArquivo;
                byte btFaltaArquivo, btFaltaImagem;
                OracleDataReader rdrResult;
                int iResultadoCount;

                strSqlCount = "Select Count(*) from Pid.Corpo_Pedido ";
                strWhere = "Where Nu_Pid = " + dblPedido;

                strSql = "";
                if (eTipoDeImagem.Equals(TipoDeImagem.Todas))
                {
                    iTotalImagens = 12;

                    if (blApenasConsulta)
                    {
                        strSql = Constante.ConsultaImagemTodos;
                    }
                    else
                    {
                        strSql = Constante.SqlTodos;

                    }
                    strSql = strSql + ", Nu_Pid ";
                    strSql = strSql + "From PID.Corpo_Pedido ";

                }
                else if (eTipoDeImagem.Equals(TipoDeImagem.Dedos))
                {
                    iTotalImagens = 10;
                    if (blApenasConsulta)
                    {
                        strSql = Constante.ConsultaImagemDedos;
                    }
                    else
                    {

                        strSql = Constante.SqlDedos;
                    }

                    strSql = strSql + ", Nu_Pid ";
                    strSql = strSql + "From PID.Corpo_Pedido ";
                }
                else
                {
                    var ResultadoNome_Campo = Imagens.Nome_Campo(eTipoDeImagem);

                    if (ResultadoNome_Campo.Item1.Equals("") && ResultadoNome_Campo.Item2.Equals(""))
                    {
                        return Enum.RetornoTrataImagem.RTipodeImagemInvalido;
                    }

                    strNomeDoCampo = ResultadoNome_Campo.Item1;
                    strTipoDeArquivo = ResultadoNome_Campo.Item2;
                    strSql = "";

                    if (blApenasConsulta)
                    {
                        strSql = "Select BMS_LOB.GetLength(" + strNomeDoCampo + ")," + strTipoDeArquivo + ", ";
                    }
                    else
                    {
                        strSql = "Select " + strNomeDoCampo + ", " + strTipoDeArquivo + ", ";
                    }
                    strSql = strSql + "Nu_Pid ";
                    strSql = strSql + "From   PID.Corpo_Pedido ";
                }

                strSqlCount = strSqlCount + strWhere;
                strSql = strSql + strWhere;

                iResultadoCount = FunctionSql.ExecutaCount(strSqlCount, cnConexaoBiografica);
                if (iResultadoCount > 1)
                {
                    return RetornoTrataImagem.RMaisdeUmPIDcomImagem;
                }
                else if (iResultadoCount.Equals(0))
                {
                    return RetornoTrataImagem.RFaltaImagem;
                }
                else if (iResultadoCount.Equals(-1))
                {
                    return RetornoTrataImagem.RFalhaConexaoBiografica;
                }

                /* a funcão AbreResult retorna 2 valores
                 * Item1 = Status da execução da query
                 * Item2 = Os registros do resultado da query
                 */

                var ResultadoAbresult = FunctionSql.AbreResult(strSql, cnConexaoBiografica);
                rAbreImagem = ResultadoAbresult.Item1;
                rdrResult = ResultadoAbresult.Item2;

                if (rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                {
                    Imagens.LimpaArray();

                    while (rdrResult.Read())
                    {
                        if (!eTipoDeImagem.Equals(TipoDeImagem.Todas) && !eTipoDeImagem.Equals(TipoDeImagem.Dedos))
                        {
                            if (rdrResult.IsDBNull(0))
                            {
                                return Enum.RetornoTrataImagem.RFaltaImagem;
                            }
                            else if (blApenasConsulta)
                            {
                                double gTamanhoArquivo = rdrResult.GetDouble(0);
                                rdrResult.Dispose();
                                rdrResult.Close();
                                return RetornoTrataImagem.Rok;
                            }

                            strArquivo = DiretorioImagens + dblPedido.ToString().PadLeft(12, '0');
                            strArquivo = strArquivo + (eTipoDeImagem.GetHashCode().ToString().PadLeft(2, '0'));
                            strArquivo = strArquivo + Imagens.Obtem_Extensao_de_Arquivo(rdrResult.GetInt32(1));
                            // Insere a propriedade com o nome do arquivo;

                            dblTamanhoGravado = Imagens.GravaArquivoImagem(strArquivo, (byte[])rdrResult.GetValue(0));
                            if (dblTamanhoGravado.Equals(0))
                            {
                                rdrResult.Close();
                                return RetornoTrataImagem.RFaltaImagem;
                            }
                            else
                            {
                                Imagens.g_TamanhoDoArquivo[0] = dblTamanhoGravado;
                                NomeDoArquivo.Add(strArquivo);
                                TamanhoDoArquivo.Add(dblTamanhoGravado);
                                resultadoConsultaPedido = RetornoTrataImagem.Rok;
                            }
                        }
                        else
                        {
                            btFaltaArquivo = 0;
                            btFaltaImagem = 0;
                            if (blApenasConsulta)
                            {
                                for (int iContador = 1; iContador <= iTotalImagens; iContador++)
                                {
                                    if (rdrResult.IsDBNull(iContador - 1))
                                    {
                                        btFaltaImagem += 1;
                                    }
                                    else
                                    {
                                        Imagens.g_TamanhoDoArquivo[iContador - 1] = rdrResult.GetDouble(iContador - 1);
                                    }
                                }

                                if (btFaltaImagem > 0)
                                {
                                    rdrResult.Close();
                                    return RetornoTrataImagem.RFaltaImagem;
                                }
                                else
                                {
                                    rdrResult.Close();
                                    return RetornoTrataImagem.Rok;
                                }
                            }

                            for (int iContador = 1; iContador <= iTotalImagens; iContador++)
                            {
                                if (rdrResult.IsDBNull(iContador + iTotalImagens - 1))
                                {
                                    strExtensaoDefault = "Jpg";
                                }
                                else
                                {
                                    strExtensaoDefault = Imagens.Obtem_Extensao_de_Arquivo(rdrResult.GetInt16(iContador + iTotalImagens - 1));
                                }

                                strArquivo = DiretorioImagens + dblPedido.ToString().PadLeft(12, '0');
                                strArquivo = strArquivo + iContador.ToString().PadLeft(2, '0');
                                strArquivo = strArquivo + strExtensaoDefault;

                                // Criar função que gera um arquivo e grava na pasta 
                                dblTamanhoGravado = Imagens.GravaArquivoImagem(strArquivo, (byte[])rdrResult.GetValue(iContador - 1));
                                Imagens.g_TamanhoDoArquivo[iContador - 1] = dblTamanhoGravado;
                                if (dblTamanhoGravado < 1)
                                {
                                    btFaltaArquivo += 1;  //btFaltaImagem;
                                }
                                else
                                {
                                    Imagens.g_NomeDoArquivo[iContador - 1] = strArquivo;
                                    // Insere as propriedades Nome do arquivo e Tamanho do Arquivo ;
                                    NomeDoArquivo.Add(strArquivo);
                                    TamanhoDoArquivo.Add(dblTamanhoGravado);
                                }
                            }

                            if (btFaltaArquivo.Equals(0))
                            {
                                resultadoConsultaPedido = RetornoTrataImagem.Rok;
                            }
                            else if (btFaltaArquivo.Equals(iTotalImagens))
                            {
                                resultadoConsultaPedido = RetornoTrataImagem.RFalhaGravacao;
                            }
                            else
                            {
                                resultadoConsultaPedido = RetornoTrataImagem.RFaltamArquivos;
                            }
                        }
                    }

                }
                else if (rAbreImagem.Equals(RetSqlFunction.retSQLError))
                {
                    rdrResult.Close();
                    rdrResult.Dispose();
                    resultadoConsultaPedido = RetornoTrataImagem.RFalhaConexaoBiografica;
                }

            }
            catch (OracleException ex)
            {
                string errorMessage = "Código : " + ex.ErrorCode + "\n" + "Mensagem: " + ex.Message;
                resultadoConsultaPedido = RetornoTrataImagem.RFalhaGravacao;
            }
            return resultadoConsultaPedido;
        }


        private string TabelaImagemCorpos(double dNuPid, double dIDCorpo, bool bConsulta)
        {
            OracleDataReader reader = null;

            try
            {
                string strSQL;
                Enum.RetSqlFunction retSQL;

                if (!bConsulta)
                {
                    return string.Format("IMAGEM.CORPOS_{0}", AnoAtual);
                }
                else
                {
                    strSQL = "SELECT NU_ANO FROM IMAGEM.CORPOS_INDEX WHERE ";
                    if (dIDCorpo > 0)
                    {
                        strSQL = strSQL + " ID_CORPO = " + dIDCorpo.ToString();
                    }
                    else
                    {
                        strSQL = strSQL + " NU_PID = " + dNuPid.ToString();
                    }

                    strSQL = strSQL + " ORDER BY NU_ANO DESC, ID_CORPO DESC ";


                    /*2019.11.27 - Rogerio Itoh */
                    /* AbreResult(OracleDataReader rdoResultset, string strSQL,
                                    ref object qyQuery, bool blnLock = true,
                                    bool OnErrorHideMsg = true, object cnRef = null)*/
                    /* Ordem dos parâmetros a serem passados na função 
                       Para não dar o erro tive que inicializar o parâmeto objRef                       
                     */


                    //object objRef = null;
                    //retSQL = conexao.AbreResult(reader, strSQL, ref objRef  ,false, true, cnConexaoDll);


                    var RetornaValorAbreResult = FunctionSql.AbreResult(strSQL, cnConexaoDll.Conectar());
                    retSQL = RetornaValorAbreResult.Item1;
                    reader = RetornaValorAbreResult.Item2;

                    /*a variável m_iAnoAtual é inicializado nas funçõoes:
                    ConectaParaLerIdentificado
                    ConectaParaGravarIdentificado*/

                    if (retSQL.Equals(RetSqlFunction.retSQLOk))
                    {
                        if (reader.HasRows)
                        {
                            if (reader.Read())
                                AnoAtual = int.Parse(reader["NU_ANO"].ToString());
                        }

                        reader.Close();
                        return "IMAGEM.CORPOS_" + string.Format("{0000}", AnoAtual);
                    }
                    else if (retSQL == Enum.RetSqlFunction.retSQLEmpty)
                    {
                        reader.Close();
                        return "IMAGEM.CORPOS_" + string.Format("{0000}", AnoAtual);

                    }
                }

                return "IMAGEM.CORPOS_" + (AnoAtual, "0000");

            }
            catch (OracleException ex)
            {
                reader.Dispose();
                string errorMessage = string.Format(" TabelaImagemCorpos(): Código : {0} \n Mensagem : {1}", ex.ErrorCode, ex.Message);
                ManipulaErro.MsgBox(0, errorMessage, "Alerta", 0);
                return "IMAGEM.CORPOS";
            }
        }


        // Junção das funções ConectaParaLerIdenticado e ConectaParaGravarIdentificado : o parâmetro strOperação 
        // deve ser em maiusculo = "GRAVAR" ou "LER"
        public RetornoTrataImagem ConectaParaLerOuGravarIdentificado(double dblUserName, string strOperacao, ref OracleConnection cnConexaoBiografica)
        {
            int iAno = 0;
            OracleDataReader rdrBiografico = null;
            string strSql = "";

            try
            {
                Imagens.LimpaArray();
                if (strOperacao.ToUpper().Equals("GRAVAR"))
                {
                    if (ConectadoGravacao)
                    {
                        return Enum.RetornoTrataImagem.Rok;
                    }
                    else
                    {
                        ConectadoGravacao = false;
                    }

                    /* Verifica Privilégio para Rodar Atualizador de Dados */
                    strSql = string.Format("Select PD.Nu_Direito From Detran.PerfilxDireitos PD, " +
                             "Detran.Perfil PE, Detran.Operador_Perfil OP, " +
                             "Detran.Operadores O Where  PD.Nu_Direito = 171 " +
                             "And PD.Nu_Perfil = PE.Nu_Perfil And PE.Nu_Perfil = OP.Nu_Perfil " +
                             "And OP.Nu_RicOper = O.Nu_RicOper And O.Nu_RicOper = {0}", dblUserName.ToString());
                }
                else
                {
                    if (ConectadoLeitura)
                    {
                        return Enum.RetornoTrataImagem.Rok;
                    }
                    else
                    {
                        ConectadoLeitura = false;
                    }
                    strSql = string.Format("Select op.nu_perfil From  Detran.Operador_Perfil OP, " +
                             "Detran.Operadores O Where op.nu_perfil in (50,52) " +
                             "and O.Nu_RicOper  = {0} and OP.Nu_RicOper = O.Nu_RicOper ", dblUserName.ToString());
                }

                var ResultadoAbreResult = FunctionSql.AbreResult(strSql, cnConexaoBiografica);
                rdrBiografico = ResultadoAbreResult.Item2;

                if (!ResultadoAbreResult.Item1.Equals(RetSqlFunction.retSQLOk))
                {
                    rdrBiografico.Close();
                    rdrBiografico.Dispose();
                    return Enum.RetornoTrataImagem.RSemPrivilegio;
                }

                ///* 2020.04.01 - Rogerio Itoh : Alterei a query para trazer somente o Ano */
                //strSql = "Select SysDate From Dual";                                

                strSql = "Select Extract(Year from SysDate) as Ano From Dual";
                ResultadoAbreResult = FunctionSql.AbreResult(strSql, cnConexaoBiografica);

                if (!ResultadoAbreResult.Item1.Equals(RetSqlFunction.retSQLOk))
                {
                    return Enum.RetornoTrataImagem.RFalhaConexaoBiografica;
                }
                else
                {
                    rdrBiografico = ResultadoAbreResult.Item2;
                    if (rdrBiografico.Read())
                    {
                        iAno = int.Parse(rdrBiografico[0].ToString());
                    }

                    AnoAtual = iAno;

                    if (strOperacao.Equals("GRAVAR"))
                        ConectadoGravacao = true;
                    else
                        ConectadoLeitura = true;

                    rdrBiografico.Close();
                    rdrBiografico.Dispose();
                    return Enum.RetornoTrataImagem.Rok;
                }

            }
            catch (OracleException ex)
            {
                string errorMessage = string.Format("Código : {0} \n Mensagem : {1}", ex.ErrorCode, ex.Message);
                ManipulaErro.MsgBox(0, errorMessage, "Alerta", 0);
                // LogErrorEx "ConectaParaLerIdentificado", Err                
                rdrBiografico.Close();
                rdrBiografico.Dispose();
                return Enum.RetornoTrataImagem.RFalhaConexaoBiografica;
            }
        }



        /* Recebe Número do Pedido , Nome do Arquivo , Tipo de Imagem (Wsq ou JPG), idCorpo) */
        public RetornoTrataImagem GravaIdentificado(double dblPedido, ref string[] lstArquivo, TipoDeImagem bTipoDeImagem, ref double dblIdCorpo)
        {
            bool blArquivoApagado;
            RetSqlFunction rAbreImagem;
            double dblTamanhoGravado, dblId;
            string strNomeDoCampo = "", strRic = "";
            string strVias = "";

            string strTipoDeImagem = "";

            OracleDataReader RsIdentificados;
            string strSQL = "";

            //var RetornoLe_Arquivo_Imagem;     //2020.04.06 - Rogerio Itoh : Valores retornados da função
            List<byte[]> lstImagemConvertida; //2020.04.06 - Rogerio Itoh : lista dos arquivos convertidos para byte
                                              //var RetornoAlocaIdCorpo = ( 0 , 0); //2020.04.06 - Rogerio Itoh : Valores retornados da função

            string sTabelaCorpo;//2020.04.06 - Rogerio Itoh : Valores retornados da função


            try
            {
                /* Limpa propriedades da classe */
                NomeDoArquivo.Clear();
                TamanhoDoArquivo.Clear();
                IdCorpo = 0;

                if (!ConectadoGravacao)
                {
                    return RetornoTrataImagem.RFalhaConexaoIdentificados;
                }

                if (bTipoDeImagem.Equals(TipoDeImagem.Sinal) || bTipoDeImagem.Equals(TipoDeImagem.Dedos))
                {
                    return RetornoTrataImagem.RTipodeImagemInvalido;
                }

                //Traz a tabela Imagem.Corpos do pedido enviado no parâmetro

                if (bTipoDeImagem.Equals(TipoDeImagem.Todas))
                {
                    // Verifica se os arquivos a serem lidos estão OK
                    if (lstArquivo.Length < 11)
                    {
                        return RetornoTrataImagem.RFaltaImagem;
                    }

                    for (int iContador = 0; iContador <= lstArquivo.Length - 1; iContador++)
                    {
                        /* Rogerio Itoh - Verifica se tem algum arquivo que não foi preenchido */
                        var vlArray = lstArquivo[iContador];
                        if (vlArray == null)
                            return RetornoTrataImagem.RFaltaImagem;
                    }
                    sTabelaCorpo = TabelaImagemCorpos(dblPedido, 0, false);
                    strSQL = "";
                    strSQL = Constante.ImagemTodos;
                    strSQL = strSQL + string.Format(", Id_Corpo, Nu_Pid," +
                             "Nu_Ric, Nu_Vias, Nu_Ano from {0} Where Nu_Pid = {1}", sTabelaCorpo, dblPedido);
                }
                else
                {
                    // Redimensiona o Array com a lista de arquivos
                    Array.Resize(ref lstArquivo, 1);
                    for (int iContador = 0; iContador <= lstArquivo.Length - 1; iContador++)
                    {
                        if (iContador.Equals(0))
                        {
                            if (lstArquivo[iContador].Length.Equals(0))
                                return RetornoTrataImagem.RFaltaImagem;
                        }
                        else
                        {
                            var vlArray = lstArquivo[iContador];
                            if (vlArray != null)
                                return RetornoTrataImagem.RMaisdeUmArquivo;
                        }
                    }

                    /*  (string, string) Nome_Campo(TipoDeImagem btTipoDeImagem) a função nome campo retorna 
                     *  dois valores string                      
                       strNomeDoCampo = item 1 
                       strTipoDeImagem = item 2 */

                    var RetornoNome_Campo = Imagens.Nome_Campo(bTipoDeImagem);
                    if (RetornoNome_Campo.Item1.Equals("") && RetornoNome_Campo.Item2.Equals(""))
                    {
                        return RetornoTrataImagem.RTipodeImagemInvalido;
                    }

                    sTabelaCorpo = TabelaImagemCorpos(dblPedido, 0, false);
                    strNomeDoCampo = RetornoNome_Campo.Item1;
                    strTipoDeImagem = RetornoNome_Campo.Item2;

                    strSQL = string.Format("Select ID_Corpo, Nu_Pid, {0} , " +
                                          "Nu_RIC, NU_VIAS, NU_ANO From {1} " +
                                          "Where Nu_Pid = {2}", strNomeDoCampo, sTabelaCorpo, dblPedido.ToString());
                }
                dblId = 0;
                blArquivoApagado = true;

                /* Retorna o status da query  e o set de registros */
                var ResultadoAbreResult = FunctionSql.AbreResult(strSQL, cnConexaoDll.Conectar());

                rAbreImagem = ResultadoAbreResult.Item1;
                RsIdentificados = ResultadoAbreResult.Item2;

                if (rAbreImagem.Equals(RetSqlFunction.retSQLEmpty) || rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                {
                    Imagens.LimpaArray();
                    if (bTipoDeImagem.Equals(TipoDeImagem.Todas))
                    {
                        blArquivoApagado = true;
                        /* 2020.04.06 - Converte os arquivos Wsq e JPG para byte, 
                         a conversão é feita em todos os arquivos, caso dê erro em 
                         na conversão de um arquivo o função retorna com o valor 
                         da dblTamanhoGravado = 0.
                        */

                        // Retorna o somatório do tamanho dos arquivos.(Item 1)
                        // Uma lista com os arquivos convertidos.(Item 2)

                        var RetornoLe_Arquivo_Imagem = Imagens.Le_Arquivo_Imagem(lstArquivo);

                        //g_TamanhoDoArquivo(IContador)  = dTamanhoArquivo
                        //--- 2020.04.05 - Ver o q fazer com essa variável 
                        // ele era preenchida qdo ia um arquivo por vez


                        // na versão anterior a rotina mandava um arquivo por vez
                        //dblTamanhoGravado = RetornoLe_Arquivo_Imagem.Item1;

                        dblTamanhoGravado = RetornoLe_Arquivo_Imagem.Item1;
                        lstImagemConvertida = RetornoLe_Arquivo_Imagem.Item2;

                        if (dblTamanhoGravado < 1)
                        {
                            RsIdentificados.Close();
                            return RetornoTrataImagem.RFalhaConexaoIdentificados;
                        }

                        var RetornoAlocaIdCorpo = AlocaIdCorpo(false);
                        dblId = RetornoAlocaIdCorpo.Item2;
                        dblIdCorpo = dblId;

                        if (RetornoAlocaIdCorpo.Item1.Equals(RetornoTrataImagem.Rok))
                        {
                            //Procura NuRic e NuVias
                            strRic = "";
                            strVias = "";
                            RecuperaRic_Vias(dblPedido, ref strRic, ref strVias);
                            if (strRic.Equals("") && strVias.Equals(""))
                            {
                                return RetornoTrataImagem.RFalhaGravacao;
                            }
                            RetSqlFunction eIncluiIdentificado;
                            eIncluiIdentificado = GravaImagemCorpos(sTabelaCorpo, "", dblId, AnoAtual, strRic, dblPedido, strVias, lstImagemConvertida);
                            if (eIncluiIdentificado.Equals(RetSqlFunction.retSQLOk))
                            {
                                if (blArquivoApagado)
                                    return RetornoTrataImagem.Rok;
                                else
                                    return RetornoTrataImagem.ROkArquivoNaoApagado;
                            }
                            else
                            {
                                return RetornoTrataImagem.RFalhaGravacao;
                            }
                        }
                    }
                    else
                    {
                        var RetornoLe_Arquivo_Imagem = Imagens.Le_Arquivo_Imagem(lstArquivo);
                        dblTamanhoGravado = RetornoLe_Arquivo_Imagem.Item1;
                        lstImagemConvertida = RetornoLe_Arquivo_Imagem.Item2;
                        if (dblTamanhoGravado < 1)
                        {
                            RsIdentificados.Close();
                            RsIdentificados.Dispose();
                            return RetornoTrataImagem.RFalhaConexaoIdentificados;
                        }
                        else
                        {
                            var RetornoAlocaIdCorpo = AlocaIdCorpo(false);
                            dblId = RetornoAlocaIdCorpo.Item2;
                            if (RetornoAlocaIdCorpo.Item1.Equals(RetornoTrataImagem.Rok))
                            {
                                dblIdCorpo = dblId;

                                //Procura NuRic e NuVias
                                strRic = "";
                                strVias = "";

                                RecuperaRic_Vias(dblPedido, ref strRic, ref strVias);
                                if (strRic.Equals("") && strVias.Equals(""))
                                {

                                    return RetornoTrataImagem.RFalhaGravacao;
                                }
                                RetSqlFunction eIncluiIdentificado;
                                eIncluiIdentificado = GravaImagemCorpos(sTabelaCorpo, strNomeDoCampo, dblId, AnoAtual, strRic, dblPedido, strVias, lstImagemConvertida);

                                if (eIncluiIdentificado.Equals(RetSqlFunction.retSQLOk))
                                {
                                    if (blArquivoApagado)
                                        return RetornoTrataImagem.Rok;
                                    else
                                        return RetornoTrataImagem.ROkArquivoNaoApagado;
                                }
                                else
                                {
                                    return RetornoTrataImagem.RFalhaGravacao;
                                }
                            }
                            else
                            {
                                RsIdentificados.Close();
                                RsIdentificados.Dispose();
                                return RetornoTrataImagem.RFalhaConexaoIdentificados;
                            }
                        }
                    }
                }
                else
                {
                    return RetornoTrataImagem.RFalhaConexaoIdentificados;
                }
            }
            catch (Exception ex)
            {
                ManipulaErro.MostraErro("GravaIdentificado(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
                //ManipulaErro.GravaEventLog("GravaIdentificado : " + dblPedido.ToString() + " - " + ex.Message, ex.GetHashCode());
                return RetornoTrataImagem.RFalhaGravacao;
            }
            return RetornoTrataImagem.RFalhaConexaoIdentificados;
        }

        private void RecuperaRic_Vias(double dblPedido, ref string strRic, ref string strVias)
        {
            string strSQL;
            OracleDataReader rdrRicVias;
            int iTpPedido;

            strSQL = string.Format("SELECT NU_RIC, NU_VIAS FROM CIVIL.IDENTIFICACOES " +
                                                          "WHERE NU_PID = {0}", dblPedido.ToString());

            rdrRicVias = FunctionSql.AbreResult(strSQL, cnConexaoDll.Conectar()).Item2;
            if (!rdrRicVias.HasRows)
            {
                rdrRicVias.Close();
                // Não existem vias na CIVIL.IDENTIDICACOES
                strSQL = string.Format("SELECT NU_RGRICVALIDO, TP_PEDIDO FROM PID.PEDIDOS " +
                                        "WHERE NU_PID = {0} ", dblPedido.ToString());

                rdrRicVias = FunctionSql.AbreResult(strSQL, cnConexaoDll.Conectar()).Item2;
                if (!rdrRicVias.HasRows)
                {
                    rdrRicVias.Close();
                }
                else
                {
                    if (rdrRicVias.Read())
                    {
                        strRic = rdrRicVias["Nu_RgRicValido"].ToString();
                        if (strRic.Equals(""))
                            strRic = "0";

                        iTpPedido = rdrRicVias.GetInt16(1);
                        rdrRicVias.Close();

                        strSQL = string.Format("Select Max(Nu_Vias) As Max_Vias " +
                                "From Civil.Identificacoes where Nu_Ric = {0}", strRic);

                        rdrRicVias = FunctionSql.AbreResult(strSQL, cnConexaoDll.Conectar()).Item2;
                        strVias = "";
                        if (rdrRicVias.Read())
                        {
                            if (rdrRicVias[0].ToString().Equals(""))
                            {
                                strVias = "1";
                            }
                            else if (!iTpPedido.Equals(7) && !iTpPedido.Equals(12) && !iTpPedido.Equals(13))
                            {
                                int vias = rdrRicVias.GetInt16(0) + 1;
                                strVias = vias.ToString();
                            }
                            else
                            {
                                strVias = "-1";
                            }
                        }
                    }
                }
            }
            else
            {
                // PID consta na CIVIL.IDENTIFICACOES
                if (rdrRicVias.Read())
                {
                    strRic = rdrRicVias["NU_RIC"].ToString();
                    strVias = rdrRicVias["NU_VIAS"].ToString();
                }
            }
        }



        private (RetornoTrataImagem, double) AlocaIdCorpo(bool blSinal)
        {
            string strSql;
            RetSqlFunction rAbreId;
            OracleDataReader rdrId;
            double dblIdCorpo;
            try
            {
                dblIdCorpo = 0;

                if (blSinal)
                {
                    strSql = "Select Imagem.Sq_Sinais.NextVal From Dual";
                }
                else
                {
                    strSql = "Select Imagem.Sq_Corpos.NextVal From Dual";
                }

                var ResultadoAbreResult = FunctionSql.AbreResult(strSql, cnConexaoDll.Conectar());
                rAbreId = ResultadoAbreResult.Item1;
                rdrId = ResultadoAbreResult.Item2;

                if (rAbreId.Equals(RetSqlFunction.retSQLOk))
                {
                    if (rdrId.HasRows)
                    {
                        while (rdrId.Read())
                        {
                            dblIdCorpo = double.Parse(rdrId[0].ToString());
                        }
                    }
                    return (RetornoTrataImagem.Rok, dblIdCorpo);
                }
                else
                {
                    return (RetornoTrataImagem.RFalhaNovoID, dblIdCorpo);
                }
            }
            catch (Exception ex)
            {
                ManipulaErro.MostraErro("AlocaIDCorpo(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
                return (RetornoTrataImagem.RFalhaNovoID, 0);
            }
        }


        private RetSqlFunction GravaImagemCorpos(string strTabela, string strNomeCampo, double dblIdCorpo, int iAno, string strRic, double dblPedido, string strVias, List<byte[]> lstArquivos)
        {
            string strSql;
            try
            {
                int iQtdeImagem;
                //Faz o insert na tabela Imagem.Corpos_AAAA
                if (!strNomeCampo.Trim().Equals(""))
                {
                    strSql = "INSERT INTO " + strTabela + " (id_corpo,nu_ano,nu_pid, nu_ric, nu_vias," + strNomeCampo + ") " +
                       "values( " + dblIdCorpo.ToString() + "," + iAno.ToString() + "," + dblPedido.ToString() + "," +
                                   strRic + "," + strVias + "," + " :BlobParameterImg1 )";
                    iQtdeImagem = 1;
                }
                else
                {
                    strSql = "INSERT INTO " + strTabela + " (id_corpo,nu_ano,nu_pid, nu_ric, nu_vias, im_dedo_01, im_dedo_02, " +
                             "im_dedo_03, im_dedo_04, im_dedo_05, im_dedo_06 , im_dedo_07, im_dedo_08, im_dedo_09, im_dedo_10," +
                             "im_foto, im_assinatura )  " +
                    "values( " + dblIdCorpo.ToString() + "," + iAno.ToString() + "," + dblPedido.ToString() + "," +
                                strRic + "," + strVias + "," + " :BlobParameterImg1 , :BlobParameterImg2, " +
                            " :BlobParameterImg3 , :BlobParameterImg4 , :BlobParameterImg5, :BlobParameterImg6," +
                            " :BlobParameterImg7, :BlobParameterImg8, :BlobParameterImg9, :BlobParameterImg10, " +
                            " :BlobParameterImg11, :BlobParameterImg12)";
                    iQtdeImagem = 12;
                }
                return CarregaParametrosGrava(iQtdeImagem, strSql, lstArquivos);
            }
            catch (Exception ex)
            {

                ManipulaErro.MsgBox(0, ex.Message, "Alerta", 0);
                return RetSqlFunction.retSQLError;
            }

        }

        public RetornoTrataImagem GravaPedido(double dblPedido, TipoDeImagem eTipoDeImagem, ref string[] lstArquivo, ref OracleConnection cnConexaoBiografica)
        {
            try
            {
                string strNomeDoCampo = "";
                string strTipoDeImagem = "";

                RetSqlFunction rAbreImagem;
                OracleDataReader rdrResult;

                List<byte[]> lstImagemConvertida;
                List<int> lstTipoExtensao;


                double dblTamanhoGravado;
                bool bInclui;
                bool bArquivoApagado;
                int iTotalImagens = 1;
                string strSql;

                bArquivoApagado = true;


                //Limpa as propriedades da classe.
                NomeDoArquivo.Clear();
                TamanhoDoArquivo.Clear();
                IdCorpo = 0;

                if (eTipoDeImagem.Equals(TipoDeImagem.Sinal))
                {
                    return RetornoTrataImagem.RTipodeImagemInvalido;
                }


                if (eTipoDeImagem.Equals(TipoDeImagem.Todas))
                {
                    iTotalImagens = 12;
                }
                else if (eTipoDeImagem.Equals(TipoDeImagem.Dedos))
                {
                    iTotalImagens = 10;
                }

                // Redimensiona o Array com a lista de arquivos
                Array.Resize(ref lstArquivo, iTotalImagens);

                if (eTipoDeImagem.Equals(TipoDeImagem.Todas) || eTipoDeImagem.Equals(TipoDeImagem.Dedos))
                {
                    for (int iContador = 0; iContador <= lstArquivo.Length - 1; iContador++)
                    {
                        /* Rogerio Itoh - Verifica se tem algum arquivo que não foi preenchido */
                        var vlArray = lstArquivo[iContador];
                        if (vlArray == null)
                            return RetornoTrataImagem.RFaltaImagem;
                    }
                }


                strSql = "";

                if (eTipoDeImagem.Equals(TipoDeImagem.Todas))
                {
                    strSql = Constante.SqlTodos;
                    strSql = strSql + string.Format("Nu_Pid from PID.Corpo_Pedido Where Nu_Pid = {0}", dblPedido.ToString());
                }
                else if (eTipoDeImagem.Equals(TipoDeImagem.Dedos))
                {
                    strSql = Constante.SqlDedos;
                    strSql = strSql + string.Format(",Nu_Pid from PID.Corpo_Pedido Where Nu_Pid = {0}", dblPedido.ToString());
                }
                else
                {
                    for (int iContador = 0; iContador <= lstArquivo.Length - 1; iContador++)
                    {
                        /* Rogerio Itoh - Verifica se tem mais de um arquivo */
                        if (iContador.Equals(0))
                        {
                            if (lstArquivo[iContador].Length.Equals(0))
                                return RetornoTrataImagem.RFaltamArquivos;
                        }
                        else
                        {
                            if (lstArquivo[iContador] != null)
                                return RetornoTrataImagem.RMaisdeUmArquivo;
                        }
                    }

                    var ResultadoNome_Campo = Imagens.Nome_Campo(eTipoDeImagem);
                    strNomeDoCampo = ResultadoNome_Campo.Item1;
                    strTipoDeImagem = ResultadoNome_Campo.Item2;

                    if (strNomeDoCampo.Equals("") && strTipoDeImagem.Equals(""))
                    {
                        return Enum.RetornoTrataImagem.RTipodeImagemInvalido;
                    }

                    strSql = string.Format("Select {0},{1},Nu_pid " +
                                           "From Pid.Corpo_Pedido " +
                                           "Where Nu_Pid = {2}", strNomeDoCampo, strTipoDeImagem, dblPedido.ToString());
                }


                /*  Converte os arquivos da lista para Byte , na conversão eu verifico 
                 *  se o tipo de extensão do arquivo é valida e o tamanho do arquivo
                 *  convertido é maior que > 1*/
                var RetornoLe_Arquivo_Imagem = Imagens.Le_Arquivo_Imagem(lstArquivo);

                // na versão anterior a rotina mandava um arquivo por vez
                dblTamanhoGravado = RetornoLe_Arquivo_Imagem.Item1;
                lstImagemConvertida = RetornoLe_Arquivo_Imagem.Item2;
                lstTipoExtensao = RetornoLe_Arquivo_Imagem.Item3;

                // Caso aconteça algum erro na conversão retorna tamanho = 0
                if (dblTamanhoGravado < 1)
                {
                    return RetornoTrataImagem.RFalhaGravacao;
                }

                // Se o tipo de extensão do arquivo for desconhecida 
                //  sai da rotina
                if (lstTipoExtensao[0].Equals(9))
                {
                    return RetornoTrataImagem.RTipodeImagemInvalido;
                }

                /* a funcão AbreResult retorna 2 valores
                 * Item1 = Status da execução da query
                 * Item2 = Os registros do resultado da query
                 */
                var ResultadoAbresult = FunctionSql.AbreResult(strSql, cnConexaoBiografica);
                rAbreImagem = ResultadoAbresult.Item1;
                rdrResult = ResultadoAbresult.Item2;

                if (rAbreImagem.Equals(RetSqlFunction.retSQLEmpty))
                {
                    /* Se o registro não existe na tabela PID.CORPO_PEDIDO 
                     * faz o insertt */

                    Imagens.LimpaArray();
                    bInclui = true;
                    if (eTipoDeImagem.Equals(TipoDeImagem.Todas) || eTipoDeImagem.Equals(TipoDeImagem.Dedos))
                    {
                        bArquivoApagado = true;
                        strSql = "";
                        if (eTipoDeImagem.Equals(TipoDeImagem.Todas))
                        {
                            strSql = "Insert into Pid.Corpo_Pedido(NU_PID, IM_DEDO_01, " +
                                     "IM_DEDO_02, IM_DEDO_03, IM_DEDO_04, IM_DEDO_05, " +
                                     "IM_DEDO_06, IM_DEDO_07, IM_DEDO_08, IM_DEDO_09, " +
                                     "IM_DEDO_10, IM_FOTO, IM_ASSINATURA, " +
                                     "TP_DEDO_01, TP_DEDO_02, TP_DEDO_03, TP_DEDO_04," +
                                     " TP_DEDO_05,TP_DEDO_06, TP_DEDO_07, TP_DEDO_08," +
                                     " TP_DEDO_09,TP_DEDO_10, TP_FOTO, TP_ASSINATURA, DT_ATUALIZA)" +
                                     "values( " + dblPedido.ToString() + ",:BlobParameterImg1," +
                                     ":BlobParameterImg2,:BlobParameterImg3,:BlobParameterImg4," +
                                     ":BlobParameterImg5,:BlobParameterImg6,:BlobParameterImg7," +
                                     ":BlobParameterImg8,:BlobParameterImg9,:BlobParameterImg10," +
                                     ":BlobParameterImg11, :BlobParameterImg12," +
                                     ":TpImagem1,:TpImage2,:TpImagem3,:TpImagem4,:TpImagem5," +
                                     ":TpImagem6,:TpImage7,:TpImagem8,:TpImagem9,:TpImagem10," +
                                     ":TpImagem11,:TpImage12, to_Date('" + DateTime.Now.ToString() + "','dd/mm/yyyy hh24:mi:ss' ) )";
                        }
                        else
                        {
                            strSql = "Insert into Pid.Corpo_Pedido(NU_PID, IM_DEDO_01, " +
                                     "IM_DEDO_02, IM_DEDO_03, IM_DEDO_04, IM_DEDO_05, " +
                                     "IM_DEDO_06, IM_DEDO_07, IM_DEDO_08, IM_DEDO_09, " +
                                     "IM_DEDO_10, " +
                                     "TP_DEDO_01, TP_DEDO_02, TP_DEDO_03, TP_DEDO_04," +
                                     " TP_DEDO_05,TP_DEDO_06, TP_DEDO_07, TP_DEDO_08," +
                                     " TP_DEDO_09,TP_DEDO_10, DT_ATUALIZA)" +
                                     "values( " + dblPedido.ToString() + ",:BlobParameterImg1," +
                                     ":BlobParameterImg2,:BlobParameterImg3,:BlobParameterImg4," +
                                     ":BlobParameterImg5,:BlobParameterImg6,:BlobParameterImg7," +
                                     ":BlobParameterImg8,:BlobParameterImg9,:BlobParameterImg10," +
                                     ":TpImagem1,:TpImagem2,:TpImagem3,:TpImagem4,:TpImagem5," +
                                     ":TpImagem6,:TpImage7,:TpImagem8,:TpImagem9,:TpImagem10," +
                                     " to_Date('" + DateTime.Now.ToString() + "', 'dd/mm/yyyy hh24:mi:ss') )";
                        }

                        //  Mando para a função que executa a Query 
                        rAbreImagem = CarregaParametrosGrava(iTotalImagens, strSql, lstImagemConvertida, lstTipoExtensao);
                        if (rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                        {
                            lstImagemConvertida.Clear();
                            lstTipoExtensao.Clear();
                            if (DeletaArquivo)
                            {
                                if (Imagens.EliminaArquivo(lstArquivo).Equals(RetornoTrataImagem.Rok))
                                {
                                    return RetornoTrataImagem.Rok;
                                }
                                else
                                {
                                    return RetornoTrataImagem.ROkArquivoNaoApagado;
                                }
                            }
                            return RetornoTrataImagem.Rok;
                        }
                    }
                    else
                    {
                        strSql = "";
                        strSql = "Insert into Pid.Corpo_Pedido(NU_PID, " +
                                 strNomeDoCampo + "," + strTipoDeImagem + "," +
                                 " DT_ATUALIZA) values( " + dblPedido.ToString() +
                                 ",:BlobParameterImg1,:TpImage01, To_Date('" + DateTime.Now.ToString() + "','dd/mm/yyyy hh24:mi:ss') )";

                        // Faz o insert do registro somente a imagem solicitada

                        rAbreImagem = CarregaParametrosGrava(iTotalImagens, strSql, lstImagemConvertida, lstTipoExtensao);
                        if (rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                        {
                            lstImagemConvertida.Clear();
                            lstTipoExtensao.Clear();
                            if (DeletaArquivo)
                            {
                                if (Imagens.EliminaArquivo(lstArquivo).Equals(RetornoTrataImagem.Rok))
                                {
                                    return RetornoTrataImagem.Rok;
                                }
                                else
                                {
                                    return RetornoTrataImagem.ROkArquivoNaoApagado;
                                }
                            }
                            return RetornoTrataImagem.Rok;
                        }
                    }
                }
                else
                {
                    strSql = "";
                    // Faz update do  registro
                    if (eTipoDeImagem.Equals(TipoDeImagem.Todas) || eTipoDeImagem.Equals(TipoDeImagem.Dedos))
                    {

                        if (eTipoDeImagem.Equals(TipoDeImagem.Todas))
                        {
                            strSql = "Update Pid.Corpo_Pedido set NU_PID = " + dblPedido.ToString() + ",IM_DEDO_01 = :BlobParameterImg1," +
                                      "IM_DEDO_02 = :BlobParameterImg2,IM_DEDO_03 = :BlobParameterImg3," +
                                      "IM_DEDO_04 = :BlobParameterImg4, IM_DEDO_05 = :BlobParameterImg5,IM_DEDO_06 = :BlobParameterImg6," +
                                      "IM_DEDO_07 = :BlobParameterImg7, IM_DEDO_08 = :BlobParameterImg8,IM_DEDO_09 = :BlobParameterImg9," +
                                      "IM_DEDO_10 = :BlobParameterImg10,IM_FOTO = :BlobParameterImg11, IM_ASSINATURA = :BlobParameterImg12," +
                                      "TP_DEDO_01 = :TpImagem1, TP_DEDO_02 = :TpImagem2,TP_DEDO_03 = :TpImagem3,TP_DEDO_04 = :TpImagem4," +
                                      "TP_DEDO_05 = :TpImagem5, TP_DEDO_06 = :TpImagem6,TP_DEDO_07 = :TpImagem7,TP_DEDO_08 = :TpImagem8," +
                                      "TP_DEDO_09 = :TpImagem9, TP_DEDO_10 = :TpImagem10,TP_FOTO = :TpImagem11,TP_ASSINATURA = :TpImagem12," +
                                      "DT_ATUALIZA = to_Date('" + DateTime.Now.ToString() + "','dd/mm/yyyy hh24:mi:ss') " +
                                      "Where NU_PID = " + dblPedido.ToString();
                        }

                        if (eTipoDeImagem.Equals(TipoDeImagem.Dedos))
                        {
                            strSql = "Update Pid.Corpo_Pedido set NU_PID = " + dblPedido.ToString() + ",IM_DEDO_01 = :BlobParameterImg1," +
                                     "IM_DEDO_02 = :BlobParameterImg2,IM_DEDO_03 = :BlobParameterImg3, IM_DEDO_04 = :BlobParameterImg4, " +
                                     "IM_DEDO_05 = :BlobParameterImg5,IM_DEDO_06 = :BlobParameterImg6, IM_DEDO_07 = :BlobParameterImg7," +
                                     "IM_DEDO_08 = :BlobParameterImg8,IM_DEDO_09 = :BlobParameterImg9, IM_DEDO_10 = :BlobParameterImg10," +
                                     "TP_DEDO_01 = :TpImagem1, TP_DEDO_02 = :TpImagem2,TP_DEDO_03 = :TpImagem3,TP_DEDO_04 = :TpImagem4," +
                                     "TP_DEDO_05 = :TpImagem5, TP_DEDO_06 = :TpImagem6,TP_DEDO_07 = :TpImagem7,TP_DEDO_08 = :TpImagem8," +
                                     "TP_DEDO_09 = :TpImagem9, TP_DEDO_10 = :TpImagem10," +
                                     "DT_ATUALIZA = to_Date('" + DateTime.Now.ToString() + "','dd/mm/yyyy hh24:mi:ss') " +
                                     "Where NU_PID = " + dblPedido.ToString();
                        }
                    }
                    else
                    {
                        strSql = "Update Pid.Corpo_Pedido set NU_PID = " + dblPedido.ToString() + "," +
                                  strNomeDoCampo + " = :BlobParameterImg1 ," +
                                  strTipoDeImagem + "= :TpImage01," +
                                  "DT_ATUALIZA = To_Date('" + DateTime.Now.ToString() + "','dd/mm/yyyy hh24:mi:ss')" +
                                  " Where NU_PID = " + dblPedido.ToString();
                    }


                    rAbreImagem = CarregaParametrosGrava(iTotalImagens, strSql, lstImagemConvertida, lstTipoExtensao);
                    if (rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                    {
                        lstImagemConvertida.Clear();
                        lstTipoExtensao.Clear();
                        if (DeletaArquivo)
                        {
                            if (Imagens.EliminaArquivo(lstArquivo).Equals(RetornoTrataImagem.Rok))
                            {
                                return RetornoTrataImagem.Rok;
                            }
                            else
                            {
                                return RetornoTrataImagem.ROkArquivoNaoApagado;
                            }
                        }
                        return RetornoTrataImagem.Rok;
                    }
                }
            }
            catch (Exception ex)
            {
                ManipulaErro.MostraErro("GravaPedido(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
                //ManipulaErro.GravaEventLog("GravaPedido : " + dblPedido + " - " + ex.Message, ex.GetHashCode());
                return RetornoTrataImagem.RFalhaGravacao;
            }
            return RetornoTrataImagem.RFalhaGravacao;
        }

        /* Grava imagem na Imagem.Corpos ou PID.Corpo_pedido 
         * o parâmetro : List<int> lstTipoImagem = null é opcional 
         * ele deve ser passado quando for gravar uma imagem na 
         * PID.Corpo_pedido  */


        public RetornoTrataImagem ConsultaIdentificado(double dblPedido, TipoDeImagem btTipoDeImagem, double dblIdCorpo = 0, bool blApenasConsulta = false)
        {
            RetornoTrataImagem ResultadoConsultaIdentificado = Enum.RetornoTrataImagem.RFalhaConexaoBiografica;
            try
            {
                string strNomeDoCampo, strTipoDeArquivo, strArquivo = "";
                /* Retorno da query */
                RetSqlFunction rAbreImagem;

                double dblTamanhoGravado;
                string strSql;
                byte btFaltaArquivo, btFaltaImagem;
                string str_msg, strSql1;

                DataSet DataSetIdentificado = new DataSet("ResultadoAdapter");
                int iUltimoReg;

                //Limpa as propriedades da classe.
                NomeDoArquivo.Clear();
                TamanhoDoArquivo.Clear();
                IdCorpo = 0;

                //Se o usuário não tem direitos para leitura ou gravação saí da função.
                //A função para verificar se o usuário tem direito é a ConectaParaLerOuGravarIdentificado

                if (!ConectadoGravacao && !ConectadoLeitura)
                {
                    ResultadoConsultaIdentificado = RetornoTrataImagem.RFalhaConexaoIdentificados;
                }

                if (btTipoDeImagem.Equals(TipoDeImagem.Todas))
                {
                    iTotalImagens = 12;

                    if (blApenasConsulta)
                    {
                        strSql = Constante.ConsultaImagemTodos;
                    }
                    else
                    {
                        strSql = Constante.ImagemTodos;
                    }

                    strSql = strSql + " , Id_Corpo, Nu_Pid ";

                    strSql1 = strSql + "From " + TabelaImagemCorpos(dblPedido, dblIdCorpo, true);

                    if (!dblIdCorpo.Equals(0))
                    {
                        strSql = " Where  Nu_Pid = " + dblPedido;
                    }
                    else
                    {
                        strSql = " Where  Id_Corpo = " + dblIdCorpo;
                    }
                    strSql1 = strSql1 + strSql;


                }
                else if (btTipoDeImagem.Equals(TipoDeImagem.Dedos))
                {
                    iTotalImagens = 10;
                    if (blApenasConsulta)
                    {
                        strSql = Constante.ConsultaImagemDedos;
                    }
                    else
                    {
                        strSql = Constante.ImagemDedos;
                    }

                    strSql = strSql + " , Id_Corpo, Nu_Pid ";

                    strSql1 = strSql + "From " + TabelaImagemCorpos(dblPedido, dblIdCorpo, true);

                    if (dblIdCorpo.Equals(0))
                    {
                        strSql = " Where  Nu_Pid = " + dblPedido;
                    }
                    else
                    {
                        strSql = " Where  Id_Corpo = " + dblIdCorpo;
                    }

                    strSql1 = strSql1 + strSql;
                }
                else
                {
                    iTotalImagens = 1;
                    var ResultadoNome_Campo = Imagens.Nome_Campo(btTipoDeImagem);

                    if (ResultadoNome_Campo.Item1.Equals("") && ResultadoNome_Campo.Item2.Equals(""))
                    {
                        ResultadoConsultaIdentificado = RetornoTrataImagem.RTipodeImagemInvalido;
                    }

                    strNomeDoCampo = ResultadoNome_Campo.Item1;
                    strTipoDeArquivo = ResultadoNome_Campo.Item2;
                    strSql = "";

                    if (blApenasConsulta)
                    {
                        strSql = strSql + "Select DBMS_LOB.GetLength(" + strNomeDoCampo + "), ";
                    }
                    else
                    {

                        strSql = string.Format("Select {0} , ", strNomeDoCampo);
                    }
                    strSql = strSql + "Id_Corpo, Nu_Pid from ";

                    /*###########################################################                    
                    'na variavel strSQL1, é colocado o SQL para que o mesmo realize
                    'a busca nas tabelas IMAGEM do banco Detran-sun
                    '
                    'Quando temos mais de um corpo na base, associado ao pid, trazemos somente o ultimo corpo
                    'gravado na base
                    '###########################################################*/

                    strSql1 = strSql + TabelaImagemCorpos(dblPedido, dblIdCorpo, true);

                    if (dblIdCorpo.Equals(0))
                    {
                        strSql = " Where  Nu_Pid = " + dblPedido;
                    }
                    else
                    {
                        strSql = " Where  Id_Corpo = " + dblIdCorpo;
                    }
                    strSql1 = strSql1 + strSql;
                }


                var ResultadoAbresult = FunctionSql.AbreResultAdapter(strSql1, cnConexaoDll.Conectar());
                rAbreImagem = ResultadoAbresult.Item1;
                DataSetIdentificado = ResultadoAbresult.Item2;

                if (rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                {

                    Imagens.LimpaArray();
                    /* Para pegar os campos do ultimo registro verifico a qtde de linhas 
                     * existent no DataSet */
                    iUltimoReg = DataSetIdentificado.Tables["ResultadoAdapter"].Rows.Count - 1;

                    dblIdCorpo = double.Parse(DataSetIdentificado.Tables["ResultadoAdapter"].Rows[iUltimoReg]["Id_Corpo"].ToString());
                    if (!dblIdCorpo.Equals(0))
                        dblPedido = double.Parse(DataSetIdentificado.Tables["ResultadoAdapter"].Rows[iUltimoReg]["Nu_Pid"].ToString());

                    if (btTipoDeImagem.Equals(TipoDeImagem.Todas) || btTipoDeImagem.Equals(TipoDeImagem.Dedos))
                    {
                        btFaltaArquivo = 0;
                        btFaltaImagem = 0;

                        if (blApenasConsulta)
                        {
                            for (int iContador = 1; iContador <= iTotalImagens; iContador++)
                            {
                                var TamanhoCampo = double.Parse(DataSetIdentificado.Tables["ResultadoAdapter"].Rows[iUltimoReg].ItemArray[iContador - 1].ToString());
                                if (TamanhoCampo.Equals(0))
                                {
                                    btFaltaImagem += 1;
                                }
                                else
                                {
                                    Imagens.g_TamanhoDoArquivo[iContador - 1] = TamanhoCampo;
                                    /* Inclui o tamanho dos arquivos na propriedade da classe */
                                    TamanhoDoArquivo.Add(TamanhoCampo);
                                    IdCorpo = dblIdCorpo;
                                }
                            }
                            DataSetIdentificado.Dispose();
                            if (btFaltaImagem > 0)
                                ResultadoConsultaIdentificado = RetornoTrataImagem.RFaltaImagem;
                            else
                                ResultadoConsultaIdentificado = RetornoTrataImagem.Rok;
                        }
                        else
                        {
                            for (int iContador = 1; iContador <= iTotalImagens; iContador++)
                            {
                                strArquivo = DiretorioImagens + dblPedido.ToString().PadLeft(12, '0');
                                if (iContador < 11)
                                    strArquivo = strArquivo + iContador.ToString().PadLeft(2, '0') + ".Wsq";
                                else
                                    strArquivo = strArquivo + iContador.ToString().PadLeft(2, '0') + ".Jpg";

                                // Criar função que gera um arquivo e grava na pasta 
                                dblTamanhoGravado = Imagens.GravaArquivoImagem(strArquivo, (byte[])DataSetIdentificado.Tables["ResultadoAdapter"].Rows[iUltimoReg].ItemArray[iContador - 1]);
                                if (dblTamanhoGravado < 1)
                                {
                                    btFaltaArquivo += 1;  //btFaltaImagem;
                                }
                                else
                                {
                                    Imagens.g_NomeDoArquivo[iContador - 1] = strArquivo;
                                    /* Inclui IdCorpo nome do Arquivo e Tamanho do arquivo nas 
                                    * propriedades da classe */
                                    NomeDoArquivo.Add(strArquivo);
                                    TamanhoDoArquivo.Add(dblTamanhoGravado);
                                    IdCorpo = dblIdCorpo;
                                }
                            }

                            if (btFaltaArquivo.Equals(0))
                                ResultadoConsultaIdentificado = RetornoTrataImagem.Rok;
                            else if (btFaltaArquivo.Equals(iTotalImagens))
                                ResultadoConsultaIdentificado = RetornoTrataImagem.RFalhaGravacao;
                            else
                                ResultadoConsultaIdentificado = RetornoTrataImagem.RFaltaImagem;
                        }
                    }
                    else
                    {
                        var TamanhoCampo = DataSetIdentificado.Tables["ResultadoAdapter"].Rows[iUltimoReg].ItemArray[0];
                        if (DataSetIdentificado.Tables["ResultadoAdapter"].Rows[iUltimoReg].ItemArray[0].Equals(0))
                        {
                            DataSetIdentificado.Dispose();
                            return RetornoTrataImagem.RFaltamArquivos;
                        }
                        else if (blApenasConsulta)
                        {
                            Imagens.g_TamanhoDoArquivo[0] = double.Parse(TamanhoCampo.ToString());
                            /* Inclui o tamanho dos arquivos na propriedade da classe */

                            TamanhoDoArquivo.Add(double.Parse(TamanhoCampo.ToString()));
                            IdCorpo = dblIdCorpo;
                            DataSetIdentificado.Dispose();
                            return RetornoTrataImagem.Rok;
                        }

                        strArquivo = DiretorioImagens + dblPedido.ToString().PadLeft(12, '0');
                        if (btTipoDeImagem.GetHashCode() < 11)
                        {
                            strArquivo = strArquivo + btTipoDeImagem.GetHashCode().ToString().PadLeft(2, '0') + ".Wsq";
                        }
                        else
                        {
                            strArquivo = strArquivo + btTipoDeImagem.GetHashCode().ToString().PadLeft(2, '0') + ".Jpg";
                        }

                        dblTamanhoGravado = Imagens.GravaArquivoImagem(strArquivo, (byte[])DataSetIdentificado.Tables["ResultadoAdapter"].Rows[iUltimoReg].ItemArray[0]);
                        if (dblTamanhoGravado < 1)
                        {
                            ResultadoConsultaIdentificado = RetornoTrataImagem.RFaltaImagem;
                        }
                        else
                        {
                            Imagens.g_NomeDoArquivo[0] = strArquivo;
                            /* Inclui o Nome do arquivos na propriedade da classe */
                            NomeDoArquivo.Add(@strArquivo);
                            TamanhoDoArquivo.Add(dblTamanhoGravado);
                            IdCorpo = dblIdCorpo;
                            ResultadoConsultaIdentificado = RetornoTrataImagem.Rok;
                        }
                        DataSetIdentificado.Dispose();
                    }

                }
                else if (rAbreImagem.Equals(RetSqlFunction.retSQLEmpty))
                {

                    ResultadoConsultaIdentificado = RetornoTrataImagem.RRegistroNaoExiste;
                    if (!HideMsgBox)
                    {
                        str_msg = "Registro não existente!";
                        if (dblIdCorpo <= 0)
                        {
                            str_msg = str_msg + " NU_PID = " + dblPedido.ToString();
                        }
                        else
                        {
                            str_msg = str_msg + " ID_CORPO = " + dblIdCorpo.ToString();
                        }
                        ManipulaErro.MostraErro(str_msg, 0, "", HideMsgBox);
                    }
                    //Luciano Lucas - 08/10/2007 - para gravar erro caso não consiga encontrar o dado
                    //Trecho inserido a pedido do Zé Pitbull

                    dblIdCorpo = ExisteCorpoCivilIdentificacoes(dblPedido);
                    if (dblIdCorpo > 0)
                    {
                        LogErroConsulta(dblIdCorpo);
                    }

                }
                else
                {
                    IdCorpo = ExisteCorpoCivilIdentificacoes(dblPedido);
                    if (dblIdCorpo > 0)
                    {
                        LogErroConsulta(dblIdCorpo);
                    }
                    ResultadoConsultaIdentificado = RetornoTrataImagem.RFalhaConexaoIdentificados;
                }
            }
            catch (OracleException ex)
            {

                ManipulaErro.MostraErro("ConsultaIdentificado(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
                //ManipulaErro.GravaEventLog("ConsultaIdentificado(): : " + dblPedido.ToString() + " - " + ex.Message, ex.GetHashCode());
                ResultadoConsultaIdentificado = RetornoTrataImagem.RFalhaConexaoIdentificados;
            }
            return ResultadoConsultaIdentificado;
        }

        /* Origianalmente o IdCorpo, era passado como referência , mas esse valor não é alterado na função, coloquei como opcional*/
        //public RetornoTrataImagem ConsultaIdentificado_01(double dblPedido, TipoDeImagem eTipoDeImagem, ref Conexao cnConexaoBiografica, ref double dblIdCorpo, bool blApenasConsulta = false)

        public RetornoTrataImagem ConsultaIdentificado_01(double dblPedido, TipoDeImagem eTipoDeImagem, ref OracleConnection cnConexaoBiografica, double dblIdCorpo = 0, bool blApenasConsulta = false)
        {

            /*Faz basicamente a mesma coisa da ConsultaIdentificado tradicional, exceto pelo fato
            'de que ela vai apenas buscar a foto do identificado na tabela Imagem_corpos_01.
            'Esta tabela se encontra na dic-01, caso a pessoa não esteja presente nesta tabela, ou
            'queira fazer a consulta de uma imagem que não seja a foto a consulta é despachada para
            'a dic-04.*/
            string strNomeDoCampo, strArquivo = "";
            RetSqlFunction rAbreImagem;
            double dblTamanhoGravado;
            string strSql = "";
            byte[] CampoImFoto = null;

            DataSet DataSetIdentificado01 = new DataSet("ResultadoAdapter");
            int iTotalReg;

            RetornoTrataImagem ResultadoConsultaIdentificado_01 = Enum.RetornoTrataImagem.RFalhaConexaoBiografica;
            try
            {
                //Limpa as propriedades da classe.
                NomeDoArquivo.Clear();
                TamanhoDoArquivo.Clear();
                IdCorpo = 0;

                if (eTipoDeImagem.Equals(TipoDeImagem.Foto))
                {
                    strNomeDoCampo = Imagens.Nome_Campo(eTipoDeImagem).Item1;
                    if (strNomeDoCampo.Equals(""))
                    {
                        return RetornoTrataImagem.RTipodeImagemInvalido;
                    }

                    if (blApenasConsulta)
                        strSql = "Select DBMS_LOB.GetLength(" + strNomeDoCampo + "),";
                    else
                        strSql = "Select " + strNomeDoCampo + ",";

                    strSql = strSql + "Id_Corpo, Nu_Pid from Imagem.Corpos_01";
                    if (dblIdCorpo.Equals(0))
                    {
                        strSql = strSql + " Where  Nu_Pid = " + dblPedido;
                    }
                    else
                    {
                        strSql = strSql + " Where  Id_Corpo = " + dblIdCorpo;
                    }
                }
                else
                {
                    return RetornoTrataImagem.RTipodeImagemInvalido;
                }

                /* a funcão AbreResult retorna 2 valores
                 * Item1 = Status da execução da query
                 * Item2 = Os registros do resultado da query
                 */
                var ResultadoAbresult = FunctionSql.AbreResultAdapter(strSql, cnConexaoBiografica);
                rAbreImagem = ResultadoAbresult.Item1;
                DataSetIdentificado01 = ResultadoAbresult.Item2;

                if (rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                {
                    Imagens.LimpaArray();
                    iTotalReg = DataSetIdentificado01.Tables["ResultadoAdapter"].Rows.Count;
                    // Recupero as informações do último registro
                    if (blApenasConsulta)
                    {
                        var TamanhoArquivo = double.Parse(DataSetIdentificado01.Tables["ResultadoAdapter"].Rows[iTotalReg - 1].ItemArray[0].ToString());
                        TamanhoDoArquivo.Add(TamanhoArquivo);
                        return RetornoTrataImagem.Rok;
                    }
                    CampoImFoto = (byte[])DataSetIdentificado01.Tables["ResultadoAdapter"].Rows[iTotalReg - 1].ItemArray[0];
                    dblPedido = double.Parse(DataSetIdentificado01.Tables["ResultadoAdapter"].Rows[iTotalReg - 1].ItemArray[2].ToString());

                    strArquivo = DiretorioImagens + dblPedido.ToString().PadLeft(12, '0');
                    strArquivo = strArquivo + eTipoDeImagem.GetHashCode().ToString().PadLeft(2, '0') + ".Jpg";
                    // Insere a propriedade com o nome do arquivo;                    
                    dblTamanhoGravado = Imagens.GravaArquivoImagem(strArquivo, CampoImFoto);
                    if (dblTamanhoGravado < 1)
                    {
                        NomeDoArquivo.Clear();
                        TamanhoDoArquivo.Clear();
                        ResultadoConsultaIdentificado_01 = RetornoTrataImagem.RFaltaImagem;
                    }
                    else
                    {
                        Imagens.g_NomeDoArquivo[0] = strArquivo;
                        // Propriedades da Classe - IdCorpo, NomeDoArquivo e TamanhoDoArquivo
                        IdCorpo = double.Parse(DataSetIdentificado01.Tables["ResultadoAdapter"].Rows[iTotalReg - 1].ItemArray[1].ToString());
                        NomeDoArquivo.Add(strArquivo);
                        TamanhoDoArquivo.Add(dblTamanhoGravado);
                        ResultadoConsultaIdentificado_01 = RetornoTrataImagem.Rok;
                    }
                    DataSetIdentificado01.Dispose();
                }
                else if (rAbreImagem.Equals(RetSqlFunction.retSQLEmpty))
                {
                    ResultadoConsultaIdentificado_01 = RetornoTrataImagem.RRegistroNaoExiste;
                }
                else
                {
                    ResultadoConsultaIdentificado_01 = RetornoTrataImagem.RFalhaConexaoIdentificados;
                }
            }
            catch (OracleException ex)
            {

                ManipulaErro.MostraErro("ConsultaIdentificado_01(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
                //ManipulaErro.GravaEventLog("ConsultaIdentificado_01(): : " + dblPedido.ToString() + " - " + ex.Message, ex.GetHashCode());
                ResultadoConsultaIdentificado_01 = RetornoTrataImagem.RFalhaConexaoIdentificados;
            }
            return ResultadoConsultaIdentificado_01;
        }



        private double ExisteCorpoCivilIdentificacoes(double dblNuPid)
        {
            string strSql;
            double ExisteCorpoCivilIdentificacoes;

            RetSqlFunction RetSql;
            OracleDataReader rdrResult;

            ExisteCorpoCivilIdentificacoes = 0;

            try
            {
                strSql = string.Format("select nu_pid,id_corpo from civil.identificacoes where nu_pid = {0}", dblNuPid);
                var ResultadoAbresult = FunctionSql.AbreResult(strSql, cnConexaoDll.Conectar());
                RetSql = ResultadoAbresult.Item1;
                rdrResult = ResultadoAbresult.Item2;
                if (RetSql.Equals(RetSqlFunction.retSQLOk))
                {
                    if (rdrResult.Read())
                    {
                        if (!rdrResult.IsDBNull(1))
                        {
                            if (double.Parse(rdrResult["id_corpo"].ToString()) > 0)
                            {
                                ExisteCorpoCivilIdentificacoes = double.Parse(rdrResult["id_corpo"].ToString());
                            }
                        }
                    }
                }
                rdrResult.Close();
                rdrResult.Dispose();
            }
            catch (Exception ex)
            {
                ManipulaErro.MostraErro("ExisteCorpoCivilIdentificacoes(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
            }
            return ExisteCorpoCivilIdentificacoes;
        }

        //Função criada para gravar log, caso aconteça algum erro para buscar o corpo do requerente
        private bool LogErroConsulta(double dblIdCorpo)
        {
            string strSql;
            try
            {
                strSql = "Insert into imagem.erro_pesquisa values (" + dblIdCorpo + ", sysdate)";
                cnConexaoDll.OraComando.Connection = cnConexaoDll.OraConn;
                cnConexaoDll.OraTransactiondll = cnConexaoDll.IniciaTransacao();
                cnConexaoDll.OraComando.CommandText = strSql;
                cnConexaoDll.OraComando.Transaction = cnConexaoDll.OraTransactiondll;
                cnConexaoDll.OraComando.ExecuteNonQuery();
                cnConexaoDll.OraTransactiondll.Commit();

                cnConexaoDll.OraComando.Dispose();
                cnConexaoDll.OraComando.Transaction.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                ManipulaErro.MostraErro("LogErroConsulta(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
                return false;
            }

        }

        /* Utilizado na função Consulta Identificado */
        private void SalvaImagensEmBranco(TipoDeImagem tpImagem)
        {
            int iIni, iFim;
            string strTmp;

            try
            {

                switch (tpImagem)
                {
                    case TipoDeImagem.Dedos:
                        iIni = 1;
                        iFim = 10;
                        break;
                    case TipoDeImagem.Todas:
                        iIni = 1;
                        iFim = 12;
                        break;
                    default:
                        iIni = tpImagem.GetHashCode();
                        iFim = tpImagem.GetHashCode();
                        break;
                }

                if (iIni < 0) iIni = 0;
                if (iFim > 12) iFim = 12;

                Imagens.LimpaArray();
                for (int iContador = iIni; iContador <= iFim; iContador++)
                {
                    strTmp = DiretorioImagens + "0".PadLeft(12, '0');
                    if (iContador < 11)
                    {
                        strTmp = strTmp + iContador.ToString().PadLeft(2, '0') + ".Wsq";
                        SaveResources(strTmp, iContador);
                    }
                    else if (iContador.Equals(11))
                    {
                        strTmp = strTmp + iContador.ToString().PadLeft(2, '0') + ".Jpg";
                        SaveResources(strTmp, iContador);
                    }
                    else if (iContador.Equals(12))
                    {
                        strTmp = strTmp + iContador.ToString().PadLeft(2, '0') + ".Jpg";
                        SaveResources(strTmp, iContador);
                    }
                    Imagens.g_NomeDoArquivo[iContador - 1] = strTmp;
                }
            }
            catch (Exception ex)
            {
                ManipulaErro.MostraErro("SalvaImagensEmBranco(): ", ex.GetHashCode(), ex.Message, HideMsgBox); 
            }

        }

        private void SaveResources(string strArquivo, int iTipoImagem)
        {
            /* Salva arquivo de Digital, Foto e Assinatura não disponivel
             Complemento da função SalvaImagensEmBranco
             */


            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = null;
            Bitmap bmp;

            try
            {

                //Lê o arquivo de imagem
                if (iTipoImagem < 11)
                {
                    myStream = myAssembly.GetManifestResourceStream("TrataImagensLib.Imagens.DigitalNaoDisponivel.wsq");
                }
                else if (iTipoImagem.Equals(11))
                {
                    myStream = myAssembly.GetManifestResourceStream("TrataImagensLib.Imagens.FotoNaoDisponivel.jpg");
                }
                else if (iTipoImagem.Equals(12))
                {
                    myStream = myAssembly.GetManifestResourceStream("TrataImagensLib.Imagens.AssinaturaNaoDisponivel.jpg");
                }

                if (iTipoImagem < 11)
                {
                    // Cria o arquivo de stream que vai receber o arquivvo wsq
                    FileStream fileWsq = new FileStream(strArquivo, FileMode.Create, FileAccess.Write);
                    // Copia o conteúdo do aruivo original para o novo arquivo
                    myStream.CopyTo(fileWsq);

                    fileWsq.Close();
                    fileWsq.Dispose();
                }
                else
                {
                    /* Converte o arquivo de stream q está no formato de bytes
                     * para imagem e salvar o arquivo*/
                    bmp = new Bitmap(myStream);
                    Image img = (Image)bmp;
                    img.Save(strArquivo);

                    bmp.Dispose();
                    img.Dispose();
                }

                myStream.Close();
                myStream.Dispose();
            }
            catch (Exception ex)
            {
                ManipulaErro.MostraErro("SaveResources(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
            }


        }


        public RetornoTrataImagem ConsultaSinalIdentificado(double dblRic, int iAnotacao, int iNuSeqFoto, ref double dblIdSinal, bool blApenasConsulta = false)
        {

            RetornoTrataImagem ConsultaSinalIdentificado = RetornoTrataImagem.RFalhaConexaoIdentificados;
            string strSql, strSqlCount, strWhere;
            OracleDataReader rdrIdentificado;
            RetSqlFunction rAbreImagem;
            string strArquivo;
            double dblTamanhoGravado;
            int iTotalCount;

            try
            {
                if (!ConectadoLeitura || !ConectadoGravacao)
                {
                    return ConsultaSinalIdentificado;
                }
                strSql = "";
                strSql = "Select Im_Sinal, Id_Sinal, Nu_Ric, Nu_Anotacao, " +
                         "Nu_SeqFoto From Imagem.Sinais ";

                strSqlCount = "Select Count(*) from Imagem.Sinais ";

                if (dblIdSinal <= 0)
                {
                    strWhere = string.Format("Where  Nu_Ric = {0}  And " +
                                "Nu_Anotacao = {1} And Nu_SeqFoto = {2}", dblRic, iAnotacao, iNuSeqFoto);
                }
                else
                {
                    strWhere = string.Format(" Where Id_Sinal = {0} ", dblIdSinal);
                }


                iTotalCount = FunctionSql.ExecutaCount(strSqlCount, cnConexaoDll.Conectar());
                if (iTotalCount < 0)
                {
                    return RetornoTrataImagem.RFalhaConexaoIdentificados;
                }
                else if (iTotalCount > 1)
                {
                    return RetornoTrataImagem.RSinalDuplicado;
                }
                else
                {
                    var ResultadoAbresult = FunctionSql.AbreResult(strSql, cnConexaoDll.Conectar());
                    rAbreImagem = ResultadoAbresult.Item1;
                    rdrIdentificado = ResultadoAbresult.Item2;
                    if (rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                    {
                        while (rdrIdentificado.Read())
                        {
                            Imagens.LimpaArray();

                            // Verifica se o campo IM_SINAL tem imagem
                            if (rdrIdentificado.IsDBNull(0))
                            {
                                ConsultaSinalIdentificado = RetornoTrataImagem.RFaltaImagem;
                                break;
                            }

                            // Se for apenas consulta, pegaa somente o tamamnho do arquivo de imagem
                            if (blApenasConsulta)
                            {
                                Imagens.g_TamanhoDoArquivo[0] = rdrIdentificado.GetByte(0);
                                ConsultaSinalIdentificado = RetornoTrataImagem.Rok;
                                break;
                            }

                            dblIdSinal = double.Parse(rdrIdentificado["Id_Sinal"].ToString());
                            if (dblIdSinal > 0)
                            {
                                dblRic = double.Parse(rdrIdentificado["Nu_Ric"].ToString());
                                iAnotacao = int.Parse(rdrIdentificado["Nu_Anotacao"].ToString());
                                iNuSeqFoto = int.Parse(rdrIdentificado["Nu_SeqFoto"].ToString());
                            }

                            //Arquivo de Imagem 
                            byte[] Im_Sinal = (byte[])rdrIdentificado.GetValue(0);

                            strArquivo = DiretorioImagens + dblRic.ToString().PadLeft(10, '0');
                            strArquivo = strArquivo + iAnotacao.ToString().PadLeft(4, '0') + iNuSeqFoto.ToString().PadLeft(4, '0');
                            strArquivo = strArquivo + ".Jpg";

                            dblTamanhoGravado = Imagens.GravaArquivoImagem(strArquivo, Im_Sinal);
                            if (dblTamanhoGravado < 1)
                            {
                                ConsultaSinalIdentificado = RetornoTrataImagem.RFalhaGravacao;
                                break;
                            }
                            else
                            {
                                this.NomeDoArquivo.Add(@strArquivo);
                                ConsultaSinalIdentificado = RetornoTrataImagem.Rok;
                                break;
                            }
                        }
                        rdrIdentificado.Close();                        
                        return ConsultaSinalIdentificado;
                    }
                }
            }
            catch (Exception Err)
            {
                ManipulaErro.MostraErro("ConsultaSinalIdentificado(): ", Err.GetHashCode(), Err.Message, HideMsgBox);
                //ManipulaErro.GravaEventLog("ConsultaSinalIdentificado() : " + Err.Message, Err.GetHashCode());
            }
            return ConsultaSinalIdentificado;
        }


        public RetornoTrataImagem ConsultaSinalIdenticado01(double dblRic, int iAnotacao, int iNuSeqFoto, ref OracleConnection CnConexaoBiografica, double dblIdSinal = 0, bool blApenasConsulta = false)
        {
            RetornoTrataImagem ConsultaSinalIdenticado01 = RetornoTrataImagem.RFalhaConexaoIdentificados;
            string strSql, strSqlCount, strWhere;
            OracleDataReader rdrIdentificado;
            RetSqlFunction rAbreImagem;
            string strArquivo;
            double dblTamanhoGravado;
            int iTotalCount;

            try
            {
                NomeDoArquivo.Clear();
                TamanhoDoArquivo.Clear();
                IdCorpo = 0;

                strSql = "";

                if (dblIdSinal <= 0)
                {
                    strWhere = string.Format("Where  Nu_Ric = {0}  And " +
                                "Nu_Anotacao = {1} And Nu_SeqFoto = {2}", dblRic, iAnotacao, iNuSeqFoto);
                }
                else
                {
                    strWhere = string.Format("Where Id_Sinal = {0} ", dblIdSinal);
                }

                strSql = "Select Im_Sinal, Id_Sinal, Nu_Ric, Nu_Anotacao, " +
                         "Nu_SeqFoto From Imagem.Sinais_01 " + strWhere;

                strSqlCount = "Select Count(*) from Imagem.Sinais_01 " + strWhere;


                iTotalCount = FunctionSql.ExecutaCount(strSqlCount, CnConexaoBiografica);
                if (iTotalCount < 0)
                {
                    return RetornoTrataImagem.RFalhaConexaoIdentificados;
                }
                else if (iTotalCount > 1)
                {
                    return RetornoTrataImagem.RSinalDuplicado;
                }
                else
                {
                    var ResultadoAbresult = FunctionSql.AbreResult(strSql, CnConexaoBiografica);
                    rAbreImagem = ResultadoAbresult.Item1;
                    rdrIdentificado = ResultadoAbresult.Item2;
                    if (rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                    {
                        while (rdrIdentificado.Read())
                        {
                            Imagens.LimpaArray();

                            // Verifica se o campo IM_SINAL tem imagem
                            if (rdrIdentificado.IsDBNull(0))
                            {
                                ConsultaSinalIdenticado01 = RetornoTrataImagem.RFaltaImagem;
                                break;
                            }

                            // Se for apenas consulta, pegaa somente o tamamnho do arquivo de imagem
                            if (blApenasConsulta)
                            {
                                Imagens.g_TamanhoDoArquivo[0] = rdrIdentificado.GetByte(0);
                                ConsultaSinalIdenticado01 = RetornoTrataImagem.Rok;
                                break;
                            }

                            dblIdSinal = double.Parse(rdrIdentificado["Id_Sinal"].ToString());
                            if (dblIdSinal > 0)
                            {
                                dblRic = double.Parse(rdrIdentificado["Nu_Ric"].ToString());
                                iAnotacao = int.Parse(rdrIdentificado["Nu_Anotacao"].ToString());
                                iNuSeqFoto = int.Parse(rdrIdentificado["Nu_SeqFoto"].ToString());
                            }

                            //Arquivo de Imagem 
                            byte[] Im_Sinal = (byte[])rdrIdentificado.GetValue(0);

                            strArquivo = DiretorioImagens + dblRic.ToString().PadLeft(10, '0');
                            strArquivo = strArquivo + iAnotacao.ToString().PadLeft(4, '0') + iNuSeqFoto.ToString().PadLeft(4, '0');
                            strArquivo = strArquivo + ".Jpg";

                            dblTamanhoGravado = Imagens.GravaArquivoImagem(strArquivo, Im_Sinal);
                            if (dblTamanhoGravado < 1)
                            {
                                ConsultaSinalIdenticado01 = RetornoTrataImagem.RFalhaGravacao;
                                break;
                            }
                            else
                            {
                                NomeDoArquivo.Add(@strArquivo);
                                TamanhoDoArquivo.Add(dblTamanhoGravado);
                                IdCorpo = dblIdSinal;
                                ConsultaSinalIdenticado01 = RetornoTrataImagem.Rok;
                                break;
                            }
                        }
                        rdrIdentificado.Close();
                        rdrIdentificado.Dispose();
                        return ConsultaSinalIdenticado01;
                    }
                }
            }
            catch (Exception Err)
            {
                ManipulaErro.MostraErro("ConsultaSinalIdenticado01(): ", Err.GetHashCode(), Err.Message, HideMsgBox);
                //ManipulaErro.GravaEventLog("ConsultaSinalIdenticado01() : " + Err.Message, Err.GetHashCode());
            }
            return ConsultaSinalIdenticado01;
        }



        public RetornoTrataImagem ConsultaSinalPac(double dblNuPid, int iAnotacao, int iNuSeqFoto, ref OracleConnection CnConexaoBiografica, bool blApenasConsulta = false)
        {
            string strSql, strSqlCount, strWhere;
            OracleDataReader rdrIdentificado;
            RetSqlFunction rAbreImagem;
            string strArquivo;
            double dblTamanhoGravado;
            int iTotalCount;

            RetornoTrataImagem ConsultaSinalPac = RetornoTrataImagem.RFalhaConexaoIdentificados;
            try
            {
                NomeDoArquivo.Clear();
                TamanhoDoArquivo.Clear();

                strSql = "";
                strSql = "Select Im_Sinal, " +
                         "Nu_Pid, " +
                         "Nu_Anotacao, " +
                         "Nu_SeqFoto," +
                         "Tp_Sinal " +
                         "From Pid.Sinal_Pac ";

                strWhere = "Where  Nu_Pid = " + dblNuPid + " And " +
                           "Nu_Anotacao = " + iAnotacao + " And " +
                           "Nu_SeqFoto = " + iNuSeqFoto;

                strSqlCount = "Select Count(*) from Pid.Sinal_Pac " + strWhere;
                strSql = strSql + strWhere;

                iTotalCount = FunctionSql.ExecutaCount(strSqlCount, CnConexaoBiografica);
                if (iTotalCount < 0)
                {
                    return RetornoTrataImagem.RFalhaConexaoIdentificados;
                }
                else if (iTotalCount > 1)
                {
                    return RetornoTrataImagem.RSinalDuplicado;
                }
                else if (iTotalCount.Equals(0))
                {
                    return RetornoTrataImagem.RRegistroNaoExiste;
                }

                var ResultadoAbresult = FunctionSql.AbreResult(strSql, CnConexaoBiografica);
                rAbreImagem = ResultadoAbresult.Item1;
                rdrIdentificado = ResultadoAbresult.Item2;
                if (rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                {
                    if (rdrIdentificado.Read())
                    {

                        if (rdrIdentificado.IsDBNull(0))
                        {
                            rdrIdentificado.Close();
                            rdrIdentificado.Dispose();
                            return RetornoTrataImagem.RFaltaImagem;
                        }
                        else if (blApenasConsulta)
                        {
                            Imagens.g_TamanhoDoArquivo[0] = rdrIdentificado.GetByte(0);
                            TamanhoDoArquivo.Add(rdrIdentificado.GetByte(0));
                            rdrIdentificado.Close();
                            rdrIdentificado.Dispose();
                            return RetornoTrataImagem.Rok;
                        }
                        byte[] Im_Sinal = (byte[])rdrIdentificado.GetValue(0);

                        strArquivo = DiretorioImagens + dblNuPid.ToString().PadLeft(12, '0');
                        strArquivo = strArquivo + iAnotacao.ToString().PadLeft(4, '0') + iNuSeqFoto.ToString().PadLeft(4, '0');
                        strArquivo = strArquivo + Imagens.Obtem_Extensao_de_Arquivo(rdrIdentificado.GetInt32(4));

                        dblTamanhoGravado = Imagens.GravaArquivoImagem(strArquivo, Im_Sinal);
                        if (dblTamanhoGravado < 1)
                        {
                            rdrIdentificado.Close();
                            rdrIdentificado.Dispose();
                            return RetornoTrataImagem.RFalhaGravacao;
                        }
                        else
                        {
                            NomeDoArquivo.Add(@strArquivo);
                            TamanhoDoArquivo.Add(dblTamanhoGravado);
                            return RetornoTrataImagem.Rok;
                        }
                    }
                }
                else if (rAbreImagem.Equals(RetSqlFunction.retSQLError))
                {
                    return RetornoTrataImagem.RFalhaConexaoIdentificados;
                }
            }
            catch (Exception Err)
            {
                ManipulaErro.MostraErro("ConsultaSinalPac(): ", Err.GetHashCode(), Err.Message, HideMsgBox);
                //ManipulaErro.GravaEventLog("ConsultaSinalPac() : " + Err.Message, Err.GetHashCode());
            }
            return ConsultaSinalPac;
        }


        public RetornoTrataImagem GravaSinalPAC(double dblPid, int iAnotacao, int iSeqFoto, int iCoSinal, int iNuIndicadorSinal, string strArquivo, ref OracleConnection cnConexaoBiografica)
        {
            bool blInclui;
            string strSql, strSqlCount, strWhere;
            double dblTamanhoGravado;
            string strData;
            string[] strListaArquivo = new string[1];
            List<byte[]> strListaArquivoConvertido;
            int iTpSinal, iTotalCount;

            RetornoTrataImagem GravaSinalPac;

            GravaSinalPac = RetornoTrataImagem.RFalhaConexaoBiografica;
            try
            {
                NomeDoArquivo.Clear();
                TamanhoDoArquivo.Clear();

                strSql = "";
                blInclui = false;

                strWhere = "Where  Nu_Pid = " + dblPid.ToString() + " And " +
                           "Nu_Anotacao = " + iAnotacao.ToString() + " And " +
                           "Nu_SeqFoto = " + iSeqFoto.ToString();

                strSqlCount = "";
                strSqlCount = "Select Count(*) from Pid.Sinal_Pac " + strWhere;


                iTotalCount = FunctionSql.ExecutaCount(strSqlCount, cnConexaoBiografica);

                if (iTotalCount < 0)
                {
                    return RetornoTrataImagem.RFalhaConexaoIdentificados;
                }
                else if (iTotalCount > 1)
                {
                    return RetornoTrataImagem.RSinalDuplicado;
                }
                else if (iTotalCount.Equals(1))
                {
                    strData = DateTime.Now.ToString();
                    strSql = "Update Pid.Sinal_Pac set DT_ATUALIZA = To_Date('" + strData + "','dd/mm/yyyy hh24:mi:ss') , " +
                             "IM_SINAL = :BlobParameterImg1 " + strWhere;
                }
                else if (iTotalCount.Equals(0))
                {
                    blInclui = true;
                    Imagens.LimpaArray();
                    /* Faz um Insert na tabela */
                    strData = DateTime.Now.ToString();
                    blInclui = true;

                    iTpSinal = Imagens.ObtemTipoDeArquivo(strArquivo);

                    strSql = string.Format("Insert into Pid.Sinal_Pac" +
                             "(NU_PID, NU_ANOTACAO, NU_SEQFOTO, CO_SINAL,NU_INDICADORSINAL,IM_SINAL, TP_SINAL, DT_ATUALIZA) " +
                             "values({0},{1},{2},{3},{4},:BlobParameterImg1,{5}, to_Date('" + strData + "','dd/mm/yyyy hh24:mi:ss'))", dblPid.ToString(),
                             iAnotacao, iSeqFoto, iCoSinal,
                             iNuIndicadorSinal, iTpSinal);
                }

                /* Fiz esta gambiarra para passar o arquivo a  ser convertido na função Le_Arquivo_Imagem*/
                strListaArquivo[0] = strArquivo;

                /*  Converte o arquivo para bytes e retorna o tamanho do arquivo */
                var RetornoLeArquivoImagem = Imagens.Le_Arquivo_Imagem(strListaArquivo);
                dblTamanhoGravado = RetornoLeArquivoImagem.Item1;
                strListaArquivoConvertido = RetornoLeArquivoImagem.Item2;

                if (dblTamanhoGravado < 1)
                {
                    GravaSinalPac = RetornoTrataImagem.RFalhaGravacao;
                }

                Imagens.g_TamanhoDoArquivo[0] = dblTamanhoGravado;
                RetSqlFunction retImagem;

                retImagem = CarregaParametrosGrava(strListaArquivoConvertido.Count, strSql, strListaArquivoConvertido);

                if (retImagem.Equals(RetSqlFunction.retSQLOk))
                {

                    NomeDoArquivo.Add(strArquivo);
                    TamanhoDoArquivo.Add(dblTamanhoGravado);
                    if (blInclui)
                        GravaSinalPac = RetornoTrataImagem.Rok;
                    else
                        GravaSinalPac = RetornoTrataImagem.RRegistroAtualizado;
                }

                if (DeletaArquivo)
                {
                    Imagens.EliminaArquivo(strListaArquivo);
                }

            }
            catch (Exception Err)
            {
                ManipulaErro.MostraErro("GravaSinalPac: ", Err.GetHashCode(), Err.Message, HideMsgBox);
                return RetornoTrataImagem.RFalhaConexaoIdentificados;
            }
            return GravaSinalPac;
        }

        public RetornoTrataImagem GravaSinalIdentificado(double dblRic, int iAnotacao, int iSeqFoto, string strArquivo, ref double dblIdSinal)
        {
            bool blInclui;
            double dblTamanhoGravado, dblSinal = 0;
            RetSqlFunction rAbreImagem;
            OracleDataReader rdrIdentificado;
            string strSql, strWhere;

            string[] strListaArquivo = new string[1];
            List<byte[]> strListaArquivoConvertido;

            RetornoTrataImagem GravaSinalIdentificado;

            try
            {
                GravaSinalIdentificado = RetornoTrataImagem.RFalhaConexaoBiografica;

                if (!ConectadoGravacao)
                {
                    return RetornoTrataImagem.RFalhaConexaoBiografica;
                }

                dblIdSinal = 0;
                strSql = "Select Count(*) from Imagem.Sinais ";
                strWhere = string.Format("Where Nu_Ric = {0} and Nu_Anotacao = {1} And Nu_SeqFoto = {2}", dblRic, iAnotacao, iSeqFoto);

                strSql = strSql + strWhere;
                var ResultadoAbresult = FunctionSql.AbreResult(strSql, cnConexaoDll.Conectar());
                rAbreImagem = ResultadoAbresult.Item1;
                rdrIdentificado = ResultadoAbresult.Item2;

                if (rAbreImagem.Equals(RetSqlFunction.retSQLOk))
                {
                    blInclui = false;
                    if (rdrIdentificado.Read())
                    {
                        int iTotal = rdrIdentificado.GetInt16(0);
                        if (iTotal.Equals(0))
                        {
                            /* Faz um Insert na tabela */
                            Imagens.LimpaArray();
                            blInclui = true;

                            var RetornoAlocaIdCorpo = AlocaIdCorpo(true);
                            GravaSinalIdentificado = RetornoAlocaIdCorpo.Item1;
                            dblSinal = RetornoAlocaIdCorpo.Item2;

                            dblIdSinal = dblSinal;
                            if (GravaSinalIdentificado.Equals(RetornoTrataImagem.Rok))
                            {
                                strSql = string.Format("Insert into Imagem.Sinais" +
                                     "(ID_SINAL, NU_RIC, NU_ANOTACAO, NU_SEQFOTO, IM_SINAL) " +
                                     "values({0},{1},{2},{3},:BlobParameterImg1)", dblIdSinal.ToString(),
                                     dblRic.ToString(), iAnotacao.ToString(), iSeqFoto.ToString());
                            }
                            else
                            {
                                rdrIdentificado.Close();
                                rdrIdentificado.Dispose();
                                return RetornoTrataImagem.RFalhaGravacao;
                            }
                        }
                        else if (iTotal.Equals(1))
                        {
                            strSql = "Update Imagem.Sinais set IM_SINAL = :BlobParameterImg1 " + strWhere;
                        }
                        else if (iTotal > 0)
                        {
                            rdrIdentificado.Close();
                            rdrIdentificado.Dispose();
                            return RetornoTrataImagem.RSinalDuplicado;
                        }


                        //Gambiarra para passar o arquivo como parâmetro 
                        strListaArquivo[0] = strArquivo;

                        /*  Converte o arquivo para bytes e retorna o tamanho do arquivo */
                        var RetornoLeArquivoImagem = Imagens.Le_Arquivo_Imagem(strListaArquivo);
                        dblTamanhoGravado = RetornoLeArquivoImagem.Item1;
                        strListaArquivoConvertido = RetornoLeArquivoImagem.Item2;

                        if (dblTamanhoGravado < 1)
                        {
                            rdrIdentificado.Close();
                            rdrIdentificado.Dispose();
                            GravaSinalIdentificado = RetornoTrataImagem.RFalhaGravacao;
                        }

                        Imagens.g_TamanhoDoArquivo[0] = dblTamanhoGravado;
                        NomeDoArquivo.Add(strArquivo);
                        TamanhoDoArquivo.Add(dblTamanhoGravado);
                        IdCorpo = dblSinal;

                        RetSqlFunction retImagem;
                        retImagem = CarregaParametrosGrava(strListaArquivoConvertido.Count, strSql, strListaArquivoConvertido);
                        if (retImagem.Equals(RetSqlFunction.retSQLOk))
                        {
                            if (blInclui)
                                GravaSinalIdentificado = RetornoTrataImagem.Rok;
                            else
                                GravaSinalIdentificado = RetornoTrataImagem.RRegistroAtualizado;
                        }
                        if (DeletaArquivo)
                        {
                            Imagens.EliminaArquivo(strListaArquivo);
                        }
                        rdrIdentificado.Close();
                        rdrIdentificado.Dispose();
                        return GravaSinalIdentificado;

                    }
                    else
                    {
                        return RetornoTrataImagem.RFalhaConexaoIdentificados;
                    }
                }
                else
                {
                    return RetornoTrataImagem.RFalhaConexaoIdentificados;
                }
            }
            catch (Exception Err)
            {
                ManipulaErro.MostraErro("GravaSinalIdentificado: ", Err.GetHashCode(), Err.Message, HideMsgBox);
                return RetornoTrataImagem.RFalhaConexaoIdentificados;
            }
        }

        public RetSqlFunction RollBack()
        {
            try
            {
                cnConexaoDll.OraTransactiondll.Rollback();
                return RetSqlFunction.retSQLOk;
            }
            catch (Exception ex)
            {
                ManipulaErro.MostraErro("RollBack(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
                return RetSqlFunction.retSQLError;
            }
        }

        public RetSqlFunction Commit()
        {
            try
            {
                cnConexaoDll.OraTransactiondll.Commit();
                return RetSqlFunction.retSQLOk;
            }
            catch (Exception ex)
            {
                ManipulaErro.MostraErro("Commit(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
                return RetSqlFunction.retSQLError;
            }
        }

        private RetSqlFunction CarregaParametrosGrava(int iQtdeImagens, string strSql, List<byte[]> lstImagens, List<int> lstTipoImagem = null)
        {
            try
            {
                int iParametro = 1;
                cnConexaoDll.OraComando.Connection = cnConexaoDll.Conectar();
                if (!cnConexaoDll.Transaction)
                    cnConexaoDll.OraTransactiondll = cnConexaoDll.IniciaTransacao();

                cnConexaoDll.OraComando.CommandText = strSql;
                cnConexaoDll.OraComando.Transaction = cnConexaoDll.OraTransactiondll;

                OracleParameter[] BlobParameterImg = new OracleParameter[iQtdeImagens];


                for (int iImagem = 0; iImagem < iQtdeImagens; iImagem++)
                {
                    BlobParameterImg[iImagem] = new OracleParameter();
                    BlobParameterImg[iImagem].OracleDbType = OracleDbType.Blob;
                    BlobParameterImg[iImagem].ParameterName = string.Format("BlobParameterImg{0}", iParametro.ToString());
                    BlobParameterImg[iImagem].Value = lstImagens[iImagem];
                    cnConexaoDll.OraComando.Parameters.Add(BlobParameterImg[iImagem]);
                    iParametro++;
                }

                if (lstTipoImagem != null)
                {
                    OracleParameter[] TpImagem = new OracleParameter[iQtdeImagens];
                    for (int iImagem = 0; iImagem < iQtdeImagens; iImagem++)
                    {
                        TpImagem[iImagem] = new OracleParameter();
                        TpImagem[iImagem].OracleDbType = OracleDbType.Int32;
                        TpImagem[iImagem].ParameterName = string.Format("TpImagem{0}", iParametro.ToString());
                        TpImagem[iImagem].Value = lstTipoImagem[iImagem];
                        cnConexaoDll.OraComando.Parameters.Add(TpImagem[iImagem]);
                        iParametro++;
                    }
                }
                cnConexaoDll.OraComando.ExecuteNonQuery();
                cnConexaoDll.OraComando.Parameters.Clear();
                return RetSqlFunction.retSQLOk;
            }
            catch (Exception ex)
            {
                ManipulaErro.MostraErro("GravaImagem(): ", ex.GetHashCode(), ex.Message, HideMsgBox);
                return RetSqlFunction.retSQLError;
            }
        }
    }

}