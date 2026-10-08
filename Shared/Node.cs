namespace PilaRepo.Shared
{
    /// <summary>
    /// Nodo de la pila (según las diapositivas): guarda un dato (T) y un enlace
    /// "Prev" al nodo que está DEBAJO de él. En la base de la pila Prev es null.
    /// No tiene constructor vacío: un nodo siempre nace con su dato.
    /// </summary>
    internal class Node<T>
    {
        /// <summary>Dato almacenado en el nodo.</summary>
        public T Data { get; set; }

        /// <summary>Enlace al nodo de abajo (null si este nodo es la base).</summary>
        public Node<T>? Prev { get; set; }

        public Node(T data, Node<T>? prev)
        {
            Data = data;
            Prev = prev;
        }
    }
}
