using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitoramentoEnergia
{
    public abstract class Sensor
    {
        public string Localizacao { get; set; }
        public DateTime UltimaLeitura { get; set; }
        public abstract void LerDados();
    }
    public class SensorConsumo : Sensor
    {
        public double ConsumoAtual { get; private set; }

        public override void LerDados()
        {
            Random rand = new Random();
            ConsumoAtual = rand.NextDouble() * 100;
            UltimaLeitura = DateTime.Now;
        }
    }

    public class RegistroFalha
    {
        public DateTime DataHora { get; set; }
        public string Local { get; set; }
        public string Descricao { get; set; }

        public RegistroFalha(string local, string descricao, DateTime? dataHora = null)
        {
            DataHora = dataHora ?? DateTime.Now;
            Local = local;
            Descricao = descricao;
        }
    }

    public class Alerta
    {
        public string Mensagem { get; set; }
        public List<string> RegioesAfetadas { get; set; }
        public DateTime DataHora { get; set; }

        public Alerta(string mensagem, List<string> regioesAfetadas, DateTime? dataHora = null)
        {
            Mensagem = mensagem;
            RegioesAfetadas = regioesAfetadas;
            DataHora = dataHora ?? DateTime.Now;
        }
    }

    public class LogEvento
    {
        public string Evento { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;
    }

    public class RelatorioStatus
    {
        public List<SensorConsumo> Sensores { get; set; } = new List<SensorConsumo>();

        public void GerarRelatorio()
        {
            Console.WriteLine("\n--- RELATÓRIO DE CONSUMO POR REGIÃO ---\n");
            foreach (var sensor in Sensores)
            {
                Console.WriteLine($"Local: {sensor.Localizacao}\nConsumo: {sensor.ConsumoAtual:F2} kWh\nÚltima Leitura: {sensor.UltimaLeitura}\n");
            }
        }
    }

    public static class SistemaLogin
    {
        public static Dictionary<string, (string senha, string tipo, string regiao)> usuarios = new Dictionary<string, (string, string, string)>
    {
        {"admin", ("1234", "admin", "Todas")},
        {"usuario", ("senha", "comum", "Centro")},
        {"usuario2", ("senha2", "comum", "Norte")},
        {"usuario3", ("senha3", "comum", "Sul")},
    };

        public static (string tipo, string regiao, string usuario)? Autenticar()
        {
            Console.Write("Usuário: ");
            string user = Console.ReadLine();
            Console.Write("Senha: ");
            string senha = Console.ReadLine();

            if (usuarios.ContainsKey(user) && usuarios[user].senha == senha)
                return (usuarios[user].tipo, usuarios[user].regiao, user);

            return null;
        }

        public static void CadastrarUsuario()
        {
            Console.Write("Novo usuário: ");
            string novoUsuario = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(novoUsuario))
            {
                Console.WriteLine("Nome de usuário não pode ser vazio!\n");
                return;
            }

            if (usuarios.ContainsKey(novoUsuario))
            {
                Console.WriteLine("Usuário já existe!\n");
                return;
            }

            Console.Write("Senha: ");
            string senha = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(senha))
            {
                Console.WriteLine("Senha não pode ser vazia!\n");
                return;
            }

            Console.WriteLine("Escolha sua região: Centro, Norte, Sul, Leste, Oeste");
            string regiao = Console.ReadLine()?.Trim();
            if (!new[] { "Centro", "Norte", "Sul", "Leste", "Oeste" }.Contains(regiao))
            {
                Console.WriteLine("Região inválida!\n");
                return;
            }

            usuarios.Add(novoUsuario, (senha, "comum", regiao));
            Console.WriteLine("\nUsuário cadastrado com sucesso!\n");
        }
    }

    public class Programa
    {
        static List<RegistroFalha> falhas = new();
        static List<Alerta> alertas = new();
        static List<LogEvento> logs = new();
        static RelatorioStatus relatorio = new();
        static List<string> regioes = new List<string> { "Centro", "Norte", "Sul", "Leste", "Oeste" };
        static Dictionary<string, List<(DateTime data, double consumo)>> historicoConsumo = new();
        static List<string> mensagensChat = new();

        static void MenuUsuarioComum(string regiao, string nome)
        {
            bool continuar = true;
            while (continuar)
            {
                Console.WriteLine("\n--- MENU USUÁRIO COMUM ---");
                Console.WriteLine("1. Ver consumo da residência");
                Console.WriteLine("2. Conferir alertas da sua região");
                Console.WriteLine("3. Relatar queda de energia");
                Console.WriteLine("4. Ver histórico de consumo");
                Console.WriteLine("5. Chat com concessionária");
                Console.WriteLine("6. Logout");
                Console.Write("Escolha uma opção: ");
                string opcao = Console.ReadLine();

                try
                {
                    Console.WriteLine();
                    switch (opcao)
                    {
                        case "1":
                            var sensor = new SensorConsumo { Localizacao = regiao };
                            sensor.LerDados();
                            Console.WriteLine($"Consumo atual: {sensor.ConsumoAtual:F2} kWh\nÚltima leitura: {sensor.UltimaLeitura}\n");
                            if (!historicoConsumo.ContainsKey(nome)) historicoConsumo[nome] = new();
                            historicoConsumo[nome].Add((sensor.UltimaLeitura, sensor.ConsumoAtual));
                            break;
                        case "2":
                            Console.WriteLine("--- Alertas para sua região ---\n");
                            foreach (var alerta in alertas.Where(a => a.RegioesAfetadas.Contains(regiao)))
                                Console.WriteLine($"[{alerta.DataHora}] {alerta.Mensagem}\n");
                            break;
                        case "3":
                            Console.Write("Descrição da queda: ");
                            string desc = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(desc))
                            {
                                Console.WriteLine("Descrição não pode ser vazia.");
                                break;
                            }
                            falhas.Add(new RegistroFalha(regiao, desc));
                            Console.WriteLine("\nRelato registrado com sucesso.\n");
                            break;
                        case "4":
                            if (historicoConsumo.ContainsKey(nome))
                            {
                                Console.WriteLine("\n--- Histórico de Consumo ---\n");
                                foreach (var item in historicoConsumo[nome])
                                    Console.WriteLine($"{item.data}: {item.consumo:F2} kWh");
                            }
                            else Console.WriteLine("Nenhum histórico disponível.\n");
                            break;
                        case "5":
                            Console.Write("Digite sua mensagem: ");
                            string msg = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(msg))
                            {
                                Console.WriteLine("Mensagem vazia não pode ser enviada.");
                                break;
                            }
                            mensagensChat.Add($"{nome} ({DateTime.Now}): {msg}");
                            Console.WriteLine("Mensagem enviada.\n");
                            break;
                        case "6":
                            continuar = false;
                            break;
                        default:
                            Console.WriteLine("\nOpção inválida.\n");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nErro: " + ex.Message + "\n");
                }
            }
        }

        static void MenuAdmin()
        {
            bool continuar = true;
            while (continuar)
            {
                Console.WriteLine("\n--- MENU ADMIN ---");
                Console.WriteLine("1. Ver consumo por região");
                Console.WriteLine("2. Criar alerta de queda de energia");
                Console.WriteLine("3. Visualizar relatos de queda de energia");
                Console.WriteLine("4. Ver histórico de consumo por região");
                Console.WriteLine("5. Ver mensagens do chat");
                Console.WriteLine("6. Logout");
                Console.Write("Escolha uma opção: ");
                string opcao = Console.ReadLine();

                try
                {
                    Console.WriteLine();
                    switch (opcao)
                    {
                        case "1":
                            relatorio.Sensores.Clear();
                            foreach (var reg in regioes)
                            {
                                var sensor = new SensorConsumo { Localizacao = reg };
                                sensor.LerDados();
                                relatorio.Sensores.Add(sensor);

                                if (!historicoConsumo.ContainsKey(reg)) historicoConsumo[reg] = new();
                                historicoConsumo[reg].Add((sensor.UltimaLeitura, sensor.ConsumoAtual));
                            }
                            relatorio.GerarRelatorio();
                            break;
                        case "2":
                            Console.Write("Mensagem do alerta: ");
                            string msg = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(msg))
                            {
                                Console.WriteLine("Mensagem não pode ser vazia.");
                                break;
                            }

                            Console.WriteLine("Data do alerta (dd/MM/yyyy) ou ENTER para agora:");
                            string dataStr = Console.ReadLine();
                            DateTime dataAlerta;

                            try
                            {
                                if (!string.IsNullOrWhiteSpace(dataStr))
                                {
                                    dataAlerta = DateTime.ParseExact(dataStr, "dd/MM/yyyy", null);
                                }
                                else
                                {
                                    dataAlerta = DateTime.Now;
                                }
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Data inválida. Use o formato dd/MM/yyyy. Alerta não criado.");
                                break;
                            }

                            Console.WriteLine("\nRegiões afetadas (separadas por vírgula):");
                            Console.WriteLine(string.Join(", ", regioes));
                            string input = Console.ReadLine();

                            List<string> regioesSelecionadas;
                            try
                            {
                                if (string.IsNullOrWhiteSpace(input))
                                    throw new Exception("Você deve informar ao menos uma região.");

                                regioesSelecionadas = input.Split(',')
                                                           .Select(r => r.Trim())
                                                           .Where(r => regioes.Contains(r))
                                                           .ToList();

                                if (regioesSelecionadas.Count == 0)
                                    throw new Exception("Nenhuma região válida foi informada.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Erro: " + ex.Message + " Alerta não criado.");
                                break;
                            }

                            alertas.Add(new Alerta(msg, regioesSelecionadas, dataAlerta));
                            Console.WriteLine("\nAlerta registrado com sucesso.\n");
                            break;

                        case "3":
                            Console.WriteLine("--- Relatos de Queda por Região ---\n");
                            var agrupados = falhas.GroupBy(f => f.Local);
                            foreach (var grupo in agrupados)
                            {
                                Console.WriteLine($"Região: {grupo.Key} - Total de relatos: {grupo.Count()}\n");
                                foreach (var falha in grupo.OrderBy(f => f.DataHora))
                                    Console.WriteLine($"  [{falha.DataHora}] {falha.Descricao}");
                                Console.WriteLine();
                            }
                            break;
                        case "4":
                            Console.WriteLine("\n--- Histórico de Consumo por Região ---\n");
                            foreach (var reg in regioes)
                            {
                                Console.WriteLine($"\nRegião: {reg}");
                                if (historicoConsumo.ContainsKey(reg))
                                    foreach (var h in historicoConsumo[reg])
                                        Console.WriteLine($"{h.data}: {h.consumo:F2} kWh");
                                else Console.WriteLine("Sem dados.");
                            }
                            break;
                        case "5":
                            Console.WriteLine("\n--- Mensagens do Chat ---\n");
                            foreach (var m in mensagensChat)
                                Console.WriteLine(m);
                            break;
                        case "6":
                            continuar = false;
                            break;
                        default:
                            Console.WriteLine("Opção inválida.\n");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro: " + ex.Message + "\n");
                }
            }
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\n--- MENU INICIAL ---");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Cadastrar");
                Console.WriteLine("3. Sair");
                Console.Write("Escolha uma opção: ");
                string opcaoInicial = Console.ReadLine();

                if (opcaoInicial == "1")
                {
                    var login = SistemaLogin.Autenticar();

                    if (login == null)
                    {
                        Console.WriteLine("\nLogin falhou. Tente novamente.\n");
                        continue;
                    }

                    string tipoUsuario = login.Value.tipo;
                    string regiao = login.Value.regiao;
                    string nome = login.Value.usuario;

                    Console.WriteLine($"\nLogin bem-sucedido como {tipoUsuario}.\n");

                    if (tipoUsuario == "admin")
                        MenuAdmin();
                    else
                        MenuUsuarioComum(regiao, nome);

                    Console.WriteLine("\nVocê saiu.\n");
                }
                else if (opcaoInicial == "2")
                {
                    SistemaLogin.CadastrarUsuario();
                }
                else if (opcaoInicial == "3")
                {
                    Console.WriteLine("\nEncerrando o programa...");
                    break;
                }
                else
                {
                    Console.WriteLine("\nOpção inválida.\n");
                }
            }
        }
    }
}
