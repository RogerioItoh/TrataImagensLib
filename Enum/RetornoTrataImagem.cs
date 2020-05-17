namespace Enum
{ 
    public enum RetornoTrataImagem : int
    {
        REmBackup = -18,
        RParticaoNaoCriada = -17,
        RParticaoCheia = -16,
        RArquivoNaoApagado = -15,
        RFaltaImagem = -14,
        RRegistroNaoExiste = -13,
        RMaisdeUmArquivo = -12,
        RFalhaInicioTransacao = -11,
        RSinalDuplicado = -10,
        RFalhaGravacao = -9,
        RMaisdeUmPIDcomImagem = -8,
        RFalhaNovoID = -7,
        RFalhaConexaoIdentificados = -6,
        RTipodeImagemInvalido = -5,
        RUsuarioExpirado = -4,
        RErroPassword = -3,
        RFalhaConexaoBiografica = -2,
        RSemPrivilegio = -1,
        Rok = 0,
        RSenhaNaoAlterada = 1,
        RRegistroAtualizado = 2,
        ROkArquivoNaoApagado = 3,
        RRegAtualizadoArquivoNaoApagado = 4,
        RFaltamArquivos = 5
    };

}
