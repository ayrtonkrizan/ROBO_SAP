using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ROBO
{
    public static class Arquivo
    {
        public static void GerarArquivoTXT(string _path, string _name, string[] _linhas, bool _sobrescrever = true)
        {
            string completePath = _path + "\\" + _name + ".txt";
            StreamWriter sw = new StreamWriter(completePath, !_sobrescrever);
            StringBuilder sb = new StringBuilder();

            foreach (var linha in _linhas)
                sb.AppendLine(linha);

            sw.Write(sb);
            sw.Close();
        }
    }
}
