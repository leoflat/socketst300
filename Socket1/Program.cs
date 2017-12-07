using System; // using System.N"et;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Socket1
{
    class Servidor
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);


        static void Main(string[] args)//public void conectar() ////cambio realizado para prueba
        {
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);// (SERVICO, TIPODATO, TIPO TCP)
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("172.31.17.41"), 8004); //(IP , PUERTO)   //IPADRESS.PARSE convierte una cadena de texto en una ip valida
            //127.0.0.1 es una direccion local de la computadora. 
            //miPrimerSocket.Bind(direccion);//bind establecemos direccion dl socket


            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();


            //miPrimerSocket.Listen(5);//listen cantidad de direcciones con ese socket
            //Console.WriteLine("Escuchando...");
            //Socket Escuchar = miPrimerSocket.Accept();//se crea un nuevo socket, este metodo accept devuelve un socket listo para trabajar en tu programa aplicacion y el cliente;
            //Console.WriteLine("Escuchado con exito");

            //byte[] ByRec = new byte[255];
            //while (true)
            //{
            //    int a = Escuchar.Receive(ByRec, 0, ByRec.Length, 0); //objeto socket metodo recibe array, desde donde, tama;o que tiene, con signo o sin signo)
            //    Array.Resize(ref ByRec, a); //le dice el nuevo tama;o del array con la longitud de a
            //    Console.WriteLine("CLIENTE DICE: " + Encoding.Default.GetString(ByRec)); //mostramos lo recibido  metodos estaticos de clase encoding que convierte el array en caracteres
            //    //hilo

            //}
            //listener.Close();
            //Console.WriteLine("presione cualquier tecla para salir");
            //Console.ReadKey();
                      
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("ST300") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Echo the data back to the client.
                    //Send(handler, "ST300CMD;;02;StatusReq");
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.


            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine($"Sent {bytesSent} bytes to client.");

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
