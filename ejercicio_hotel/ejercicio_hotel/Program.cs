using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ejercicio_hotel
{
    class Program
    {

       static String connectionString = ConfigurationManager.ConnectionStrings["conexionHotel"].ConnectionString;
       static SqlConnection conexion = new SqlConnection(connectionString);
       static string cadena;
       static SqlCommand comando;

        static void Main(string[] args)
        {
            

            Menu();

        }

        //Hay que crear un menú, el cual incluiremos en un método llamado Menu(). Tendrá las siguientes opciones:
        public static void Menu()
        {
            int opcion, cont=0;


            
            Console.WriteLine("\nBienvenindo al Hotel Boutique Helene");
            
            do
            {
                if (cont>0)
                {
                    Console.WriteLine("\n Introduzca un numero válido");
                }
                
                Console.WriteLine("\n¿Que desea hacer? (Elija un a de estas opciones)");
                Console.WriteLine("\n1.Registrar Cliente");
                Console.WriteLine("\n2.Editar Cliente");
                Console.WriteLine("\n3.Hacer Check In");
                Console.WriteLine("\n4.Hacer Check Out");
                Console.WriteLine("\n5.Salir");
                opcion = Convert.ToInt32(Console.ReadLine());
                cont++;
            }
            while (opcion<0 || opcion>5);

            switch (opcion)
            {
                case 1:
                    RegistrarCliente();
                    break;
                case 2:
                    EditarCliente();
                    break;
                case 3:
                    CheckIn();
                    break;
                case 4:
                    CheckOut();
                    break;
                case 5:
                    Salir();
                    break;
            }
           

        }
        //Registrar cliente: Aquí registramos un nuevo cliente, puesto que no se puede reserva una habitación si previamente no se a registrado al cliente.
        //Haremos un método RegistrarCliente() en el cual pediremos por teclado el nombre, apellido y DNI y haremos un Insert a la tabla de Clientes.
        public static void RegistrarCliente()
        {
            conexion.Open();
            
            string nombre, apellido, dni;
            int cont = 0;

            Console.WriteLine("\n Introduzca el Nombre del cliente:");
            nombre = Console.ReadLine();

            Console.WriteLine("\n Introduzca el Apellido del cliente:");
            apellido = Console.ReadLine();

            do
            {
                if(cont>0)
                {
                    Console.WriteLine("\n El Dni introducido no es correcto");
                }
                Console.WriteLine("\n Introduzca el Dni del cliente:");
                dni = Console.ReadLine();
                cont++;
            }
            while (dni.Length != 9);
            cadena = "INSERT INTO huesped values ('" + dni + "','" + nombre + "','" + apellido+"')";
            comando = new SqlCommand(cadena, conexion);
            comando.ExecuteNonQuery();

            conexion.Close();
            Menu();
        }
        //Editar cliente: Aquí tendremos la opción de cambiar el nombre y el apellido de un cliente que ya está registrado en la BBDD.
        //Para ello crearemos un método EditarCliente(String DNI). Pediremos por teclado el DNI del cliente al que queremos cambiar los datos.
        //Nos lo pedirá continuamente  hasta que el DNI introducido sea correcto.
        //Entonces le pediremos que nos introduzca el Nombre y el Apellido de nuevo.

        private static void EditarCliente()
        {
            conexion.Open();
           
            string nombre, apellido,dni;
            int cont = 0;
            SqlDataReader match;
            bool ok = false;
            do
            {

                if (cont > 0)
                {
                    Console.WriteLine("\n El Dni introducido no es correcto");
                }
                Console.WriteLine("\n Introduzca el Dni del cliente:");
                dni = Console.ReadLine();
                cont++;
                cadena = "SELECT * FROM huesped WHERE dni like '" + dni + "'";
                comando = new SqlCommand(cadena, conexion);
                match = comando.ExecuteReader();

                if (match.Read())
                {
                    ok = true;
                }
                match.Close();
            }
            while (ok == false);

            Console.WriteLine("\n Introduzca el Nombre del cliente:");
            nombre = Console.ReadLine();

            Console.WriteLine("\n Introduzca el Apellido del cliente:");
            apellido = Console.ReadLine();

            cadena = "UPDATE huesped SET nombre='" + nombre + "', apellido='" + apellido + "' WHERE dni like '"+dni+"'";
            comando = new SqlCommand(cadena, conexion);
            comando.ExecuteNonQuery();

            conexion.Close();
            Menu();
        }

        //Check-in: Aquí pediremos el DNI del cliente que quiere hacer la reserva.
        //Si el cliente no existe en la tabla clientes aparecerá un mensaje que nos indique que el cliente no está registrado y por lo tanto no puede hacer una reserva.
        //Si el cliente está registrado, le aparecerá un listado con las habitaciones disponibles del hotel para que seleccione la que quiera reservar.
        //Una vez validado que el número de la habitación que ha introducido es correcto, tendremos que hacer un update a la tabla de HABITACIONES para poner la habitación como ocupada.

        private static void CheckIn()
        {
            conexion.Open();

            string dni;
            int hab, nuevaReserva=0;
                                    
            Console.WriteLine("\n Introduzca el Dni del cliente:");
            dni = Console.ReadLine();
                
            cadena = "SELECT * FROM huesped WHERE dni like '" + dni + "'";
            comando = new SqlCommand(cadena, conexion);
            SqlDataReader libre = comando.ExecuteReader();


            if (!libre.Read())
            {
                Console.WriteLine("\n El Cliente no esta registrado o el DNI introducido no es correcto");
            }
            else
            {
                libre.Close();
                Console.WriteLine("\nEstas son las habitaciones disponibles");
                cadena = "SELECT NumeroHabitacion FROM habitacion WHERE Estado like 'l'";
                comando = new SqlCommand(cadena, conexion);
                SqlDataReader numHab = comando.ExecuteReader();
                while (numHab.Read())
                {
                    Console.WriteLine(numHab["NumeroHabitacion"].ToString());
                }
                numHab.Close();
                Console.WriteLine("\nElige una habitacion");
                hab = Convert.ToInt32(Console.ReadLine());
                cadena = "UPDATE habitacion SET Estado = 'o' WHERE NumeroHabitacion = " + hab ;
                comando = new SqlCommand(cadena, conexion);
                comando.ExecuteNonQuery();


                
                //la columna en sql pierde su nombre, asi que se pone:
                // cadena = "SELECT Max(CodReserva) AS 'Codreserva' FROM Reserva WHERE Estado like 'o'";
                //o se pone asi (CodReserva[0].ToString())
                cadena = "SELECT Max(CodReserva) AS 'CodReserva' FROM reservas";
                comando = new SqlCommand(cadena, conexion);
                SqlDataReader codReserva = comando.ExecuteReader();
                if (codReserva.Read())
                    {
                    nuevaReserva = Convert.ToInt32(codReserva["CodReserva"].ToString())+1;
                    //cadena = "UPDATE CodReserva SET Estado = 'o' WHERE NumeroHabitacion = " + hab;
                    }
                codReserva.Close();

                //Console.WriteLine("\nLa fecha del CheckIn es " + DateTime.UtcNow.ToString("yyyy-MM-dd"));
                cadena = "INSERT INTO reservas (CodReserva,DniHuesped,NumeroHabitacion,CheckIn) VALUES (" + nuevaReserva + ",'" + dni + "'," + hab + ",'" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "')";
                comando = new SqlCommand(cadena, conexion);
                comando.ExecuteNonQuery();

            }
            libre.Close();
 
            conexion.Close();
            Menu();
        }

        //Checkout: Aqui tendreis que hacer un método CheckOut(String DNI, ). Dado un DNI correcto, se harán dos updates.
        //Uno a la tabla RESERVAS, el cual ingresará la fecha del checkOut y otro a la tabla HABITACIONES que pondrá la habitación a disponible.


        public static void CheckOut()
        {
            conexion.Open();

            string dni;
            

            Console.WriteLine("\n Introduzca el Dni del cliente:");
            dni = Console.ReadLine();

            cadena = "SELECT * FROM huesped WHERE dni like '" + dni + "' AND CheckOut is NULL";
            comando = new SqlCommand(cadena, conexion);
            SqlDataReader salida = comando.ExecuteReader();


            if (!salida.Read())
            {
                Console.WriteLine("\n El Cliente no esta registrado o el DNI introducido no es correcto");
            }
            else
            {
                salida.Close();
                cadena = "SELECT CodReserva FROM reservas WHERE dni like '"+ dni +"'";
                comando = new SqlCommand(cadena, conexion);
                SqlDataReader reservaOut = comando.ExecuteReader();
                while (reservaOut.Read())
                {
                    Console.WriteLine(reservaOut["Reserva"].ToString());
                }
                salida.Close();
               
                cadena = "UPDATE habitacion SET Estado = 'l' WHERE reserva = " + reservaOut;
                comando = new SqlCommand(cadena, conexion);
                comando.ExecuteNonQuery();

                cadena = "INSERT INTO reservas (CheckOut) VALUES ('" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "') WHERE dni = " +dni;
                comando = new SqlCommand(cadena, conexion);
                comando.ExecuteNonQuery();

                                
            }
            salida.Close();

            conexion.Close();
            Menu();
        }

        //Salir

        public static void Salir()
        {

        }

    }
}
