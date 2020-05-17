namespace Constantes
{
    static class Constante
    {
        public const string strDiretorioImagem = @"C:\ImagensDic\";
        public const string strPathLog = @"C:\DETRAN - RJ\LOGS\";

        public const string ConsultaImagemTodos = "Select DBMS_LOB.GetLength(Im_Dedo_01), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_02), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_03), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_04), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_05), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_06), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_07), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_08), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_09), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_10), " +
                                                      "DBMS_LOB.GetLength(Im_Foto), " +
                                                      "DBMS_LOB.GetLength(Im_Assinatura) ";


        public const string ConsultaImagemDedos = "Select DBMS_LOB.GetLength(Im_Dedo_01), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_02), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_03), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_04), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_05), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_06), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_07), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_08), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_09), " +
                                                      "DBMS_LOB.GetLength(Im_Dedo_10)  ";

        public const string ImagemTodos = "Select Im_Dedo_01, " +
                                              "Im_Dedo_02, " +
                                              "Im_Dedo_03, " +
                                              "Im_Dedo_04, " +
                                              "Im_Dedo_05, " +
                                              "Im_Dedo_06, " +
                                              "Im_Dedo_07, " +
                                              "Im_Dedo_08, " +
                                              "Im_Dedo_09, " +
                                              "Im_Dedo_10, " +
                                              "Im_Foto, " +
                                              "Im_Assinatura ";


        public const string ImagemDedos = "Select Im_Dedo_01, " +
                                              "Im_Dedo_02, " +
                                              "Im_Dedo_03, " +
                                              "Im_Dedo_04, " +
                                              "Im_Dedo_05, " +
                                              "Im_Dedo_06, " +
                                              "Im_Dedo_07, " +
                                              "Im_Dedo_08, " +
                                              "Im_Dedo_09, " +
                                              "Im_Dedo_10, " +
                                              "Im_Foto, " +
                                              "Im_Assinatura ";

        public const string SqlTodos = "Select Im_Dedo_01, Im_Dedo_02," +
                                       "Im_Dedo_03, Im_Dedo_04," +
                                       "Im_Dedo_05, Im_Dedo_06," +
                                       "Im_Dedo_07, Im_Dedo_08," +
                                       "Im_Dedo_09, Im_Dedo_10," +
                                       "Im_Foto, Im_Assinatura," +
                                       "Tp_Dedo_01, Tp_Dedo_02," +
                                       "Tp_Dedo_03, Tp_Dedo_04," +
                                       "Tp_Dedo_05, Tp_Dedo_06," +
                                       "Tp_Dedo_07, Tp_Dedo_08," +
                                       "Tp_Dedo_09, Tp_Dedo_10, " +
                                       "Tp_Foto, Tp_Assinatura ";

        public const string SqlDedos = "Select Im_Dedo_01, Im_Dedo_02," +
                                       "Im_Dedo_03, Im_Dedo_04," +
                                       "Im_Dedo_05, Im_Dedo_06," +
                                       "Im_Dedo_07, Im_Dedo_08," +
                                       "Im_Dedo_09, Im_Dedo_10," +
                                       "Tp_Dedo_01, Tp_Dedo_02," +
                                       "Tp_Dedo_03, Tp_Dedo_04," +
                                       "Tp_Dedo_05, Tp_Dedo_06," +
                                       "Tp_Dedo_07, Tp_Dedo_08," +
                                       "Tp_Dedo_09, Tp_Dedo_10 ";




    }
}
