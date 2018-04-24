using System;
using System.Collections.Generic;
namespace ROBO.Transacoes.Entidade
{
    class Transaction_ErrorDao
    {
        /// <summary>
        /// Registra no log a transação que ocorreu erro.
        /// </summary>
        /// <param name="Id">Id da Transação</param>
        /// <param name="transType">Tipo de transação</param>
        /// <param name="message">Mensagem a registrar</param>
        public static void Register(int id, int transType, string message)
        {
            using(var rs = new MyLibs.v2.sbo.DI.MyRecordSet())
            {
                var sql = String.Format(Properties.Querys.InsereLog_3,id,transType,message);
                rs.Execute(sql);
            }
        }

        /// <summary>
        /// Remove todos os registros.
        /// </summary>
        /// <param name="Id">Id da Transação</param>
        /// <param name="transType">Tipo de transação</param>
        public static void RemoveIfExist(int id, int transType)
        {
            using (var rs = new MyLibs.v2.sbo.DI.MyRecordSet())
            {
                var sql = String.Format(Properties.Querys.ApagaLog_2, id, transType);
                rs.Execute(sql);
            }
        }
    }
}
