using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Constantes;

namespace Classes
{
    public static class ManipulaErro
    {

        [DllImport("User32.dll", EntryPoint = "MessageBox", CharSet = CharSet.Auto)]
        internal static extern int MsgBox(int hWnd, string lpText, string lpCaption, uint uType);

        public static string LogFileName { get; private set; }
        public static string UltimaMensagemErro { get; private set; }

        /******* Inicializa Classe ******/
        static ManipulaErro()
        {
            if (LogFileName == null)
            {
                Imagens.CriaPath(Constante.strPathLog);
                LogFileName = string.Format("{0}MSG{1}{2}.log", Constante.strPathLog, DateTime.Now.ToString("yyyyMMdd"), Process.GetCurrentProcess().ProcessName);
            }
        }

        public static void LimpaMesagemErro()
        {
            UltimaMensagemErro = "";
        }

        public static bool LogarMensagem(string strMensagem, long lngErrNumber, string strErrDescription)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(LogFileName, true))
                {
                    //se usuário informou o número do erro do sistema
                    if (lngErrNumber != 0)
                    {
                        strMensagem = strMensagem + " ; Número " + lngErrNumber.ToString();
                    }

                    //se usuário informou a descrição do erro do sistema
                    if (strErrDescription != "")
                    {
                        strMensagem = strMensagem + "; Descrição: " + strErrDescription;
                    }

                    strMensagem = strMensagem + " - Hora : " + DateTime.Now.ToString();
                    writer.WriteLine(strMensagem);
                }
                return true;
            }
            catch (Exception ex)
            {
                string strMsgErro = " " + ex.Message + " Classe Manipula Erro";
                MsgBox(0, strMsgErro, "Alerta", 0);
                return false;
            }
        }

        public static void MostraErro(string strMensagem, long lngErrNumber = 0, string strErrDescription = "", bool bolHideMensagem = true)
        {
            try
            {
                UltimaMensagemErro = strMensagem;

                //se usuário informou o número do erro do sistema
                if (lngErrNumber != 0)
                {
                    strMensagem = strMensagem + " Número: " + lngErrNumber;
                }

                //se usuário informou a descrição do erro do sistema
                if (strErrDescription != "")
                {
                    strMensagem = strMensagem + "  Descrição: " + strErrDescription;
                }

                LogarMensagem(strMensagem, lngErrNumber, strErrDescription);

                // se o usuario quer mostrar a mensagem (ShowMessage)
                if (!bolHideMensagem)
                    ManipulaErro.MsgBox(0, strMensagem, "Alerta !", 0);

            }
            catch (Exception ex)
            {
                strMensagem = string.Format("MostraErro(): {0} Número: {1} Descrição : {2}",
                strMensagem, ex.Source, ex.Message);
                ManipulaErro.MsgBox(0, strMensagem, "Alerta !", 0);
            }
        }

        public static void GravaEventLog(string strMensagem, int codErro)
        {

            /* Para visualizar o Evento : Abrir o Visualizador de Eventos
             * localizado no Painel de Controle -> Ferramentas Administrativas
             * opção Log do Windows -> Aplicativo */

            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(strMensagem, EventLogEntryType.Error, 101, 1);
            }

        }
    }
}
