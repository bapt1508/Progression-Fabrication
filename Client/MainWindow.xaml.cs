using System;
using System.Text;
using System.Windows;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.ComponentModel;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket server;
        private Socket client;
        private CancellationTokenSource cancellationTokenSource;
        public MainWindow()
        {
            InitializeComponent();

            // Démarrer le serveur dans un thread
            Thread serverThread = new Thread(SeConnecterEtEcouter);
            serverThread.Start();
            Closing += Window_Closing;
        }

        private void SeConnecterEtEcouter()
        {
            // Se connecter au serveur et obtenir le socket client
            client = SeConnecter();

            // Initialiser le CancellationTokenSource
            cancellationTokenSource = new CancellationTokenSource();

            // Démarrer l'écoute des progrès dans un autre thread avec le CancellationToken
            Thread progressThread = new Thread(() => ListenForProgress(cancellationTokenSource.Token));
            progressThread.Start();
        }


        private static Socket SeConnecter()
        {
            // Créer un socket client
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Définir l'adresse IP et le port du serveur
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1"); // Remplacez par l'adresse IP du serveur
            int port = 12345; // Remplacez par le port du serveur

            // Se connecter au serveur
            IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, port);
            clientSocket.Connect(serverEndPoint);

            Console.WriteLine("Connecté au serveur.");

            return clientSocket;
        }

        private void ListenForProgress(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    byte[] buffer = new byte[4];
                    int bytesRead = client.Receive(buffer);

                    if (bytesRead == 0)
                    {
                        // La connexion a été fermée par le serveur, sortir de la boucle
                        break;
                    }

                    if (bytesRead == 4)
                    {
                        int progress = BitConverter.ToInt32(buffer, 0);
                        Dispatcher.Invoke(() => UpdateProgressBar(progress));
                    }
                }
            }
            catch (SocketException ex)
            {
                // Gérer la fermeture de la connexion ici
                Console.WriteLine($"La connexion a été fermée par le serveur. Message d'erreur : {ex.Message}");
            }
            finally
            {
                // Fermer proprement la connexion et le token lorsque le thread se termine
                client?.Close();
                cancellationTokenSource?.Cancel();
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Annuler la tâche asynchrone côté client lors de la fermeture de l'application
            cancellationTokenSource?.Cancel();
            client?.Close();
        }

        private void UpdateProgressBar(int progress)
        {
            // Mettez à jour la ProgressBar dans l'interface utilisateur
            progressBar.Value = progress;
            lb_etat_prog_server.Content = progressBar.Value.ToString() + "%";
        }
    }
}
