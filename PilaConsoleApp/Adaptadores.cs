using System;
using PilaRepo.Shared;

namespace PilaConsoleApp
{
    /// <summary>Operaciones del Menú 2, iguales para cualquier modo de pila.</summary>
    internal interface IMenuPila
    {
        string Nombre { get; }
        void Push();
        void Pop();
        void Peek();
        void Count();
        void Clear();
        void Contains();
        void Ver();
    }

    /// <summary>Adaptador para Pila&lt;T&gt; (estática, dinámica o nativa).</summary>
    internal sealed class AdaptadorPila<T> : IMenuPila
    {
        private readonly Pila<T> _pila;
        private readonly Func<T> _leerParaPush;
        private readonly Func<T> _leerParaBuscar;
        private readonly Func<T, string> _formato;

        public string Nombre { get; }

        public AdaptadorPila(string nombre, Pila<T> pila, Func<T> leerParaPush, Func<T> leerParaBuscar, Func<T, string> formato)
        {
            Nombre = nombre;
            _pila = pila;
            _leerParaPush = leerParaPush;
            _leerParaBuscar = leerParaBuscar;
            _formato = formato;
        }

        public void Push()
        {
            T v = _leerParaPush();
            _pila.Push(v);   // puede lanzar "Stack Overflow (la pila está llena)"
            Console.WriteLine($"Push({_formato(v)}) -> apilado. Count = {_pila.Count}");
        }

        public void Pop()
        {
            T v = _pila.Pop();   // puede lanzar "Stack Underflow (la pila está vacía)"
            Console.WriteLine($"Pop() -> {_formato(v)}. Count = {_pila.Count}");
        }

        public void Peek()
        {
            T v = _pila.Peek();
            Console.WriteLine($"Peek() -> {_formato(v)} (no se retira)");
        }

        public void Count() => Console.WriteLine($"Count = {_pila.Count}");

        public void Clear()
        {
            _pila.Clear();
            Console.WriteLine("Clear() -> pila vacía.");
        }

        public void Contains()
        {
            T v = _leerParaBuscar();
            Console.WriteLine($"Contains({_formato(v)}) -> {_pila.Contains(v)}");
        }

        public void Ver()
        {
            var copia = _pila.ObtenerCopia();   // base -> tope
            int n = copia.Count;
            int inicio = Math.Max(_pila.Modo == ModoPila.Estatica ? _pila.Capacidad : 0, n) - 1;

            Console.WriteLine($"--- {Nombre} | top = {_pila.Top} | Count = {n} ---");
            if (inicio < 0) Console.WriteLine("(vacía)");
            for (int i = inicio; i >= 0; i--)
            {
                string marca = (i == n - 1) ? "Tope ->" : "       ";
                string dato = i < n ? _formato(copia[i]) : "(libre)";
                Console.WriteLine($"{marca} [{i}] {dato}");
            }
        }
    }

    /// <summary>Adaptador para la pila de contactos (List&lt;Contacto&gt;).</summary>
    internal sealed class AdaptadorContactos : IMenuPila
    {
        private readonly PilaContactos _pila = new PilaContactos();

        public string Nombre => "Pila de Contactos (List<T>)";

        public void Push()
        {
            string nombre = Entrada.LeerTexto("Nombre: ");
            string tel = Entrada.LeerTexto("Teléfono: ");
            string dir = Entrada.LeerTexto("Dirección: ");
            string correo = Entrada.LeerTexto("Correo: ");
            var c = new Contacto(nombre, tel, dir, correo);
            _pila.Push(c);
            Console.WriteLine($"Push({c.Nombre}) -> apilado. Count = {_pila.Count}");
        }

        public void Pop()
        {
            Contacto c = _pila.Pop();
            Console.WriteLine($"Pop() -> {c}. Count = {_pila.Count}");
        }

        public void Peek() => Console.WriteLine($"Peek() -> {_pila.Peek()} (no se retira)");

        public void Count() => Console.WriteLine($"Count = {_pila.Count}");

        public void Clear()
        {
            _pila.Clear();
            Console.WriteLine("Clear() -> pila vacía.");
        }

        public void Contains()
        {
            string nombre = Entrada.LeerTexto("Nombre a buscar: ");
            Console.WriteLine($"Contains(\"{nombre}\") -> {_pila.Contains(nombre)}");
        }

        public void Ver()
        {
            var copia = _pila.ObtenerCopia();
            int n = copia.Count;
            Console.WriteLine($"--- {Nombre} | top = {n - 1} | Count = {n} ---");
            if (n == 0) Console.WriteLine("(vacía)");
            for (int i = n - 1; i >= 0; i--)
                Console.WriteLine($"{(i == n - 1 ? "Tope ->" : "       ")} [{i}] {copia[i]}");
        }
    }
}
