using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace p7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private Socket server;
        private Socket client;
		Thread serverThread;


		public MainWindow()
        {
            InitializeComponent();


			serverThread = new Thread(SeConnecterEtEcouter);
			serverThread.Start();



		}
		void SeConnecterEtEcouter()
        {
			server = SeConnecter(); // Utilisez la variable de classe

			// Appeler la méthode AccepterConnexion()
			client = AccepterConnexion(server);

		}
		
		// Méthode qui initialise la barre de progression 
		void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			for (int i = 1; i <= 100; i++)
			{
				(sender as BackgroundWorker).ReportProgress(i);
				
				Thread.Sleep(2000);
				
			}	
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{   
			//initialisation de la barre de progression avec le pourcentage de progression
			pbstatus1.Value   = e.ProgressPercentage;

			//Affichage de la progression sur un label
			lb_etat_prog_server.Content = pbstatus1.Value.ToString() +"%";
			SendProgressToSecondProject(e.ProgressPercentage);




		}

		// lancer la barre de progression en créant un objet de type BackgroundWorker
		//BackgroundWorker :
		private void Button_Click(object sender, RoutedEventArgs e)
        {
			//création, initialisation et mise à jour de l'objet BackgroundWorker
			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += worker_DoWork;
			worker.ProgressChanged += worker_ProgressChanged;
			worker.RunWorkerAsync();
		}
		private void SendProgressToSecondProject(int progress)
		{
            // Appeler la méthode seConnecter()
            

			// Appeler la méthode EcouterReseau()
			EcouterReseau(client,progress);
		}
		private static Socket SeConnecter()
		{
			// Créer un socket serveur
			Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Associer le socket à une adresse IP et un port
			IPAddress ipAddress = IPAddress.Parse("127.0.0.1"); // Remplacez par l'adresse IP souhaitée
			int port = 12345; // Remplacez par le port souhaité
			IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
			serverSocket.Bind(endPoint);

			// Mettre le socket à l'écoute des connexions clientes
			serverSocket.Listen(10);

			Console.WriteLine("Le serveur est en attente de connexions...");

			return serverSocket;
		}

		private static Socket AccepterConnexion(Socket serverSocket)
		{
			// Accepter une connexion cliente
			Socket clientSocket = serverSocket.Accept();

			// Récupérer l'adresse IP et le port du client
			IPEndPoint clientEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
			Console.WriteLine($"Client connecté depuis {clientEndPoint.Address}:{clientEndPoint.Port}");

			return clientSocket;
		}

		private void EcouterReseau(Socket client,int progress)
		{
			try
			{
				if (client != null && client.Connected)
				{
					byte[] progressBytes = BitConverter.GetBytes(progress);

					// Envoyer le pourcentage au client
					client.Send(progressBytes);
				}
			}
			catch (SocketException ex)
			{
				client.Shutdown(SocketShutdown.Both);
				client.Close();
				server.Close();
                restartThread();


				// Gérer la fermeture de la connexion ici

			}

		}
		void restartThread()
        {
			serverThread.Join();
			serverThread = new Thread(SeConnecterEtEcouter);
			serverThread.Start();
		}



	}
}
