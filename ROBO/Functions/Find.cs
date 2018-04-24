using MyLibs.v2.local.Register;
using MyLibs.v2.sbo.DI;
using System;
using System.Collections.Generic;

namespace ROBO.Functions
{
    class Find
    {
        /// <summary>
        /// Faz uma consulta na tabela RSD_IsItemOrResource
        /// </summary>
        /// <param name="reference">Valor do recurso ou código de barras.</param>
        /// <returns></returns>
        public static string Item(string reference)
        {
            using(var rs = new MyRecordSet())
            {
                rs.DoQuery(String.Format("SELECT Codigo FROM RSD_IsItemOrResource WHERE Referencia = '{0}'", reference));

                if(rs.HasNext())
                {
                    return rs.GetFieldValue("Codigo").ToString();
                }
                else
                {
                    throw new LogException("RSD_IsItemOrResource"
                        ,System.Diagnostics.EventLogEntryType.Error
                        , Properties.LogMessage.GLOBAL_0029_1
                        , reference);
                }
            }  
        }
    }
}
