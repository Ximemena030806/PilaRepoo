using System;
using System.Collections.Generic;
using System.Linq;

namespace PilaRepo.Shared
{
    /// <summary>Modo de funcionamiento de una Pila&lt;T&gt;, elegido al crearla.</summary>
    internal enum ModoPila
    {
        Estatica,
        Dinamica,
        Nativa
    }

    /// <summary>
    /// Pila genérica con TRES modos (se elige al construirla):
    ///   1) Estática : arreglo propio Node&lt;T&gt;[] de tamaño fijo + entero "top" (empieza en -1).
    ///                 Con N elementos, top = N-1 (índice base 0).
    ///   2) Dinámica : nodos enlazados; "topNodo" apunta al último nodo y cada nodo tiene "Prev".
    ///   3) Nativa   : respaldada por System.Collections.Generic.Stack&lt;T&gt;.
    ///
    /// API igual a la de Stack&lt;T&gt;: Push, Pop, Peek, Clear, Contains y Count (PROPIEDAD).
    /// NO existe IsEmpty: Pop/Peek revisan "Count == 0" internamente.
    /// </summary>
    internal class Pila<T>
    {
        // Mensajes de error EXACTOS de las diapositivas.
        private const string MsgOverflow = "Stack Overflow (la pila está llena)";
        private const string MsgUnderflow = "Stack Underflow (la pila está vacía)";

        private readonly ModoPila _modo;

        // ---- Modo estático ----
        private readonly Node<T>[]? _arreglo;   // arreglo propio de tamaño fijo
        private int top = -1;                   // índice del tope; -1 = vacía

        // ---- Modo dinámico ----
        private Node<T>? topNodo;               // referencia al último nodo (el tope)

        // ---- Modo nativo ----
        private readonly Stack<T>? _nativa;     // pila del framework

        // ------------------------------------------------------------------
        // Constructores
        // ------------------------------------------------------------------

        /// <summary>Pila DINÁMICA (nodos enlazados). Crece sin límite fijo.</summary>
        public Pila()
        {
            _modo = ModoPila.Dinamica;
        }

        /// <summary>Pila ESTÁTICA de tamaño fijo (por defecto en la demo: 4 -> índices 0-3).</summary>
        public Pila(int tamaño)
        {
            if (tamaño <= 0)
                throw new ArgumentOutOfRangeException(nameof(tamaño), "El tamaño debe ser mayor que cero.");
            _modo = ModoPila.Estatica;
            _arreglo = new Node<T>[tamaño];
        }

        // Constructor privado para el modo nativo (se usa solo desde CrearNativa).
        private Pila(ModoPila modo)
        {
            _modo = modo;
            _nativa = new Stack<T>();
        }

        /// <summary>Pila NATIVA respaldada por Stack&lt;T&gt;.</summary>
        public static Pila<T> CrearNativa() => new Pila<T>(ModoPila.Nativa);

        // ------------------------------------------------------------------
        // Información (solo lectura)
        // ------------------------------------------------------------------

        /// <summary>Modo con el que se creó la pila.</summary>
        public ModoPila Modo => _modo;

        /// <summary>Tamaño fijo del arreglo (solo estática); 0 en los otros modos.</summary>
        public int Capacidad => _arreglo?.Length ?? 0;

        /// <summary>
        /// Índice del tope. En la estática es el campo "top" (-1 si está vacía);
        /// en los demás modos equivale a Count - 1 (solo informativo).
        /// </summary>
        public int Top => _modo == ModoPila.Estatica ? top : Count - 1;

        /// <summary>
        /// Número de elementos (PROPIEDAD, no método).
        ///  - Estática : top + 1 (cálculo inmediato; "sumamos 1 porque las posiciones empiezan en 0").
        ///               O(1).
        ///  - Dinámica : como en las diapositivas, un cursor "current" parte de top y baja por Prev
        ///               contando nodos hasta llegar a null (count 0 -> 1 -> 2 -> 3 -> 4).
        ///               No se guarda ningún contador: es O(n) en cada consulta.
        ///  - Nativa   : Stack&lt;T&gt;.Count, mantenido por el framework. O(1).
        /// </summary>
        public int Count
        {
            get
            {
                switch (_modo)
                {
                    case ModoPila.Estatica:
                        return top + 1;

                    case ModoPila.Dinamica:
                        int cuenta = 0;
                        Node<T>? current = topNodo;      // el cursor empieza en el tope
                        while (current != null)
                        {
                            cuenta++;                    // cuenta el nodo actual
                            current = current.Prev;      // baja al nodo de abajo
                        }
                        return cuenta;                   // pila vacía: top = null -> 0

                    default:
                        return _nativa!.Count;
                }
            }
        }

        // ------------------------------------------------------------------
        // Operaciones
        // ------------------------------------------------------------------

        /// <summary>
        /// Apila un elemento en la cima.
        /// Propia estática: O(1) ESTRICTO (top++ y asignar); si top llega al final del arreglo
        ///   lanza "Stack Overflow" porque el tamaño es fijo y no se redimensiona.
        /// Propia dinámica: O(1) estricto (el nodo nuevo apunta con Prev al tope viejo y pasa a ser
        ///   el tope, como en las diapositivas); no hay overflow
        ///   lógico, solo el límite de memoria; cada nodo gasta memoria extra por el enlace Prev
        ///   y el objeto en el heap, y todo el mantenimiento es manual.
        /// Stack&lt;T&gt;: O(1) AMORTIZADO: si el arreglo interno está lleno, el framework lo
        ///   redimensiona (duplica y copia, O(n) ocasional); nunca hay overflow por tamaño y el
        ///   mantenimiento lo hace la clase del framework.
        /// </summary>
        public void Push(T item)
        {
            switch (_modo)
            {
                case ModoPila.Estatica:
                    if (top == _arreglo!.Length - 1)
                        throw new InvalidOperationException(MsgOverflow);
                    top++;
                    // Prev apunta al nodo de abajo (null si es la base).
                    _arreglo[top] = new Node<T>(item, top > 0 ? _arreglo[top - 1] : null);
                    break;

                case ModoPila.Dinamica:
                    // El nuevo nodo apunta (Prev) al antiguo tope y pasa a ser el tope.
                    topNodo = new Node<T>(item, topNodo);
                    break;

                default:
                    _nativa!.Push(item);
                    break;
            }
        }

        /// <summary>
        /// Retira y devuelve el elemento de la cima.
        /// Propia estática: O(1) estricto; lee _arreglo[top], libera esa casilla (null, para que el
        ///   recolector pueda liberar el objeto) y hace top--. El arreglo NO se encoge.
        /// Propia dinámica: el tope pasa a ser Prev y el nodo viejo queda sin referencias (lo recoge
        ///   el GC); la memoria SÍ se libera nodo a nodo. Mover el tope es O(1), pero la revisión
        ///   "Count == 0" del inicio recorre la lista (Count dinámico es O(n), como en las diapositivas),
        ///   así que Pop completo cuesta O(n) en este modo.
        /// Stack&lt;T&gt;: O(1) (el arreglo interno nunca se encoge solo; conserva su capacidad).
        ///
        /// NOTA (modo nativo): Stack&lt;T&gt; guarda solo el VALOR, no nodos. Por eso, para mantener
        /// el mismo esquema de retorno que los otros modos, Pop crea aquí un Node&lt;T&gt; NUEVO con
        /// el valor extraído y devuelve su Data; ese objeto de retorno no existía dentro de la pila.
        /// </summary>
        /// <exception cref="InvalidOperationException">"Stack Underflow (la pila está vacía)".</exception>
        public T Pop()
        {
            if (Count == 0)
                throw new InvalidOperationException(MsgUnderflow);

            Node<T> nodo;
            switch (_modo)
            {
                case ModoPila.Estatica:
                    nodo = _arreglo![top];
                    _arreglo[top] = null!;      // libera la referencia de la casilla
                    top--;
                    break;

                case ModoPila.Dinamica:
                    nodo = topNodo!;
                    topNodo = nodo.Prev;        // el de abajo pasa a ser el tope
                    nodo.Prev = null;           // desenlaza el nodo retirado
                    break;

                default:
                    // Modo nativo: se crea un nodo NUEVO como objeto de retorno.
                    nodo = new Node<T>(_nativa!.Pop(), null);
                    break;
            }
            return nodo.Data;
        }

        /// <summary>
        /// Devuelve el elemento de la cima SIN retirarlo.
        /// Propia estática: O(1) estricto, solo lee _arreglo[top].
        /// Propia dinámica: lee topNodo.Data (O(1)), pero la revisión "Count == 0" recorre la lista
        ///   (O(n)) porque Count dinámico cuenta nodos con el cursor "current".
        /// Stack&lt;T&gt;: O(1); lee la última posición del arreglo interno.
        ///
        /// NOTA (modo nativo): igual que en Pop, Stack&lt;T&gt;.Peek devuelve solo el valor; aquí se
        /// crea un Node&lt;T&gt; NUEVO como objeto de retorno (no existe dentro de la pila).
        /// </summary>
        /// <exception cref="InvalidOperationException">"Stack Underflow (la pila está vacía)".</exception>
        public T Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException(MsgUnderflow);

            Node<T> nodo;
            switch (_modo)
            {
                case ModoPila.Estatica:
                    nodo = _arreglo![top];
                    break;
                case ModoPila.Dinamica:
                    nodo = topNodo!;
                    break;
                default:
                    nodo = new Node<T>(_nativa!.Peek(), null);   // objeto de retorno nuevo
                    break;
            }
            return nodo.Data;
        }

        /// <summary>
        /// Vacía la pila.
        /// Propia estática: Array.Clear libera las referencias (O(n)) y top vuelve a -1; el arreglo
        ///   conserva su tamaño fijo.
        /// Propia dinámica: basta con soltar topNodo (top = null); el GC recoge toda la cadena
        ///   porque nadie la referencia. Count volverá a dar 0 al no haber nodos.
        /// Stack&lt;T&gt;: Clear() limpia el contenido (O(n) con tipos de referencia) pero conserva la
        ///   capacidad del arreglo interno ya redimensionado.
        /// </summary>
        public void Clear()
        {
            switch (_modo)
            {
                case ModoPila.Estatica:
                    Array.Clear(_arreglo!, 0, _arreglo!.Length);
                    top = -1;
                    break;
                case ModoPila.Dinamica:
                    topNodo = null;
                    break;
                default:
                    _nativa!.Clear();
                    break;
            }
        }

        /// <summary>
        /// Indica si el elemento está en la pila. Saca una COPIA (ToList()) y recorre la copia con
        /// el cursor "current", comparando de la cima hacia la base. NUNCA toca la pila original
        /// ni su top (a diferencia de ir haciendo Pop, que la destruiría).
        /// Costo: O(n) en tiempo y O(n) extra en memoria por la copia, igual en los tres modos.
        /// En Stack&lt;T&gt; existe un Contains propio del framework; aquí se usa el mismo método
        /// de la presentación para que los tres modos se comporten igual.
        /// </summary>
        public bool Contains(T item)
        {
            List<T> copia = ObtenerCopia();          // copia (ToList) en orden base -> tope
            int current = copia.Count - 1;           // cursor: empieza en la cima
            while (current >= 0)
            {
                if (EqualityComparer<T>.Default.Equals(copia[current], item))
                    return true;
                current--;                           // baja hacia la base
            }
            return false;
        }

        /// <summary>
        /// Copia de los elementos en orden base -> tope (el último de la lista es el tope).
        /// Sirve para Contains y para dibujar la pila en pantalla sin modificar la original.
        /// Estática: copia de las casillas 0..top. Dinámica: se recorre la cadena Prev desde el
        /// tope y se invierte. Nativa: Stack&lt;T&gt;.ToList() enumera de la cima a la base, por eso
        /// se invierte.
        /// </summary>
        public List<T> ObtenerCopia()
        {
            List<T> copia;
            switch (_modo)
            {
                case ModoPila.Estatica:
                    copia = _arreglo!.Take(top + 1).Select(n => n.Data).ToList();
                    break;

                case ModoPila.Dinamica:
                    copia = new List<T>();
                    for (Node<T>? n = topNodo; n != null; n = n.Prev)
                        copia.Add(n.Data);
                    copia.Reverse();
                    break;

                default:
                    copia = _nativa!.ToList();
                    copia.Reverse();
                    break;
            }
            return copia;
        }
    }
}
