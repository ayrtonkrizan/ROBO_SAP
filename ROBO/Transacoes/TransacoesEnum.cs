using System;
using System.Collections.Generic;
using System.Text;

namespace ROBO.Transacoes
{

    #region Enum
    public enum TransEnum
    {            
        T38DevolucaoSobra          = -38,
        T32DevolucaoPedidoDeVenda  = -32,
        T08CancelamentoNovaPeca    = -8,
        T01RecebimentoMercadoria   = 1,
        T05AumentoValorItem        = 5,    /*Generic*/
        T06EntradaLocalizacaoPeca  = 6,    /*Generic*/
        T08CriacaoNovaPeca         = 8,            
        T32PedidoDeVenda           = 32,
        T35ReducaoValorItem        = 35,   /*Generic*/
        T36SaidaLocalizacaoPeca    = 36,   /*Generic*/
        T38ComsumirMateiral        = 38,
        T98AjustFiesicoEstoque     = 98,
        TWOCriacaoOrdemProdMont    = 40,  /*  WO */
        T41AlteraEstruturaOP     = 41,  /*  41 */
        TWOCriacaoOrdemProdDesm    = -40, /* -WO */
        T00EstruturaProutdos
    }
    #endregion
}
