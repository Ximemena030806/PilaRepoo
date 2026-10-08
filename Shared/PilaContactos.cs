using System;
using System.Collections.Generic;
using System.Linq;

namespace PilaRepo.Shared
{
    /// <summary>
    /// Pila de contactos respaldada por List&lt;Contacto&gt; usada COMO pila:
    ///   Push = Add (al final), Pop/Peek = último índice, Pop = RemoveAt(último).
    /// El tope es siempre el último elemento de la lista. Arranca PRECARGADA con 3 contactos.
    /// No existe IsEmpty: Pop/Peek revisan "Count == 0".
    ///
    /// Diferencias frente a una pila propia / Stack&lt;T&gt;:
    ///  - Add y RemoveAt(último) son O(1) amortizado: List&lt;T&gt; duplica su arreglo interno
    ///    cuando se llena (copia O(n) ocasional). Una pila estática propia es O(1) estricto
    ///    pero se desborda; aquí NUNCA hay Stack Overflow (la lista crece sola).
    ///  - Como List&lt;T&gt; expone todo (Insert, RemoveAt(0)...), la disciplina LIFO
    ///    depende de usar solo esta clase; Stack&lt;T&gt; la garantiza por diseño.
    /// </summary>
    internal class PilaContactos
    {
        private const string MsgUnderflow = "Stack Underflow (la pila está vacía)";

        private readonly List<Contacto> _lista = new List<Contacto>();

        public PilaContactos()
        {
            // Datos de ejemplo (el último es el tope).
            _lista.Add(new Contacto("Ana López", "555-0101", "Av. Juárez 120", "ana@correo.com"));
            _lista.Add(new Contacto("Luis Pérez", "555-0202", "Calle Hidalgo 45", "luis@correo.com"));
            _lista.Add(new Contacto("María García", "555-0303", "Blvd. Colosio 900", "maria@correo.com"));
        }

        /// <summary>Número de elementos (PROPIEDAD, no método).</summary>
        public int Count => _lista.Count;

        /// <summary>Apila al final de la lista. Amortizado O(1) (puede redimensionar el arreglo interno).</summary>
        public void Push(Contacto contacto)
        {
            if (contacto == null) throw new ArgumentNullException(nameof(contacto));
            _lista.Add(contacto);
        }

        /// <summary>Retira y devuelve el último elemento (RemoveAt del último índice: O(1), sin desplazar nada).</summary>
        public Contacto Pop()
        {
            if (Count == 0) throw new InvalidOperationException(MsgUnderflow);
            Contacto tope = _lista[_lista.Count - 1];
            _lista.RemoveAt(_lista.Count - 1);
            return tope;
        }

        /// <summary>Devuelve el último elemento sin retirarlo. O(1).</summary>
        public Contacto Peek()
        {
            if (Count == 0) throw new InvalidOperationException(MsgUnderflow);
            return _lista[_lista.Count - 1];
        }

        /// <summary>Vacía la pila. O(n) (List.Clear limpia las referencias); la capacidad interna se conserva.</summary>
        public void Clear() => _lista.Clear();

        /// <summary>
        /// Busca por nombre sobre una COPIA (ToList), con el cursor "current", de la cima
        /// hacia la base. Nunca modifica la pila original.
        /// </summary>
        public bool Contains(string nombre)
        {
            List<Contacto> copia = _lista.ToList();
            string buscado = (nombre ?? string.Empty).Trim();
            int current = copia.Count - 1;           // cursor: empieza en la cima
            while (current >= 0)
            {
                if (string.Equals(copia[current].Nombre, buscado, StringComparison.OrdinalIgnoreCase))
                    return true;
                current--;                           // baja hacia la base
            }
            return false;
        }

        /// <summary>Copia de los elementos en orden base -> tope (para mostrar en pantalla).</summary>
        public List<Contacto> ObtenerCopia() => _lista.ToList();
    }
}
