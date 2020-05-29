using System.IO;
using System.Collections.Generic;
using Constantes;
using System.Drawing;
using Enum;
using System;

namespace Classes
{

    public static class Imagens
    {

        public static string g_strDsNameIdentificados;
        public static string g_strDiretorioImagens;
        public static string[] g_NomeDoArquivo = new string[12];
        public static double[] g_TamanhoDoArquivo = new double[12];

        public static RetornoTrataImagem EliminaArquivo(string[] lstArquivo)
        {
            try
            {
                foreach (string strArquivo in lstArquivo)
                    File.Delete(@strArquivo);
                return RetornoTrataImagem.Rok;
            }
            catch (Exception Err)
            {
                ManipulaErro.MostraErro("EliminaArquivo():", Err.GetHashCode(), Err.Message, false);
                ManipulaErro.GravaEventLog("EliminaArquivo" + Err.Message, Err.GetHashCode());
                return RetornoTrataImagem.RArquivoNaoApagado;
            }
        }

        public static bool CriaPath(string strPath)
        {
            try
            {
                if (Directory.Exists(strPath))
                {
                    return true;
                }
                else
                {
                    Directory.CreateDirectory(strPath);
                    return true;
                }
            }
            catch (Exception Err)
            {
                ManipulaErro.MostraErro("EliminaArquivo():", Err.GetHashCode(), Err.Message, false);
                ManipulaErro.GravaEventLog("EliminaArquivo" + Err.Message, Err.GetHashCode());
                return false;
            }
        }

        public static void GeraArquivoIni()
        {
            IniFile ini = new IniFile("TrataImagens.Ini");
            ini.IniWriteString("Arquivos de Imagens", "Diretorio", Constante.strDiretorioImagem);
        }

        public static void LimpaArray()
        {
            for (int iContador = 0; iContador < 11; iContador++)
            {
                g_NomeDoArquivo[iContador] = "";
                g_TamanhoDoArquivo[iContador] = -1;
            }
        }

        public static string Obtem_Extensao_de_Arquivo(int ICodigoExtensao)
        {
            string strExtensao;
            switch (ICodigoExtensao)
            {
                case 0:
                    strExtensao = ".Wsq";
                    break;
                case 1:
                    strExtensao = ".Jpg";
                    break;
                case 2:
                    strExtensao = ".Bmp";
                    break;
                case 3:
                    strExtensao = ".Tif";
                    break;
                case 4:
                    strExtensao = ".Gif";
                    break;
                default:
                    strExtensao = "";
                    break;
            }
            return strExtensao;
        }


        public static int ObtemTipoDeArquivo(string strArquivo)
        {
            int iPosicaoPonto = strArquivo.IndexOf(".");

            if (iPosicaoPonto > 0)
            {
                iPosicaoPonto += 1;
                string strExtensao = strArquivo.Substring(iPosicaoPonto, strArquivo.Length - iPosicaoPonto).Trim();

                switch (strExtensao.ToUpper())
                {
                    case ("JPG"):
                        return TipoExtensao.Jpg.GetHashCode();
                    case "JPEG":
                        return TipoExtensao.Jpg.GetHashCode();
                    case "BMP":
                        return TipoExtensao.Bmp.GetHashCode();
                    case "WSQ":
                        return TipoExtensao.Wsq.GetHashCode();
                    case "TIF":
                        return TipoExtensao.Tif.GetHashCode();
                    case "TIFF":
                        return TipoExtensao.Tif.GetHashCode();
                    case "GIF":
                        return TipoExtensao.Gif.GetHashCode();
                    case "GIFF":
                        return TipoExtensao.Gif.GetHashCode();
                    default:
                        return TipoExtensao.Desconhecida.GetHashCode();
                }
            }
            else
            {
                return TipoExtensao.Desconhecida.GetHashCode();
            }
        }

        /* retorna dois valores string  */
        public static (string, string) Nome_Campo(TipoDeImagem enumTipoDeImagem)
        {
            string strNomeCampo = "";
            string strCampoTipoDeImagem = "";

            switch (enumTipoDeImagem)
            {
                case TipoDeImagem.PolegarDireito:
                    strNomeCampo = "IM_DEDO_01";
                    strCampoTipoDeImagem = "TP_DEDO_01";
                    break;
                case TipoDeImagem.IndicadorDireito:
                    strNomeCampo = "IM_DEDO_02";
                    strCampoTipoDeImagem = "TP_DEDO_02";
                    break;
                case TipoDeImagem.MedioDireito:
                    strNomeCampo = "IM_DEDO_03";
                    strCampoTipoDeImagem = "TP_DEDO_03";
                    break;
                case TipoDeImagem.AnularDireito:
                    strNomeCampo = "IM_DEDO_04";
                    strCampoTipoDeImagem = "TP_DEDO_04";
                    break;
                case TipoDeImagem.MinimoDireito:
                    strNomeCampo = "IM_DEDO_05";
                    strCampoTipoDeImagem = "TP_DEDO_05";
                    break;
                case TipoDeImagem.PolegarEsquerdo:
                    strNomeCampo = "IM_DEDO_06";
                    strCampoTipoDeImagem = "TP_DEDO_06";
                    break;
                case TipoDeImagem.IndicadorEsquerdo:
                    strNomeCampo = "IM_DEDO_07";
                    strCampoTipoDeImagem = "TP_DEDO_07";
                    break;
                case TipoDeImagem.MedioEsquerdo:
                    strNomeCampo = "IM_DEDO_08";
                    strCampoTipoDeImagem = "TP_DEDO_08";
                    break;
                case TipoDeImagem.AnularEsquerdo:
                    strNomeCampo = "IM_DEDO_09";
                    strCampoTipoDeImagem = "TP_DEDO_09";
                    break;
                case TipoDeImagem.MinimoEsquerdo:
                    strNomeCampo = "IM_DEDO_10";
                    strCampoTipoDeImagem = "TP_DEDO_10";
                    break;
                case TipoDeImagem.Foto:
                    strNomeCampo = "IM_FOTO";
                    strCampoTipoDeImagem = "TP_FOTO";
                    break;
                case TipoDeImagem.Assinatura:
                    strNomeCampo = "IM_ASSINATURA";
                    strCampoTipoDeImagem = "TP_ASSINATURA";
                    break;
                case TipoDeImagem.Sinal:
                    strNomeCampo = "IM_SINAL";
                    strCampoTipoDeImagem = "TP_SINAL";
                    break;
            }
            return (strNomeCampo, strCampoTipoDeImagem);
        }


        public static double GravaArquivoImagem(string strNomeArquivo, byte[] campo)
        {
            double dblBufferLength = 0;
            try
            {
                StreamWriter oStreamWriter;
                byte[] buffer = campo;
                // Lê o conteúdo dos campos.

                if (buffer.Length > 0)
                {

                    /* Verifico de o arquivo existe e deleto*/
                    if (File.Exists(@strNomeArquivo))
                        File.Delete(@strNomeArquivo);

                    oStreamWriter = new StreamWriter(strNomeArquivo, false);
                    oStreamWriter.BaseStream.Write(buffer, 0, buffer.Length);
                    // Fecha e limpa o objeto 
                    oStreamWriter.Close();
                }
                dblBufferLength = buffer.Length;

            }
            catch (Exception erro)
            {
                ManipulaErro.MostraErro("GravaArquivoImagem(): " + erro.Message, erro.GetHashCode());
            }
            return dblBufferLength;
        }


        public static (double, List<byte[]>, List<int>) Le_Arquivo_Imagem(string[] strArquivoaLer)
        {
            /* Esta função lê os arquivos Wsq e Jpg  e converte em bytes para serem 
             Gravados na tabela */

            List<byte[]> lstImagemConvertida = new List<byte[]>();
            List<int> lstTipoExtensao = new List<int>();
            double dblTamanhoArquivo = 0;
            int iTipoExtensao;

            try
            {
                // Verifico se a lista de arquivos não está zerada
                for (int iArquivo = 0; iArquivo < strArquivoaLer.Length; iArquivo++)
                {
                    // Verifico se a extensão do arquivo é válida.
                    iTipoExtensao = ObtemTipoDeArquivo(strArquivoaLer[iArquivo]);

                    /* Tipo de Extensão desconhecida */
                    if (iTipoExtensao.Equals(9))
                    {
                        dblTamanhoArquivo = 0;
                        lstTipoExtensao.Clear();
                        lstTipoExtensao.Add(9);
                        lstImagemConvertida.Clear();
                        break;
                    }

                    FileStream fs = new FileStream(strArquivoaLer[iArquivo], FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);
                    if (fs.Length > 0)
                    {
                        byte[] imagemArray = br.ReadBytes((int)fs.Length);

                        // Adiciona o arquivo convertido ao array
                        lstImagemConvertida.Add(imagemArray);
                        lstTipoExtensao.Add(iTipoExtensao);

                        dblTamanhoArquivo += fs.Length;
                    }
                    else
                    {
                        // Se encontrar um arquivo vazio interrompe a rotina
                        dblTamanhoArquivo = 0;
                        lstImagemConvertida.Clear();
                        lstTipoExtensao.Clear();
                        break;
                    }

                    br.Close();
                    fs.Close();
                }
                /* Retorna o tamanho total dos arquivos convertidos
                 * Uma lista como a conversão dos arquivos para Byte
                 * e lista com o tipo de Extensao */
                
                return (dblTamanhoArquivo, lstImagemConvertida, lstTipoExtensao);
            }
            catch (Exception ex)
            {
                lstImagemConvertida.Clear();
                lstTipoExtensao.Clear();
                ManipulaErro.MostraErro("Le_Arquivo_Imagem(): Erro na conversão de arquivos.", ex.GetHashCode(), ex.Message);
                ManipulaErro.LogarMensagem("Le_Arquivo_Imagem(): Erro na conversão de arquivos.", ex.GetHashCode(), ex.Message);
                return (0, lstImagemConvertida, lstTipoExtensao);
            }
        }
        

        public static Image ConverteByteParaImagem(Object valor)
        {

            //Converte o campo da Foto ou Assinatura para Imagem
            Image imagem = null;
            try
            {
                if (valor != System.DBNull.Value)
                {
                    //converte os bytes vindos do banco em uma imagem
                    byte[] imagem_aray = (byte[])valor;
                    MemoryStream ms = new MemoryStream(imagem_aray);
                    imagem = Image.FromStream(ms, false);
                }
            }
            catch
            {
                imagem = null;
            }

            return imagem;
        }

        public static MemoryStream ConverteByteParaWsq(Object objValor)
        {

            /* Converte os campos WSQ */
            MemoryStream ms = null;
            try
            {
                if (objValor != System.DBNull.Value)
                {
                    //converte os bytes vindos do banco em uma imagem
                    byte[] imagem_aray = (byte[])objValor;
                    ms = new MemoryStream(imagem_aray);
                    return ms;
                }
            }
            catch
            {
                ms = null;
            }
            return ms;
        }

        public static void SalvaFotoAssinatura(string arquivo, Image imagem)
        {
            try
            {
                imagem.Save("C:\\Temp\\" + arquivo);
            }
            catch
            {
                //MessageBox(0, "Erro ao salvar o " + arquivo, "Alerta", 0);
            }
        }

        public static void SalvaWsq(string arquivo, MemoryStream msWsq)
        {
            try
            {
                using (FileStream file = new FileStream("C:\\temp\\" + arquivo, FileMode.Create, FileAccess.Write))
                    msWsq.CopyTo(file);
            }
            catch
            {
                //MessageBox(0, "Erro ao salvar o " + arquivo, "Alerta", 0);
            }
        }


    }

}
