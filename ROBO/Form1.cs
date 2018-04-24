using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SAPbobsCOM;
using System.IO;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using MyLibs;
using B1WizardBase;
namespace ROBO
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string t = JeruelDLL.OPService.Import(textBox1.Text);
            //MessageBox.Show(t);
        }

        public static Company oCompany { get; set; }
        
        private void button2_Click(object sender, EventArgs e)
        {
            connectaB1();
            oCompany.Disconnect();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int i = 0;
            connectaB1();

            if (oCompany.Connected == true)
            {
                var tabs = tabelasDB();
                progressBar1.Minimum = 0;
                progressBar1.Maximum = tabs.Length;
                for (int j = 0; j < tabs.Length; j++)
                {
                    lblMsg.Text = string.Format("MSG: Tabelas {0}/{1}", j + 1, tabs.Length);
                    progressBar1.Value = j;
                    i = tabs[j].Add(oCompany);
                    if (i != 0)
                        MessageBox.Show(string.Format("Erro ao criar tabela {0} - Erro: {1} - Mensagem: {2}", tabs[j].Name, i.ToString(), oCompany.GetLastErrorDescription().ToString()));
                }


                var cols = colunasDB();
                progressBar1.Minimum = 0;
                progressBar1.Maximum = cols.Length;
                for (int j = 0; j < cols.Length; j++)
                {
                    lblMsg.Text = string.Format("MSG: Colunas {0}/{1}", j + 1, cols.Length);
                    progressBar1.Value = j;
                    i = cols[j].Add(oCompany);
                    if (i != 0)
                        MessageBox.Show(string.Format("Erro ao criar campo {0} - Erro: {1} - Mensagem: {2}", cols[j].Name, i.ToString(), oCompany.GetLastErrorDescription().ToString()));
                }

                udosDB();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            oCompany = principal.conexao(Server.Text, Company.Text, DbUser.Text, DbPass.Text, UserSap.Text, PassSAP.Text, DbServerType.Text, License.Text);

            if(oCompany.Connected == true)
            {

            }
            else
            {
                MessageBox.Show("Não conectado");
            }
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            Server.Text = ConfigurationManager.AppSettings["Server"];
            Company.Text = ConfigurationManager.AppSettings["CompanyDB"];
            DbUser.Text = ConfigurationManager.AppSettings["DbUser"];
            DbPass.Text = ConfigurationManager.AppSettings["DbPass"];
            UserSap.Text = ConfigurationManager.AppSettings["SAPUser"];
            PassSAP.Text = ConfigurationManager.AppSettings["SAPPass"];

            License.Text = ConfigurationManager.AppSettings["SAPLicense"];
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            
        }

        private void richTextBox1_TextChanged(object sender, System.EventArgs e)
        {

        }


        private void connectaB1()
        {
            oCompany = new Company();
            oCompany.Server = Server.Text;
            oCompany.CompanyDB = Company.Text;
            //oCompany.DbUserName = DbUser.Text;
            //oCompany.DbPassword = DbPass.Text;
            oCompany.UserName = UserSap.Text;
            oCompany.Password = PassSAP.Text;

            switch (DbServerType.Text)
            {
                case "dst_MSSQL2008":
                    oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2008;
                    break;

                case "dst_MSSQL2012":
                    oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2012;
                    break;

                case "dst_MSSQL2014":
                    oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2014;
                    break;

                case "dst_HANADB":
                    oCompany.DbServerType = BoDataServerTypes.dst_HANADB;
                    break;
            }
            oCompany.LicenseServer = License.Text;

            // tenta conectar no sap
            if (oCompany.Connect() != 0)
            {
                MessageBox.Show("Não conectado " + oCompany.GetLastErrorDescription().ToString());
            }
            else
            {
                MessageBox.Show("Conectado com Sucesso.");
            }
        }


        private B1DbTable[] tabelasDB()
        {
            var Tables = new B1DbTable[] {
                
                new B1DbTable("@RSMV_OTXR", "RSD : Registro de Tributação", BoUTBTableType.bott_MasterData)
                ,new B1DbTable("@RSMV_OSLT", "RSD : Tipos de Pedidos", BoUTBTableType.bott_MasterData)
                ,new B1DbTable("@RSMV_MAPUTIL", "RSD:Mapeamento de Utilização ", BoUTBTableType.bott_MasterData)
                ,new B1DbTable("@RSMV_MAPUTIL_DET", "RSD:Mapeamento de Utili DET", BoUTBTableType.bott_MasterDataLines)
                ,new B1DbTable("@RSMV_OBON", "RSD:Motivo de Bonificação ", BoUTBTableType.bott_MasterData)
                ,new B1DbTable("@RSMV_OSLR", "RSD:Motivo de Venda Não Venda ", BoUTBTableType.bott_MasterData)
                ,new B1DbTable("@RSMV_PARAM", "RSD:Parâmetros de Config", BoUTBTableType.bott_MasterData)
            };

            return Tables;
        }
        private B1DbColumn[] colunasDB()
        {
            var Columns = new B1DbColumn[] {

                //Cond.Pgto - CamposUsuarios
                new B1DbColumn("OCTG", "DiscIncl", "Abater Comissao", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OCTG", "Sample", "Amostra", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)   
                ,new B1DbColumn("OCTG", "AdvPmt", "Pgto Antecipado", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OCTG", "Discount", "Desconto", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 10, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("OCTG", "ToAppr", "Sujeito a aprovação", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                
                //Regioes - CamposUsuarios
                ,new B1DbColumn("OTER", "MinOrderAmt", "Valor minimo pgto", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OTER", "MinInstAmt", "Valor minimo Parcela", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OTER", "DiscFrght", "Desconto", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OTER", "MaxIntr", "Qtd dias fatura", BoFieldTypes.db_Numeric, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OTER", "DiaEntr", "Dias para Entrega", BoFieldTypes.db_Numeric, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OTER", "DiaMax", "Dia Máximo Fat", BoFieldTypes.db_Numeric, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OTER", "MaxIntr", "Qtd dias fatura", BoFieldTypes.db_Numeric, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OTER", "DiscIncl", "Abater Comissao", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OTER", "WDay1", "Domingo", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OTER", "WDay2", "Segunda-feira", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OTER", "WDay3", "Terça-feira", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OTER", "WDay4", "Quarta-Feira", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OTER", "WDay5", "Quinta-Feira", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OTER", "WDay6", "Sexta-Feira", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OTER", "WDay7", "Sábado", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OTER", "MinInvIntr", "Qtd min dia entrega", BoFieldTypes.db_Numeric, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)

                //Clientes - CamposUsuarios
                ,new B1DbColumn("OCRD", "WDay1", "Domingo", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OCRD", "WDay2", "Segunda-feira", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OCRD", "WDay3", "Terça-feira", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OCRD", "WDay4", "Quarta-Feira", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OCRD", "WDay5", "Quinta-Feira", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OCRD", "WDay6", "Sexta-Feira", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OCRD", "WDay7", "Sábado", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)    
                ,new B1DbColumn("OCRD", "TextPV", "Texto para PV", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 50, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("OCRD", "IDAFV", "Sábado", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("OCRD", "Transp", "Transportadora", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] {}, -1)    


                //Pedido de Venda
                ,new B1DbColumn("ORDR", "IDAFV", "Id do AFV", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("ORDR", "TpPedido", "Tipo de Pedido", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("ORDR", "DtEntregaPL", "Data de Entrega Planejada", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 20, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("ORDR", "MotBon", "Motivo de Bonificação", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 50, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("ORDR", "AproInterno", "Aprovação Interna", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("ORDR", "MotAprov", "Motivo para aprovação", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("ORDR", "Campanha", "Campanha", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)    

                ,new B1DbColumn("RDR1", "DescPerm", "Desconto Permitido", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 15, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("RDR1", "DescAbat", "Desconto Abatido de Comiss", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 15, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("RDR1", "PercComiss", "Percentual Comiss", BoFieldTypes.db_Float, BoFldSubTypes.st_Percentage, 15, new B1DbValidValue[] {}, -1)    
                ,new B1DbColumn("RDR1", "Campanha", "Campanha", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)    


                //Tabela de Preços - CamposUsuarios
                ,new B1DbColumn("ITM1", "MinOrderAmt", "Valor min pgto", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("ITM1", "MinInstAmt", "Valor min Parce", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("ITM1", "DiscFrght", "Desconto", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("ITM1", "DiscIncl", "Abater Comissao", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)

                //Descontos por volume
                ,new B1DbColumn("ITM1", "DiscIncl", "Abater Comissao", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)
                ,new B1DbColumn("SSP2", "DiscIncl", "Abater Comissao", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)

                //Lista de Preço
                ,new B1DbColumn("OPLN", "DescMax", "Desconto Maximo", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)
                ,new B1DbColumn("OPLN", "AbatComiss", "Abat Comissão", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)

                //Catalogo de PN
                ,new B1DbColumn("OSCN", "ExportAFV", "Exporta AFV", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)

                //Motivos de n visita e n venda
                ,new B1DbColumn("@RSMV_OSLR", "Tipo", "Tipo", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OSLR", "Descricao", "Descricao", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)

                //Motivo de Bonificação
                ,new B1DbColumn("@RSMV_OBON", "Descricao", "Descricao", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 50, new B1DbValidValue[] {}, -1)

                //Vendedores
                ,new B1DbColumn("OSLP", "TabletPwd", "Senha Tablet", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OSLP", "MinVer", "Versao Minima", BoFieldTypes.db_Numeric, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OSLP", "MaxVer", "Versao Maxima", BoFieldTypes.db_Numeric, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)

                //Grupos de comissões
                ,new B1DbColumn("OCOG", "PriceList", "CodigoTabelaPreco", BoFieldTypes.db_Numeric, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OCOG", "DiscFrom", "Descontono inicial", BoFieldTypes.db_Numeric, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OCOG", "DiscTo", "Desconto Final", BoFieldTypes.db_Numeric, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)

                //Tipo de Envio 
                ,new B1DbColumn("OSHP", "GerAtividade", "GerAtividade", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OSHP", "Atividade", "Atividade", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OSHP", "Tipo", "Tipo", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("OSHP", "Assunto", "Assunto", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1) 

                //Periodo e desconto por quantidade
                ,new B1DbColumn("OSPP", "AbatComiss", "Abater da Comissão", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1) 

                //Tipo de Pedido 
                ,new B1DbColumn("@RSMV_OSLT", "Descricao", "Descrição", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 50, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OSLT", "DocGerado", "Doc Gerado", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OSLT", "GerOpor", "Gera oportunidade documento", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OSLT", "Utilizacao", "Utilização", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)

                //Mapeamento de Utilização
                ,new B1DbColumn("@RSMV_MAPUTIL_DET", "UsageDefault", "Usage Default", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_MAPUTIL_DET", "ITMGrupo", "Grupo de Itens", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_MAPUTIL_DET", "DescrGrupo", "Descrição do Grupo ", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 100, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_MAPUTIL_DET", "Usage", "Utilização", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 15, new B1DbValidValue[] {}, -1)

                //Tributacao (TUser)
                ,new B1DbColumn("@RSMV_OTXR", "CNAE", "Codigo CNAE", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OTXR", "BPCode", "CodigoCliente", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OTXR", "NCM", "NCM do Prod", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OTXR", "CEST", "Codigo CEST", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OTXR", "State", "UF de Destino", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] {}, -1)

                ,new B1DbColumn("@RSMV_OTXR", "FP", "Zona Franca", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)
                ,new B1DbColumn("@RSMV_OTXR", "Exmp", "Isento", BoFieldTypes.db_Alpha, BoFldSubTypes.st_None, 10, new B1DbValidValue[] { new B1DbValidValue("N", "Não"), new B1DbValidValue("Y", "Sim") }, -1)

                ,new B1DbColumn("@RSMV_OTXR", "IPIRate" ,"Aliquota IPI",  BoFieldTypes.db_Float, BoFldSubTypes.st_Percentage, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OTXR", "ICMSRate" ,"Aliquota ICMS",  BoFieldTypes.db_Float, BoFldSubTypes.st_Percentage, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OTXR", "ICMSRed" ,"RedICMS", BoFieldTypes.db_Float, BoFldSubTypes.st_Percentage, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OTXR", "ICMSIntRate" ,"Aliq.Int.ICMS", BoFieldTypes.db_Float, BoFldSubTypes.st_Percentage, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OTXR", "ICMSTSMrg" ,"MargemICMS", BoFieldTypes.db_Float, BoFldSubTypes.st_Percentage, 10, new B1DbValidValue[] {}, -1)
                ,new B1DbColumn("@RSMV_OTXR", "ICMSTSRed" ,"Reducao ICMS ST", BoFieldTypes.db_Float, BoFldSubTypes.st_Percentage, 10, new B1DbValidValue[] {}, -1)

                //ParÂmetros de configuração
                ,new B1DbColumn("@RSMV_PARAM", "Valor" ,"Valor", BoFieldTypes.db_Float, BoFldSubTypes.st_Price, 10, new B1DbValidValue[] {}, -1)

            };

            return Columns;
        }
        
        private void udosDB()
        {
            string colunas;
            colunas = "Code;Code|Name;Name|U_CNAE;CNAE|U_BPCode;BPCode|U_NCM;NCM|U_CEST;CEST|U_State;State|U_FP;FP|U_Exmp;Exmp|U_IPIRate;IPIRate|U_ICMSRate;ICMSRate|U_ICMSRed;ICMSRed|U_ICMSIntRate;ICMSIntRate|U_ICMSTSMrg;ICMSTSMrg|ICMSTSRed;ICMSTSRed";
            CreateUDOs("RSMV_OTXR", colunas, "RSD : Registro de Tributação");

            colunas = "Code;Code|Name;Name|U_Descricao;Descricao|U_DocGerado;DocGerado|U_GerOpor;GerOpor|U_Utilizacao;Utilizacao";
            CreateUDOs("RSMV_OSLT", colunas, "RSD : Tipos de Pedidos");

            colunas = "Code;Code|Name;Name";
            CreateUDOs("RSMV_MAPUTIL", colunas, "RSD:Mapeamento de Utilização");

            colunas = "Code;Code|Name;Name|U_UsageDefault;UsageDefault|U_ITMGrupo;ITMGrupo|U_DescrGrupo;DescrGrupo|U_Usage;Usage";
            CreateUDOs("RSMV_MAPUTIL_DET", colunas, "RSD:Mapeamento de Utili DET");

            colunas = "Code;Code|Name;Name|U_Descricao;Descricao";
            CreateUDOs("RSMV_OBON", colunas, "RSD:Motivo de Bonificação");

            colunas = "Code;Code|Name;Name|U_Tipo;Tipo|U_Descricao;Descricao";
            CreateUDOs("RSMV_OSLR", colunas, "RSD:Motivo de Venda Não Venda");

            colunas = "Code;Code|Name;Name|U_Valor;Valor";
            CreateUDOs("RSMV_PARAM", colunas, "RSD:Parâmetros de Config");

            

        }


        public void CreateUDOs(string tabela, string Colunas, string descricao)
        {
            string[] auxValores1, auxValores2;
            
            int iQtColunas, lRetCode;
            
            var oUserObjectMD = ((UserObjectsMD)(oCompany.GetBusinessObject(BoObjectTypes.oUserObjectsMD)));

            oUserObjectMD.CanCancel = BoYesNoEnum.tNO;
            oUserObjectMD.CanClose = BoYesNoEnum.tNO;
            oUserObjectMD.CanCreateDefaultForm = BoYesNoEnum.tYES;
            oUserObjectMD.CanDelete = BoYesNoEnum.tYES;
            oUserObjectMD.CanFind = BoYesNoEnum.tYES;
            oUserObjectMD.CanYearTransfer = BoYesNoEnum.tNO;
            oUserObjectMD.CanLog = BoYesNoEnum.tNO;
            oUserObjectMD.Code = tabela;
            oUserObjectMD.ManageSeries = BoYesNoEnum.tNO;
            oUserObjectMD.Name = tabela;
            oUserObjectMD.ObjectType = BoUDOObjType.boud_MasterData;
            oUserObjectMD.TableName = tabela;

            oUserObjectMD.MenuItem = BoYesNoEnum.tYES;
            oUserObjectMD.FatherMenuID = 2048;
            oUserObjectMD.MenuUID = tabela;
            oUserObjectMD.MenuCaption = descricao;
            oUserObjectMD.Position = 99;
            oUserObjectMD.EnableEnhancedForm = BoYesNoEnum.tNO;

            if (Colunas != "")
            {
                iQtColunas = 0;
                auxValores1 = Colunas.Split('|');
                foreach (string auxValor in auxValores1)
                {
                    if (iQtColunas > 0)
                    {
                        oUserObjectMD.FindColumns.Add();
                        oUserObjectMD.FormColumns.Add();
                    }
                    auxValores2 = auxValor.Split(';');
                    oUserObjectMD.FindColumns.ColumnAlias = auxValores2[0];
                    oUserObjectMD.FindColumns.ColumnDescription = auxValores2[1];

                    oUserObjectMD.FormColumns.FormColumnAlias = auxValores2[0];
                    oUserObjectMD.FormColumns.FormColumnDescription = auxValores2[1];

                    iQtColunas++;
                }
            }
            lRetCode = oUserObjectMD.Add();
            if (lRetCode != 0)
            {
                MessageBox.Show(string.Format("Erro ao inserir UDO {0}. Erro: {1} - Mensagem: {2}", oUserObjectMD.TableName, lRetCode.ToString(), oCompany.GetLastErrorDescription()));
            }
                    
        }
    }
}
