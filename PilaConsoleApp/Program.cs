using System;
using System.Text;
using PilaRepo.Shared;

namespace PilaConsoleApp
{
    /// <summary>Lectura de datos desde la consola.</summary>
    internal static class Entrada
    {
        public static string LeerTexto(string mensaje)
        {
            Console.Write(mensaje);
            return Console.ReadLine() ?? string.Empty;
        }

        public static int LeerEntero(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out int v)) return v;
                Console.WriteLine("Número inválido, intenta de nuevo.");
            }
        }
    }

    internal static class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            bool salir = false;

            while (!salir)
            {
                IMenuPila? pila = ElegirModo();          // Menú 1
                if (pila == null) return;                // "Salir"

                bool cambiarModo = false;
                while (!cambiarModo && !salir)           // Menú 2
                {
                    MostrarMenuOperaciones(pila.Nombre);
                    string opcion = (Console.ReadLine() ?? "").Trim();
                    try
                    {
                        switch (opcion)
                        {
                            case "1": pila.Push(); break;
                            case "2": pila.Pop(); break;
                            case "3": pila.Peek(); break;
                            case "4": pila.Count(); break;
                            case "5": pila.Clear(); break;
                            case "6": pila.Contains(); break;
                            case "7": pila.Ver(); break;
                            case "8": cambiarModo = true; break;
                            case "9": salir = true; break;
                            default: Console.WriteLine("Opción no válida."); break;
                        }
                    }
                    catch (InvalidOperationException ex)   // Stack Overflow / Stack Underflow
                    {
                        Console.WriteLine($"ERROR: {ex.Message}");
                    }
                }
            }
        }

        // ---------------- Menú 1: elegir modo ----------------
        private static IMenuPila? ElegirModo()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("===== PILA: elige el modo =====");
                Console.WriteLine("1. Estática (pide tamaño)");
                Console.WriteLine("2. Dinámica (libros, precargada)");
                Console.WriteLine("3. Nativa (Stack<T>)");
                Console.WriteLine("4. Contactos (List<T>, precargada)");
                Console.WriteLine("5. Salir");
                Console.Write("Opción: ");

                switch ((Console.ReadLine() ?? "").Trim())
                {
                    case "1":
                        int tam = Entrada.LeerEntero("Tamaño de la pila (por defecto en las diapositivas: 4): ");
                        if (tam <= 0) { Console.WriteLine("El tamaño debe ser mayor que cero."); break; }
                        return NuevaPilaEnteros($"Pila Estática (tamaño {tam})", new Pila<int>(tam));
                    case "2":
                        return new AdaptadorPila<Libro>("Pila Dinámica (libros)", Libro.CrearPilaPrecargada(),
                            () => new Libro(Entrada.LeerTexto("Título: "), Entrada.LeerTexto("Autor: ")),
                            () => new Libro(Entrada.LeerTexto("Título a buscar: "), ""),
                            l => $"{l.Titulo} / {l.Autor}");
                    case "3":
                        return NuevaPilaEnteros("Pila Nativa (Stack<T>)", Pila<int>.CrearNativa());
                    case "4":
                        return new AdaptadorContactos();
                    case "5":
                        return null;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        private static IMenuPila NuevaPilaEnteros(string nombre, Pila<int> pila) =>
            new AdaptadorPila<int>(nombre, pila,
                () => Entrada.LeerEntero("Valor entero: "),
                () => Entrada.LeerEntero("Valor entero a buscar: "),
                v => v.ToString());

        // ---------------- Menú 2: operaciones ----------------
        private static void MostrarMenuOperaciones(string nombre)
        {
            Console.WriteLine();
            Console.WriteLine($"===== {nombre} =====");
            Console.WriteLine("1. Push");
            Console.WriteLine("2. Pop");
            Console.WriteLine("3. Peek");
            Console.WriteLine("4. Count");
            Console.WriteLine("5. Clear");
            Console.WriteLine("6. Contains");
            Console.WriteLine("7. Ver pila completa");
            Console.WriteLine("8. Cambiar de modo");
            Console.WriteLine("9. Salir");
            Console.Write("Opción: ");
        }
    }
}
